using Client.Views;
using UnityEngine;
using UnityEngine.Serialization;

namespace Client
{
    [CreateAssetMenu(fileName = "LevelsConfig")]
    public class LevelsConfig : LevelsViewDataStorageBase<LevelViewDataConfig>
    {
        [FormerlySerializedAs("levelCellView")] [SerializeField] private CellView levelACellView;

        public CellView ACellView => levelACellView;
    }
}