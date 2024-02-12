using System;
using System.Collections.Generic;
using System.Linq;
using Ji2.Models;
using Ji2.Models.Analytics;
using Ji2.Utils;
using Ji2.Utils.Shuffling;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Client.Models
{
    public class LevelPlayableDecorator : ILevel
    {
        public event Action<Vector2Int> TileSelected;
        public event Action<Vector2Int> TileDeselected;
        public event Action<Vector2Int, Vector2Int> TilesSwapped;
        public event Action<Vector2Int, int> TileRotated;
        public event Action<Vector2Int> TileSet;

        public Cell[,] CurrentPoses;

        private readonly int _rotationAngle;
        private readonly ILevel _level;
        public int SelectedTilesCount => _selectedPositions.Count;
        
        private readonly HashSet<Vector2Int> _setTiles;
        private readonly Analytics _analytics;
        private readonly LevelData _levelData;
        private readonly List<Vector2Int> _selectedPositions = new(2);

        public Vector2Int Size => new(CurrentPoses.GetLength(0), CurrentPoses.GetLength(1));

        public Vector2Int FirstSelected => _selectedPositions.FirstOrDefault();
        
        public ProgressBase Progress => _level.Progress;
        public string Name => _level.Name;
        public int LevelCount => _level.LevelCount;
        public Difficulty Difficulty => _level.Difficulty;
        public LevelData LevelData => _level.LevelData;
        public event Action EventLevelCompleted
        {
            add => _level.EventLevelCompleted += value;
            remove => _level.EventLevelCompleted -= value;
        }
        
        public LevelPlayableDecorator(bool[,] cutTemplate, int rotationAngle, ILevel level)
        {
            _rotationAngle = rotationAngle;
            _level = level;
            _setTiles = new HashSet<Vector2Int>(cutTemplate.GetLength(0) * cutTemplate.GetLength(1));

            BuildLevel();

            void BuildLevel()
            {

                CurrentPoses = new Cell[cutTemplate.GetLength(0), cutTemplate.GetLength(1)];
                
                var availableElements = new List<Vector2Int>(cutTemplate.GetLength(0) * cutTemplate.GetLength(1));
                
                for (var i = 0; i < cutTemplate.GetLength(0); i++)
                for (var j = 0; j < cutTemplate.GetLength(1); j++)
                {
                    if (!cutTemplate[i, j])
                    {
                        availableElements.Add(new Vector2Int(i, j));
                    }
                }

                var shuffledIndexes = Shufflling.CreateShuffledArray(availableElements.Count);
                    int elementsUsed = 0;
                
                for (var i = 0; i < cutTemplate.GetLength(0); i++)
                for (var j = 0; j < cutTemplate.GetLength(1); j++)
                {
                    var current = new Vector2Int(i,j);
                    if (cutTemplate[i, j])
                    {
                        CurrentPoses[i, j] = Cell.Disabled(current);
                        _setTiles.Add(current);
                    }
                    else
                    {
                        int rotationsCount = rotationAngle == 0 ? 0 : 360 / rotationAngle;
                        int rotation = rotationAngle == 0 ? 0 : Random.Range(0, rotationsCount) * _rotationAngle;
                        CurrentPoses[i, j] = new Cell(availableElements[shuffledIndexes[elementsUsed++]], rotation);
                    }
                }
            }
        }

        public ClickResult ClickTile(Vector2Int tilePosition)
        {
            if (!CurrentPoses.IsInRange2D(tilePosition.x, tilePosition.y))
            {
                DeselectCurrent();
                return ClickResult.OutOfRange;
            }

            var currentPos = CurrentPoses[tilePosition.x, tilePosition.y];
            if (!currentPos.IsActive)
            {
                DeselectCurrent();
                return ClickResult.ClickOnInactive;
            }

            if (!currentPos.IsOnRightPlace(tilePosition.x, tilePosition.y) || !currentPos.IsDefaultRotation())
            {
                switch (_selectedPositions.Count)
                {
                    case 0:
                        SelectTile(tilePosition);
                        return ClickResult.Select;
                    case 1:
                        if (_selectedPositions.Contains(tilePosition))
                        {
                            _selectedPositions.Remove(tilePosition);
                            TileDeselected?.Invoke(tilePosition);
                            return ClickResult.Select;
                        }
                        else
                        {
                            SelectTile(tilePosition);
                            SwapTiles();
                            return ClickResult.Swap;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                DeselectCurrent();
                return ClickResult.Deselect;
            }

            void SwapTiles()
            {
                Swap(_selectedPositions[0], _selectedPositions[1]);
                TilesSwapped?.Invoke(_selectedPositions[0], _selectedPositions[1]);

                _selectedPositions.Clear();

                CheckComplete();
            }
        }
        
        public bool TryGetRandomNotSelectedCell(out Vector2Int selectedTile)
        {
            if (_selectedPositions.Count == 1)
            {
                selectedTile = CurrentPoses[_selectedPositions[0].x, _selectedPositions[0].y].OriginalPos;
                return true;
            }

            List<Vector2Int> notSetTiles = new List<Vector2Int>();

            for (var x = 0; x < CurrentPoses.GetLength(0); x++)
            {
                for (var y = 0; y < CurrentPoses.GetLength(1); y++)
                {
                    var cell = CurrentPoses[x, y];

                    if (!cell.IsOnRightPlace(x, y))
                    {
                        notSetTiles.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (notSetTiles.Count == 0)
            {
                selectedTile = default;
                return false;
            }

            selectedTile = notSetTiles[Random.Range(0, notSetTiles.Count)];
            return true;
        }

        public void TrySwipe(RotationDirection direction)
        {
            if (_selectedPositions.Count == 1)
            {
                ref Cell selectedTile = ref CurrentPoses[_selectedPositions[0].x, _selectedPositions[0].y];
                int directionMultiplier;
                switch (direction)
                {
                    case RotationDirection.CounterClockwise:
                        directionMultiplier = -1;
                        break;
                    case RotationDirection.Clockwise:
                        directionMultiplier = 1;
                        break;
                    default:
                        return;
                }

                selectedTile.Rotation = ClampAngle(selectedTile.Rotation + directionMultiplier * _rotationAngle);
                TileRotated?.Invoke(_selectedPositions[0], selectedTile.Rotation);

                CheckComplete();
            }

            int ClampAngle(int rotation)
            {
                return (rotation % 360 + 360) % 360;
            }
        }
        
        private void CheckComplete()
        {
            bool isFailed = false;

            for (var i = 0; i < CurrentPoses.GetLength(0); i++)
            {
                for (var j = 0; j < CurrentPoses.GetLength(1); j++)
                {
                    var cellIndex = new Vector2Int(i, j);
                    Cell cell = CurrentPoses[i, j];

                    if (cell.IsOnRightPlace(i, j) && cell.IsDefaultRotation())
                    {
                        if (!_setTiles.Contains(cellIndex))
                        {
                            SetTile(cellIndex);
                        }
                    }
                    else
                    {
                        isFailed = true;
                    }
                }
            }

            if (!isFailed)
            {
                _level.Complete();
            }
        }

        private void SetTile(Vector2Int cellIndex)
        {
            _setTiles.Add(cellIndex);
            if (_selectedPositions.Contains(cellIndex))
            {
                DeselectCurrent();
            }
            TileSet?.Invoke(cellIndex);
        }

        private void Swap(Vector2Int pos1, Vector2Int pos2)
        {
            (CurrentPoses[pos1.x, pos1.y], CurrentPoses[pos2.x, pos2.y]) =
                (CurrentPoses[pos2.x, pos2.y], CurrentPoses[pos1.x, pos1.y]);
        }

        private void SelectTile(Vector2Int tilePosition)
        {
            _selectedPositions.Add(tilePosition);
            if (_selectedPositions.Count != 2)
            {
                TileSelected?.Invoke(tilePosition);
            }
        }

        private void DeselectCurrent()
        {
            for (var i = 0; i < _selectedPositions.Count; i++)
            {
                var selected = _selectedPositions[i];
                var pos = selected;
                _selectedPositions.Remove(pos);
                TileDeselected?.Invoke(pos);
            }
        }

        public void Complete()
        {
            _level.Complete();
        }

        public void Start()
        {
            _level.Start();
        }
        
        public void AppendPlayTime(float time)
        {
            _level.AppendPlayTime(time);
        }

        public void LoadProgress(ProgressBase progress)
        {
            _level.LoadProgress(progress);
        }

        public void CreateProgress()
        {
            _level.CreateProgress();
        }

        public enum ClickResult
        {
            OutOfRange,
            ClickOnInactive,
            Select,
            Swap,
            Deselect
        }
    }

}