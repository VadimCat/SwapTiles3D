using Client.Models;
using Client.Presenters;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2;
using Ji2.Context;
using Ji2.States;
using Ji2.UI.Screens;
using Ji2Core.Core.ScreenNavigation;
using UI.Background;

namespace Client.States
{
    public class LoadLevelState : IPayloadedState<LoadLevelStatePayload>
    {
        private const string GameSceneName = "LevelScene";

        private readonly StateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly ScreenNavigator _screenNavigator;
        private readonly IDependenciesProvider _dp;
        private readonly LevelsConfig _levelsConfig;
        private readonly BackgroundService _backgroundService;
        private readonly LevelFactory _levelFactory;
        private readonly LevelPresenterFactory _levelPresenterFactory;

        private LoadingScreen _loadingScreen;
        private LevelData _levelData;

        public LoadLevelState(IDependenciesProvider dp, StateMachine stateMachine, SceneLoader sceneLoader,
            ScreenNavigator screenNavigator, LevelsConfig levelsConfig, BackgroundService backgroundService,
            LevelFactory levelFactory, LevelPresenterFactory levelPresenterFactory)
        {
            _dp = dp;
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _screenNavigator = screenNavigator;
            _levelsConfig = levelsConfig;
            _backgroundService = backgroundService;
            _levelFactory = levelFactory;
            _levelPresenterFactory = levelPresenterFactory;
        }

        public async UniTask Enter(LoadLevelStatePayload payload)
        {
            _levelData = payload.LevelData;
            var sceneTask = _sceneLoader.LoadScene(GameSceneName);
            _loadingScreen = await _screenNavigator.PushScreen<LoadingScreen>();

            if (payload.FakeLoadingTime == 0)
            {
                _sceneLoader.OnProgressUpdate += _loadingScreen.SetProgress;
                await sceneTask;
                _sceneLoader.OnProgressUpdate -= _loadingScreen.SetProgress;
            }
            else
            {
                var progressBarTask = _loadingScreen.AnimateLoadingBar(payload.FakeLoadingTime);
                await UniTask.WhenAll(sceneTask, progressBarTask);
            }

            var gamePayload = BuildLevel();

            _stateMachine.Enter<GameState, GameStatePayload>(gamePayload).Forget();
        }

        private GameStatePayload BuildLevel()
        {
            var view = _dp.GetService<FieldView>();
            var viewConfig = _levelsConfig.GetData(_levelData.Name);
            _backgroundService.SwitchBackground(viewConfig.Background);
            var viewData = viewConfig.ViewData(_levelData.LvlLoop);
            //HACK
            _levelData.Difficulty = viewData.Difficulty;
            var levelModel = _levelFactory.Create(_levelData, viewData.CutTemplate, viewData.DiscreteRotationAngle);
            var levelPresenter = _levelPresenterFactory.Create(view, levelModel);

            levelPresenter.BuildLevel();

            return new GameStatePayload
            {
                LevelPresenter = levelPresenter
            };
        }

        public async UniTask Exit()
        {
            await _screenNavigator.CloseScreen<LoadingScreen>();
        }
    }

    public class LoadLevelStatePayload
    {
        public readonly float FakeLoadingTime;
        public readonly LevelData LevelData;

        public LoadLevelStatePayload(LevelData levelData, float fakeLoadingTime = 0)
        {
            LevelData = levelData;
            FakeLoadingTime = fakeLoadingTime;
        }
    }
}