using UnityEngine;

namespace Client.Views
{
    [CreateAssetMenu(menuName = "View/CellAnimationConfig", fileName = "CellAnimationConfig")]
    public class CellAnimationConfig : ScriptableObject
    {
        [SerializeField] private float selectTime = .5f;
        [SerializeField] private float moveTime = 1.5f;
        [SerializeField] private float selectScale = 1.1f;

        public float SelectScale => selectScale;
        public float MoveTime => moveTime;
        public float SelectTime => selectTime;
    }
}