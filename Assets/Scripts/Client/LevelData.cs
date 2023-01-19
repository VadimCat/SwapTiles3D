using UnityEngine;

namespace Client
{
    public class LevelViewData
    {
        public Sprite Image;
        public Vector2Int CutSize;
        
        public float Aspect => this.Image.Aspect();
    }
}