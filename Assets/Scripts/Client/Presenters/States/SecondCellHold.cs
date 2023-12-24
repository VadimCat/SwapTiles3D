using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2Core.Core.States;
using NUnit.Framework;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class SecondCellHold : IPayloadedState<(CellView cell, PointerEventData pointerEventData)>, ISwipeController
    {
        public bool IsSwipesAllowed => false;

        private readonly StateMachine _stateMachine;
        private readonly FieldView _fieldView;
        private readonly Level _level;
        private (CellView cell, PointerEventData pointerEventData) _payload;

        public SecondCellHold(StateMachine stateMachine, FieldView fieldView, Level level)
        {
            _stateMachine = stateMachine;
            _fieldView = fieldView;
            _level = level;
        }

        public UniTask Enter((CellView cell, PointerEventData pointerEventData) payload)
        {
            Assert.AreEqual(1, _level.SelectedTilesCount);
            _payload = payload;
            _payload.cell.EventPointerUp += PointerUp;
            _payload.cell.EventPointerMove += PointerMove;
            return default;
        }

        private void PointerMove(CellView cell, PointerEventData pointerEventData)
        {
            _stateMachine.Enter<FirstCellSelected>().Forget();
        }

        private void PointerUp(CellView cell, PointerEventData pointerEventData)
        {
            _level.ClickTile(_fieldView.CellToPos[cell]);
            _stateMachine.Enter<NoCellsState>().Forget();
        }

        public UniTask Exit()
        {
            _payload.cell.EventPointerUp -= PointerUp;
            _payload.cell.EventPointerMove -= PointerMove;

            return default;
        }
    }
}