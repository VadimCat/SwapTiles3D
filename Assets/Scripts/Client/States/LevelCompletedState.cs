using Client.Models;
using Client.UI.Screens;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2.Audio;
using Ji2.States;
using Ji2Core.Core.ScreenNavigation;

namespace Client.States
{
    public class LevelCompletedState : IPayloadedState<LevelCompletedPayload>
    {
        private readonly StateMachine stateMachine;
        private readonly ScreenNavigator screenNavigator;
        private readonly LevelsLoopProgress levelsLoopProgress;
        private readonly LevelsConfig levelsConfig;
        private readonly Sound _sound;

        public LevelCompletedState(StateMachine stateMachine, ScreenNavigator screenNavigator,
            LevelsLoopProgress levelsLoopProgress, LevelsConfig levelsConfig, Sound sound)
        {
            this.stateMachine = stateMachine;
            this.screenNavigator = screenNavigator;
            this.levelsLoopProgress = levelsLoopProgress;
            this.levelsConfig = levelsConfig;
            this._sound = sound;
        }

        public async UniTask Enter(LevelCompletedPayload payload)
        {
            var screen = await screenNavigator.PushScreen<LevelCompletedScreen>();
            var levelName = payload.LevelPlayableDecorator.Name;
            var levelViewConfig = levelsConfig.GetData(levelName);
            var levelResultImage = levelViewConfig.Image;
            screen.SetLevelResult(levelResultImage);

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
            _sound.PlaySfxAsync(SoundNamesCollection.ButtonTap);
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
        public LevelPlayableDecorator LevelPlayableDecorator;
    }
}