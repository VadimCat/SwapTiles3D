using JetBrains.Annotations;
using Ji2.Configs.Levels;
using Ji2.Models;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Client
{
    [CreateAssetMenu(fileName = "LevelViewConfig")]
    public class LevelViewDataConfig : SerializedScriptableObject, ILevelViewData
    {
        [SerializeField] private string id;

        [field: SerializeField, PreviewField(150, ObjectFieldAlignment.Left)]
        public Sprite Image { get; private set; }

        [SerializeField] private Sprite background;

        [field: SerializeField] private LevelRules[] LevelRules { get; set; }

        public Sprite Background => background;
        public string Id => id;

        public LevelViewData ViewData(int loop)
        {
            var difficulty = (Difficulty)Mathf.Clamp(loop, 0, LevelRules.Length);
            var rules = LevelRules[Mathf.Clamp(loop, 0, LevelRules.Length - 1)];

            var cutTemplate = rules.GetTemplate();

            return new LevelViewData()
            {
                Difficulty = difficulty,
                CutTemplate = cutTemplate,
                Image = Image,
                DiscreteRotationAngle = rules.IsRotationAvailable ? GetRotationAngle() : 0
            };

            int GetRotationAngle()
            {
                Vector2Int cellSize = new Vector2Int(Image.texture.width / cutTemplate.GetLength(0),
                    Image.texture.height / cutTemplate.GetLength(1));
                return cellSize.x == cellSize.y ? 90 : 180;
            }
        }

#if UNITY_EDITOR
        public void SetData(string id, Sprite image, Sprite backSprite)
        {
            this.id = id;
            this.Image = image;
            background = backSprite;
        }
#endif
    }

    [ShowOdinSerializedPropertiesInInspector]
    [UsedImplicitly]
    public class LevelRules
    {
        [field: SerializeField] bool[,] LevelTemplate { get; set; }
        [field: SerializeField] public bool IsRotationAvailable { get; private set; }

        public bool[,] GetTemplate()
        {
            var height = LevelTemplate.GetLength(1);
            var width = LevelTemplate.GetLength(0);
            bool[,] template = new bool[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    template[i, j] = LevelTemplate[i, height - j - 1];
                }
            }

            return template;
        }
    }
}