using System.Collections.Generic;
using System.Linq;
using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2.Presenters;
using Ji2Core.Core.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class FirstCellSelected : IPayloadedState<(CellView cell, PointerEventData pointerEventData)>, IState, ISwipeController
    {
        public bool IsSwipesAllowed => true;

        private readonly StateMachine _stateMachine;
        private readonly SwipeListener _swipeListener;
        private readonly FieldView _fieldView;
        private readonly Level _level;
        private readonly ModelAnimator _modelAnimator;
        private (CellView cell, PointerEventData pointerEventData) _payload;
        private IEnumerable<CellView> _cellExceptSelected;

        public FirstCellSelected(StateMachine stateMachine, SwipeListener swipeListener, FieldView fieldView,
            Level level, ModelAnimator modelAnimator)
        {
            _stateMachine = stateMachine;
            _swipeListener = swipeListener;
            _fieldView = fieldView;
            _level = level;
            _modelAnimator = modelAnimator;
        }

        public UniTask Enter((CellView cell, PointerEventData pointerEventData) payload)
        {
            Assert.AreEqual(1, _level.SelectedTilesCount);

            _payload = payload;
            return Enter();
        }

        public async UniTask Enter()
        {
            await _modelAnimator.AwaitAllAnimationsEnd();
            _swipeListener.Enable();
            _payload.cell.EventPointerDown += C1Down;
            _level.TileSet += OnTileSet;
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
            _swipeListener.Disable();

            _level.TileSet -= OnTileSet;
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
            _level.ClickTile(_fieldView.CellToPos[cell]);
            _stateMachine.Enter<NoCellsState>().Forget();
        }
    }
}