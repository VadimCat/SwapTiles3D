using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Views
{
    public class ACellView3D : ACellView
    {
        [SerializeField] private CellAnimationConfig animationConfig;

        [field: SerializeField] public RawImage Image { get; private set; }
        [field: SerializeField] public Button Button { get; private set; }
        [field: SerializeField] public Collider Collider { get; private set; }

        public override event Action<ACellView> EventClicked;

        public override void SetData(Sprite sprite, bool isActive, Vector2Int position, float initialRotation,
            int columns,
            int rows)
        {
            if (isActive)
            {
                Image.texture = sprite.texture;
                float w = (float)1 / columns;
                float h = (float)1 / rows;
                float x = w * position.x;
                float y = h * position.y;
                Image.uvRect = new Rect(x, y, w, h);
                transform.localRotation = Quaternion.Euler(0, 0, initialRotation);
                Button.onClick.AddListener(Click);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void Click()
        {
            EventClicked?.Invoke(this);
        }

        public override async UniTask Pulse()
        {
            await transform.DOScale(1.1f, .1f).AwaitForComplete();
        }

        public override void EnableInteraction()
        {
        }

        public override void DisableInteraction()
        {
        }

        public override async UniTask PlaySelectAnimation()
        {
            DisableInteraction();
            await transform.DOMoveZ(-1, animationConfig.SelectTime);
            EnableInteraction();
        }

        public override async UniTask PlayDeselectAnimation()
        {
            DisableInteraction();

            await transform.DOMoveZ(0, animationConfig.SelectTime);

            EnableInteraction();
        }

        public override async UniTask PlayMoveAnimation(ACellView aCellView)
        {
            DisableInteraction();

            var moveSequence = DOTween.Sequence();

            moveSequence.Append(transform.DOMoveZ(-1.5f, animationConfig.SelectTime));
            var position = aCellView.transform.position;
            moveSequence.Append(transform.DOMoveX(position.x, animationConfig.MoveTime));
            moveSequence.Append(transform.DOMoveY(position.y, animationConfig.MoveTime).SetEase(Ease.OutExpo));
            moveSequence.Join(transform.DOMoveX(position.x, animationConfig.MoveTime).SetEase(Ease.OutExpo));
            moveSequence.Append(transform.DOMoveZ(0, animationConfig.SelectTime));

            await moveSequence.AwaitForComplete();

            EnableInteraction();
        }

        public override async UniTask PlayRotationAnimation(int rotation)
        {
            await transform.DORotate(new Vector3(0, 0, rotation), .5f).AwaitForComplete();
        }

        public override UniTask PlaySetAnimation()
        {
            DisableInteraction();
            return UniTask.CompletedTask;
        }
    }
}