using System;
using System.Collections.Generic;
using Client.Models;
using Client.Views;
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
        private readonly PositionProvider _positionProvider;

        public CellsInteractionStatesFactory(SwipeListener swipeListener, FieldView fieldView, Level level, CameraProvider cameraProvider, PositionProvider positionProvider)
        {
            _swipeListener = swipeListener;
            _fieldView = fieldView;
            _level = level;
            _cameraProvider = cameraProvider;
            _positionProvider = positionProvider;
        }

        public Dictionary<Type, IExitableState> GetStates(StateMachine stateMachine)
        {
            var dict = new Dictionary<Type, IExitableState>();

            dict[typeof(NoCellsState)] = new NoCellsState(stateMachine, _swipeListener, _fieldView, _level);
            dict[typeof(FirstCellHold)] = new FirstCellHold(stateMachine, _swipeListener, _fieldView, _level);
            dict[typeof(FirstCellSelected)] = new FirstCellSelected(stateMachine, _swipeListener, _fieldView, _level);
            dict[typeof(SecondCellHold)] = new SecondCellHold(stateMachine, _fieldView, _level);
            dict[typeof(FirstCellMoving)] = new FirstCellMoving(stateMachine, _swipeListener, _fieldView, _level, _cameraProvider, _positionProvider);

            return dict;
        }
    }
}