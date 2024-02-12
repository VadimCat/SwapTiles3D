using UnityEngine;

namespace Client.Models
{
    public class Cell
    {
        public readonly Vector2Int OriginalPos;
        public readonly bool IsActive;
        public int Rotation { get; set; }

        private Cell(Vector2Int originalPos)
        {
            OriginalPos = originalPos;
            IsActive = false;
            Rotation = 0;
        }

        public Cell(Vector2Int originalPos, int rotation)
        {
            OriginalPos = originalPos;
            Rotation = rotation;
            IsActive = true;
        }

        public static Cell Disabled(Vector2Int originalPos)
        {
            return new Cell(originalPos);
        }

        public bool IsOnRightPlace(int x, int y)
        {
            return OriginalPos.x == x && OriginalPos.y == y;
        }

        public bool IsDefaultRotation()
        {
            return Rotation == 0;
        }
    }
}