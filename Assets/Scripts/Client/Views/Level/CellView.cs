using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ji2Core.Core.Pools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Client.Views
{
    public class CellView : MonoBehaviour, IPoolable, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        [SerializeField] private CellAnimationConfig animationConfig;
        [SerializeField] private GameObject frame;
        [SerializeField] private Image image;
        [field: SerializeField] public MeshRenderer Renderer { get; private set; }
        
        private Vector3 _scale;
        private bool _isInteractable = true;
        private GridFieldPositionCalculator _gridFieldPositionCalculator;
        private Vector3 _startPosition;
        public event Action<CellView, PointerEventData> EventPointerDown;
        public event Action<CellView, PointerEventData> EventPointerMove;
        public event Action<CellView, PointerEventData> EventPointerUp;

        private MaterialPropertyBlock _propertyBlock;
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int BaseMapSt = Shader.PropertyToID("_BaseMap_ST");

        private CancellationTokenSource _tokenSource;
        
        public void SetData(Sprite sprite, Vector2Int originPosition, Vector2Int initialPosition, float initialRotation,
            int columns, int rows, Vector3 scale, GridFieldPositionCalculator gridFieldPositionCalculator)
        {
            _gridFieldPositionCalculator = gridFieldPositionCalculator;
            _startPosition = _gridFieldPositionCalculator.GetPoint(initialPosition);
            float w = (float)1 / columns;
            float h = (float)1 / rows;
            float x = w * originPosition.x;
            float y = h * originPosition.y;
            Transform transform1 = transform;
            transform1.localRotation = Quaternion.Euler(0, 0, initialRotation);

            _propertyBlock = new MaterialPropertyBlock();
            _propertyBlock.SetTexture(BaseMap, sprite.texture);
            _propertyBlock.SetVector(BaseMapSt, new Vector4(w, h, x,y));
            
            Renderer.SetPropertyBlock(_propertyBlock);
            _scale = scale;
            transform1.localScale = scale;
        }

        public async UniTask Pulse()
        {
            await transform.DOScale(1.1f, .1f).AwaitForComplete();
        }

        private void EnableInteraction()
        {
            image.raycastTarget = true;
            _isInteractable = true;
        }

        private void DisableInteraction()
        {
            image.raycastTarget = false;
            _isInteractable = false;
        }

        public async UniTask PlaySelectAnimation()
        {
            await DOTween.Sequence()
                .Join(transform.DOMoveZ(-1, animationConfig.SelectTime))
                .AwaitForComplete();

        }

        public async UniTask PlayDeselectAnimation()
        {
            DisableInteraction();

            await DOTween.Sequence()
                .Join(transform.DOMove(_startPosition, animationConfig.SelectTime))
                .AwaitForComplete();

            EnableInteraction();
        }

        public async UniTask PlayMoveAnimation(Vector2Int indexPosition, int upFloor)
        {

            _startPosition = _gridFieldPositionCalculator.GetPoint(indexPosition);
            DisableInteraction();
            var moveSequence = DOTween.Sequence();
            
            moveSequence.Append(transform.DOMoveZ(-1 * upFloor, animationConfig.SelectTime));
            moveSequence.Append(transform.DOMoveX(_startPosition.x, animationConfig.MoveTime));
            moveSequence.Append(transform.DOMoveY(_startPosition.y, animationConfig.MoveTime).SetEase(Ease.OutExpo));
            moveSequence.Join(transform.DOMoveX(_startPosition.x, animationConfig.MoveTime).SetEase(Ease.OutExpo));

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
            frame.gameObject.SetActive(false);

            return UniTask.CompletedTask;
        }

        public void Spawn()
        {
            gameObject.SetActive(true);
        }

        public void DeSpawn()
        {
            EnableInteraction();
            gameObject.SetActive(false);
            frame.gameObject.SetActive(true);

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