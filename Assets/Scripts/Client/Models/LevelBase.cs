using System.Collections.Generic;
using Ji2Core.Core.Analytics;
using Models;

namespace Client.Models
{
    public abstract class LevelBase
    {
        private readonly Analytics analytics;
        private readonly LevelData levelData;
        private float playTime = 0;

        public string name => levelData.name;
        
        public LevelBase(Analytics analytics, LevelData levelData)
        {
            this.analytics = analytics;
            this.levelData = levelData;
        }
        
        public void AppendPlayTime(float time)
        {
            playTime += time;
        }

        public void LogAnalyticsLevelStart()
        {
            var eventData = new Dictionary<string, object>
            {
                [Constants.LevelNumberKey] = levelData.playedTotal,
                [Constants.LevelNameKey] = levelData.name,
                [Constants.LevelCountKey] = levelData.playedTotal,
                [Constants.LevelLoopKey] = levelData.lvlLoop
            };
            analytics.LogEventDirectlyTo<YandexMetricaLogger>(Constants.StartEvent, eventData);
            analytics.ForceSendDirectlyTo<YandexMetricaLogger>();
        }
        
        public void LogAnalyticsLevelFinish(LevelExitType levelExitType = LevelExitType.win)
        {
            var eventData = new Dictionary<string, object>
            {
                [Constants.LevelNumberKey] = levelData.playedTotal,
                [Constants.LevelNameKey] = levelData.name,
                [Constants.LevelCountKey] = levelData.playedTotal,
                [Constants.LevelLoopKey] = levelData.lvlLoop,
                [Constants.ResultKey] = ((int)levelExitType).ToString(),
                [Constants.TimeKey] = playTime
            };

            analytics.LogEventDirectlyTo<YandexMetricaLogger>(Constants.FinishEvent, eventData);
            analytics.ForceSendDirectlyTo<YandexMetricaLogger>();
        }
    }
}