using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2.Presenters;
using Ji2Core.Core.States;
using NUnit.Framework;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class FirstCellHold : IPayloadedState<(CellView cell, PointerEventData pointerEventData)>
    {
        private readonly StateMachine _stateMachine;
        private readonly SwipeListener _swipeListener;
        private readonly FieldView _fieldView;
        private readonly Level _level;
        private readonly ModelAnimator _modelAnimator;
        private (CellView cell, PointerEventData pointerEventData) _payload;

        public FirstCellHold(StateMachine stateMachine, SwipeListener swipeListener, FieldView fieldView, Level level,
            ModelAnimator modelAnimator)
        {
            _stateMachine = stateMachine;
            _swipeListener = swipeListener;
            _fieldView = fieldView;
            _level = level;
            _modelAnimator = modelAnimator;
        }

        public UniTask Enter((CellView cell, PointerEventData pointerEventData) payload)
        {
            // await _modelAnimator.AwaitAllAnimationsEnd();
            Assert.AreEqual(1, _level.SelectedTilesCount);
            _payload = payload;
            _swipeListener.Disable();
            payload.cell.EventPointerUp += PointerUp;
            payload.cell.EventPointerMove += PointerMove;
            return default;
        }

        private void PointerUp(CellView cell, PointerEventData pointerEventData)
        {
            _stateMachine.Enter<FirstCellSelected, (CellView cell, PointerEventData pointerEventData)>((cell,
                pointerEventData));
        }

        private void PointerMove(CellView cell, PointerEventData pointerEventData)
        {
            _stateMachine.Enter<FirstCellMoving, (CellView cell, PointerEventData pointerEventData)>((cell,
                pointerEventData));
        }

        public UniTask Exit()
        {
            _payload.cell.EventPointerUp -= PointerUp;
            _payload.cell.EventPointerMove -= PointerMove;
            return default;
        }
    }
}