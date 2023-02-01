using System.Collections.Generic;
using Client.Models;
using Ji2Core.Core.SaveDataContainer;
using Random = System.Random;

namespace Client
{
    public class LevelsLoopProgress
    {
        private readonly Random rnd = new(19021999);
        private readonly ISaveDataContainer save;
        private readonly string[] levelOrder;

        private const string LastLevelNumberKey = "LastLevelNumber";
        private const string LastLevelCountKey = "LastLevelCount";
        private const string RandomLevelsKey = "RandomLevels";

        public List<int> randomLevels;

        public LevelsLoopProgress(ISaveDataContainer save, string[] levelOrder)
        {
            this.save = save;
            this.levelOrder = levelOrder;
        }

        public void Load()
        {
            randomLevels = save.GetValue(RandomLevelsKey, new List<int>());
            for (int i = 0; i < randomLevels.Count; i++)
            {
                rnd.Next(levelOrder.Length);
            }
        }

        public LevelData GetNextLevelData()
        {
            int playedTotal = save.GetValue<int>(LastLevelNumberKey);
            return GetLevelData(playedTotal);
        }

        public void IncLevel()
        {
            int playedUnique = save.GetValue<int>(LastLevelCountKey);
            save.SaveValue(LastLevelCountKey, playedUnique + 1);

            int playedTotal = save.GetValue<int>(LastLevelNumberKey);
            save.SaveValue(LastLevelNumberKey, playedTotal + 1);
        }

        public LevelData GetRetryLevelData()
        {
            int playedTotal = save.GetValue<int>(LastLevelCountKey) - 1;
            return GetLevelData(playedTotal);
        }

        public void Reset()
        {
            save.ResetKey(LastLevelCountKey);
            save.ResetKey(LastLevelNumberKey);
            save.ResetKey(RandomLevelsKey);
        }

        private LevelData GetLevelData(int playedUnique)
        {
            int levelCount = save.GetValue<int>(LastLevelCountKey);
            int lvlLoop = playedUnique / levelOrder.Length;

            string lvlId = GetLevelName(playedUnique);

            return new LevelData
            {
                name = lvlId,
                uniqueLevelNumber = playedUnique,
                levelCount = levelCount,
                lvlLoop = lvlLoop
            };
        }

        private string GetLevelName(int playedUniqueTotal)
        {
            if (playedUniqueTotal >= levelOrder.Length)
            {
                int randomLvl = playedUniqueTotal - levelOrder.Length;
                if (randomLvl >= randomLevels.Count)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        randomLevels.Add(rnd.Next(levelOrder.Length));
                        save.SaveValue(RandomLevelsKey, randomLevels);
                    }
                }

                return levelOrder[randomLevels[randomLvl]];
            }
            else
            {
                return levelOrder[playedUniqueTotal % levelOrder.Length];
            }
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
    public Difficulty difficulty;
}