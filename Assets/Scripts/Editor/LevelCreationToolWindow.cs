using System.Collections.Generic;
using System.IO;
using System.Linq;
using Client;
using Client.Views;
using Ji2.Configs.Levels;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class LevelCreationToolWindow : EditorWindow
    {
        protected const string BaseArtPath = "Assets/Art/Levels/";
        protected static LevelsConfig StorageBase;

        private static Sprite _levelSprite;
        private static Sprite _backSprite;
        private static string _levelName;
        private static Vector2Int _defaultCutSize;

        private static void DrawGUI()
        {
            EditorGUILayout.LabelField("LevelsStorage");
            StorageBase = (LevelsConfig)EditorGUILayout.ObjectField(StorageBase, typeof(LevelsConfig), true);
            EditorGUILayout.LabelField("Level name");
            _levelName = EditorGUILayout.TextField(_levelName);
            EditorGUILayout.LabelField("Level sprite");
            _levelSprite = (Sprite)EditorGUILayout.ObjectField(_levelSprite, typeof(Sprite), true);
            _backSprite = (Sprite)EditorGUILayout.ObjectField(_backSprite, typeof(Sprite), true);

            _defaultCutSize = EditorGUILayout.Vector2IntField("CutSize", _defaultCutSize);
        }

        [MenuItem("Tools/LevelCreationTool")]
        private static void ShowWindow()
        {
            SetDefaultData();

            var wind = GetWindow(typeof(LevelCreationToolWindow));
            wind.Show();
        }

        private static void SetDefaultData()
        {
            var levelDataStorage = AssetDatabase.FindAssets("t:LevelsConfig")[0];
            StorageBase = AssetDatabase.LoadAssetAtPath<LevelsConfig>(AssetDatabase.GUIDToAssetPath(levelDataStorage));
        }

        private void OnGUI()
        {
            DrawGUI();

            if (GUILayout.Button("Add level"))
            {
                if (StorageBase.LevelIdExists(_levelName))
                {
                    throw new LevelExistsException(_levelName);
                }

                CreateLevel(_levelName, _levelSprite, _defaultCutSize, _backSprite);
            }

            if (GUILayout.Button("Fill levels automaticly"))
            {
                var assets = AssetDatabase.FindAssets("t:Sprite");

                var pathes = from guid in assets
                    where AssetDatabase.GUIDToAssetPath(guid).Contains(BaseArtPath)
                    select AssetDatabase.GUIDToAssetPath(guid);

                var spritePathes = pathes.ToArray();

                var levelSprites = GetSpritesFromPathes(spritePathes);
                foreach (var spr in levelSprites)
                {
                    CreateLevel(spr.name, spr,_defaultCutSize, _backSprite);
                }
            }
        }

        private void CreateLevel(string id, Sprite sprite, Vector2Int size, Sprite backSprite)
        {
            var config = CreateInstance<LevelViewDataConfig>();
            config.name = id;
            config.SetData(id, sprite, backSprite);

            var path = Path.Combine("Assets\\Configs\\Levels", $"{id}_ViewData.asset");

            StorageBase.AddLevel(config);
            EditorUtility.SetDirty(StorageBase);
            
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
        }

        private static List<Sprite> GetSpritesFromPathes(string[] spritePathes)
        {
            var greyAtlases = from path in spritePathes select path;

            List<Sprite> sprites = new List<Sprite>();
            foreach (var atlas in greyAtlases)
            {
                var spriteObjs = AssetDatabase.LoadAllAssetsAtPath(atlas);
                foreach (var obj in spriteObjs)
                {
                    if (obj is Sprite spr)
                        sprites.Add(spr);
                }
            }

            return sprites;
        }

        private void OnFocus()
        {
            SetDefaultData();
        }
    }
}