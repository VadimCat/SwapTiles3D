using System;
using System.Collections.Generic;
using Ji2Core.Core;
using Ji2Core.Core.SaveDataContainer;
using Ji2Core.Core.States;

namespace Client.Tutorial
{
    public class TutorialService
    {
        private readonly ISaveDataContainer saveDataContainer;
        private readonly TutorialFactory factory;

        public List<ITutorialStep> steps = new();
        
        public TutorialService(ISaveDataContainer saveDataContainer, TutorialFactory factory)
        {
            this.saveDataContainer = saveDataContainer;
            this.factory = factory;
            
            steps.Add(factory.Create<InitialTutorialStep>());
        }

        public void TryRunSteps()
        {
            foreach (var step in steps)
            {
                if (!saveDataContainer.GetValue<bool>(step.SaveKey))
                {
                    step.Run();
                    step.Completed += () => saveDataContainer.SaveValue(step.SaveKey, true);
                }
            }
        }
    }

    public class TutorialFactory
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
                return new InitialTutorialStep(context.GetService<StateMachine>(), context.GetService<TutorialPointer>());
            }

            throw new NotImplementedException($"No create implementation {typeof(TStep)} ");
        }
    }
}