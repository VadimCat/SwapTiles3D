using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ji2Core.Core.Pools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Client.Views
{
    public class CellView : MonoBehaviour, IPoolable, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler,
        IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private CellAnimationConfig animationConfig;
        [SerializeField] private Canvas canvas;

        [field: SerializeField] public RawImage Image { get; private set; }

        private Vector3 _scale;
        private bool _isInteractable = true;
        private PositionProvider _positionProvider;
        private Vector3 _startPosition;
        public event Action<CellView, PointerEventData> EventPointerDown;
        public event Action<CellView, PointerEventData> EventPointerMove;
        public event Action<CellView, PointerEventData> EventPointerUp;
        public event Action<CellView, PointerEventData> EventPointerExit;
        public event Action<CellView, PointerEventData> EventPointerClick;

        public void SetData(Sprite sprite, Vector2Int position, float initialRotation,
            int columns, int rows, Vector3 scale, PositionProvider positionProvider)
        {
            _positionProvider = positionProvider;
            _startPosition = _positionProvider.GetPoint(position);
            Image.texture = sprite.texture;
            float w = (float)1 / columns;
            float h = (float)1 / rows;
            float x = w * position.x;
            float y = h * position.y;
            Image.uvRect = new Rect(x, y, w, h);
            Transform transform1 = transform;
            transform1.localRotation = Quaternion.Euler(0, 0, initialRotation);

            _scale = scale;
            transform1.localScale = scale;
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
            canvas.sortingOrder = 100;
            // DisableInteraction();

            await DOTween.Sequence()
                .Join(transform.DOMoveZ(-1, animationConfig.SelectTime))
                .Join(transform.DOScale(_scale * .85f, animationConfig.SelectTime))
                .AwaitForComplete();

            // EnableInteraction();
        }

        public async UniTask PlayDeselectAnimation()
        {
            canvas.sortingOrder = 0;
            // DisableInteraction();

            await DOTween.Sequence()
                .Join(transform.DOScale(_scale, animationConfig.SelectTime))
                .Join(transform.DOMove(_startPosition, animationConfig.SelectTime))
                .AwaitForComplete();

            EnableInteraction();
        }

        public async UniTask PlayMoveAnimation(Vector2Int indexPosition)
        {
            _startPosition = _positionProvider.GetPoint(indexPosition);
            // DisableInteraction();
            var moveSequence = DOTween.Sequence();
            
            moveSequence.Append(transform.DOMoveZ(-1.5f, animationConfig.SelectTime));
            // var position = aCellView.transform.position;
            moveSequence.Append(transform.DOMoveX(_startPosition.x, animationConfig.MoveTime));
            moveSequence.Append(transform.DOMoveY(_startPosition.y, animationConfig.MoveTime).SetEase(Ease.OutExpo));
            moveSequence.Join(transform.DOMoveX(_startPosition.x, animationConfig.MoveTime).SetEase(Ease.OutExpo));
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
            Debug.LogError(gameObject.name);
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

            // Debug.LogError("down");
            EventPointerDown?.Invoke(this, eventData);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!_isInteractable)
                return;

            // Debug.LogError("move");
            EventPointerMove?.Invoke(this, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isInteractable)
                return;

            // Debug.LogError("up");
            EventPointerUp?.Invoke(this, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isInteractable)
                return;

            // Debug.LogError("exit");
            EventPointerExit?.Invoke(this, eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isInteractable)
                return;

            // Debug.LogError("click");
            EventPointerClick?.Invoke(this, eventData);
        }
    }
}