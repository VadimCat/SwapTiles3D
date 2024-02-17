using Client.Presenters;
using Client.UI.Screens;
using Cysharp.Threading.Tasks;
using Ji2.States;
using Ji2Core.Core.ScreenNavigation;
using UI.Background;

namespace Client.States
{
    public class GameState : IPayloadedState<GameStatePayload>
    {
        private readonly StateMachine _stateMachine;
        private readonly ScreenNavigator _screenNavigator;
        private readonly BackgroundService _backgroundService;
        private readonly LevelsLoopProgress _levelsLoopProgress;

        private GameStatePayload _payload;
        
        public GameState(StateMachine stateMachine, ScreenNavigator screenNavigator)
        {
            this._stateMachine = stateMachine;
            this._screenNavigator = screenNavigator;
        }

        public GameStatePayload Payload => _payload;

        public async UniTask Enter(GameStatePayload payload)
        {
            this._payload = payload;
            await _screenNavigator.PushScreen<LevelScreen>();

            payload.LevelPresenter.StartLevel();
            payload.LevelPresenter.LevelCompleted += OnLevelComplete;
        }

        private void OnLevelComplete()
        {
            var levelCompletedPayload = new LevelCompletedPayload()
            {
                LevelPlayableDecorator = _payload.LevelPresenter.Model
            };
            _stateMachine.Enter<LevelCompletedState, LevelCompletedPayload>(levelCompletedPayload);
        }

        public async UniTask Exit()
        {
            _payload.LevelPresenter.Dispose();
            _payload.LevelPresenter.LevelCompleted -= OnLevelComplete;
            await _screenNavigator.CloseScreen<LevelScreen>();
        }
    }

    public class GameStatePayload
    {
        public LevelPresenter LevelPresenter;
    }
}