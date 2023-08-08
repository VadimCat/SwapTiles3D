using Client.Models;
using Client.Views;
using Cysharp.Threading.Tasks;
using Ji2Core.Core;
using Ji2Core.Core.States;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class FirstCellMoving : IPayloadedState<(CellView cell, PointerEventData pointerEventData)>
    {
        private readonly StateMachine _stateMachine;
        private readonly SwipeListener _swipeListener;
        private readonly FieldView _fieldView;
        private readonly Level _level;
        private readonly CameraProvider _cameraProvider;
        private readonly GridFieldPositionCalculator _gridFieldPositionCalculator;
        private (CellView cell, PointerEventData pointerEventData) _payload;
        private Vector3 _prevPos;

        public FirstCellMoving(StateMachine stateMachine, SwipeListener swipeListener, FieldView fieldView, Level level,
            CameraProvider cameraProvider, GridFieldPositionCalculator gridFieldPositionCalculator)
        {
            _stateMachine = stateMachine;
            _swipeListener = swipeListener;
            _fieldView = fieldView;
            _level = level;
            _cameraProvider = cameraProvider;
            _gridFieldPositionCalculator = gridFieldPositionCalculator;
        }

        public UniTask Enter((CellView cell, PointerEventData pointerEventData) payload)
        {
            _payload = payload;
            payload.cell.EventPointerMove += OnCellMove;
            payload.cell.EventPointerUp += OnCellUp;
            
            _prevPos = GetWorldPositionOnPlane(payload.pointerEventData.position); 
            // Debug.LogError(payload.pointerEventData.position);
            // Debug.LogError(_prevPos);
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
            // Debug.LogError(pointerEventData.position);
            // Debug.LogError(movePos - _prevPos);

            cell.transform.position += movePos - _prevPos;
            _prevPos = movePos;
        }

        private void OnCellUp(CellView cell, PointerEventData pointerEventData)
        {
            var firstCellPos = _gridFieldPositionCalculator.GetReversePoint(_payload.cell.transform.position);
            _level.ClickTile(firstCellPos);
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