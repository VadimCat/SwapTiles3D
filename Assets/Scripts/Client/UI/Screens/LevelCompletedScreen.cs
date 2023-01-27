using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        [SerializeField] private Image animateResult;

        public event Action ClickNext;

        private void Awake()
        {
            AnimateLevelResultImage();

            AnimateNextButton();

            nextButton.onClick.AddListener(FireNext);
        }
        
        private void AnimateLevelResultImage()
        {
            animateResult.transform.DOMoveY(-0.08f, 1.4f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear);
        }

        private void AnimateNextButton()
        {
            nextButton.image.DOFade(1, 1f);
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