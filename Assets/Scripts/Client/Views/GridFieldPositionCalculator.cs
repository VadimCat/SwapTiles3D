using UnityEngine;

namespace Client.Views
{
    public class GridFieldPositionCalculator
    {
        private const float BorderL = 56;
        private const float BorderR = 56;
        private const float BorderT = 56;
        private const float BorderB = 109;

        public readonly Vector2Int Size;
        private readonly Vector2 _availableScreenSize;

        public readonly float CellWidth;
        public readonly float CellHeight;

        public readonly Vector2 FieldImageSize;
        public Vector3 CellSize => new Vector3(CellWidth, CellHeight, 1);


        public GridFieldPositionCalculator(Vector2Int size, Vector2 screenSize, float screenNavigatorScaleFactor,
            float imageAspect)
        {
            Size = size;
            _availableScreenSize = screenSize - new Vector2(BorderL + BorderR, BorderB + BorderT);

            float worldToPixels = screenSize.y / 4;
            float imageWidth = _availableScreenSize.x / worldToPixels;
            float imageHeight = imageWidth / imageAspect;
            CellWidth = imageWidth / size.x;

            CellHeight = imageHeight / size.y;

            FieldImageSize = new Vector2(size.x * CellWidth, size.y * CellHeight) * worldToPixels
                             + new Vector2(BorderL + BorderR, BorderB + BorderT) * screenNavigatorScaleFactor;
        }

        public Vector3 GetPoint(Vector2Int position)
        {
            return GetPoint(position.x, position.y);
        }

        public Vector3 GetPoint(int x, int y)
        {
            return new Vector3(CellWidth * (x - (float)Size.x / 2 + .5f), CellHeight * (y - (float)Size.y / 2 + .5f));
        }

        public Vector2Int GetReversePoint(Vector3 position)
        {
            return new Vector2Int(Mathf.RoundToInt((position.x + CellWidth * Size.x / 2) / CellWidth),
                Mathf.RoundToInt((position.y + CellWidth * Size.y / 2) / CellWidth));
        }
    }
}