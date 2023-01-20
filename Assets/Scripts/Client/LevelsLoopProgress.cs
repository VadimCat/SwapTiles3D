using Ji2Core.Core.SaveDataContainer;
using UnityEngine;

namespace Client
{
    public class LevelsLoopProgress
    {
        private readonly ISaveDataContainer saveDataContainer;
        private readonly string[] levelOrder;

        private const string LevelsPlayedTotalIndexKey = "LastPlayedLevel";

        public LevelsLoopProgress(ISaveDataContainer saveDataContainer, string[] levelOrder)
        {
            this.saveDataContainer = saveDataContainer;
            this.levelOrder = levelOrder;
        }

        public LevelData GetNextLevelData()
        {
            int playedTotal = saveDataContainer.GetValue<int>(LevelsPlayedTotalIndexKey);
            int lvlIndex = playedTotal % levelOrder.Length;
            int lvlLoop = playedTotal / levelOrder.Length;
            string lvlId = levelOrder[lvlIndex];
            
            return new LevelData()
            {
                name = lvlId,
                playedTotal = playedTotal,
                lvlIndex = lvlIndex,
                lvlLoop = lvlLoop
            };
        }

        public void IncLevel()
        {
            int playedTotal = saveDataContainer.GetValue<int>(LevelsPlayedTotalIndexKey);
            saveDataContainer.SaveValue(LevelsPlayedTotalIndexKey, playedTotal + 1);
        }
    }
}

public class LevelData
{
    public string name;
    public int playedTotal;
    public int lvlLoop;
    public int lvlIndex;
}