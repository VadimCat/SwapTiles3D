using Cysharp.Threading.Tasks;
using Ji2.Context;
using UnityEngine;

namespace Client.Views
{
    public class LevelView3D : AFieldView
    {
        private Context _context;
        public override Transform SpawnRoot => transform;

        public override void SetGridSizeByData(int columns, int rows, float imageAspect)
        {
        }

        public override UniTask AnimateWin()
        {
            return default;
        }
    }
}
