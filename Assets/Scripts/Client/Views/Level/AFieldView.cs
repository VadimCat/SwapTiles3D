using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Views
{
    public abstract class AFieldView: MonoBehaviour 
    {
        public abstract void SetGridSizeByData(int columns, int rows, float imageAspect);
        public abstract UniTask AnimateWin();
        public abstract Transform SpawnRoot { get; }
    }
}