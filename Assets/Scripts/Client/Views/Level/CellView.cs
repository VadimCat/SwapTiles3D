using UnityEngine;
using UnityEngine.UI;

namespace Client.Views.Level
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] private RawImage image;
        [SerializeField] private Canvas sortingCanvas;


        public void SetData(LevelViewData viewData, Vector2Int position)
        {
            image.texture = viewData.Image.texture;
            float w = (float)1 / viewData.CutSize.x;
            float h = (float)1 / viewData.CutSize.y;
            float x = w * position.x;
            float y = h * position.y;
            image.uvRect = new Rect(x, y, w, h);
        }
    }
}