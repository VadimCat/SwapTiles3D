using System;
using Client.Models;
using Client.UI.Screens;
using Client.Views;
using Core.Compliments;
using Cysharp.Threading.Tasks;
using Ji2.CommonCore;
using Ji2.Presenters;
using Ji2.Utils;
using Ji2Core.Core;
using Ji2Core.Core.Audio;
using Ji2Core.Core.Pools;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.UserInput;
using UnityEngine;

namespace Client.Presenters
{
    public class LevelPresenter : IUpdatable, IDisposable
    {
        public event Action LevelCompleted;

        private readonly FieldView _view;
        private readonly Level _model;
        private readonly ScreenNavigator _screenNavigator;
        private readonly Pool<CellView> _cellsPool;
        private readonly UpdateService _updateService;
        private readonly LevelsConfig _levelConfig;
        private readonly LevelsLoopProgress _levelsLoopProgress;
        private readonly Sound _sound;
        private readonly ICompliments _compliments;
        private readonly MouseInput _mouseInput;

        private readonly ModelAnimator _modelAnimator = new();

        private LevelScreen _levelScreen;
        private int _tilesSet;
        private readonly SwipeListener _swipeListener;

        private readonly Camera _camera;
        private readonly GridFieldPositionCalculator _gridFieldPositionCalculator;
        private readonly CellsInteractionHandler _cellsInteractionHandler;

        public Level Model => _model;

        public LevelPresenter(FieldView view, Level model, ScreenNavigator screenNavigator, Pool<CellView> cellsPool,
            UpdateService updateService, LevelsConfig levelConfig, LevelsLoopProgress levelsLoopProgress,
            Sound sound, ICompliments compliments, MouseInput mouseInput, CameraProvider cameraProvider)
        {
            _view = view;
            _model = model;
            _screenNavigator = screenNavigator;
            _cellsPool = cellsPool;
            _updateService = updateService;
            _levelConfig = levelConfig;
            _levelsLoopProgress = levelsLoopProgress;
            _sound = sound;
            _compliments = compliments;
            _mouseInput = mouseInput;
            _camera = cameraProvider.MainCamera;

            _gridFieldPositionCalculator =
                new GridFieldPositionCalculator(_model.Size, _screenNavigator.Size, _screenNavigator.ScaleFactor,
                    _levelConfig.GetData(_model.Name).Image.Aspect());

            var cellFactory = new CellFactory(_model, _cellsPool, _gridFieldPositionCalculator, _view,
                _levelConfig.GetData(_model.Name).Image);
            _view.SetDependencies(cellFactory, sound);

            model.LevelCompleted += OnLevelCompleted;
            model.TileSelected += SelectTile;
            model.TilesSwapped += SwapTiles;
            model.TileDeselected += DeselectTile;
            model.TileSet += SetTile;
            model.TurnCompleted += UpdateTurn;
            model.TileRotated += Rotate;

            _swipeListener = new SwipeListener(updateService);
            _swipeListener.EventSwiped += TrySwipe;

            _cellsInteractionHandler =
                new CellsInteractionHandler(_gridFieldPositionCalculator, model, view, _swipeListener, cameraProvider);
        }

        public void BuildLevel()
        {
            _view.BuildLevel(_model.Size.x, _model.Size.y);
            _cellsInteractionHandler.Initialize();
        }

        public void StartLevel()
        {
            _model.LogAnalyticsLevelStart();
            _levelScreen = (LevelScreen)_screenNavigator.CurrentScreen;
            _levelScreen.SetLevelName($"Level {_model.LevelCount + 1}");

            _levelScreen.SetUpProgressBar(_model.OkResult, _model.GoodResult, _model.PerfectResult);

            _updateService.Add(this);
        }

        public Vector3 GetTilePos(Vector2Int pos)
        {
            return _view.PosToCell[pos].transform.position;
        }

        public void OnUpdate()
        {
            _model.AppendPlayTime(Time.deltaTime);
        }

        private void TrySwipe(Vector2 from, Vector2 to)
        {
            var swipeDir = to - from;
            var tileScreenPos = _camera.WorldToScreenPoint(_view.PosToCell[_model.FirstSelected].transform.position);
            Vector2 tileToTouch = (Vector2)tileScreenPos - to;
            var signedAngle = Vector2.SignedAngle(swipeDir, tileToTouch);
            _model.TrySwipe(signedAngle > 0 ? RotationDirection.Clockwise : RotationDirection.CounterClockwise);
        }

        private void Rotate(Vector2Int pos, int rotation)
        {
            _modelAnimator.Enqueue(async () =>
            {
                _swipeListener.Disable();
                await _view.PlayRotationAnimation(pos, rotation);
                _swipeListener.Enable();
            }).Forget();
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
                if (closureSet % 4 == 0)
                {
                    _compliments.ShowRandomFromScreenPosition(_mouseInput.lastPos);
                }

                return _view.PlaySetAnimation(pos);
            }).Forget();
        }

        private void DeselectTile(Vector2Int pos)
        {
            _modelAnimator.Animate(_view.PlayDeselectAnimation(pos)).Forget();
        }

        private void SwapTiles(Vector2Int pos1, Vector2Int pos2)
        {
            _modelAnimator.Enqueue(() => _view.SwapAnimation(pos1, pos2)).Forget();
        }

        private void SelectTile(Vector2Int tilePos)
        {
            if (_model.SelectedTilesCount == 1)
            {
                _swipeListener.Enable();
            }
            else
            {
                _swipeListener.Disable();
            }

            _modelAnimator.Animate(_view.PlaySelectAnimation(tilePos)).Forget();
        }

        private async void OnLevelCompleted()
        {
            _model.LogAnalyticsLevelFinish();
            _levelsLoopProgress.IncLevel();
            _updateService.Remove(this);

            _modelAnimator.Enqueue(_view.AnimateWin).Forget();
            await _modelAnimator.AwaitAllAnimationsEnd();

            _sound.PlaySfxAsync(SoundNamesCollection.Win).Forget();
            // Object.Destroy(_view.gameObject);

            LevelCompleted?.Invoke();
        }

        public void Dispose()
        {
            _swipeListener.EventSwiped -= TrySwipe;

            foreach (CellView cell in _view.PosToCell.Values)
            {
                _cellsPool.DeSpawn(cell);
            }
        }
    }
}