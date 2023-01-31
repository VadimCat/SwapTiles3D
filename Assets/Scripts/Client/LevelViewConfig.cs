using Client.Models;
using UnityEngine;

namespace Client
{
    [CreateAssetMenu(fileName = "LevelViewConfig")]
    public class LevelViewConfig : ScriptableObject, ILevelView
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite image;
        [SerializeField] private Vector2Int[] cutSize = { new(3, 3) };
        [SerializeField] private Sprite background;

        public Sprite Background => this.background;
        public string Id => this.id;
        public Sprite Image => image;

        public LevelViewData ViewData(int loop)
        {
            return new LevelViewData()
            {
                //HACK
                difficulty = (Difficulty)Mathf.Clamp(loop, 0, cutSize.Length),
                cutSize = cutSize[Mathf.Clamp(loop, 0, cutSize.Length)],
                image = image
            };
        }

#if UNITY_EDITOR
        public void SetData(string id, Sprite image, Vector2Int cutSize, Sprite backSprite)
        {
            this.id = id;
            this.image = image;
            this.cutSize = new[] { cutSize };
            this.background = backSprite;
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