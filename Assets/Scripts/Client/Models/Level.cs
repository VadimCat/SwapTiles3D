using Ji2Core.Core.Analytics;
using Ji2Core.Utils.Shuffling;
using UnityEngine;

namespace Client.Models
{
    public class Level : LevelBase
    {
        private readonly Analytics analytics;
        private readonly LevelData levelData;
        
        public Vector2Int[,] CurrentPoses;

        public Level(Analytics analytics, LevelData levelData, Vector2Int cutSize) : base(analytics, levelData)
        {
            CurrentPoses = Shufflling.CreatedShuffled2DimensionalArray(cutSize);
        }
    }
}