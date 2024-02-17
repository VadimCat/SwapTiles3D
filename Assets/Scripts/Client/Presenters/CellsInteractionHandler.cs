using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2;
using Ji2.Presenters;
using Ji2.States;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class CellsInteractionHandler
    {
        private (CellView cell, PointerEventData pointerData)? _downData;
        private readonly StateMachine _stateMachine;

        public bool IsSwipesAllowed => ((ISwipeController)_stateMachine.CurrentState).IsSwipesAllowed;

        public CellsInteractionHandler(GridFieldPositionCalculator gridFieldPositionCalculator, LevelPlayableDecorator levelPlayableDecorator,
            FieldView fieldView, SwipeListener swipeListener, CameraProvider cameraProvider,
            ModelAnimator modelAnimator)
        {
            _stateMachine = new StateMachine(new CellsInteractionStatesFactory(swipeListener, fieldView, levelPlayableDecorator,
                cameraProvider, gridFieldPositionCalculator, modelAnimator));
        }

        public void Initialize()
        {
            _stateMachine.Load();
            _stateMachine.Enter<NoCellsState>().Forget();
        }

        public void Stop()
        {
            _stateMachine.ExitCurrent().Forget();
        }
    }
}