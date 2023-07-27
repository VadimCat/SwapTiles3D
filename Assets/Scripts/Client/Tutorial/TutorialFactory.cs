using System;
using Ji2.Context;
using Ji2.Presenters.Tutorial;
using Ji2.UI;
using Ji2Core.Core;
using Ji2Core.Core.States;

namespace Client.Tutorial
{
    public class TutorialFactory : ITutorialFactory
    {
        private readonly StateMachine _stateMachine;
        private readonly TutorialPointerView _tutorialPointerView;
        private readonly CameraProvider _cameraProvider;

        public TutorialFactory(IDependenciesProvider dp)
        {
            _stateMachine = dp.GetService<StateMachine>();
            _tutorialPointerView = dp.GetService<TutorialPointerView>();
            _cameraProvider = dp.GetService<CameraProvider>();
        }

        public ITutorialStep Create<TStep>() where TStep : ITutorialStep
        {
            if (typeof(TStep) == typeof(InitialTutorialStep))
            {
                return new InitialTutorialStep(_stateMachine, _tutorialPointerView, _cameraProvider);
            }

            throw new NotImplementedException($"No create implementation {typeof(TStep)} ");
        }
    }
}