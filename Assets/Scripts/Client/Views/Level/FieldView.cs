using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Ji2.Context;
using Ji2Core.Core.Audio;
using UnityEngine;

namespace Client.Views
{
    public class FieldView : MonoBehaviour
    {
        private Context _context;
        private PositionProvider _positionProvider;
        public Transform SpawnRoot => transform;

        public IReadOnlyDictionary<Vector2Int, CellView> PosToCell => _posToCell;
        public IReadOnlyDictionary<CellView, Vector2Int> CellToPos => _cellToPos;

        private readonly Dictionary<Vector2Int, CellView> _posToCell = new();
        private readonly Dictionary<CellView, Vector2Int> _cellToPos = new();
        private Sound _sound;
        private ICellViewFactory _cellViewFactory;

        public void SetDependencies(ICellViewFactory cellViewFactory, Sound sound)
        {
            _cellViewFactory = cellViewFactory;
            _sound = sound;
        }
        
        public async UniTask AnimateWin()
        {
            List<UniTask> pulseTasks = new List<UniTask>();
            foreach (var view in _cellToPos.Keys)
            {
                var task = view.Pulse();
                pulseTasks.Add(task);
            }

            await UniTask.WhenAll(pulseTasks);
        }

        public void BuildLevel(int width, int height)
        {
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                CellView cellView = _cellViewFactory.Create(x, y);
                RegisterCell(x, y, cellView);
            }
            
            void RegisterCell(int x, int y, CellView cellView)
            {
                _posToCell[new Vector2Int(x, y)] = cellView;
                _cellToPos[cellView] = new Vector2Int(x, y);
            }
        }
        
        public async UniTask SwapAnimation(Vector2Int pos1, Vector2Int pos2)
        {
            _sound.PlaySfxAsync(SoundNamesCollection.Swap).Forget();

            var cell1 = PosToCell[pos1];
            var cell2 = PosToCell[pos2];

            await UniTask.WhenAll(cell1.PlayMoveAnimation(cell2),
                cell2.PlayMoveAnimation(cell1));
        }

        public async UniTask PlaySelectAnimation(Vector2Int tilePos)
        {
            _sound.PlaySfxAsync(SoundNamesCollection.TileTap).Forget();
            await PosToCell[tilePos].PlaySelectAnimation();
        }

        public async UniTask PlayDeselectAnimation(Vector2Int pos)
        {
            _sound.PlaySfxAsync(SoundNamesCollection.TileTap).Forget();
            
            await PosToCell[pos].PlayDeselectAnimation();
        }

        public async UniTask PlaySetAnimation(Vector2Int pos)
        {
            _sound.PlaySfxAsync(SoundNamesCollection.TileSet).Forget();

            await PosToCell[pos].PlaySetAnimation();
        }

        public async UniTask PlayRotationAnimation(Vector2Int pos, int rotation)
        {
            await PosToCell[pos].PlayRotationAnimation(rotation);
        }
    }
}