using Client.Models;
using Client.UI.Screens;
using Client.Views.Level;
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

        public GameState(StateMachine stateMachine, ScreenNavigator screenNavigator,
            LevelsLoopProgress levelsLoopProgress)
        {
            this.stateMachine = stateMachine;
            this.screenNavigator = screenNavigator;
            this.levelsLoopProgress = levelsLoopProgress;
        }

        public async UniTask Enter(GameStatePayload payload)
        {
            var screen = await screenNavigator.PushScreen<LevelScreen>();
            screen.ClickNext += OnClickNext; 
            screen.SetLevelName(payload.level.name);
            screen.ShowNextButton();
        }

        private void OnClickNext()
        {
            levelsLoopProgress.IncLevel();
            var payload = new LoadLevelStatePayload(levelsLoopProgress.GetNextLevelData(), .2f);
            stateMachine.Enter<LoadLevelState, LoadLevelStatePayload>(payload);
        }

        public async UniTask Exit()
        {
            await screenNavigator.CloseScreen<LevelScreen>();
        }
    }

    public class GameStatePayload
    {
        public LevelView levelView;
        public Level level;
    }
}