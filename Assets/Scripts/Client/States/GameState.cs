using Client.Presenters;
using Client.UI.Screens;
using Cysharp.Threading.Tasks;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;

namespace Client.States
{
    public class GameState : IPayloadedState<GameStatePayload>
    {
        private readonly StateMachine stateMachine;
        private readonly ScreenNavigator screenNavigator;
        private readonly LevelsLoopProgress levelsLoopProgress;

        public GameState(StateMachine stateMachine, ScreenNavigator screenNavigator)
        {
            this.stateMachine = stateMachine;
            this.screenNavigator = screenNavigator;
        }

        public async UniTask Enter(GameStatePayload payload)
        {
            await screenNavigator.PushScreen<LevelScreen>();
            
            payload.levelPresenter.StartLevel();
            payload.levelPresenter.LevelCompleted += OnLevelComplete;
        }

        private void OnLevelComplete()
        {
            stateMachine.Enter<LevelCompletedState, LevelCompletedPayload>(new LevelCompletedPayload());
        }

        public async UniTask Exit()
        {
            await screenNavigator.CloseScreen<LevelScreen>();
        }
    }

    public class LevelCompletedState : IPayloadedState<LevelCompletedPayload>
    {
        private readonly StateMachine stateMachine;
        private readonly ScreenNavigator screenNavigator;

        public LevelCompletedState(StateMachine stateMachine, ScreenNavigator screenNavigator)
        {
            this.stateMachine = stateMachine;
            this.screenNavigator = screenNavigator;
        }
        
        public async UniTask Enter(LevelCompletedPayload payload)
        {
            await screenNavigator.PushScreen<LevelCompletedScreen>();
        }

        public UniTask Exit()
        {
            throw new System.NotImplementedException();
        }
    }

    public class LevelCompletedPayload
    {
    }

    public class GameStatePayload
    {
        public LevelPresenter levelPresenter;
    }
}