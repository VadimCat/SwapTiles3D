using System;
using Client.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Client.Input
{
    public class TouchscreenInput
    {
        private readonly TouchScreenInputActions _touch;

        private float _threshold = 5; 
        private Vector2? _startPos;
        
        public TouchscreenInput()
        {
            _touch = new TouchScreenInputActions();
            _touch.Enable();
            _touch.Input.TouchPhase.performed += CheckTouchPhase;
        }

        public event Action<Direction> EventSwipeDirectional;
        
        private void CheckTouchPhase(InputAction.CallbackContext obj)
        {
            var phase = obj.ReadValue<TouchPhase>();
            
            switch (phase)
            {
                case TouchPhase.Began:
                    _startPos = _touch.Input.TouchPosition.ReadValue<Vector2>();
                    break;
                case TouchPhase.Ended when _startPos.HasValue:
                {
                    var dir = _touch.Input.TouchPosition.ReadValue<Vector2>() - _startPos.Value;
                    if (dir.sqrMagnitude > _threshold * _threshold)
                    {                    
                        _startPos = null;
                        
                        var xAbs = Mathf.Abs(dir.x);
                        var yAbs = Mathf.Abs(dir.y);
                        
                        if (xAbs > yAbs)
                        {
                            EventSwipeDirectional?.Invoke(dir.x > 0 ? Direction.Left : Direction.Right);
                        }
                        else
                        {
                            EventSwipeDirectional?.Invoke(dir.y > 0 ? Direction.Down : Direction.Right);
                        }
                    }

                    break;
                }
                case TouchPhase.Canceled:
                    _startPos = null;
                    break;
            }
        }
    }
}
