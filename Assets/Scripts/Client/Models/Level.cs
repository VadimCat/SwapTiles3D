using System;
using System.Collections.Generic;
using Ji2.CommonCore.SaveDataContainer;
using Ji2.Models;
using Ji2.Models.Analytics;
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
        public event Action<Vector2Int, float> TileRotated;
        public event Action<Vector2Int> TileSet;
        public event Action<int> TurnCompleted;

        public Vector2Int[,] CurrentPoses;
        private readonly bool[,] CutTemplate;
        private int _turnsCount;
        private Dictionary<Vector2Int, float> _tilesRotation = new();

        public IReadOnlyDictionary<Vector2Int, float> TilesRotation => _tilesRotation;
        public readonly int PerfectResult;
        public readonly int GoodResult;
        public readonly int OkResult;
        public int SelectedTilesCount => _selectedTiles.Count;

        private HashSet<Vector2Int> _settedTiles;
        private readonly Analytics _analytics;
        private readonly LevelData _levelData;
        private readonly List<Vector2Int> _selectedTiles = new(2);

        public LevelResult Result { get; private set; } = LevelResult.None;

        public Level(IAnalytics analytics, LevelData levelData, bool[,] cutTemplate,
            ISaveDataContainer saveDataContainer)
            : base(analytics, levelData, saveDataContainer)
        {
            CutTemplate = cutTemplate;
            _settedTiles = new HashSet<Vector2Int>(cutTemplate.GetLength(0) * cutTemplate.GetLength(1));
            PerfectResult = TurnsCountForResult(LevelResult.Perfect);
            GoodResult = TurnsCountForResult(LevelResult.Good);
            OkResult = TurnsCountForResult(LevelResult.Ok);

            BuildLevel();

            void BuildLevel()
            {
                CurrentPoses =
                    Shufflling.CreatedShuffled2DimensionalArray(new Vector2Int(cutTemplate.GetLength(0),
                        cutTemplate.GetLength(1)));

                for (var i = 0; i < CutTemplate.GetLength(0); i++)
                for (var j = 0; j < CutTemplate.GetLength(1); j++)
                {
                    var originPos = CurrentPoses[i, j];
                    if (CutTemplate[originPos.x, originPos.y])
                    {
                        ClickTile(originPos);
                        ClickTile(new Vector2Int(i, j));
                    }
                }
                
                for (var i = 0; i < CutTemplate.GetLength(0); i++)
                for (var j = 0; j < CutTemplate.GetLength(1); j++)
                {
                    var originPos = CurrentPoses[i, j];
                    if (CutTemplate[originPos.x, originPos.y])
                    {
                        _tilesRotation[originPos] = 0;
                    }
                    else
                    {
                        _tilesRotation[originPos] = Random.Range(0, 4) * 90;
                    }
                }
            }
        }

        public void ClickTile(Vector2Int tilePosition)
        {
            switch (_selectedTiles.Count)
            {
                case 0:
                    _selectedTiles.Add(tilePosition);
                    TileSelected?.Invoke(tilePosition);
                    break;
                case 1:
                    if (_selectedTiles.Contains(tilePosition))
                    {
                        _selectedTiles.Remove(tilePosition);
                        TileDeselected?.Invoke(tilePosition);
                    }
                    else
                    {
                        _selectedTiles.Add(tilePosition);
                        TileSelected?.Invoke(tilePosition);
                        SwapTiles();
                        _selectedTiles.Clear();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool TryGetRandomNotSelectedCell(out Vector2Int selectedTile)
        {
            if (_selectedTiles.Count == 1)
            {
                selectedTile = CurrentPoses[_selectedTiles[0].x, _selectedTiles[0].y];
                return true;
            }

            List<Vector2Int> notSettedTiles = new List<Vector2Int>();

            for (var x = 0; x < CurrentPoses.GetLength(0); x++)
            {
                for (var y = 0; y < CurrentPoses.GetLength(1); y++)
                {
                    var cell = CurrentPoses[x, y];
                    if (cell.x != x || cell.y != y)
                    {
                        notSettedTiles.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (notSettedTiles.Count == 0)
            {
                selectedTile = default;
                return false;
            }

            selectedTile = notSettedTiles[Random.Range(0, notSettedTiles.Count)];
            return true;
        }

        private LevelResult GetResult()
        {
            if (_turnsCount <= PerfectResult)
            {
                return LevelResult.Perfect;
            }

            if (_turnsCount <= GoodResult)
            {
                return LevelResult.Good;
            }

            if (_turnsCount <= OkResult)
            {
                return LevelResult.Ok;
            }

            return LevelResult.Worst;
        }

        public int TurnsCountForResult(LevelResult result)
        {
            switch (result)
            {
                case LevelResult.Ok:
                    return CutTemplate.GetLength(0) * CutTemplate.GetLength(1) * 3;
                    break;
                case LevelResult.Good:
                    return CutTemplate.GetLength(0) * CutTemplate.GetLength(1) * 2;
                    break;
                case LevelResult.Perfect:
                    return CutTemplate.GetLength(0) * CutTemplate.GetLength(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

        private void SwapTiles()
        {
            (CurrentPoses[_selectedTiles[0].x, _selectedTiles[0].y],
                CurrentPoses[_selectedTiles[1].x, _selectedTiles[1].y]) = (
                CurrentPoses[_selectedTiles[1].x, _selectedTiles[1].y],
                CurrentPoses[_selectedTiles[0].x, _selectedTiles[0].y]);

            TilesSwapped?.Invoke(_selectedTiles[0], _selectedTiles[1]);
            _turnsCount++;
            TurnCompleted?.Invoke(Mathf.Clamp(_turnsCount, 0, OkResult + 2));
            CheckComplete();
        }

        private void CheckComplete()
        {
            bool isFailed = false;

            for (var i = 0; i < CurrentPoses.GetLength(0); i++)
            {
                for (var j = 0; j < CurrentPoses.GetLength(1); j++)
                {
                    var posToCheck = new Vector2Int(i, j);
                    if (CurrentPoses[i, j].x != i || CurrentPoses[i, j].y != j)
                    {
                        isFailed = true;
                    }
                    else if (!_settedTiles.Contains(posToCheck))
                    {
                        TileSet?.Invoke(posToCheck);
                        _settedTiles.Add(posToCheck);
                    }
                }
            }

            if (!isFailed)
            {
                Result = GetResult();
                LevelCompleted?.Invoke();
                saveDataContainer.ResetKey(Name);
            }
        }

        public void TrySwipe(Direction direction)
        {
            if (_selectedTiles.Count == 1)
            {
                switch(direction)
                {
                    case Direction.Left:
                        _tilesRotation[_selectedTiles[0]] -= 90;
                        TileRotated?.Invoke(_selectedTiles[0], _tilesRotation[_selectedTiles[0]]);
                        break;
                    case Direction.Right:
                        _tilesRotation[_selectedTiles[0]] += 90;
                        TileRotated?.Invoke(_selectedTiles[0], _tilesRotation[_selectedTiles[0]]);
                        break;
                }
            }
        }
    }

    public enum Direction
    {
        None,
        Up,
        Right,
        Down,
        Left
    }
}