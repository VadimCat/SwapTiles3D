using Ji2Core.Core.SaveDataContainer;
using Random = System.Random;

namespace Client
{
    public class LevelsLoopProgress
    {
        private readonly Random rnd = new(19021999);
        private readonly ISaveDataContainer save;
        private readonly string[] levelOrder;

        private const string LevelsPlayedTotalIndexKey = "LastPlayedLevel";
        private const string LevelsPlayedUniqueTotalKey = "LastPlayedLevel";

        public LevelsLoopProgress(ISaveDataContainer save, string[] levelOrder)
        {
            this.save = save;
            this.levelOrder = levelOrder;
        }

        public LevelData GetNextLevelData()
        {
            int playedTotal = save.GetValue<int>(LevelsPlayedTotalIndexKey);
            return GetLevelData(playedTotal);
        }

        private LevelData GetLevelData(int playedTotal)
        {
            int playedUniqueTotal = save.GetValue<int>(LevelsPlayedUniqueTotalKey);
            int lvlLoop = playedUniqueTotal / levelOrder.Length;
            int lvlIndex = playedUniqueTotal % levelOrder.Length;
            string lvlId = GetLevelName(playedUniqueTotal);

            return new LevelData
            {
                name = lvlId,
                uniqueLevelNumber = playedUniqueTotal,
                levelCount = playedTotal,
                lvlLoop = lvlLoop
            };
        }

        private string GetLevelName(int playedUniqueTotal)
        {
            var lvlIndex = playedUniqueTotal % levelOrder.Length;
            if (lvlIndex >= levelOrder.Length)
            {
                lvlIndex = rnd.Next(levelOrder.Length);
            }
            return levelOrder[lvlIndex];
        }

        public void IncLevel()
        {
            int playedUnique = save.GetValue<int>(LevelsPlayedUniqueTotalKey);
            save.SaveValue(LevelsPlayedUniqueTotalKey, playedUnique + 1);
            
            int playedTotal = save.GetValue<int>(LevelsPlayedTotalIndexKey);
            save.SaveValue(LevelsPlayedTotalIndexKey, playedTotal + 1);
        }

        public LevelData GetRetryLevelData()
        {
            int playedTotal = save.GetValue<int>(LevelsPlayedUniqueTotalKey) - 1;
            return GetLevelData(playedTotal);
        }
    }
}

public class LevelData
{
    public string name;
    public int uniqueLevelNumber;
    public int levelCount;
    public int lvlLoop;
    public int isRandom;
}