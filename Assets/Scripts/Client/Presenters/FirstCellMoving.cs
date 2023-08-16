using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2.Presenters;
using Ji2Core.Core;
using Ji2Core.Core.States;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class FirstCellMoving : IPayloadedState<(CellView cell, PointerEventData pointerEventData)>
    {
        private readonly StateMachine _stateMachine;
        private readonly Level _level;
        private readonly CameraProvider _cameraProvider;
        private readonly GridFieldPositionCalculator _gridFieldPositionCalculator;
        private readonly ModelAnimator _modelAnimator;
        private (CellView cell, PointerEventData pointerEventData) _payload;
        private Vector3 _prevPos;

        public FirstCellMoving(StateMachine stateMachine, Level level, CameraProvider cameraProvider,
            GridFieldPositionCalculator gridFieldPositionCalculator, ModelAnimator modelAnimator)
        {
            _stateMachine = stateMachine;
            _level = level;
            _cameraProvider = cameraProvider;
            _gridFieldPositionCalculator = gridFieldPositionCalculator;
            _modelAnimator = modelAnimator;
        }

        public async UniTask Enter((CellView cell, PointerEventData pointerEventData) payload)
        {
            await _modelAnimator.AwaitAllAnimationsEnd();
            _payload = payload;
            payload.cell.EventPointerMove += OnCellMove;
            payload.cell.EventPointerUp += OnCellUp;
            
            _prevPos = GetWorldPositionOnPlane(payload.pointerEventData.position);
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
            // Debug.LogError(pointerEventData.position);
            // Debug.LogError(movePos - _prevPos);

            cell.transform.position += movePos - _prevPos;
            _prevPos = movePos;
        }

        private void OnCellUp(CellView cell, PointerEventData pointerEventData)
        {
            var firstCellPos = _gridFieldPositionCalculator.GetReversePoint(_payload.cell.transform.position);
            _level.ClickTile(firstCellPos);
            Debug.LogError(_level.SelectedTilesCount);
            _stateMachine.Enter<NoCellsState>();
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