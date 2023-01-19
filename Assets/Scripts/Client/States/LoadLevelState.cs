using Client.Models;
using Client.Views.Level;
using Cysharp.Threading.Tasks;
using Ji2Core.Core;
using Ji2Core.Core.Analytics;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using Ji2Core.UI.Screens;
using UnityEngine;

namespace Client.States
{
    public class LoadLevelState : IPayloadedState<LoadLevelStatePayload>
    {
        private const string GAME_SCENE_NAME = "LevelScene";

        private readonly StateMachine stateMachine;
        private readonly SceneLoader sceneLoader;
        private readonly ScreenNavigator screenNavigator;
        private readonly Context context;
        private readonly LevelsConfig levelsConfig;

        private LoadingScreen loadingScreen;
        private LevelData levelData;

        public LoadLevelState(Context context, StateMachine stateMachine, SceneLoader sceneLoader,
            ScreenNavigator screenNavigator, LevelsConfig levelsConfig)
        {
            this.context = context;
            this.stateMachine = stateMachine;
            this.sceneLoader = sceneLoader;
            this.screenNavigator = screenNavigator;
            this.levelsConfig = levelsConfig;
        }

        public async UniTask Enter(LoadLevelStatePayload payload)
        {
            levelData = payload.LevelData;
            var sceneTask = sceneLoader.LoadScene(GAME_SCENE_NAME);
            loadingScreen = await screenNavigator.PushScreen<LoadingScreen>();

            if (payload.FakeLoadingTime == 0)
            {
                sceneLoader.OnProgressUpdate += loadingScreen.SetProgress;
                await sceneTask;
                sceneLoader.OnProgressUpdate -= loadingScreen.SetProgress;
            }
            else
            {
                var progressBarTask = loadingScreen.AnimateLoadingBar(payload.FakeLoadingTime);
                await UniTask.WhenAll(sceneTask, progressBarTask);
            }

            var gamePayload = BuildLevel();

            stateMachine.Enter<GameState, GameStatePayload>(gamePayload);
        }

        private GameStatePayload BuildLevel()
        {
            var view = context.GetService<LevelView>();
            var viewConfig = levelsConfig.GetData(levelData.name);
            var viewData = viewConfig.ViewData();
            view.SetGridSizeByData(viewData);

            var levelModel = new Level(context.GetService<Analytics>(), levelData, viewData.CutSize);

            foreach (var position in levelModel.CurrentPoses)
            {
                var cellView = Object.Instantiate(levelsConfig.CellView, view.GridRoot);
                cellView.SetData(viewData, position);
            }

            return new GameStatePayload()
            {
                levelView = view,
                level = levelModel
            };
        }

        public async UniTask Exit()
        {
            await screenNavigator.CloseScreen<LoadingScreen>();
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