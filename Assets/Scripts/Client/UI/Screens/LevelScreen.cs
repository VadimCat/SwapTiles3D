using DG.Tweening;
using Ji2Core.UI.Screens;
using TMPro;
using UnityEngine;

namespace Client.UI.Screens
{
    public class LevelScreen : BaseScreen
    {
        [SerializeField] private TMP_Text levelName;

        private Sequence animationsSequence;

        public void SetLevelName(string name)
        {
            levelName.text = name;
        }
    }
}