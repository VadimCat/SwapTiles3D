using Client.Models;
using Client.Views;
using Core.Compliments;
using Ji2.Audio;
using Ji2.CommonCore;
using Ji2.Context;
using Ji2Core.Core;
using Ji2Core.Core.Pools;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.UserInput;

namespace Client.Presenters
{
    public class LevelPresenterFactory
    {
        private readonly ScreenNavigator _screenNavigator;
        private readonly UpdateService _updateService;
        private readonly LevelsConfig _levelsConfig;
        private readonly LevelsLoopProgress _levelsLoopProgress;
        private readonly Sound _sound;
        private readonly ICompliments _compliments;
        private readonly CameraProvider _cameraProvider;
        private readonly Pool<CellView> _cellsPool;

        public LevelPresenterFactory(IDependenciesProvider dp)
        {
            _screenNavigator = dp.GetService<ScreenNavigator>();
            _updateService = dp.GetService<UpdateService>();
            _levelsConfig = dp.GetService<LevelsConfig>();
            _levelsLoopProgress = dp.GetService<LevelsLoopProgress>();
            _sound = dp.GetService<Sound>();
            _compliments = dp.GetService<ICompliments>();
            //TODO: replace legacy input with new input system class
            _cameraProvider = dp.GetService<CameraProvider>();
            _cellsPool = dp.GetService<Pool<CellView>>();
        }

        public LevelPresenter Create(FieldView view, LevelPlayableDecorator model)
        {
            return new LevelPresenter(view, model, _screenNavigator, _cellsPool, _updateService, _levelsConfig,
                _levelsLoopProgress, _sound, _compliments, _cameraProvider);
        }
    }
}