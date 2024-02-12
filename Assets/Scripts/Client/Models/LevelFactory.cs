using Ji2.CommonCore.SaveDataContainer;
using Ji2.Context;
using Ji2.Models;
using Ji2.Models.Analytics;

namespace Client.Models
{
    public class LevelFactory
    {
        private readonly ISaveDataContainer _saveDataContainer;
        private readonly IAnalytics _analytics;

        public LevelFactory(IDependenciesProvider dependenciesProvider)
        {
            _saveDataContainer = dependenciesProvider.GetService<ISaveDataContainer>();
            _analytics = dependenciesProvider.GetService<IAnalytics>();
        }

        public LevelPlayableDecorator Create(LevelData levelData, bool[,] cutTemplate, int rotationAngle)
        {
            return new LevelPlayableDecorator(cutTemplate, rotationAngle, 
                new LevelAnalyticsLoggerDecorator(_analytics, _saveDataContainer,
                    new LevelBase(levelData, _saveDataContainer)));
        }
    }
}