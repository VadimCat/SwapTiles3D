using Client.Models;
using Client.Views;
using Ji2Core.Core.Pools;
using UnityEngine;

namespace Client.Presenters
{
    public class CellFactory : ICellViewFactory
    {
        private readonly LevelPlayableDecorator _levelPlayableDecorator;
        private readonly Pool<CellView> _cellsPool;
        private readonly GridFieldPositionCalculator _gridFieldPositionCalculator;
        private readonly FieldView _fieldView;
        private readonly Sprite _image;

        public CellFactory(LevelPlayableDecorator levelPlayableDecorator, Pool<CellView> cellsPool,
            GridFieldPositionCalculator gridFieldPositionCalculator,
            FieldView fieldView, Sprite image)
        {
            _levelPlayableDecorator = levelPlayableDecorator;
            _cellsPool = cellsPool;
            _gridFieldPositionCalculator = gridFieldPositionCalculator;
            _fieldView = fieldView;
            _image = image;
        }

        public CellView Create(int x, int y)
        {
            var position = _levelPlayableDecorator.CurrentPoses[x, y].OriginalPos;
            int rotation = _levelPlayableDecorator.CurrentPoses[x, y].Rotation;

            Vector2Int pos = new Vector2Int(x, y);
            var cellView = _cellsPool.Spawn(_gridFieldPositionCalculator.GetPoint(pos), Quaternion.identity,
                _fieldView.SpawnRoot,
                true);

            cellView.SetData(_image, position, pos, rotation, _levelPlayableDecorator.Size.x,
                _levelPlayableDecorator.Size.y,
                _gridFieldPositionCalculator.CellSize, _gridFieldPositionCalculator);

            if (!_levelPlayableDecorator.CurrentPoses[x, y].IsActive)
            {
                cellView.gameObject.SetActive(false);
            }

            return cellView;
        }
    }
}