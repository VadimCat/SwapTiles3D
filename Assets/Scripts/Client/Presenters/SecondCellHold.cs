using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2.Presenters;
using Ji2Core.Core.States;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class SecondCellHold : IPayloadedState<(CellView cell, PointerEventData pointerEventData)>
    {
        private readonly StateMachine _stateMachine;
        private readonly FieldView _fieldView;
        private readonly Level _level;
        private readonly ModelAnimator _modelAnimator;
        private (CellView cell, PointerEventData pointerEventData) _payload;

        public SecondCellHold(StateMachine stateMachine, FieldView fieldView, Level level, ModelAnimator modelAnimator)
        {
            _stateMachine = stateMachine;
            _fieldView = fieldView;
            _level = level;
            _modelAnimator = modelAnimator;
        }

        public async UniTask Enter((CellView cell, PointerEventData pointerEventData) payload)
        {
            await _modelAnimator.AwaitAllAnimationsEnd();
            _payload = payload;
            _payload.cell.EventPointerUp += PointerUp;
            _payload.cell.EventPointerMove += PointerMove;
        }

        private void PointerMove(CellView cell, PointerEventData pointerEventData)
        {
            _stateMachine.Enter<FirstCellSelected>();
        }

        private void PointerUp(CellView cell, PointerEventData pointerEventData)
        {
            _level.ClickTile(_fieldView.CellToPos[cell]);
            _stateMachine.Enter<NoCellsState>();
        }

        public UniTask Exit()
        {
            _payload.cell.EventPointerUp -= PointerUp;
            _payload.cell.EventPointerMove -= PointerMove;

            return default;
        }
    }
}