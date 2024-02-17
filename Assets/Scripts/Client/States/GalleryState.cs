using System;
using Client.UI.Screens;
using Cysharp.Threading.Tasks;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using UnityEngine;
using UnityEngine.UI;

namespace Client.States
{
    public class GalleryState : IState
    {
        private readonly StateMachine _stateMachine;
        private readonly ScreenNavigator _screenNavigator;

        public event Action EventClickNext;
        
        public GalleryState(StateMachine stateMachine, ScreenNavigator screenNavigator)
        {
            _stateMachine = stateMachine;
            _screenNavigator = screenNavigator;
        }
        
        public async UniTask Enter()
        {
            await _screenNavigator.PushScreen<LevelCollectionScreen>();
        }
        
        public async UniTask Exit()
        {
            await _screenNavigator.CloseScreen<LevelCollectionScreen>();
        }

        private void OnDestroy()
        {
            EventClickNext = null;
        }
    }

    public class LevelView: MonoBehaviour
    {
        [SerializeField] private Button startButton;
        
        public event Action EventClicked;

        private void Awake()
        {
            startButton.onClick.AddListener(Click);
        }

        private void Click()
        {
            EventClicked?.Invoke();
        }

        private void OnDestroy()
        {
            startButton.onClick.RemoveListener(Click);
        }
    }

    public class LevelViewModel
    {
        public LevelViewModel()
        {
            
        }
    }
}