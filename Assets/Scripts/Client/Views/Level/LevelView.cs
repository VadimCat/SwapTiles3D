using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ji2.Utils;
using Ji2Core.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Views.Level
{
    public class LevelView : MonoBehaviour
    {
        private const float CellSizeRatio = 0.96F;
        private const float CellDistanceRatio = 0.04F;

        [SerializeField] private RectTransform cellsRootTransform;
        [SerializeField] private GridLayoutGroup grid;

        public Transform GridRoot => grid.transform;

        private Context _context;

        public void Awake()
        {
            _context = Context.GetInstance();
            _context.Register(this);
        }

        public void OnDestroy()
        {
            _context.Unregister(this);
        }

        public void SetGridSizeByData(LevelViewData levelData)
        {
            var rect = cellsRootTransform.rect;
            float cellWidth = rect.width / levelData.CutTemplate.GetLength(0);
            float aspectHeight = rect.width / levelData.Aspect;

            float cellHeight = aspectHeight / levelData.CutTemplate.GetLength(1);
            grid.constraintCount = levelData.CutTemplate.GetLength(0);
            Vector2 cellSize = new Vector2(cellWidth, cellHeight);

            grid.cellSize = cellSize * CellSizeRatio;
            grid.spacing = cellSize * CellDistanceRatio;
        }

        public async UniTask AnimateWin()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(grid.DoSpacing(Vector2.zero, .2f))
                .Append(grid.transform.DOScale(.9f, .2f));
            await sequence.AwaitForComplete();
        }
    }
}