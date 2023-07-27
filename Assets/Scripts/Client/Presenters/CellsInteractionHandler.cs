using Client.Models;
using Client.Views;
using UnityEngine.EventSystems;

namespace Client.Presenters
{
    public class CellsInteractionHandler
    {
        private readonly PositionProvider _positionProvider;
        private readonly Level _level;
        private readonly FieldView _fieldView;

        private (CellView cell, PointerEventData pointerData) _downData;
        
        public CellsInteractionHandler(PositionProvider positionProvider, Level level, FieldView fieldView)
        {
            _positionProvider = positionProvider;
            _level = level;
            _fieldView = fieldView;
        }

        public void Initialize()
        {
            foreach (var cell in _fieldView.PosToCell.Values)
            {
                cell.EventPointerDown += CellDown;
                cell.EventPointerMove += CellMove;
                cell.EventPointerUp += CellUp;
            }
        }

        private void CellDown(CellView cell, PointerEventData pointerData)
        {
            _downData = (cell, pointerData);
            
            _level.ClickTile(_fieldView.CellToPos[cell]);
        }

        private void CellMove(CellView cell, PointerEventData pointerData)
        {
            
        }

        private void CellUp(CellView cell, PointerEventData pointerData)
        {
            
        }
    }
}