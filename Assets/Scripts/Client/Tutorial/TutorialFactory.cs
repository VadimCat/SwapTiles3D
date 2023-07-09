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
        private readonly Context _context;

        public TutorialFactory(Context context)
        {
            _context = context;
        }

        public ITutorialStep Create<TStep>() where TStep : ITutorialStep
        {
            if (typeof(TStep) == typeof(InitialTutorialStep))
            {
                return new InitialTutorialStep(_context.GetService<StateMachine>(),
                    _context.GetService<TutorialPointerView>(), _context.GetService<CameraProvider>());
            }

            throw new NotImplementedException($"No create implementation {typeof(TStep)} ");
        }
    }
}