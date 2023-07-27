using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ji2Core.Core.Pools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Client.Views
{
    public class CellView : MonoBehaviour, IPoolable, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        [SerializeField] private CellAnimationConfig animationConfig;

        [field: SerializeField] public RawImage Image { get; private set; }

        private float _scale;
        private bool _isInteractable = true;

        public event Action<CellView, PointerEventData> EventPointerDown;
        public event Action<CellView, PointerEventData> EventPointerMove;
        public event Action<CellView, PointerEventData> EventPointerUp;

        public void SetData(Sprite sprite, bool isActive, Vector2Int position, float initialRotation,
            int columns, int rows, float scale)
        {
            if (isActive)
            {
                Image.texture = sprite.texture;
                float w = (float)1 / columns;
                float h = (float)1 / rows;
                float x = w * position.x;
                float y = h * position.y;
                Image.uvRect = new Rect(x, y, w, h);
                Transform transform1 = transform;
                transform1.localRotation = Quaternion.Euler(0, 0, initialRotation);

                _scale = scale;
                transform1.localScale = Vector3.one * scale;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public async UniTask Pulse()
        {
            await transform.DOScale(1.1f, .1f).AwaitForComplete();
        }

        private void EnableInteraction()
        {
            _isInteractable = true;
        }

        private void DisableInteraction()
        {
            _isInteractable = false;
        }

        public async UniTask PlaySelectAnimation()
        {
            DisableInteraction();

            await DOTween.Sequence()
                .Join(transform.DOMoveZ(-1, animationConfig.SelectTime))
                .Join(transform.DOScale(_scale * .85f, animationConfig.SelectTime))
                .AwaitForComplete();

            EnableInteraction();
        }

        public async UniTask PlayDeselectAnimation()
        {
            DisableInteraction();

            await DOTween.Sequence()
                .Join(transform.DOMoveZ(0, animationConfig.SelectTime))
                .Join(transform.DOScale(_scale, animationConfig.SelectTime))
                .AwaitForComplete();

            EnableInteraction();
        }

        public async UniTask PlayMoveAnimation(CellView aCellView)
        {
            DisableInteraction();

            var moveSequence = DOTween.Sequence();

            moveSequence.Append(transform.DOMoveZ(-1.5f, animationConfig.SelectTime));
            var position = aCellView.transform.position;
            moveSequence.Append(transform.DOMoveX(position.x, animationConfig.MoveTime));
            moveSequence.Append(transform.DOMoveY(position.y, animationConfig.MoveTime).SetEase(Ease.OutExpo));
            moveSequence.Join(transform.DOMoveX(position.x, animationConfig.MoveTime).SetEase(Ease.OutExpo));
            // moveSequence.Append(transform.DOMoveZ(0, animationConfig.SelectTime));

            await moveSequence.AwaitForComplete();
            await PlayDeselectAnimation();
            EnableInteraction();
        }

        public async UniTask PlayRotationAnimation(int rotation)
        {
            await transform.DORotate(new Vector3(0, 0, rotation), .5f).AwaitForComplete();
        }

        public UniTask PlaySetAnimation()
        {
            DisableInteraction();
            return UniTask.CompletedTask;
        }

        public void Spawn()
        {
            gameObject.SetActive(true);
        }

        public void DeSpawn()
        {
            gameObject.SetActive(false);

            EventPointerDown = null;
            EventPointerMove = null;
            EventPointerUp = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_isInteractable)
                return;

            EventPointerDown?.Invoke(this, eventData);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!_isInteractable)
                return;

            EventPointerMove?.Invoke(this, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isInteractable)
                return;

            EventPointerUp?.Invoke(this, eventData);
        }
    }
}