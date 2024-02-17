using Cysharp.Threading.Tasks;
using Facebook.Unity;
using Ji2.CommonCore.SaveDataContainer;
using Ji2.Presenters.Tutorial;
using Ji2.States;
using Ji2.UI.Screens;
using Ji2Core.Core.ScreenNavigation;
using UnityEngine;

namespace Client.States
{
    public class InitialState : IState
    {
        private readonly StateMachine _stateMachine;
        private readonly ScreenNavigator _screenNavigator;
        private readonly TutorialService _tutorialService;
        private readonly LevelsLoopProgress _levelsLoopProgress;
        private readonly ISaveDataContainer _saveDataContainer;


        public InitialState(StateMachine stateMachine, ScreenNavigator screenNavigator, TutorialService tutorialService,
            LevelsLoopProgress levelsLoopProgress, ISaveDataContainer saveDataContainer)
        {
            this._stateMachine = stateMachine;
            this._screenNavigator = screenNavigator;
            this._tutorialService = tutorialService;
            this._levelsLoopProgress = levelsLoopProgress;
            this._saveDataContainer = saveDataContainer;
        }

        public async UniTask Enter()
        {
            var facebookTask = LoadFb();

            _saveDataContainer.Load();
            _levelsLoopProgress.Load();
            _tutorialService.TryRunSteps();
            
            await UniTask.WhenAll(facebookTask, _screenNavigator.PushScreen<LoadingScreen>()) ;

            float fakeLoadingTime = 0;
#if !UNITY_EDITOR
            fakeLoadingTime = 5;
#endif
            _stateMachine.Enter<LoadLevelState, LoadLevelStatePayload>(
                new LoadLevelStatePayload(_levelsLoopProgress.GetNextLevelData(), fakeLoadingTime));
        }

        public async UniTask Exit()
        {
            await _screenNavigator.CloseScreen<LoadingScreen>();
        }

        private async UniTask LoadFb()
        {
#if UNITY_EDITOR
            await UniTask.CompletedTask;
            Debug.LogWarning("FB IS NOT SETTED");
#else 

            var taskCompletionSource = new UniTaskCompletionSource<bool>();
            FB.Init(() => OnFbInitComplete(taskCompletionSource));

            await taskCompletionSource.Task;
            if (!FB.IsInitialized)
            {
                FB.ActivateApp();
            }
#endif
        }

        private void OnFbInitComplete(UniTaskCompletionSource<bool> uniTaskCompletionSource)
        {
            uniTaskCompletionSource.TrySetResult(FB.IsInitialized);
        }
    }
}