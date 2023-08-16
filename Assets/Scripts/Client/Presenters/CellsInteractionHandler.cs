using Client.Models;
using Client.Views;
using Ji2.Presenters;
using Ji2Core.Core;
using Ji2Core.Core.States;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class CellsInteractionHandler
    {
        private readonly GridFieldPositionCalculator _gridFieldPositionCalculator;
        private readonly Level _level;
        private readonly FieldView _fieldView;
        private readonly SwipeListener _swipeListener;

        private (CellView cell, PointerEventData pointerData)? _downData;
        private readonly StateMachine _stateMachine;

        public CellsInteractionHandler(GridFieldPositionCalculator gridFieldPositionCalculator, Level level,
            FieldView fieldView, SwipeListener swipeListener, CameraProvider cameraProvider,
            ModelAnimator modelAnimator)
        {
            _gridFieldPositionCalculator = gridFieldPositionCalculator;
            _level = level;
            _fieldView = fieldView;
            _swipeListener = swipeListener;

            _stateMachine = new StateMachine(new CellsInteractionStatesFactory(swipeListener, fieldView, level,
                cameraProvider, gridFieldPositionCalculator, modelAnimator));
        }

        public void Initialize()
        {
            _stateMachine.Load();
            _stateMachine.Enter<NoCellsState>();
        }
    }
}