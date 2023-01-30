using System;
using System.Threading;
using Client.Models;
using Client.Presenters;
using Client.States;
using Client.Views.Level;
using Cysharp.Threading.Tasks;
using Ji2Core.Core.States;

namespace Client.Tutorial
{
    public class InitialTutorialStep : ITutorialStep
    {
        private readonly StateMachine stateMachine;
        private readonly TutorialPointer pointer;

        private LevelView levelView;

        public string SaveKey => "InitialTutorialStepCompleted";
        public event Action Completed;

        private CancellationTokenSource cancellationTokenSource;
        private LevelPresenter presenter;
        private Level model;

        public InitialTutorialStep(StateMachine stateMachine, TutorialPointer pointer)
        {
            this.stateMachine = stateMachine;
            this.pointer = pointer;
        }

        public void Run()
        {
            stateMachine.StateEntered += OnStateEnter;
        }

        private void OnStateEnter(IExitableState obj)
        {
            if (obj is GameState state)
            {
                presenter = state.Payload.levelPresenter;
                model = presenter.Model;

                ShowSwapTip();
            }
        }

        private void Cancel()
        {
            cancellationTokenSource.Cancel();
        }

        private async UniTask ShowSwapTip()
        {
            cancellationTokenSource = new CancellationTokenSource();
            
            if (model.TryGetRandomNotSelectedCell(out var pos))
            {
                pointer.ShowTooltip();
                await UniTask.Delay(50);

                var position = presenter.GetTilePos(pos);
                pointer.PlayClickAnimation(position, cancellationTokenSource.Token);
                int currentSelectionCount = model.SelectedTilesCount;
                await UniTask.WaitWhile(() => currentSelectionCount == model.SelectedTilesCount);
                Cancel();

                ShowSwapTip();
            }
            else
            {
                stateMachine.StateEntered -= OnStateEnter;
                pointer.HideTooltip();
                pointer.Hide();
                Completed?.Invoke();
            }
        }
    }
}