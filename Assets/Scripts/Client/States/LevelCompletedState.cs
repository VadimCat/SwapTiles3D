using Client.Models;
using Client.UI.Screens;
using Cysharp.Threading.Tasks;
using Ji2Core.Core.Audio;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;

namespace Client.States
{
    public class LevelCompletedState : IPayloadedState<LevelCompletedPayload>
    {
        private readonly StateMachine stateMachine;
        private readonly ScreenNavigator screenNavigator;
        private readonly LevelsLoopProgress levelsLoopProgress;
        private readonly LevelsConfig levelsConfig;
        private readonly AudioService audioService;
        private readonly LevelResultViewConfig levelResultViewConfig;

        public LevelCompletedState(StateMachine stateMachine, ScreenNavigator screenNavigator,
            LevelsLoopProgress levelsLoopProgress, LevelsConfig levelsConfig, AudioService audioService,
            LevelResultViewConfig levelResultViewConfig)
        {
            this.stateMachine = stateMachine;
            this.screenNavigator = screenNavigator;
            this.levelsLoopProgress = levelsLoopProgress;
            this.levelsConfig = levelsConfig;
            this.audioService = audioService;
            this.levelResultViewConfig = levelResultViewConfig;
        }

        public async UniTask Enter(LevelCompletedPayload payload)
        {
            var screen = await screenNavigator.PushScreen<LevelCompletedScreen>();
            var levelName = payload.level.Name;
            var levelViewConfig = levelsConfig.GetData(levelName);
            var levelResultImage = levelViewConfig.Image;
            var color = levelResultViewConfig.GetColor(payload.level.Result);
            screen.SetLevelResult(levelResultImage, color);

            screen.ClickNext += OnClickNext;
            screen.ClickRetry += OnClickRetry;
        }

        private void OnClickRetry()
        {
            var levelData = levelsLoopProgress.GetRetryLevelData();
            stateMachine.Enter<LoadLevelState, LoadLevelStatePayload>(new LoadLevelStatePayload(levelData, 1f));
        }

        private void OnClickNext()
        {
            audioService.PlaySfxAsync(SoundNamesCollection.ButtonTap);
            var levelData = levelsLoopProgress.GetNextLevelData();
            stateMachine.Enter<LoadLevelState, LoadLevelStatePayload>(new LoadLevelStatePayload(levelData, 1f));
        }

        public async UniTask Exit()
        {
            await screenNavigator.CloseScreen<LevelCompletedScreen>();
        }
    }


    public class LevelCompletedPayload
    {
        public Level level;
    }
}