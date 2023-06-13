using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ji2.UI;
using Ji2.Utils;
using Ji2Core.UI.Screens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI.Screens
{
    public class LevelScreen : BaseScreen
    {
        [SerializeField] private TMP_Text levelName;
        [SerializeField] private IProgressBar progressBar;

        [SerializeField] private RectTransform commonProgressArea;
        [SerializeField] private RectTransform worstArea;
        [SerializeField] private RectTransform okArea;
        [SerializeField] private RectTransform goodArea;
        [SerializeField] private RectTransform perfectArea;
        [SerializeField] private Slider slider;
        [SerializeField] private Image handleImage;
        [SerializeField] private LevelResultViewConfig resultViewConfig;
        
        private Sequence animationsSequence;
        
        private int perfectCount;
        private int goodCount;
        private int okCount;

        public void SetLevelName(string name)
        {
            levelName.text = name;
        }

        public async UniTask SetTurnsCount(int turnsCount)
        {
            if (turnsCount > okCount)
            {
                await AnimateHandleColor(LevelResult.Worst);
            }
            else if(turnsCount > goodCount)
            {
                await AnimateHandleColor(LevelResult.Ok);
            }
            else if(turnsCount > perfectCount)
            {
                await AnimateHandleColor(LevelResult.Good);
            }
            else
            {
                await AnimateHandleColor(LevelResult.Perfect);
            }
            await progressBar.AnimateProgressAsync(turnsCount);
        }

        private async UniTask AnimateHandleColor(LevelResult result)
        {
            await handleImage.DOColor(resultViewConfig.GetColor(result), 1f).SetLink(gameObject)
                .AwaitForComplete();
        }

        public void SetUpProgressBar(int okCount, int goodCount, int perfectCount)
        {
            this.perfectCount = perfectCount;
            this.goodCount = goodCount;
            this.okCount = okCount;

            handleImage.color = resultViewConfig.GetColor(LevelResult.Perfect);
            
            int totalCount = okCount + 2;

            float worstAreaPercent = 2 / (float)totalCount;
            float okAreaPercent = (okCount - goodCount) / (float)totalCount;
            float goodAreaPercent = (goodCount - perfectCount) / (float)totalCount;
            float perfectAreaPercent = perfectCount / (float)totalCount;

            worstArea.SetWidth(worstAreaPercent * commonProgressArea.rect.width);
            okArea.SetWidth(okAreaPercent * commonProgressArea.rect.width);
            goodArea.SetWidth(goodAreaPercent * commonProgressArea.rect.width);
            perfectArea.SetWidth(perfectAreaPercent * commonProgressArea.rect.width);

            slider.maxValue = totalCount;
            
            worstArea.transform.SetLocalX(0);
            okArea.transform.SetLocalX(worstArea.sizeDelta.x);
            goodArea.transform.SetLocalX(worstArea.sizeDelta.x + okArea.sizeDelta.x);
            perfectArea.transform.SetLocalX(worstArea.sizeDelta.x + okArea.sizeDelta.x + goodArea.sizeDelta.x);
        }
    }
}