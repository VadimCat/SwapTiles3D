using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Views
{
    public class ACellView2D : ACellView
    {
        [SerializeField] private Button button;
        [SerializeField] private RawImage image;
        [SerializeField] private Canvas sortingCanvas;
        [SerializeField] private CellAnimationConfig animationConfig;
        [SerializeField] private Image maskImage;
        [SerializeField] private Image rootImage;

        private Transform Root => sortingCanvas.transform;

        private RawImage MainImage => image;
        
        public override event Action<ACellView> EventClicked;

        public override void EnableInteraction()
        {
            sortingCanvas.overrideSorting = true;
        }

        public override void DisableInteraction()
        {
            button.interactable = false;
        }

        public override async UniTask Pulse()
        {
            await transform.DOScale(1.1f, .1f).AwaitForComplete();
        }

        private void Awake()
        {
            button.onClick.AddListener(Click);
        }

        private void Click()
        {
            EventClicked ?.Invoke(this);
        }
        
        public override void SetData(Sprite sprite, bool isActive, Vector2Int position, float initialRotation, int columns,
            int rows)
        {
            gameObject.name += isActive;
            if (isActive)
            {
                image.texture = sprite.texture;
                float w = (float)1 / columns;
                float h = (float)1 / rows;
                float x = w * position.x;
                float y = h * position.y;
                image.uvRect = new Rect(x, y, w, h);
                maskImage.transform.localRotation = Quaternion.Euler(0, 0, initialRotation);
            }
            else
            {
                rootImage.color = Color.clear;
                MainImage.color = Color.clear;
                button.interactable = false;
            }
        }

        public override async UniTask PlaySelectAnimation()
        {
            sortingCanvas.overrideSorting = true;
            sortingCanvas.sortingOrder = 1000;

            button.interactable = false;
            await Root.DOScale(animationConfig.SelectScale, animationConfig.SelectTime)
                .AwaitForComplete();
            button.interactable = true;
        }

        public override async UniTask PlayDeselectAnimation()
        {
            button.interactable = false;
            await Root.DOScale(1, animationConfig.SelectTime).AwaitForComplete();
            button.interactable = true;
            sortingCanvas.overrideSorting = false;
        }

        public override async UniTask PlayMoveAnimation(ACellView aCellView)
        {
            button.interactable = false;
            var rectToSet = MainImage.uvRect;
            var rotationToSet = maskImage.transform.localRotation;

            await Root.DOMove(aCellView.transform.position, animationConfig.MoveTime).AwaitForComplete();
            await Root.DOScale(1, animationConfig.SelectTime).AwaitForComplete();

            Root.transform.localPosition = Vector3.zero;
            sortingCanvas.overrideSorting = false;
            button.interactable = true;
            Root.localPosition = Vector3.zero;

            // cellView.MainImage.uvRect = rectToSet;
            // cellView.maskImage.transform.localRotation = rotationToSet;
        }

        public override async UniTask PlayRotationAnimation(int rotation)
        {
            sortingCanvas.overrideSorting = true;
            sortingCanvas.sortingOrder = 1000;
            var rotationEndValue = new Vector3(0, 0, rotation);
            await maskImage.transform.DOLocalRotate(rotationEndValue, .5f).AwaitForComplete();
            sortingCanvas.overrideSorting = false;
        }

        public override UniTask PlaySetAnimation()
        {
            button.interactable = false;
            maskImage.sprite = rootImage.sprite;
            return UniTask.CompletedTask;
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(Click);
        }
    }
}