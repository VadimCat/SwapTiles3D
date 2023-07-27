using UnityEngine;

namespace Client.Views
{
    public class PositionProvider
    {
        private const float BorderL = 56;
        private const float BorderR = 56;
        private const float BorderT = 56;
        private const float BorderB = 109;

        public readonly Vector2Int Size;
        private readonly Vector2 _availableScreenSize;

        public readonly float CellSize;

        public readonly Vector2 FieldImageSize;

        public PositionProvider(Vector2Int size, Vector2 screenSize, float screenNavigatorScaleFactor)
        {
            Size = size;
            _availableScreenSize = screenSize - new Vector2(BorderL + BorderR, BorderB + BorderT);

            float worldToPixels = screenSize.y / 4;
            float worldWidth = _availableScreenSize.x / worldToPixels;

            CellSize = worldWidth / size.x;

            FieldImageSize = new Vector2(size.x, size.y) * (CellSize * worldToPixels)
                             + new Vector2(BorderL + BorderR, BorderB + BorderT) * screenNavigatorScaleFactor;
        }

        public Vector3 GetPoint(Vector2Int position)
        {
            return GetPoint(position.x, position.y);
        }

        public Vector3 GetPoint(int x, int y)
        {
            return new Vector3(x * CellSize - CellSize * Size.x / 2, 
                       y * CellSize - CellSize * Size.x / 2) + new Vector3(CellSize, CellSize) / 2;
        }
    }
}