using System;

namespace Client.Tutorial
{
    public interface ITutorialStep
    {
        public string SaveKey { get; }
        public void Run();
        public event Action Completed;
    }
}