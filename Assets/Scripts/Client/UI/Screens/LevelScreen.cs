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

        private Sequence animationsSequence;

        public void SetLevelName(string name)
        {
            levelName.text = name;
        }

        public void SetTurnsCount(int turnsCount)
        {
            progressBar.AnimateProgressAsync(turnsCount);
        }
        
        public void SetUpProgressBar(int okCount, int goodCount, int perfectCount)
        {
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