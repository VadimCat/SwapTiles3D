using Client.Models;
using Client.Views;
using Ji2Core.Core.Pools;
using UnityEngine;

namespace Client.Presenters
{
    public class CellFactory : ICellViewFactory
    {
        private readonly Level _level;
        private readonly Pool<CellView> _cellsPool;
        private readonly GridFieldPositionCalculator _gridFieldPositionCalculator;
        private readonly FieldView _fieldView;
        private readonly Sprite _image;

        public CellFactory(Level level, Pool<CellView> cellsPool, GridFieldPositionCalculator gridFieldPositionCalculator,
            FieldView fieldView, Sprite image)
        {
            _level = level;
            _cellsPool = cellsPool;
            _gridFieldPositionCalculator = gridFieldPositionCalculator;
            _fieldView = fieldView;
            _image = image;
        }

        public CellView Create(int x, int y)
        {
            var position = _level.CurrentPoses[x, y].OriginalPos;
            int rotation = _level.CurrentPoses[x, y].Rotation;

            if (_level.CurrentPoses[x, y].IsActive)
            {
                Vector2Int pos = new Vector2Int(x, y);
                var cellView = _cellsPool.Spawn(_gridFieldPositionCalculator.GetPoint(pos), Quaternion.identity,
                    _fieldView.SpawnRoot,
                    true);

                cellView.SetData(_image, position, pos, rotation, _level.Size.x, _level.Size.y,
                    _gridFieldPositionCalculator.CellSize, _gridFieldPositionCalculator);

                return cellView;
            }
            else
            {
                return null;
            }
        }
    }
}