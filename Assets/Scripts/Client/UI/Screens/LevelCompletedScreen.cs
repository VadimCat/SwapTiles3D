using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ji2Core.Scripts.Utils;
using Ji2Core.UI.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI.Screens
{
    public class LevelCompletedScreen : BaseScreen
    {
        private const string levelNamePattern = "LEVEL {0}";
        
        [SerializeField] private Button nextButton;
        [SerializeField] private Image levelResult;

        public event Action ClickNext;

        private void Awake()
        {
            // AnimateLevelResultImage();

            nextButton.onClick.AddListener(FireNext);
        }
        
        private void AnimateLevelResultImage()
        {
            levelResult.transform.DoPulseScale(1.06f, 1, gameObject);
            levelResult.transform.DORotate(Vector3.forward * 2.2f, 1)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.Linear)
                .SetLink(gameObject);
        }

        public void SetLevelResult(Sprite levelResult, int levelNumber)
        {
            this.levelResult.sprite = levelResult;
        }

        private async void FireNext()
        {
            await nextButton.transform.DOScale(0.9f, 0.1f).AwaitForComplete();
            Complete();
        }

        private void Complete()
        {
            ClickNext?.Invoke();
        }

        private void OnDestroy()
        {
            ClickNext = null;
        }
    }
}