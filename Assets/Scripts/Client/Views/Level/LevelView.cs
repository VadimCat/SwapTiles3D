using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Ji2Core.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Views.Level
{
    public class LevelView : MonoBehaviour
    {
        private const float CELL_SIZE_RATIO = 0.96F;
        private const float CELL_DISTANCE_RATIO = 0.04F;
        
        [SerializeField] private RectTransform cellsRootTransform;
        [SerializeField] private GridLayoutGroup grid;
        
        private readonly Dictionary<Vector2Int, CellView> cellViews = new();

        public Transform GridRoot => grid.transform;
        
        private Context context;
        
        public virtual void Awake()
        {
            context = Context.GetInstance();
            context.Register(this);   
        }

        public virtual void OnDestroy()
        {
            context.Unregister(this);
        }
        
        public void SetGridSizeByData(LevelViewData levelData)
        {
            float cellWidth = cellsRootTransform.rect.width / levelData.cutSize.x;
            float aspectHeight = cellsRootTransform.rect.width / levelData.Aspect;
            
            float cellHeight = aspectHeight / levelData.cutSize.y; 
            grid.constraintCount = levelData.cutSize.x;
            Vector2 cellSize = new Vector2(cellWidth, cellHeight);
            
            grid.cellSize = cellSize * CELL_SIZE_RATIO;
            grid.spacing = cellSize * CELL_DISTANCE_RATIO;
        }

        public async UniTask AnimateWin()
        {
            await UniTask.CompletedTask;
        }
    }
}