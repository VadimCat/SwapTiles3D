using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Views.Level
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private RawImage image;
        [SerializeField] private Canvas sortingCanvas;
        [SerializeField] private CellAnimationConfig animationConfig;
        [SerializeField] private Transform disableAnimationRoot;

        private Transform root => sortingCanvas.transform;

        public event Action<CellView> Clicked;

        private void Awake()
        {
            button.onClick.AddListener(Click);
        }

        private void Click()
        {
            Clicked?.Invoke(this);
        }

        public void SetData(LevelViewData viewData, Vector2Int position)
        {
            image.texture = viewData.Image.texture;
            float w = (float)1 / viewData.CutSize.x;
            float h = (float)1 / viewData.CutSize.y;
            float x = w * position.x;
            float y = h * position.y;
            image.uvRect = new Rect(x, y, w, h);
        }

        public async UniTask PlaySelectAnimation()
        {
            sortingCanvas.overrideSorting = true;
            sortingCanvas.sortingOrder = 1000;

            button.interactable = false;
            await root.DOScale(animationConfig.SelectScale, animationConfig.SelectTime)
                .AwaitForComplete();
            button.interactable = true;
        }

        public async UniTask PlayDeselectAnimation()
        {
            button.interactable = false;
            await root.DOScale(1, animationConfig.SelectTime).AwaitForComplete();
            button.interactable = true;
            sortingCanvas.overrideSorting = false;
        }

        public async UniTask PlayMoveAnimation(Vector3 pos)
        {
            button.interactable = false;

            await root.DOMove(pos, animationConfig.MoveTime).AwaitForComplete();
            await root.DOScale(1, animationConfig.SelectTime).AwaitForComplete();

            sortingCanvas.overrideSorting = false;
            button.interactable = true;

            root.localPosition = Vector3.zero;
        }

        public async UniTask PlaySettedAnimation()
        {
            button.interactable = false;
            await disableAnimationRoot.transform.DOScale(animationConfig.SelectScale, animationConfig.SelectTime)
                .SetLink(gameObject)
                .AwaitForComplete();
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(Click);
        }
    }
}