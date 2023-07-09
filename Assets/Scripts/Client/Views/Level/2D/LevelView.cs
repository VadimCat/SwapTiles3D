using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ji2.Context;
using Ji2.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Views
{
    public class LevelView : AFieldView
    {
        private const float CellSizeRatio = 0.96F;
        private const float CellDistanceRatio = 0.04F;

        [SerializeField] private RectTransform cellsRootTransform;
        [SerializeField] private GridLayoutGroup grid;
        
        private Context _context;

        public override Transform SpawnRoot => transform;

        public void Awake()
        {
            _context = Context.GetOrCreateInstance();
            _context.Register(this);
        }

        public void OnDestroy()
        {
            _context.Unregister(GetType());
        }

        public override void SetGridSizeByData(int columns, int rows, float imageAspect)
        {
            var rect = cellsRootTransform.rect;
            
            float cellWidth = rect.width / columns;
            float aspectHeight = rect.width / imageAspect;

            float cellHeight = aspectHeight / rows;
            grid.constraintCount = columns;
            Vector2 cellSize = new Vector2(cellWidth, cellHeight);

            grid.cellSize = cellSize * CellSizeRatio;
            grid.spacing = cellSize * CellDistanceRatio;
        }

        public override async UniTask AnimateWin()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(grid.DoSpacing(Vector2.zero, .2f))
                .Append(grid.transform.DOScale(.9f, .2f));
            await sequence.AwaitForComplete();
        }
    }
}