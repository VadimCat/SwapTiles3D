using UnityEngine;

namespace Client
{
    [CreateAssetMenu(fileName = "LevelViewConfig")]
    public class LevelViewConfig : ScriptableObject, ILevelView
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite image;
        [SerializeField] private Vector2Int cutSize = new Vector2Int(3, 3);

        public string Id => this.id;
        public Vector2Int CutSize => this.cutSize;
        public Sprite Image => image;

        public LevelViewData ViewData()
        {
            return new LevelViewData()
            {
                CutSize = cutSize,
                Image = image
            };
        }

#if UNITY_EDITOR
        public void SetData(string id, Sprite image, Vector2Int cutSize)
        {
            this.id = id;
            this.image = image;
            this.cutSize = cutSize;
        }
#endif
    }

    public static class SpriteExtensions
    {
        public static float Aspect(this Sprite sprite)
        {
            return sprite.texture.width / sprite.texture.width;
        }
    }
}