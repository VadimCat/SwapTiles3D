using System.Collections.Generic;
using System.Threading;
using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2.Presenters;
using Ji2Core.Core.States;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class NoCellsState : IState, ISwipeController
    {
        public bool IsSwipesAllowed => false;

        private readonly StateMachine _stateMachine;
        private readonly SwipeListener _swipeListener;
        private readonly FieldView _fieldView;
        private readonly LevelPlayableDecorator _levelPlayableDecorator;
        private readonly ModelAnimator _modelAnimator;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly List<CellView> _cachedCells = new();
        
        public NoCellsState(StateMachine stateMachine, SwipeListener swipeListener, FieldView fieldView,
            LevelPlayableDecorator levelPlayableDecorator,
            ModelAnimator modelAnimator)
        {
            _stateMachine = stateMachine;
            _swipeListener = swipeListener;
            _fieldView = fieldView;
            _levelPlayableDecorator = levelPlayableDecorator;
            _modelAnimator = modelAnimator;
        }

        public async UniTask Enter()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            await _modelAnimator.AwaitAllAnimationsEnd(_cancellationTokenSource.Token);

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }
            
            Assert.AreEqual(0, _levelPlayableDecorator.SelectedTilesCount);
            
            _swipeListener.Disable();
            foreach (var key in _fieldView.CellToPos.Keys)
            {
                _cachedCells.Add(key);
                key.EventPointerDown += OnCellDown;
            }
        }

        public UniTask Exit()
        {
            _cancellationTokenSource.Cancel();

            foreach (var cell in _cachedCells)
            {
                cell.EventPointerDown -= OnCellDown;
            }
            _cachedCells.Clear();
            return UniTask.CompletedTask;
        }

        private void OnCellDown(CellView cell, PointerEventData pointerEventData)
        {
            Assert.AreEqual(0, _levelPlayableDecorator.SelectedTilesCount);
            _levelPlayableDecorator.ClickTile(_fieldView.CellToPos[cell]);

            _stateMachine.Enter<FirstCellHold, (CellView, PointerEventData)>((cell, pointerEventData)).Forget();
        }
    }
}