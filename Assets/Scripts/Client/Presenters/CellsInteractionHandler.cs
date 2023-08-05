using System;
using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2Core.Core;
using Ji2Core.Core.States;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class CellsInteractionHandler
    {
        private readonly PositionProvider _positionProvider;
        private readonly Level _level;
        private readonly FieldView _fieldView;
        private readonly SwipeListener _swipeListener;

        private (CellView cell, PointerEventData pointerData)? _downData;
        private readonly StateMachine _stateMachine;

        public CellsInteractionHandler(PositionProvider positionProvider, Level level, FieldView fieldView, SwipeListener swipeListener, CameraProvider cameraProvider)
        {
            _positionProvider = positionProvider;
            _level = level;
            _fieldView = fieldView;
            _swipeListener = swipeListener;

            _stateMachine = new StateMachine(new CellsInteractionStatesFactory(swipeListener, fieldView, level, cameraProvider));
        }

        public void Initialize()
        {
            _stateMachine.Load();
            _stateMachine.Enter<NoCellsState>();
        }
    }
}