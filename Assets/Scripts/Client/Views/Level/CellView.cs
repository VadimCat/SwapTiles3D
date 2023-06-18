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
        [SerializeField] private Image maskImage;
        [SerializeField] private Image rootImage;

        private Transform Root => sortingCanvas.transform;

        private RawImage MainImage => image;

        public event Action<CellView> Clicked;

        private void Awake()
        {
            button.onClick.AddListener(Click);
        }

        private void Click()
        {
            Clicked?.Invoke(this);
        }

        public void SetData(LevelViewData viewData, Vector2Int position, float initialRotation)
        {
            if (viewData.CutTemplate[position.x, position.y])
            {
                rootImage.color = Color.clear;
                MainImage.color = Color.clear;
                button.interactable = false;
            }
            else
            {
                image.texture = viewData.Image.texture;
                float w = (float)1 / viewData.CutTemplate.GetLength(0);
                float h = (float)1 / viewData.CutTemplate.GetLength(1);
                float x = w * position.x;
                float y = h * position.y;
                image.uvRect = new Rect(x, y, w, h);
                image.transform.localRotation = Quaternion.Euler(0, 0, initialRotation);
            }
        }

        public async UniTask PlaySelectAnimation()
        {
            sortingCanvas.overrideSorting = true;
            sortingCanvas.sortingOrder = 1000;

            button.interactable = false;
            await Root.DOScale(animationConfig.SelectScale, animationConfig.SelectTime)
                .AwaitForComplete();
            button.interactable = true;
        }

        public async UniTask PlayDeselectAnimation()
        {
            button.interactable = false;
            await Root.DOScale(1, animationConfig.SelectTime).AwaitForComplete();
            button.interactable = true;
            sortingCanvas.overrideSorting = false;
        }

        public async UniTask PlayMoveAnimation(CellView cellView)
        {
            button.interactable = false;
            var rectToSet = MainImage.uvRect;
            var rotationToSet = maskImage.transform.localRotation;
            
            await Root.DOMove(cellView.transform.position, animationConfig.MoveTime).AwaitForComplete();
            await Root.DOScale(1, animationConfig.SelectTime).AwaitForComplete();
            
            Root.transform.localPosition = Vector3.zero;
            sortingCanvas.overrideSorting = false;
            button.interactable = true;
            Root.localPosition = Vector3.zero;
            
            cellView.MainImage.uvRect = rectToSet;
            cellView.maskImage.transform.localRotation = rotationToSet;
        }

        public async UniTask PlayRotationAnimation(float rotation)
        {
            sortingCanvas.overrideSorting = true;
            sortingCanvas.sortingOrder = 1000;
            var rotationEndValue = new Vector3(0, 0, rotation);
            await maskImage.transform.DOLocalRotate(rotationEndValue, .5f).AwaitForComplete();
            
            sortingCanvas.overrideSorting = false;
        }
        
        public UniTask PlaySetAnimation()
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