using Client.Models;
using Client.Presenters;
using Client.States;
using Ji2Core.Core;
using Ji2Core.Core.Audio;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using Ji2Core.Plugins.AppMetrica;
using UI.Background;
using UnityEngine;
using Client.Tutorial;
using Client.Views;
using Core.Compliments;
using Ji2.CommonCore;
using Ji2.CommonCore.SaveDataContainer;
using Ji2.Context;
using Ji2.Models.Analytics;
using Ji2.Presenters.Tutorial;
using Ji2.UI;
using Ji2Core.Core.Pools;
using UnityEngine.Serialization;

namespace Client
{
    public class  Bootstrap : BootstrapBase
    {
        [SerializeField] private LevelsConfig levelsConfig;
        [SerializeField] private ScreenNavigator screenNavigator;
        [SerializeField] private BackgroundService backgroundService;
        [SerializeField] private UpdateService updateService;
        [SerializeField] private ImageCompliments compliments;
        [FormerlySerializedAs("audioService")] [SerializeField] private Sound sound;
        [SerializeField] private TutorialPointerView tutorialPointer;
        [SerializeField] private LevelResultViewConfig levelResultViewConfig;

        private AppSession _appSession;

        private readonly Context _context = Context.GetOrCreateInstance();

        protected override void Start()
        {
            DontDestroyOnLoad(this);
            InstallCamera();
            InstallAudioService();
            InstallLevelsData();
            InstallNavigator();
            InstallInputService();
            _context.Register(updateService);
            _context.Register(backgroundService);
            var sceneLoader = new SceneLoader(updateService);

            InstallAnalytics();

            ISaveDataContainer dataContainer = new PlayerPrefsSaveDataContainer();
            _context.Register(dataContainer);
            _context.Register(levelResultViewConfig);
            _context.Register(new LevelsLoopProgress(dataContainer, levelsConfig.GetLevelsOrder()));


            _context.Register(sceneLoader);
            _context.Register<ICompliments>(compliments);
            _context.Register(new Pool<CellView>(levelsConfig.ACellView, transform));
            
            InstallFactories();
            
            InstallStateMachine();
            InstallTutorial();
            StartApplication();
        }

        private void InstallFactories()
        {
            _context.Register(new LevelFactory(_context));
            _context.Register(new LevelPresenterFactory(_context));
        }

        private void InstallTutorial()
        {
            _context.Register(tutorialPointer);
            ITutorialFactory factory = new TutorialFactory(_context);
            ITutorialStep[] steps = { factory.Create<InitialTutorialStep>() };
            var tutorialService = new TutorialService(_context.GetService<ISaveDataContainer>(), steps);
            _context.Register(tutorialService);
        }

        private void InstallStateMachine()
        {
            StateMachine appStateMachine = new StateMachine(new StateFactory(_context));
            _context.Register(appStateMachine);
        }

        private void InstallAnalytics()
        {
            IAnalytics analytics = new Analytics();
            analytics.AddLogger(new YandexMetricaLogger(AppMetrica.Instance));
            _context.Register(analytics);
        }

        private void StartApplication()
        {
            var appStateMachine = _context.GetService<StateMachine>();
            appStateMachine.Load();
            _appSession = new AppSession(appStateMachine);
            _appSession.StateMachine.Enter<InitialState>();
        }

        private void InstallCamera()
        {
            _context.Register(new CameraProvider());
        }

        private void InstallInputService()
        {
            _context.Register(new Ji2Core.Core.UserInput.MouseInput(updateService));
        }

        private void InstallAudioService()
        {
            sound.Bootstrap();
            sound.PlayMusic(SoundNamesCollection.BackgroundMusic);
            _context.Register(sound);
        }

        private void InstallNavigator()
        {
            screenNavigator.Bootstrap();
            _context.Register(screenNavigator);
        }

        private void InstallLevelsData()
        {
            levelsConfig.Bootstrap();
            _context.Register(levelsConfig);
        }
    }
}