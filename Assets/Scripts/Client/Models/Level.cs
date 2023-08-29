using System;
using System.Collections.Generic;
using System.Linq;
using Ji2.CommonCore.SaveDataContainer;
using Ji2.Models;
using Ji2.Models.Analytics;
using Ji2.Utils;
using Ji2.Utils.Shuffling;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Client.Models
{
    public class Level : LevelBase<ProgressBase>
    {
        public event Action LevelCompleted;
        public event Action<Vector2Int> TileSelected;
        public event Action<Vector2Int> TileDeselected;
        public event Action<Vector2Int, Vector2Int> TilesSwapped;
        public event Action<Vector2Int, int> TileRotated;
        public event Action<Vector2Int> TileSet;
        public event Action<int> TurnCompleted;

        public CellData[,] CurrentPoses;
        private int _turnsCount;

        private readonly bool[,] _cutTemplate;
        private readonly int _rotationAngle;

        public readonly int PerfectResult;
        public readonly int GoodResult;
        public readonly int OkResult;
        public int SelectedTilesCount => _selectedPositions.Count;


        private readonly HashSet<Vector2Int> _setTiles;
        private readonly Analytics _analytics;
        private readonly LevelData _levelData;
        private readonly List<Vector2Int> _selectedPositions = new(2);

        public Vector2Int Size => new(CurrentPoses.GetLength(0), CurrentPoses.GetLength(1));

        public LevelResult Result { get; private set; } = LevelResult.None;
        public Vector2Int FirstSelected => _selectedPositions.FirstOrDefault();

        public Level(IAnalytics analytics, LevelData levelData, bool[,] cutTemplate, int rotationAngle,
            ISaveDataContainer saveDataContainer)
            : base(analytics, levelData, saveDataContainer)
        {
            _cutTemplate = cutTemplate;
            _rotationAngle = rotationAngle;
            _setTiles = new HashSet<Vector2Int>(cutTemplate.GetLength(0) * cutTemplate.GetLength(1));
            PerfectResult = TurnsCountForResult(LevelResult.Perfect);
            GoodResult = TurnsCountForResult(LevelResult.Good);
            OkResult = TurnsCountForResult(LevelResult.Ok);

            BuildLevel();

            void BuildLevel()
            {
                var shuffledImagePoses = Shufflling.CreatedShuffled2DimensionalArray(new Vector2Int(
                    cutTemplate.GetLength(0),
                    cutTemplate.GetLength(1)));

                CurrentPoses = new CellData[cutTemplate.GetLength(0), cutTemplate.GetLength(1)];

                for (var i = 0; i < _cutTemplate.GetLength(0); i++)
                for (var j = 0; j < _cutTemplate.GetLength(1); j++)
                {
                    var imagePos = shuffledImagePoses[i, j];
                    if (_cutTemplate[imagePos.x, imagePos.y])
                    {
                        CurrentPoses[i, j] = CellData.Disabled(imagePos);
                    }
                    else
                    {
                        int rotationsCount = rotationAngle == 0 ? 0 : 360 / rotationAngle;
                        int rotation = rotationAngle == 0 ? 0 : Random.Range(0, rotationsCount) * _rotationAngle;

                        CurrentPoses[i, j] = new CellData(new Vector2Int(imagePos.x, imagePos.y), rotation);
                    }
                }

                for (var i = 0; i < _cutTemplate.GetLength(0); i++)
                for (var j = 0; j < _cutTemplate.GetLength(1); j++)
                {
                    while (!CurrentPoses[i, j].IsActive && !CurrentPoses[i, j].IsOnRightPlace(i, j))
                    {
                        Swap(new Vector2Int(i, j), CurrentPoses[i, j].OriginalPos);
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

                IncTurnsCount();
                CheckComplete();
            }
        }

        private void Swap(Vector2Int pos1, Vector2Int pos2)
        {
            (CurrentPoses[pos1.x, pos1.y], CurrentPoses[pos2.x, pos2.y]) =
                (CurrentPoses[pos2.x, pos2.y], CurrentPoses[pos1.x, pos1.y]);
        }

        private void IncTurnsCount()
        {
            _turnsCount++;
            TurnCompleted?.Invoke(Mathf.Clamp(_turnsCount, 0, OkResult + 2));
        }

        private void SelectTile(Vector2Int tilePosition)
        {
            _selectedPositions.Add(tilePosition);
            TileSelected?.Invoke(tilePosition);
        }

        private void DeselectCurrent()
        {
            if (_selectedPositions.Count > 0)
            {
                var pos = _selectedPositions[0];
                _selectedPositions.Remove(pos);
                TileDeselected?.Invoke(pos);
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

        private int TurnsCountForResult(LevelResult result)
        {
            switch (result)
            {
                case LevelResult.Ok:
                    return _cutTemplate.GetLength(0) * _cutTemplate.GetLength(1) * 3;
                case LevelResult.Good:
                    return _cutTemplate.GetLength(0) * _cutTemplate.GetLength(1) * 2;
                case LevelResult.Perfect:
                    return _cutTemplate.GetLength(0) * _cutTemplate.GetLength(1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
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
                    CellData cell = CurrentPoses[i, j];

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
                Result = GetResult();
                LevelCompleted?.Invoke();
                saveDataContainer.ResetKey(Name);
            }

            LevelResult GetResult()
            {
                if (_turnsCount <= PerfectResult)
                {
                    return LevelResult.Perfect;
                }

                if (_turnsCount <= GoodResult)
                {
                    return LevelResult.Good;
                }

                return _turnsCount <= OkResult ? LevelResult.Ok : LevelResult.Worst;
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

        public void TrySwipe(RotationDirection direction)
        {
            if (_selectedPositions.Count == 1)
            {
                ref CellData selectedTile = ref CurrentPoses[_selectedPositions[0].x, _selectedPositions[0].y];
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
                if (_setTiles.Contains(_selectedPositions[0]))
                {
                    _selectedPositions.Clear();
                }
            }

            int ClampAngle(int rotation)
            {
                while (rotation >= 360)
                {
                    rotation -= 360;
                }

                while (rotation < 0)
                {
                    rotation += 360;
                }

                return rotation;
            }
        }
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