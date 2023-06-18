using Ji2.Models;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Client
{
    [CreateAssetMenu(fileName = "LevelViewConfig")]
    public class LevelViewConfig : SerializedScriptableObject, ILevelView
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite image;
        [SerializeField] private Vector2Int[] cutSize = { new(3, 3) };
        [SerializeField] private Sprite background;
        [field: SerializeField] private bool[][,] LevelSizes { get; set; }

        public Sprite Background => this.background;
        public string Id => this.id;
        public Sprite Image => image;

        public LevelViewData ViewData(int loop)
        {
            var difficulty = (Difficulty)Mathf.Clamp(loop, 0, cutSize.Length);
            var cutTemplate = LevelSizes[Mathf.Clamp(loop, 0, cutSize.Length - 1)];
            
            return new LevelViewData()
            {
                Difficulty = difficulty,
                CutTemplate = cutTemplate,
                Image = image
            };
        }

#if UNITY_EDITOR
        public void SetData(string id, Sprite image, Vector2Int cutSize, Sprite backSprite)
        {
            this.id = id;
            this.image = image;
            this.cutSize = new[] { cutSize };
            background = backSprite;
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