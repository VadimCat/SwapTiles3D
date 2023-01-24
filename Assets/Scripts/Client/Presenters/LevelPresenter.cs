using System;
using System.Collections.Generic;
using Client.Models;
using Client.UI.Screens;
using Client.Views.Level;
using Cysharp.Threading.Tasks;
using Ji2Core.Core;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Utils.Shuffling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Client.Presenters
{
    public class LevelPresenter : IUpdatable
    {
        public event Action LevelCompleted;

        private readonly LevelView view;
        private readonly Level model;
        private readonly ScreenNavigator screenNavigator;
        private readonly UpdateService updateService;
        private readonly LevelsConfig levelViewConfig;
        private readonly LevelsLoopProgress levelsLoopProgress;

        private ModelAnimator modelAnimator = new();
        private LevelScreen levelScreen;
        private Dictionary<Vector2Int, CellView> posToCell = new();
        private Dictionary<CellView, Vector2Int> viewToPos = new();

        public LevelPresenter(LevelView view, Level model, ScreenNavigator screenNavigator,
            UpdateService updateService, LevelsConfig levelViewConfig, LevelsLoopProgress levelsLoopProgress)
        {
            this.view = view;
            this.model = model;
            this.screenNavigator = screenNavigator;
            this.updateService = updateService;
            this.levelViewConfig = levelViewConfig;
            this.levelsLoopProgress = levelsLoopProgress;

            model.LevelCompleted += OnLevelCompleted;
            model.TileSelected += SelectTile;
            model.TilesSwapped += SwapTiles;
            model.TileDeselected += DeselectTile;
        }

        public Level Model => model;

        public void BuildLevel()
        {
            var levelViewData = levelViewConfig.GetData(model.Name).ViewData();
            view.SetGridSizeByData(levelViewData);

            for (var y = 0; y < model.currentPoses.GetLength(0); y++)
            for (var x = 0; x < model.currentPoses.GetLength(1); x++)
            {
                var position = model.currentPoses[x, y];
                
                var cellView = Object.Instantiate(levelViewConfig.CellView, view.GridRoot);
                cellView.SetData(levelViewData, position);
                
                var tilePos = new Vector2Int(x, y);
                
                posToCell[new Vector2Int(x, y)] = cellView;
                viewToPos[cellView] = tilePos;

                cellView.Clicked += OnTileClick;
            }
        }

        public void StartLevel()
        {
            model.LogAnalyticsLevelStart();
            levelScreen = (LevelScreen)screenNavigator.CurrentScreen;
            levelScreen.SetLevelName($"Level {model.LevelCount + 1}");

            updateService.Add(this);
        }

        public void OnUpdate()
        {
            model.AppendPlayTime(Time.deltaTime);
        }

        private void OnTileClick(CellView cellView)
        {
            var pos = viewToPos[cellView];
            model.ClickTile(pos);
        }

        private void DeselectTile(Vector2Int pos)
        {
            modelAnimator.Animate(posToCell[pos].PlayDeselectAnimation());
        }

        private async void SwapTiles(Vector2Int pos1, Vector2Int pos2)
        {
            var cell1 = posToCell[pos1];
            var cell2 = posToCell[pos2];
            
            var task1 = cell1.PlayMoveAnimation(cell2.transform.position);
            var task2 = cell2.PlayMoveAnimation(cell1.transform.position);
            
            var p1 = Shufflling.Vector2IntToInt(pos1, model.cutSize);
            var p2 = Shufflling.Vector2IntToInt(pos2, model.cutSize);
            
            await modelAnimator.Animate(UniTask.WhenAll(task1, task2));

            if(p1 < p2)
            {
                cell1.transform.SetSiblingIndex(p2);
                cell2.transform.SetSiblingIndex(p1);
            }
            else
            {
                cell2.transform.SetSiblingIndex(p1);
                cell1.transform.SetSiblingIndex(p2);
            }
            
            (posToCell[pos1], posToCell[pos2]) = (posToCell[pos2], posToCell[pos1]);

            viewToPos[posToCell[pos1]] = pos1;
            viewToPos[posToCell[pos2]] = pos2;
        }

        private void SelectTile(Vector2Int tilePos)
        {
            modelAnimator.Animate(posToCell[tilePos].PlaySelectAnimation());
        }

        private async void OnLevelCompleted()
        {
            await modelAnimator.AwaitAllAnimationsEnd();
            Object.Destroy(view.gameObject);
            model.LogAnalyticsLevelFinish();
            levelsLoopProgress.IncLevel();
            updateService.Remove(this);
            LevelCompleted?.Invoke();
        }
    }

    public class ModelAnimator
    {
        private List<UniTask> animations = new();
        
        public async UniTask Animate(UniTask uniTask)
        {
            animations.Add(uniTask);
            await uniTask;
            animations.Remove(uniTask);
        }

        public async UniTask AwaitAllAnimationsEnd()
        {
            await UniTask.WaitUntil(CheckAnimationsListEmpty);
        }

        private bool CheckAnimationsListEmpty()
        {
            return animations.Count == 0;
        }
    }
}