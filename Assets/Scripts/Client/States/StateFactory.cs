using System;
using System.Collections.Generic;
using Client.Models;
using Client.Presenters;
using Client.Views;
using Ji2.CommonCore.SaveDataContainer;
using Ji2.Context;
using Ji2.Presenters.Tutorial;
using Ji2Core.Core;
using Ji2Core.Core.Audio;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using UI.Background;

namespace Client.States
{
    public class StateFactory : IStateFactory
    {
        private readonly IDependenciesProvider _dp;

        public StateFactory(IDependenciesProvider dp)
        {
            this._dp = dp;
        }

        public Dictionary<Type, IExitableState> GetStates(StateMachine stateMachine)
        {
            var screenNavigator = _dp.GetService<ScreenNavigator>();
            var dict = new Dictionary<Type, IExitableState>();

            dict[typeof(InitialState)] = new InitialState(stateMachine, screenNavigator,
                _dp.GetService<TutorialService>(),
                _dp.GetService<LevelsLoopProgress>(), _dp.GetService<ISaveDataContainer>());

            dict[typeof(LoadLevelState)] = new LoadLevelState(_dp, stateMachine, _dp.GetService<SceneLoader>(),
                screenNavigator, _dp.GetService<LevelsConfig>(), _dp.GetService<BackgroundService>(),
                _dp.GetService<LevelFactory>(), _dp.GetService<LevelPresenterFactory>());

            dict[typeof(GameState)] = new GameState(stateMachine, screenNavigator);

            dict[typeof(LevelCompletedState)] = new LevelCompletedState(stateMachine, screenNavigator,
                _dp.GetService<LevelsLoopProgress>(), _dp.GetService<LevelsConfig>(), _dp.GetService<Sound>());

            return dict;
        }
    }
}