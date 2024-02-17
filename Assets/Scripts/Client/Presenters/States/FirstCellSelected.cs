using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2.Presenters;
using Ji2.States;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class FirstCellSelected : IPayloadedState<(CellView cell, PointerEventData pointerEventData)>, IState, ISwipeController
    {
        public bool IsSwipesAllowed => true;

        private readonly StateMachine _stateMachine;
        private readonly SwipeListener _swipeListener;
        private readonly FieldView _fieldView;
        private readonly LevelPlayableDecorator _levelPlayableDecorator;
        private readonly ModelAnimator _modelAnimator;
        private (CellView cell, PointerEventData pointerEventData) _payload;
        private IEnumerable<CellView> _cellExceptSelected;
        private CancellationTokenSource _cancellationTokenSource;

        public FirstCellSelected(StateMachine stateMachine, SwipeListener swipeListener, FieldView fieldView,
            LevelPlayableDecorator levelPlayableDecorator, ModelAnimator modelAnimator)
        {
            _stateMachine = stateMachine;
            _swipeListener = swipeListener;
            _fieldView = fieldView;
            _levelPlayableDecorator = levelPlayableDecorator;
            _modelAnimator = modelAnimator;
        }

        public UniTask Enter((CellView cell, PointerEventData pointerEventData) payload)
        {
            Assert.AreEqual(1, _levelPlayableDecorator.SelectedTilesCount);

            _payload = payload;
            return Enter();
        }

        public async UniTask Enter()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            await _modelAnimator.AwaitAllAnimationsEnd(_cancellationTokenSource.Token);
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }
            _swipeListener.Enable();
            _payload.cell.EventPointerDown += C1Down;
            _levelPlayableDecorator.TileSet += OnTileSet;
            _cellExceptSelected = _fieldView.CellToPos.Keys.Except(new[] { _payload.cell });
            foreach (var cell in _cellExceptSelected)
            {
                cell.EventPointerDown += C2Down;
            }
        }

        private void OnTileSet(Vector2Int _)
        {
            _stateMachine.Enter<NoCellsState>().Forget();
        }

        public UniTask Exit()
        {
            _cancellationTokenSource.Cancel();
            _swipeListener.Disable();

            _levelPlayableDecorator.TileSet -= OnTileSet;
            _payload.cell.EventPointerDown -= C1Down;
            foreach (var cell in _cellExceptSelected)
            {
                cell.EventPointerDown -= C2Down;
            }
            return UniTask.CompletedTask;
        }

        private void C2Down(CellView cell, PointerEventData pointerEventData)
        {
            _stateMachine.Enter<SecondCellHold, (CellView cell, PointerEventData pointerEventData)>((cell, pointerEventData)).Forget();
        }

        private void C1Down(CellView cell, PointerEventData pointerEventData)
        {
            _levelPlayableDecorator.ClickTile(_fieldView.CellToPos[cell]);
            _stateMachine.Enter<NoCellsState>().Forget();
        }
    }
}