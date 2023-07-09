using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Views
{
    public abstract class ACellView : MonoBehaviour
    {
        public abstract void SetData(Sprite sprite, bool isActive, Vector2Int position, float initialRotation, int columns,
            int rows);

        public abstract UniTask PlaySelectAnimation();
        public abstract UniTask PlayDeselectAnimation();
        public abstract UniTask PlayMoveAnimation(ACellView aCellView);
        public abstract UniTask PlayRotationAnimation(int rotation);
        public abstract UniTask PlaySetAnimation();

        public abstract event Action<ACellView> EventClicked;
        public abstract void EnableInteraction();
        public abstract void DisableInteraction();
        public abstract UniTask Pulse();
    }
}