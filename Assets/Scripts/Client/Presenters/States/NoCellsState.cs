using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2.Presenters;
using Ji2Core.Core.States;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using Assert = NUnit.Framework.Assert;

namespace Client.Presenters
{
    public class NoCellsState : IState, ISwipeController
    {
        public bool IsSwipesAllowed => false;

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

            Assert.AreEqual(_level.SelectedTilesCount, 0);

            _swipeListener.Disable();
            foreach (var key in _fieldView.CellToPos.Keys)
            {
                key.EventPointerDown += OnCellDown;
            }
        }

        public UniTask Exit()
        {
            foreach (var key in _fieldView.CellToPos.Keys)
            {
                key.EventPointerDown -= OnCellDown;
            }

            return UniTask.CompletedTask;
        }

        private void OnCellDown(CellView cell, PointerEventData pointerEventData)
        {
            Assert.AreEqual(0, _level.SelectedTilesCount);
            _level.ClickTile(_fieldView.CellToPos[cell]);
 
            Assert.AreEqual(1, _level.SelectedTilesCount);
            _stateMachine.Enter<FirstCellHold, (CellView, PointerEventData)>((cell, pointerEventData)).Forget();
        }
    }
}