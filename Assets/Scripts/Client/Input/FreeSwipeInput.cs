using System;
using Client.Models;
using Ji2.CommonCore;
using UnityEngine;

namespace Client.Input
{
    public class FreeSwipeInput: IUpdatable
    {
        private readonly UpdateService _updateService;
        private readonly TouchScreenInputActions _touch;
        private const float MaxSwipeTime = .3f;
        private const float Threshold = 30;
        private float _startTime;
        private (Vector2 pos, float time)? _startSwipeData;
        
        public event Action<Direction> EventSwipeDirectional;

        public FreeSwipeInput(UpdateService updateService)
        {
            _updateService = updateService;
            _touch = new TouchScreenInputActions();
        }

        public void Enable()
        {
            
            _touch.Enable();
            _updateService.Add(this);
        }

        public void Disable()
        {
            _touch.Enable();
            _updateService.Remove(this);
        }

        public void OnUpdate()
        {
            if (!_startSwipeData.HasValue)
            {
                
            }
            else
            {
                
            }
        }
    }
}
