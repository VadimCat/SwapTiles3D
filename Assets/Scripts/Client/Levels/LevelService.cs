using System;
using System.Threading.Tasks;
using Client.UI.Screens;
using Cysharp.Threading.Tasks;
using Ji2Core.Core;
using Ji2Core.Core.Analytics;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Models;
using UI.Background;
using UnityEngine;

namespace Client
{
    public class LevelService : ISavable, IUpdatable
    {
        public event Action<float> OnProgressUpdate;

        private const string SAVE_KEY = "lvl";

        private int currentLvlInd = -1;

        // private readonly LevelsViewDataStorageBase<> levelsViewDataStorageBase;
        // private readonly LevelViewContainer levelView;
        private readonly ScreenNavigator screenNavigator;
        private readonly UpdateService updateService;
        private readonly SceneLoader sceneLoader;
        private readonly Analytics analytics;
        private readonly BackgroundService backgroundService;
        private readonly Context context;

        private AsyncOperation currentLoadingTask;

        //TODO: Check dependencies and use context where needed
        public LevelService(
            // LevelsViewDataStorageBase<> levelsViewDataStorageBase, 
            // LevelViewContainer levelView,
            ScreenNavigator screenNavigator, UpdateService updateService, BackgroundService backgroundService,
            Context context, SceneLoader sceneLoader, Analytics analytics)
        {
            // this.levelsViewDataStorageBase = levelsViewDataStorageBase;
            // this.levelView = levelView;
            this.screenNavigator = screenNavigator;
            this.updateService = updateService;
            this.sceneLoader = sceneLoader;
            this.analytics = analytics;
            this.backgroundService = backgroundService;
            this.context = context;

            Load();
        }

        private async Task LoadLevelScene()
        {
            backgroundService.SwitchBackground(BackgroundService.Background.Game);
            var sceneLoadingTask = sceneLoader.LoadScene("LevelScene");
            await UniTask.WhenAll(sceneLoadingTask, UniTask.Delay(2000));
        }

        public void OnUpdate()
        {
            OnProgressUpdate?.Invoke(currentLoadingTask.progress);
        }

        public void Save()
        {
            PlayerPrefs.SetInt(SAVE_KEY, currentLvlInd);
        }

        public void Load()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
                currentLvlInd = PlayerPrefs.GetInt(SAVE_KEY);
        }

        public void ClearSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
        }
    }
}