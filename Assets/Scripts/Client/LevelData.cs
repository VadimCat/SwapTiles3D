using Ji2.Models;
using UnityEngine;

namespace Client
{
    public class LevelViewData
    {
        public Sprite Image;
        public bool[,] CutTemplate;
        public Difficulty Difficulty;
        public int DiscreteRotationAngle;
        public float Aspect => Image.Aspect();
    }
}