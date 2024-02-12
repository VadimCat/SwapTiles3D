using System;
using Client.Presenters;
using Client.UI.Screens;
using Cysharp.Threading.Tasks;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using UI.Background;

namespace Client.States
{
    public class GameState : IPayloadedState<GameStatePayload>
    {
        private readonly StateMachine stateMachine;
        private readonly ScreenNavigator screenNavigator;
        private readonly BackgroundService backgroundService;
        private readonly LevelsLoopProgress levelsLoopProgress;

        private GameStatePayload payload;
        
        public GameState(StateMachine stateMachine, ScreenNavigator screenNavigator)
        {
            this.stateMachine = stateMachine;
            this.screenNavigator = screenNavigator;
        }

        public GameStatePayload Payload => payload;

        public async UniTask Enter(GameStatePayload payload)
        {
            this.payload = payload;
            await screenNavigator.PushScreen<LevelScreen>();

            payload.levelPresenter.StartLevel();
            payload.levelPresenter.LevelCompleted += OnLevelComplete;
        }

        private void OnLevelComplete()
        {
            var levelCompletedPayload = new LevelCompletedPayload()
            {
                LevelPlayableDecorator = payload.levelPresenter.Model
            };
            stateMachine.Enter<LevelCompletedState, LevelCompletedPayload>(levelCompletedPayload);
        }

        public async UniTask Exit()
        {
            payload.levelPresenter.Dispose();
            payload.levelPresenter.LevelCompleted -= OnLevelComplete;
            await screenNavigator.CloseScreen<LevelScreen>();
        }
    }

    public class GameStatePayload
    {
        public LevelPresenter levelPresenter;
    }
}