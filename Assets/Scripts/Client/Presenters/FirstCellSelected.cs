using System.Collections.Generic;
using System.Linq;
using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2Core.Core.States;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class FirstCellSelected : IPayloadedState<(CellView cell, PointerEventData pointerEventData)>, IState
    {
        private readonly StateMachine _stateMachine;
        private readonly SwipeListener _swipeListener;
        private readonly FieldView _fieldView;
        private readonly Level _level;
        private (CellView cell, PointerEventData pointerEventData) _payload;
        private IEnumerable<CellView> _cellExceptSelected;

        public FirstCellSelected(StateMachine stateMachine, SwipeListener swipeListener, FieldView fieldView,
            Level level)
        {
            _stateMachine = stateMachine;
            _swipeListener = swipeListener;
            _fieldView = fieldView;
            _level = level;
        }

        public UniTask Enter((CellView cell, PointerEventData pointerEventData) payload)
        {
            _payload = payload;
            return Enter();
        }

        public UniTask Enter()
        {
            _swipeListener.Enable();
            _payload.cell.EventPointerDown += C1Down;
            _cellExceptSelected = _fieldView.CellToPos.Keys.Except(new[] { _payload.cell });
            foreach (var cell in _cellExceptSelected)
            {
                cell.EventPointerDown += C2Down;
            }

            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            _payload.cell.EventPointerDown -= C1Down;
            foreach (var cell in _cellExceptSelected)
            {
                cell.EventPointerDown -= C2Down;
            }
            return UniTask.CompletedTask;
        }

        private void C2Down(CellView cell, PointerEventData pointerEventData)
        {
            _stateMachine.Enter<SecondCellHold, (CellView cell, PointerEventData pointerEventData)>((cell, pointerEventData));
        }

        private void C1Down(CellView cell, PointerEventData pointerEventData)
        {
            _level.ClickTile(_fieldView.CellToPos[cell]);
            _stateMachine.Enter<NoCellsState>();
        }
    }
}