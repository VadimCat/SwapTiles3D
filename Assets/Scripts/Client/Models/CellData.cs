using UnityEngine;

namespace Client.Models
{
    public struct CellData
    {
        public readonly Vector2Int OriginalPos;
        public readonly bool IsActive;
        public int Rotation { get; set; }

        private CellData(Vector2Int originalPos)
        {
            OriginalPos = originalPos;
            IsActive = false;
            Rotation = 0;
        }

        public CellData(Vector2Int originalPos, int rotation)
        {
            OriginalPos = originalPos;
            Rotation = rotation;
            IsActive = true;
        }

        public static CellData Disabled(Vector2Int originalPos)
        {
            return new CellData(originalPos);
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