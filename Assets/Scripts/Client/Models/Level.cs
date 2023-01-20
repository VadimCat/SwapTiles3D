using System;
using System.Collections.Generic;
using Ji2Core.Core.Analytics;
using Ji2Core.Core.SaveDataContainer;
using Ji2Core.Utils.Shuffling;
using UnityEngine;

namespace Client.Models
{
    public class Level : LevelBase
    {
        public event Action LevelCompleted;
        public event Action<Vector2Int> TileSelected;
        public event Action<Vector2Int> TileDeselected;
        public event Action<Vector2Int, Vector2Int> TilesSwapped;

        public readonly Vector2Int[,] currentPoses;
        public readonly Vector2Int cutSize;
        
        private readonly Analytics analytics;
        private readonly LevelData levelData;
        private readonly List<Vector2Int> selectedTiles = new(2);

        public Level(Analytics analytics, LevelData levelData, Vector2Int cutSize, ISaveDataContainer saveDataContainer)
            : base(analytics, levelData, saveDataContainer)
        {
            this.cutSize = cutSize;
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

        private void SwapTiles()
        {
            (currentPoses[selectedTiles[0].x, selectedTiles[0].y],
                currentPoses[selectedTiles[1].x, selectedTiles[1].y]) = (
                currentPoses[selectedTiles[1].x, selectedTiles[1].y],
                currentPoses[selectedTiles[0].x, selectedTiles[0].y]);
            
            TilesSwapped?.Invoke(selectedTiles[0], selectedTiles[1]);
            CheckComplete();
        }

        private void CheckComplete()
        {
            bool isFailed = false;

            for (var i = 0; i < currentPoses.GetLength(0); i++)
            {
                for (var j = 0; j < currentPoses.GetLength(1); j++)
                {
                    if (currentPoses[i, j].x != i || currentPoses[i, j].y != j)
                        isFailed = true;
                }

                if (isFailed)
                    break;
            }

            if (!isFailed)
            {
                LevelCompleted?.Invoke();
                saveDataContainer.ResetKey(Name);
            }
        }
    }
}