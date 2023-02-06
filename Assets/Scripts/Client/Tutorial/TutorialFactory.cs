using System;
using Ji2.Presenters.Tutorial;
using Ji2.UI;
using Ji2Core.Core;
using Ji2Core.Core.States;

namespace Client.Tutorial
{
    public class TutorialFactory : ITutorialFactory
    {
        private readonly Context context;

        public TutorialFactory(Context context)
        {
            this.context = context;
        }
        
        public ITutorialStep Create<TStep>() where TStep : ITutorialStep
        {
            if (typeof(TStep) == typeof(InitialTutorialStep))
            {
                return new InitialTutorialStep(context.GetService<StateMachine>(), context.GetService<TutorialPointerView>());
            }

            throw new NotImplementedException($"No create implementation {typeof(TStep)} ");
        }
    }
}