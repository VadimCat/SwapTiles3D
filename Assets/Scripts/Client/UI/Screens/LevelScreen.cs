using DG.Tweening;
using Ji2Core.UI.Screens;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI.Screens
{
    public class LevelScreen : BaseScreen
    {
        [SerializeField] private TMP_Text levelName;
        [SerializeField] private MarkerProgressBar progressBar;

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

        public void SetTurnsCount(int turnsCount)
        {
            progressBar.AnimateProgressAsync(turnsCount);
            if (turnsCount > okCount)
            {
                AnimateHandleColor(LevelResult.Worst);
            }
            else if(turnsCount > goodCount)
            {
                AnimateHandleColor(LevelResult.Ok);
            }
            else if(turnsCount > perfectCount)
            {
                AnimateHandleColor(LevelResult.Good);
            }
            else
            {
                AnimateHandleColor(LevelResult.Perfect);
            }
        }

        private void AnimateHandleColor(LevelResult result)
        {
            handleImage.DOColor(resultViewConfig.GetColor(result), 1f)
                .SetLink(gameObject);
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

    public static class RectTransformUtils
    {
        public static void SetWidth(this RectTransform transform, float width)
        {
            var size = transform.sizeDelta;
            size.x = width;
            transform.sizeDelta = size;
        }
    }

    public static class TransformUtils
    {
        public static void SetLocalX(this Transform transform, float x)
        {
            var localPos = transform.localPosition;
            localPos.x = x;
            transform.localPosition = localPos;
        }
    }
}