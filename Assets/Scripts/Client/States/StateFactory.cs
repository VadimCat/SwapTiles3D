using System;
using System.Collections.Generic;
using Client.Tutorial;
using Client.Utils;
using Ji2Core.Core;
using Ji2Core.Core.Audio;
using Ji2Core.Core.States;
using UI.Background;

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
            var screenNavigator = context.ScreenNavigator();
            var dict = new Dictionary<Type, IExitableState>();

            dict[typeof(InitialState)] = new InitialState(stateMachine, screenNavigator,
                context.GetService<TutorialService>(),
                context.LevelsLoopProgress(), context.ISaveDataContainer());

            dict[typeof(LoadLevelState)] = new LoadLevelState(context, stateMachine, context.SceneLoader(),
                screenNavigator, context.GetService<LevelsConfig>(), context.GetService<BackgroundService>());

            dict[typeof(GameState)] = new GameState(stateMachine, screenNavigator);

            dict[typeof(LevelCompletedState)] = new LevelCompletedState(stateMachine, screenNavigator,
                context.LevelsLoopProgress(), context.GetService<LevelsConfig>(), context.GetService<AudioService>(),
                context.GetService<LevelResultViewConfig>());

            return dict;
        }
    }
}