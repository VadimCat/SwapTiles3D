using System;
using System.Collections.Generic;
using Client.Utils;
using Ji2Core.Core;
using Ji2Core.Core.States;

namespace Client.States
{
    public class StateFactory : IStateFactory
    {
        private readonly Context context;

        public StateFactory(Context context)
        {
            this.context = context;
        }

        public Dictionary<Type, IExitableState> GetStates(StateMachine stateMachine)
        {

            var dict = new Dictionary<Type, IExitableState>();

            dict[typeof(InitialState)] = new InitialState(stateMachine, context.ScreenNavigator(),
                context.LevelsLoopProgress(), context.ISaveDataContainer());
            
            dict[typeof(LoadLevelState)] = new LoadLevelState(context, stateMachine, context.SceneLoader(),
                context.ScreenNavigator(), context.GetService<LevelsConfig>());
            
            dict[typeof(GameState)] = new GameState(stateMachine, context.ScreenNavigator());

            return dict;
        }
    }
}