using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2;
using Ji2.States;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class FirstCellMoving : IPayloadedState<(CellView cell, PointerEventData pointerEventData)>, ISwipeController
    {
        public bool IsSwipesAllowed => false;

        private readonly StateMachine _stateMachine;
        private readonly LevelPlayableDecorator _levelPlayableDecorator;
        private readonly CameraProvider _cameraProvider;
        private readonly GridFieldPositionCalculator _gridFieldPositionCalculator;
        private (CellView cell, PointerEventData pointerEventData) _payload;
        private Vector3 _prevPos;

        public FirstCellMoving(StateMachine stateMachine, LevelPlayableDecorator levelPlayableDecorator, CameraProvider cameraProvider,
            GridFieldPositionCalculator gridFieldPositionCalculator)
        {
            _stateMachine = stateMachine;
            _levelPlayableDecorator = levelPlayableDecorator;
            _cameraProvider = cameraProvider;
            _gridFieldPositionCalculator = gridFieldPositionCalculator;
        }

        public UniTask Enter((CellView cell, PointerEventData pointerEventData) payload)
        {
            Assert.AreEqual(1, _levelPlayableDecorator.SelectedTilesCount);

            _payload = payload;
            payload.cell.EventPointerMove += OnCellMove;
            payload.cell.EventPointerUp += OnCellUp;
            
            _prevPos = GetWorldPositionOnPlane(payload.pointerEventData.position);
            return default;
        }

        public UniTask Exit()
        {

            _payload.cell.EventPointerMove -= OnCellMove;
            _payload.cell.EventPointerUp -= OnCellUp;
            return default;
        }

        private void OnCellMove(CellView cell, PointerEventData pointerEventData)
        {
            var movePos = GetWorldPositionOnPlane(pointerEventData.position);

            cell.transform.position += movePos - _prevPos;
            _prevPos = movePos;
        }

        private void OnCellUp(CellView cell, PointerEventData pointerEventData)
        {
            var firstCellPos = _gridFieldPositionCalculator.GetReversePoint(_payload.cell.transform.position);
            _levelPlayableDecorator.ClickTile(firstCellPos);
            _stateMachine.Enter<NoCellsState>().Forget();
        }
        
        private Vector3 GetWorldPositionOnPlane(Vector3 screenPosition)
        {
            Ray ray = _cameraProvider.MainCamera.ScreenPointToRay(screenPosition);
            Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, 0));
            xy.Raycast(ray, out var distance);
            return ray.GetPoint(distance);
        }
    }
}