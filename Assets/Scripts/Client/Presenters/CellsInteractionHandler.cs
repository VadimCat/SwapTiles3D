using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2.Presenters;
using Ji2Core.Core;
using Ji2Core.Core.States;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class CellsInteractionHandler
    {
        private (CellView cell, PointerEventData pointerData)? _downData;
        private readonly StateMachine _stateMachine;

        public CellsInteractionHandler(GridFieldPositionCalculator gridFieldPositionCalculator, Level level,
            FieldView fieldView, SwipeListener swipeListener, CameraProvider cameraProvider,
            ModelAnimator modelAnimator)
        {
            _stateMachine = new StateMachine(new CellsInteractionStatesFactory(swipeListener, fieldView, level,
                cameraProvider, gridFieldPositionCalculator, modelAnimator));
        }

        public void Initialize()
        {
            _stateMachine.Load();
            _stateMachine.Enter<NoCellsState>().Forget();
        }
    }
}