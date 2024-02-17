using Ji2.ScreenNavigation;
using TMPro;
using UnityEngine;

namespace Client.UI.Screens
{
    public class LevelScreen : BaseScreen
    {
        [SerializeField] private TMP_Text levelName;
        
        public void SetLevelName(string lvlName)
        {
            levelName.text = lvlName;
        }
    }
}