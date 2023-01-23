using Cysharp.Threading.Tasks;
using Facebook.Unity;
using Ji2Core.Core.SaveDataContainer;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using Ji2Core.UI.Screens;
using UnityEngine;

namespace Client.States
{
    public class InitialState : IState
    {
        private readonly StateMachine stateMachine;
        private readonly ScreenNavigator screenNavigator;
        private readonly LevelsLoopProgress levelsLoopProgress;
        private readonly ISaveDataContainer saveDataContainer;


        public InitialState(StateMachine stateMachine, ScreenNavigator screenNavigator,
            LevelsLoopProgress levelsLoopProgress, ISaveDataContainer saveDataContainer)
        {
            this.stateMachine = stateMachine;
            this.screenNavigator = screenNavigator;
            this.levelsLoopProgress = levelsLoopProgress;
            this.saveDataContainer = saveDataContainer;
        }

        public async UniTask Exit()
        {
            await screenNavigator.CloseScreen<LoadingScreen>();
        }

        public async UniTask Enter()
        {
            var facebookTask = LoadFb();
            var dataLoadingTask = saveDataContainer.Load();

            await screenNavigator.PushScreen<LoadingScreen>();
            await UniTask.WhenAll(facebookTask, dataLoadingTask);

            stateMachine.Enter<LoadLevelState, LoadLevelStatePayload>(
                new LoadLevelStatePayload(levelsLoopProgress.GetNextLevelData()));
        }

        private async UniTask LoadFb()
        {
// #if UNITY_EDITOR
//             await UniTask.CompletedTask;
//             Debug.LogWarning("FB IS NOT SETTED");
// #else 

            var taskCompletionSource = new UniTaskCompletionSource<bool>();
            FB.Init(() => OnFbInitComplete(taskCompletionSource));

            await taskCompletionSource.Task;
            if (!FB.IsInitialized)
            {
                FB.ActivateApp();
            }
// #endif
        }

        private void OnFbInitComplete(UniTaskCompletionSource<bool> uniTaskCompletionSource)
        {
            uniTaskCompletionSource.TrySetResult(FB.IsInitialized);
        }
    }
}