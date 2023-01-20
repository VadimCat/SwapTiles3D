using System;
using System.Collections.Generic;
using Client.Models;
using Client.UI.Screens;
using Client.Views.Level;
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

        private LevelScreen levelScreen;
        private Dictionary<Vector2Int, CellView> posToView = new();
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

        public void BuildLevel()
        {
            var levelViewData = levelViewConfig.GetData(model.Name).ViewData();
            view.SetGridSizeByData(levelViewData);

            foreach (var position in model.currentPoses)
            {
                var cellView = Object.Instantiate(levelViewConfig.CellView, view.GridRoot);
                cellView.SetData(levelViewData, position);
                posToView[position] = cellView;
                viewToPos[cellView] = position;

                cellView.Clicked += OnTileClick;
            }
        }

        public void StartLevel()
        {
            model.LogAnalyticsLevelStart();
            levelScreen = (LevelScreen)screenNavigator.CurrentScreen;
            levelScreen.SetLevelName(model.Name);

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
            posToView[pos].PlayDeselectAnimation();
        }

        private void SwapTiles(Vector2Int pos1, Vector2Int pos2)
        {
            var view1 = posToView[pos1];
            var view2 = posToView[pos2];
            
            posToView[pos1]
                .PlayMoveAnimation(view2.transform.position, Shufflling.Vector2IntToInt(pos1, model.cutSize));
            
            posToView[pos2]
                .PlayMoveAnimation(view1.transform.position, Shufflling.Vector2IntToInt(pos2, model.cutSize));
            
            (posToView[pos1], posToView[pos2]) = (posToView[pos2], posToView[pos1]);
            viewToPos[posToView[pos1]] = pos1;
            viewToPos[posToView[pos2]] = pos2;
        }

        private void SelectTile(Vector2Int tilePos)
        {
            posToView[tilePos].PlaySelectAnimation();
        }

        private void OnLevelCompleted()
        {
            model.LogAnalyticsLevelFinish();
            levelsLoopProgress.IncLevel();
            levelScreen.ShowNextButton();
            levelScreen.ClickNext += CloseLevel;
        }

        private void CloseLevel()
        {
            updateService.Remove(this);
            LevelCompleted?.Invoke();
        }
    }
}