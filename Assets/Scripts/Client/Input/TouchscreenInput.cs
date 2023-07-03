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
        private const float MaxSwipeTime = .3f;
        private const float Threshold = 30;
        private float _startTime;
        private (Vector2 pos, float time)? _startSwipeData;
        
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
                    _startSwipeData = (_touch.Input.TouchPosition.ReadValue<Vector2>(), Time.time);
                    break;
                case TouchPhase.Ended when _startSwipeData.HasValue:
                {
                    var dir = _touch.Input.TouchPosition.ReadValue<Vector2>() - _startSwipeData.Value.pos;
                    var swipeTIme = Time.time - _startSwipeData.Value.time;
                    if (dir.sqrMagnitude > Threshold * Threshold && swipeTIme <= MaxSwipeTime)
                    {                    
                        _startSwipeData = null;
                        
                        var xAbs = Mathf.Abs(dir.x);
                        var yAbs = Mathf.Abs(dir.y);
                        
                        if (xAbs > yAbs)
                        {
                            EventSwipeDirectional?.Invoke(dir.x > 0 ? Direction.Left : Direction.Right);
                        }
                        else
                        {
                            EventSwipeDirectional?.Invoke(dir.y > 0 ? Direction.Down : Direction.Up);
                        }
                    }
                    else
                    {
                        _startSwipeData = null;
                    }

                    break;
                }
                case TouchPhase.Canceled:
                    _startSwipeData = null;
                    break;
            }
        }
    }
}