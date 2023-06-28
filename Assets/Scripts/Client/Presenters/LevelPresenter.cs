using System;
using System.Collections.Generic;
using Client.Input;
using Client.Models;
using Client.UI.Screens;
using Client.Views.Level;
using Core.Compliments;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ji2.CommonCore;
using Ji2.Presenters;
using Ji2Core.Core.Audio;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.UserInput;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Client.Presenters
{
    public class LevelPresenter : IUpdatable
    {
        public event Action LevelCompleted;

        private readonly LevelView _view;
        private readonly Level _model;
        private readonly ScreenNavigator _screenNavigator;
        private readonly UpdateService _updateService;
        private readonly LevelsConfig _levelViewConfig;
        private readonly LevelsLoopProgress _levelsLoopProgress;
        private readonly AudioService _audioService;
        private readonly ICompliments _compliments;
        private readonly InputService _inputService;

        private readonly ModelAnimator _modelAnimator = new();
        private readonly Dictionary<Vector2Int, CellView> _posToCell = new();
        private readonly Dictionary<CellView, Vector2Int> _viewToPos = new();

        private LevelScreen _levelScreen;
        private int _tilesSet;
        private readonly TouchscreenInput _touchScreenInput;

        public Level Model => _model;

        public LevelPresenter(LevelView view, Level model, ScreenNavigator screenNavigator,
            UpdateService updateService, LevelsConfig levelViewConfig, LevelsLoopProgress levelsLoopProgress,
            AudioService audioService, ICompliments compliments, InputService inputService)
        {
            _view = view;
            _model = model;
            _screenNavigator = screenNavigator;
            _updateService = updateService;
            _levelViewConfig = levelViewConfig;
            _levelsLoopProgress = levelsLoopProgress;
            _audioService = audioService;
            _compliments = compliments;
            _inputService = inputService;

            model.LevelCompleted += OnLevelCompleted;
            model.TileSelected += SelectTile;
            model.TilesSwapped += SwapTiles;
            model.TileDeselected += DeselectTile;
            model.TileSet += SetTile;
            model.TurnCompleted += UpdateTurn;
            model.TileRotated += Rotate;

            _touchScreenInput = new TouchscreenInput();
            _touchScreenInput.EventSwipeDirectional += TrySwipe;
        }

        private void Rotate(Vector2Int pos, int rotation)
        {
            _modelAnimator.Enqueue(() => _posToCell[pos].PlayRotationAnimation(rotation)).Forget();
        }

        private void TrySwipe(Direction dir)
        {
            _model.TrySwipe(dir);
        }

        private void UpdateTurn(int turnCount)
        {
            _levelScreen.SetTurnsCount(turnCount).Forget();
        }

        private void SetTile(Vector2Int pos)
        {
            _tilesSet++;
            int closureSet = _tilesSet;
            _modelAnimator.Enqueue(() =>
            {
                if (closureSet % 2 == 0)
                {
                    _compliments.ShowRandomFromScreenPosition(_inputService.lastPos);
                }

                _audioService.PlaySfxAsync(SoundNamesCollection.TileSet).Forget();
                return _posToCell[pos].PlaySetAnimation();
            }).Forget();
        }

        public void BuildLevel()
        {
            LevelViewData levelViewData = _levelViewConfig.GetData(_model.Name).ViewData((int)_model.Difficulty);
            _view.SetGridSizeByData(levelViewData);
            int i = 0;
            for (var x = 0; x < _model.CurrentPoses.GetLength(0); x++)
            for (var y = 0; y < _model.CurrentPoses.GetLength(1); y++)
            {
                var position = _model.CurrentPoses[x, y].OriginalPos;
                int rotation = _model.CurrentPoses[x, y].Rotation;

                var cellView = Object.Instantiate(_levelViewConfig.CellView, _view.GridRoot);
                cellView.SetData(levelViewData, position, rotation);
                var tilePos = new Vector2Int(x, y);

                _posToCell[new Vector2Int(x, y)] = cellView;
                _viewToPos[cellView] = tilePos;

                cellView.Clicked += OnTileClick;
            }
        }

        public void StartLevel()
        {
            _model.LogAnalyticsLevelStart();
            _levelScreen = (LevelScreen)_screenNavigator.CurrentScreen;
            _levelScreen.SetLevelName($"Level {_model.LevelCount + 1}");

            _levelScreen.SetUpProgressBar(_model.OkResult, _model.GoodResult, _model.PerfectResult);

            _updateService.Add(this);
        }

        public void OnUpdate()
        {
            _model.AppendPlayTime(Time.deltaTime);
        }

        private void OnTileClick(CellView cellView)
        {
            _audioService.PlaySfxAsync(SoundNamesCollection.TileTap).Forget();

            var pos = _viewToPos[cellView];
            _model.ClickTile(pos);
        }

        private void DeselectTile(Vector2Int pos)
        {
            _audioService.PlaySfxAsync(SoundNamesCollection.TileTap).Forget();

            _modelAnimator.Animate(_posToCell[pos].PlayDeselectAnimation()).Forget();
        }

        private async void SwapTiles(Vector2Int pos1, Vector2Int pos2)
        {
            await _modelAnimator.Enqueue(() =>
            {
                _audioService.PlaySfxAsync(SoundNamesCollection.Swap).Forget();
                return SwapAnimation(pos1, pos2);
            });
        }

        private async UniTask SwapAnimation(Vector2Int pos1, Vector2Int pos2)
        {
            var cell1 = _posToCell[pos1];
            var cell2 = _posToCell[pos2];

            await UniTask.WhenAll(cell1.PlayMoveAnimation(cell2),
                cell2.PlayMoveAnimation(cell1));
        }

        private void SelectTile(Vector2Int tilePos)
        {
            _modelAnimator.Animate(_posToCell[tilePos].PlaySelectAnimation()).Forget();
        }

        private async void OnLevelCompleted()
        {
            _model.LogAnalyticsLevelFinish();
            _levelsLoopProgress.IncLevel();
            _updateService.Remove(this);

            _modelAnimator.Enqueue(_view.AnimateWin).Forget();
            _modelAnimator.Enqueue(PulseTiles).Forget();
            await _modelAnimator.AwaitAllAnimationsEnd();

            _audioService.PlaySfxAsync(SoundNamesCollection.Win).Forget();
            Object.Destroy(_view.gameObject);

            LevelCompleted?.Invoke();
        }

        private async UniTask PulseTiles()
        {
            List<UniTask> pulseTasks = new List<UniTask>();
            foreach (var view in _viewToPos.Keys)
            {
                var task = view.transform.DOScale(1.1f, .1f).AwaitForComplete();
                pulseTasks.Add(task);
            }

            await UniTask.WhenAll(pulseTasks);
        }

        public Vector3 GetTilePos(Vector2Int pos)
        {
            return _posToCell[pos].transform.position;
        }
    }
}