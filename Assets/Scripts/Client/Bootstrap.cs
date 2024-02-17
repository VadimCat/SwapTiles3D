using Client.Models;
using Client.Presenters;
using Client.States;
using Ji2Core.Core;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using Ji2Core.Plugins.AppMetrica;
using UI.Background;
using UnityEngine;
using Client.Tutorial;
using Client.Views;
using Core.Compliments;
using Ji2.Audio;
using Ji2.CommonCore;
using Ji2.CommonCore.SaveDataContainer;
using Ji2.Context;
using Ji2.Models.Analytics;
using Ji2.Presenters.Tutorial;
using Ji2.UI;
using Ji2Core.Core.Pools;

namespace Client
{
    public class  Bootstrap : BootstrapBase
    {
        [SerializeField] private LevelsConfig levelsConfig;
        [SerializeField] private ScreenNavigator screenNavigator;
        [SerializeField] private BackgroundService backgroundService;
        [SerializeField] private UpdateService updateService;
        [SerializeField] private ImageCompliments compliments;
        [SerializeField] private Sound sound;
        [SerializeField] private TutorialPointerView tutorialPointer;

        private AppSession _appSession;

        private readonly DiContext _diContext = DiContext.GetOrCreateInstance();

        protected override void Start()
        {
            DontDestroyOnLoad(this);
            InstallCamera();
            InstallAudioService();
            InstallLevelsData();
            InstallNavigator();
            _diContext.Register(updateService);
            _diContext.Register(backgroundService);
            var sceneLoader = new SceneLoader(updateService);

            InstallAnalytics();

            ISaveDataContainer dataContainer = new PlayerPrefsSaveDataContainer();
            _diContext.Register(dataContainer);
            _diContext.Register(new LevelsLoopProgress(dataContainer, levelsConfig.GetLevelsOrder()));


            _diContext.Register(sceneLoader);
            _diContext.Register<ICompliments>(compliments);
            _diContext.Register(new Pool<CellView>(levelsConfig.ACellView, transform));
            
            InstallFactories();
            
            InstallStateMachine();
            InstallTutorial();
            StartApplication();
        }

        private void InstallFactories()
        {
            _diContext.Register(new LevelFactory(_diContext));
            _diContext.Register(new LevelPresenterFactory(_diContext));
        }

        private void InstallTutorial()
        {
            _diContext.Register(tutorialPointer);
            ITutorialFactory factory = new TutorialFactory(_diContext);
            ITutorialStep[] steps = { factory.Create<InitialTutorialStep>() };
            var tutorialService = new TutorialService(_diContext.GetService<ISaveDataContainer>(), steps);
            _diContext.Register(tutorialService);
        }

        private void InstallStateMachine()
        {
            StateMachine appStateMachine = new StateMachine(new StateFactory(_diContext));
            _diContext.Register(appStateMachine);
        }

        private void InstallAnalytics()
        {
            IAnalytics analytics = new Analytics();
            analytics.AddLogger(new YandexMetricaLogger(AppMetrica.Instance));
            _diContext.Register(analytics);
        }

        private void StartApplication()
        {
            var appStateMachine = _diContext.GetService<StateMachine>();
            appStateMachine.Load();
            _appSession = new AppSession(appStateMachine);
            _appSession.StateMachine.Enter<InitialState>();
        }

        private void InstallCamera()
        {
            _diContext.Register(new CameraProvider());
        }

        private void InstallAudioService()
        {
            sound.Bootstrap();
            sound.PlayMusic(SoundNamesCollection.BackgroundMusic);
            _diContext.Register(sound);
        }

        private void InstallNavigator()
        {
            screenNavigator.Bootstrap();
            _diContext.Register(screenNavigator);
        }

        private void InstallLevelsData()
        {
            levelsConfig.Bootstrap();
            _diContext.Register(levelsConfig);
        }
    }
}