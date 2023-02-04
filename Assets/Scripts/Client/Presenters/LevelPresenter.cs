using System;
using System.Collections.Generic;
using Client.Models;
using Client.UI.Screens;
using Client.Views.Level;
using Core.Compliments;
using Cysharp.Threading.Tasks;
using Ji2Core.Core;
using Ji2Core.Core.Audio;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.UserInput;
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
        private readonly AudioService audioService;
        private readonly ICompliments compliments;
        private readonly InputService inputService;

        private readonly ModelAnimator modelAnimator = new();
        private readonly Dictionary<Vector2Int, CellView> posToCell = new();
        private readonly Dictionary<CellView, Vector2Int> viewToPos = new();
        
        private LevelScreen levelScreen;
        private int tilesSet;

        public Level Model => model;

        public LevelPresenter(LevelView view, Level model, ScreenNavigator screenNavigator,
            UpdateService updateService, LevelsConfig levelViewConfig, LevelsLoopProgress levelsLoopProgress,
            AudioService audioService, ICompliments compliments, InputService inputService)
        {
            this.view = view;
            this.model = model;
            this.screenNavigator = screenNavigator;
            this.updateService = updateService;
            this.levelViewConfig = levelViewConfig;
            this.levelsLoopProgress = levelsLoopProgress;
            this.audioService = audioService;
            this.compliments = compliments;
            this.inputService = inputService;

            model.LevelCompleted += OnLevelCompleted;
            model.TileSelected += SelectTile;
            model.TilesSwapped += SwapTiles;
            model.TileDeselected += DeselectTile;
            model.TileSetted += SetTile;
            model.TurnCompleted += UpdateTurn;
        }

        private void UpdateTurn(int turnCount)
        {
            levelScreen.SetTurnsCount(turnCount);
        }

        private void SetTile(Vector2Int pos)
        {
            tilesSet++;
            int closureSet = tilesSet;
            modelAnimator.EnqueueAnimation(() =>
            {
                if (closureSet % 2 == 0)
                {
                    compliments.ShowRandomFromScreenPosition(inputService.lastPos);
                }
                audioService.PlaySfxAsync(AudioClipName.TileSet);
                return posToCell[pos].PlaySetAnimation();
            });
        }

        public void BuildLevel()
        {
            var levelViewData = levelViewConfig.GetData(model.Name).ViewData((int)model.Difficulty);
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
            
            levelScreen.SetUpProgressBar(model.OkResult, model.GoodResult, model.PerfectResult);
            
            updateService.Add(this);
        }

        public void OnUpdate()
        {
            model.AppendPlayTime(Time.deltaTime);
        }

        private void OnTileClick(CellView cellView)
        {
            audioService.PlaySfxAsync(AudioClipName.TileTapFx);

            var pos = viewToPos[cellView];
            model.ClickTile(pos);
        }

        private void DeselectTile(Vector2Int pos)
        {
            audioService.PlaySfxAsync(AudioClipName.TileTapFx);

            modelAnimator.Animate(posToCell[pos].PlayDeselectAnimation());
        }

        private async void SwapTiles(Vector2Int pos1, Vector2Int pos2)
        {
            await modelAnimator.EnqueueAnimation(() =>
            {
                audioService.PlaySfxAsync(AudioClipName.Swap);
                return SwapAnimation(pos1, pos2);
            });
        }

        private async UniTask SwapAnimation(Vector2Int pos1, Vector2Int pos2)
        {
            var cell1 = posToCell[pos1];
            var cell2 = posToCell[pos2];

            await UniTask.WhenAll(cell1.PlayMoveAnimation(cell2.transform.position),
                cell2.PlayMoveAnimation(cell1.transform.position));
            var p1 = Shufflling.Vector2IntToInt(pos1, model.cutSize);
            var p2 = Shufflling.Vector2IntToInt(pos2, model.cutSize);

            if (p1 < p2)
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
            model.LogAnalyticsLevelFinish();
            levelsLoopProgress.IncLevel();
            updateService.Remove(this);

            modelAnimator.EnqueueAnimation(() => view.AnimateWin());
            await modelAnimator.AwaitAllAnimationsEnd();

            audioService.PlaySfxAsync(AudioClipName.WinFX);
            Object.Destroy(view.gameObject);

            LevelCompleted?.Invoke();
        }

        public Vector3 GetTilePos(Vector2Int pos)
        {
            return posToCell[pos].transform.position;
        }
    }
}