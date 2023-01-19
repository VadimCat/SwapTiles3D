using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;

namespace Client.States
{
    public class GameCompletedState
    {
        private readonly StateMachine stateMachine;
        private readonly ScreenNavigator screenNavigator;
        private readonly LevelsLoopProgress loopProgress;

        public GameCompletedState(StateMachine stateMachine, ScreenNavigator screenNavigator, LevelsLoopProgress loopProgress)
        {
            this.stateMachine = stateMachine;
            this.screenNavigator = screenNavigator;
            this.loopProgress = loopProgress;
        }
    }
}