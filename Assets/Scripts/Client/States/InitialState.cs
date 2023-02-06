using Cysharp.Threading.Tasks;
using Facebook.Unity;
using Ji2.CommonCore.SaveDataContainer;
using Ji2.Presenters.Tutorial;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using UI.Screens;

namespace Client.States
{
    public class InitialState : IState
    {
        private readonly StateMachine stateMachine;
        private readonly ScreenNavigator screenNavigator;
        private readonly TutorialService tutorialService;
        private readonly LevelsLoopProgress levelsLoopProgress;
        private readonly ISaveDataContainer saveDataContainer;


        public InitialState(StateMachine stateMachine, ScreenNavigator screenNavigator, TutorialService tutorialService,
            LevelsLoopProgress levelsLoopProgress, ISaveDataContainer saveDataContainer)
        {
            this.stateMachine = stateMachine;
            this.screenNavigator = screenNavigator;
            this.tutorialService = tutorialService;
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

            saveDataContainer.Load();
            levelsLoopProgress.Load();
            tutorialService.TryRunSteps();
            
            await screenNavigator.PushScreen<LoadingScreen>();
            await facebookTask;

            float fakeLoadingTime = 5;
#if !UNITY_EDITOR
            fakeLoadingTime = 5;
#endif
            stateMachine.Enter<LoadLevelState, LoadLevelStatePayload>(
                new LoadLevelStatePayload(levelsLoopProgress.GetNextLevelData(), fakeLoadingTime));
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