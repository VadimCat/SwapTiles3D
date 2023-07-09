using UnityEngine;
using UnityEngine.Serialization;

namespace Client.Views
{
    [CreateAssetMenu(fileName = "LevelsConfig")]
    public class LevelsConfig : LevelsViewDataStorageBase<LevelViewDataConfig>
    {
        [FormerlySerializedAs("levelCellView")] [SerializeField] private ACellView levelACellView;

        public ACellView ACellView => levelACellView;
    }
}