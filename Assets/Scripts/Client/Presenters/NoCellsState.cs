using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2.Presenters;
using Ji2Core.Core.States;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class NoCellsState : IState
    {
        private readonly StateMachine _stateMachine;
        private readonly SwipeListener _swipeListener;
        private readonly FieldView _fieldView;
        private readonly Level _level;
        private readonly ModelAnimator _modelAnimator;

        public NoCellsState(StateMachine stateMachine, SwipeListener swipeListener, FieldView fieldView, Level level,
            ModelAnimator modelAnimator)
        {
            _stateMachine = stateMachine;
            _swipeListener = swipeListener;
            _fieldView = fieldView;
            _level = level;
            _modelAnimator = modelAnimator;
        }

        public async UniTask Enter()
        {
            await _modelAnimator.AwaitAllAnimationsEnd();
            _swipeListener.Disable();
            foreach (var key in _fieldView.CellToPos.Keys)
            {
                key.EventPointerDown += OnCellDown;
            }
        }

        public UniTask Exit()
        {
            _swipeListener.Disable();
            foreach (var key in _fieldView.CellToPos.Keys)
            {
                key.EventPointerDown -= OnCellDown;
            }

            return UniTask.CompletedTask;
        }

        private void OnCellDown(CellView cell, PointerEventData pointerEventData)
        {
            _level.ClickTile(_fieldView.CellToPos[cell]);
            _stateMachine.Enter<FirstCellHold, (CellView, PointerEventData)>((cell, pointerEventData));
        }
    }
}