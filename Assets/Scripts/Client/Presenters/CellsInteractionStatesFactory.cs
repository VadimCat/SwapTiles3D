using System;
using System.Collections.Generic;
using Client.Models;
using Client.Views;
using Ji2.Presenters;
using Ji2Core.Core;
using Ji2Core.Core.States;

namespace Client.Presenters
{
    public class CellsInteractionStatesFactory : IStateFactory
    {
        private readonly SwipeListener _swipeListener;
        private readonly FieldView _fieldView;
        private readonly Level _level;
        private readonly CameraProvider _cameraProvider;
        private readonly GridFieldPositionCalculator _gridFieldPositionCalculator;
        private readonly ModelAnimator _modelAnimator;

        public CellsInteractionStatesFactory(SwipeListener swipeListener, FieldView fieldView, Level level,
            CameraProvider cameraProvider, GridFieldPositionCalculator gridFieldPositionCalculator,
            ModelAnimator modelAnimator)
        {
            _swipeListener = swipeListener;
            _fieldView = fieldView;
            _level = level;
            _cameraProvider = cameraProvider;
            _gridFieldPositionCalculator = gridFieldPositionCalculator;
            _modelAnimator = modelAnimator;
        }

        public Dictionary<Type, IExitableState> GetStates(StateMachine stateMachine)
        {
            var dict = new Dictionary<Type, IExitableState>();

            dict[typeof(NoCellsState)] = new NoCellsState(stateMachine, _swipeListener, _fieldView, _level, _modelAnimator);
            dict[typeof(FirstCellHold)] = new FirstCellHold(stateMachine, _swipeListener, _fieldView, _level, _modelAnimator);
            dict[typeof(FirstCellSelected)] = new FirstCellSelected(stateMachine, _swipeListener, _fieldView, _level, _modelAnimator);
            dict[typeof(SecondCellHold)] = new SecondCellHold(stateMachine, _fieldView, _level, _modelAnimator);
            dict[typeof(FirstCellMoving)] = new FirstCellMoving(stateMachine, _level, _cameraProvider, _gridFieldPositionCalculator, _modelAnimator);

            return dict;
        }
    }
}