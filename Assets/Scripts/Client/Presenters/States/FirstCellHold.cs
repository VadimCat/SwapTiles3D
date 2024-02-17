using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2.States;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class FirstCellHold : IPayloadedState<(CellView cell, PointerEventData pointerEventData)>, ISwipeController
    {
        public bool IsSwipesAllowed => false;

        private readonly StateMachine _stateMachine;
        private readonly SwipeListener _swipeListener;
        private readonly LevelPlayableDecorator _levelPlayableDecorator;
        private (CellView cell, PointerEventData pointerEventData) _payload;

        public FirstCellHold(StateMachine stateMachine, SwipeListener swipeListener, LevelPlayableDecorator levelPlayableDecorator)
        {
            _stateMachine = stateMachine;
            _swipeListener = swipeListener;
            _levelPlayableDecorator = levelPlayableDecorator;
        }

        public UniTask Enter((CellView cell, PointerEventData pointerEventData) payload)
        {
            Assert.AreEqual(1, _levelPlayableDecorator.SelectedTilesCount);
            _payload = payload;
            _swipeListener.Disable();
            payload.cell.EventPointerUp += PointerUp;
            payload.cell.EventPointerMove += PointerMove;
            return UniTask.CompletedTask;
        }

        private void PointerUp(CellView cell, PointerEventData pointerEventData)
        {
            _stateMachine.Enter<FirstCellSelected, (CellView cell, PointerEventData pointerEventData)>((cell,
                pointerEventData)).Forget();
        }

        private void PointerMove(CellView cell, PointerEventData pointerEventData)
        {
            _stateMachine.Enter<FirstCellMoving, (CellView cell, PointerEventData pointerEventData)>((cell,
                pointerEventData)).Forget();
        }

        public UniTask Exit()
        {
            _payload.cell.EventPointerUp -= PointerUp;
            _payload.cell.EventPointerMove -= PointerMove;
            return UniTask.CompletedTask;
        }

    }
}