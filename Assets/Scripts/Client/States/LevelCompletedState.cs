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
        private readonly StateMachine _stateMachine;
        private readonly ScreenNavigator _screenNavigator;
        private readonly LevelsLoopProgress _levelsLoopProgress;
        private readonly LevelsConfig _levelsConfig;
        private readonly Sound _sound;

        public LevelCompletedState(StateMachine stateMachine, ScreenNavigator screenNavigator,
            LevelsLoopProgress levelsLoopProgress, LevelsConfig levelsConfig, Sound sound)
        {
            this._stateMachine = stateMachine;
            this._screenNavigator = screenNavigator;
            this._levelsLoopProgress = levelsLoopProgress;
            this._levelsConfig = levelsConfig;
            this._sound = sound;
        }

        public async UniTask Enter(LevelCompletedPayload payload)
        {
            var screen = await _screenNavigator.PushScreen<LevelCompletedScreen>();
            var levelName = payload.LevelPlayableDecorator.Name;
            var levelViewConfig = _levelsConfig.GetData(levelName);
            var levelResultImage = levelViewConfig.Image;
            screen.SetLevelResult(levelResultImage);

            screen.ClickNext += OnClickNext;
            screen.ClickRetry += OnClickRetry;
        }

        private void OnClickRetry()
        {
            var levelData = _levelsLoopProgress.GetRetryLevelData();
            _stateMachine.Enter<LoadLevelState, LoadLevelStatePayload>(new LoadLevelStatePayload(levelData, 1f));
        }

        private void OnClickNext()
        {
            _sound.PlaySfxAsync(SoundNamesCollection.ButtonTap);
            var levelData = _levelsLoopProgress.GetNextLevelData();
            _stateMachine.Enter<LoadLevelState, LoadLevelStatePayload>(new LoadLevelStatePayload(levelData, 1f));
        }

        public async UniTask Exit()
        {
            await _screenNavigator.CloseScreen<LevelCompletedScreen>();
        }
    }


    public class LevelCompletedPayload
    {
        public LevelPlayableDecorator LevelPlayableDecorator;
    }
}