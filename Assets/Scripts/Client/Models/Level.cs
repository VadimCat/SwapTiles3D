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
    public class Level : LevelBase
    {
        public event Action LevelCompleted;
        public event Action<Vector2Int> TileSelected;
        public event Action<Vector2Int> TileDeselected;
        public event Action<Vector2Int, Vector2Int> TilesSwapped;
        public event Action<Vector2Int> TileSetted;
        public event Action<int> TurnCompleted;

        public readonly Vector2Int[,] currentPoses;
        public readonly Vector2Int cutSize;
        public int turnsCount;
        public int PerfectResult;
        public int GoodResult;
        public int OkResult;
        public int SelectedTilesCount => selectedTiles.Count;

        public Vector2Int? selectedTile => selectedTiles.Count == 0 ? null : selectedTiles[0];


        private HashSet<Vector2Int> settedTiles;
        private readonly Analytics analytics;
        private readonly LevelData levelData;
        private readonly List<Vector2Int> selectedTiles = new(2);


        public LevelResult Result { get; private set; } = LevelResult.None;

        public Level(Analytics analytics, LevelData levelData, Vector2Int cutSize, ISaveDataContainer saveDataContainer)
            : base(analytics, levelData, saveDataContainer)
        {
            this.cutSize = cutSize;
            settedTiles = new HashSet<Vector2Int>(cutSize.x * cutSize.y);
            PerfectResult = TurnsCountForResult(LevelResult.Perfect);
            GoodResult = TurnsCountForResult(LevelResult.Good);
            OkResult = TurnsCountForResult(LevelResult.Ok);

            currentPoses = Shufflling.CreatedShuffled2DimensionalArray(cutSize);
        }

        public void ClickTile(Vector2Int tilePosition)
        {
            switch (selectedTiles.Count)
            {
                case 0:
                    selectedTiles.Add(tilePosition);
                    TileSelected?.Invoke(tilePosition);
                    break;
                case 1:
                    if (selectedTiles.Contains(tilePosition))
                    {
                        selectedTiles.Remove(tilePosition);
                        TileDeselected?.Invoke(tilePosition);
                    }
                    else
                    {
                        selectedTiles.Add(tilePosition);
                        TileSelected?.Invoke(tilePosition);
                        SwapTiles();
                        selectedTiles.Clear();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool TryGetRandomNotSelectedCell(out Vector2Int selectedTile)
        {
            if (selectedTiles.Count == 1)
            {
                selectedTile = currentPoses[selectedTiles[0].x, selectedTiles[0].y];
                return true;
            }
            else
            {
                List<Vector2Int> notSettedTiles = new List<Vector2Int>();

                for (var x = 0; x < currentPoses.GetLength(0); x++)
                {
                    for (var y = 0; y < currentPoses.GetLength(1); y++)
                    {
                        var cell = currentPoses[x, y];
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
                else
                {
                    selectedTile = notSettedTiles[Random.Range(0, notSettedTiles.Count)];
                    return true;
                }
            }
        }

        private LevelResult GetResult()
        {
            if (turnsCount <= PerfectResult)
            {
                return LevelResult.Perfect;
            }
            else if (turnsCount <= GoodResult)
            {
                return LevelResult.Good;
            }
            else if (turnsCount <= OkResult)
            {
                return LevelResult.Ok;
            }
            else
            {
                return LevelResult.Worst;
            }
        }

        public int TurnsCountForResult(LevelResult result)
        {
            switch (result)
            {
                case LevelResult.Ok:
                    return cutSize.x * cutSize.y * 3;
                    break;
                case LevelResult.Good:
                    return (int)(cutSize.x * cutSize.y * 2);
                    break;
                case LevelResult.Perfect:
                    return (int)(cutSize.x * cutSize.y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

        private void SwapTiles()
        {
            (currentPoses[selectedTiles[0].x, selectedTiles[0].y],
                currentPoses[selectedTiles[1].x, selectedTiles[1].y]) = (
                currentPoses[selectedTiles[1].x, selectedTiles[1].y],
                currentPoses[selectedTiles[0].x, selectedTiles[0].y]);

            TilesSwapped?.Invoke(selectedTiles[0], selectedTiles[1]);
            turnsCount++;
            TurnCompleted?.Invoke(Mathf.Clamp(turnsCount, 0, OkResult + 2));
            CheckComplete();
        }

        private void CheckComplete()
        {
            bool isFailed = false;

            for (var i = 0; i < currentPoses.GetLength(0); i++)
            {
                for (var j = 0; j < currentPoses.GetLength(1); j++)
                {
                    var posToCheck = new Vector2Int(i, j);
                    if (currentPoses[i, j].x != i || currentPoses[i, j].y != j)
                    {
                        isFailed = true;
                    }
                    else if(!settedTiles.Contains(posToCheck))
                    {
                        TileSetted?.Invoke(posToCheck);
                        settedTiles.Add(posToCheck);
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
    }
}