using System;
using System.Collections.Generic;
using Ji2.Context;
using UnityEngine;

namespace Client.Views
{
    public class GameSceneInstaller : SceneInstaller
    {
        [SerializeField] private AFieldView fieldView;

        protected override IEnumerable<(Type type, object obj)> GetDependencies()
        {
            yield return (typeof(AFieldView), fieldView);
        }
    }
}
