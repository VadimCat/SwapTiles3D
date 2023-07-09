using Client.Models;
using Client.Presenters;
using Client.Views;
using Core.Compliments;
using Cysharp.Threading.Tasks;
using Ji2.CommonCore;
using Ji2.CommonCore.SaveDataContainer;
using Ji2.Context;
using Ji2.Models.Analytics;
using Ji2Core.Core;
using Ji2Core.Core.Audio;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using Ji2Core.Core.UserInput;
using UI.Background;
using UI.Screens;

namespace Client.States
{
    public class LoadLevelState : IPayloadedState<LoadLevelStatePayload>
    {
        private const string GameSceneName = "LevelScene";

        private readonly StateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly ScreenNavigator _screenNavigator;
        private readonly Context _context;
        private readonly LevelsConfig _levelsConfig;
        private readonly BackgroundService _backgroundService;

        private LoadingScreen _loadingScreen;
        private LevelData _levelData;

        public LoadLevelState(Context context, StateMachine stateMachine, SceneLoader sceneLoader,
            ScreenNavigator screenNavigator, LevelsConfig levelsConfig, BackgroundService backgroundService)
        {
            _context = context;
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _screenNavigator = screenNavigator;
            _levelsConfig = levelsConfig;
            _backgroundService = backgroundService;
        }

        public async UniTask Enter(LoadLevelStatePayload payload)
        {
            _levelData = payload.LevelData;
            var sceneTask = _sceneLoader.LoadScene(GameSceneName);
            _loadingScreen = await _screenNavigator.PushScreen<LoadingScreen>();

            // if (payload.FakeLoadingTime == 0)
            // {
            _sceneLoader.OnProgressUpdate += _loadingScreen.SetProgress;
            await sceneTask;
            _sceneLoader.OnProgressUpdate -= _loadingScreen.SetProgress;
            // }
            // else
            // {
            //     var progressBarTask = _loadingScreen.AnimateLoadingBar(payload.FakeLoadingTime);
            //     await UniTask.WhenAll(sceneTask, progressBarTask);
            // }

            var gamePayload = BuildLevel();

            _stateMachine.Enter<GameState, GameStatePayload>(gamePayload);
        }

        private GameStatePayload BuildLevel()
        {
            var view = _context.GetService<AFieldView>();
            var viewConfig = _levelsConfig.GetData(_levelData.name);
            _backgroundService.SwitchBackground(viewConfig.Background);
            var viewData = viewConfig.ViewData(_levelData.lvlLoop);
            //HACK
            _levelData.difficulty = viewData.Difficulty;
            var levelModel = new Level(_context.GetService<Analytics>(), _levelData, viewData.CutTemplate,
                viewData.DiscreteRotationAngle,
                _context.GetService<ISaveDataContainer>());

            var levelPresenter =
                new LevelPresenter(view, levelModel, _screenNavigator, _context.GetService<UpdateService>(), _levelsConfig,
                    _context.GetService<LevelsLoopProgress>(), _context.GetService<AudioService>(),
                    _context.GetService<ICompliments>(), _context.GetService<InputService>(),
                    _context.GetService<CameraProvider>());

            levelPresenter.BuildLevel();

            return new GameStatePayload
            {
                levelPresenter = levelPresenter
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