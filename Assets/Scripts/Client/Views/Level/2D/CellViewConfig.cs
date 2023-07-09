﻿using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Client.Views
{
    [CreateAssetMenu(menuName = "CellViewConfig", fileName = "CellViewConfig")]
    public class CellViewConfig : SerializedScriptableObject
    {
        [FormerlySerializedAs("_cellView")] [SerializeField] private ACellView aCellView;

        public ACellView ACellView => aCellView;
    }
}