using System;
using System.Threading;
using Client.Models;
using Client.Presenters;
using Client.States;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2.Presenters.Tutorial;
using Ji2.UI;
using Ji2Core.Core;
using Ji2Core.Core.States;

namespace Client.Tutorial
{
    public class InitialTutorialStep : ITutorialStep
    {
        private readonly StateMachine _stateMachine;
        private readonly TutorialPointerView _pointer;
        private readonly CameraProvider _cameraProvider;

        private FieldView _levelView;

        public string SaveKey => "InitialTutorialStepCompleted";
        public event Action Completed;

        private CancellationTokenSource _cancellationTokenSource;
        private LevelPresenter _presenter;
        private LevelPlayableDecorator _model;

        public InitialTutorialStep(StateMachine stateMachine, TutorialPointerView pointer, CameraProvider cameraProvider)
        {
            _stateMachine = stateMachine;
            _pointer = pointer;
            _cameraProvider = cameraProvider;
        }

        public void Run()
        {
            _stateMachine.StateEntered += OnStateEnter;
        }

        private void OnStateEnter(IExitableState obj)
        {
            if (obj is GameState state)
            {
                _presenter = state.Payload.levelPresenter;
                _model = _presenter.Model;
                _pointer.SetCamera(_cameraProvider.MainCamera);
                
                ShowSwapTip().Forget();
            }
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        private async UniTask ShowSwapTip()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            
            if (_model.TryGetRandomNotSelectedCell(out var pos))
            {
                _pointer.ShowTooltip();
                await UniTask.Delay(50);

                var position = _presenter.GetTilePos(pos);
                _pointer.PlayClickAnimation(position, _cancellationTokenSource.Token).Forget();
                int currentSelectionCount = _model.SelectedTilesCount;
                await UniTask.WaitWhile(() => currentSelectionCount == _model.SelectedTilesCount);
                Cancel();

                ShowSwapTip().Forget();
            }
            else
            {
                _stateMachine.StateEntered -= OnStateEnter;
                _pointer.HideTooltip();
                _pointer.Hide();
                Completed?.Invoke();
            }
        }
    }
}