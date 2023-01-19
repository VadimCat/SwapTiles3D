using Client.Views.Level;
using UnityEngine;
using Views.Cell;

namespace Client
{
    [CreateAssetMenu(fileName = "LevelsConfig")]
    public class LevelsConfig : LevelsViewDataStorageBase<LevelViewConfig>
    {
        [SerializeField] private CellView levelCellView;

        public CellView CellView => levelCellView;
    }
}