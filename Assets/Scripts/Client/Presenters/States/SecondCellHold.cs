using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2Core.Core.States;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class SecondCellHold : IPayloadedState<(CellView cell, PointerEventData pointerEventData)>, ISwipeController
    {
        public bool IsSwipesAllowed => false;

        private readonly StateMachine _stateMachine;
        private readonly FieldView _fieldView;
        private readonly LevelPlayableDecorator _levelPlayableDecorator;
        private (CellView cell, PointerEventData pointerEventData) _payload;

        public SecondCellHold(StateMachine stateMachine, FieldView fieldView, LevelPlayableDecorator levelPlayableDecorator)
        {
            _stateMachine = stateMachine;
            _fieldView = fieldView;
            _levelPlayableDecorator = levelPlayableDecorator;
        }

        public UniTask Enter((CellView cell, PointerEventData pointerEventData) payload)
        {
            Assert.AreEqual(1, _levelPlayableDecorator.SelectedTilesCount);
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
            _levelPlayableDecorator.ClickTile(_fieldView.CellToPos[cell]);
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