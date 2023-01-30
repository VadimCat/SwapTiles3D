using System;
using System.Linq;
using UnityEngine;

namespace Client
{
    [CreateAssetMenu(menuName = "Configs/LevelResultConfig", fileName = "LevelResultConfig")]
    public class LevelResultViewConfig : ScriptableObject
    {
        [SerializeField] private ResultViewData[] resultsViewData;

        public Color GetColor(LevelResult levelResult)
        {
            return resultsViewData.First(el => el.result == levelResult).color;
        }
    }

    [Serializable]
    public class ResultViewData
    {
        public LevelResult result;
        public Color color = Color.white;
    }

    public enum LevelResult
    {
        None,
        Worst,
        Ok,
        Good,
        Perfect
    }
}