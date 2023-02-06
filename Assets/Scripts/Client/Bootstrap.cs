using Client.States;
using Ji2Core.Core;
using Ji2Core.Core.Audio;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.States;
using Ji2Core.Core.UserInput;
using Ji2Core.Plugins.AppMetrica;
using UI.Background;
using UnityEngine;
using Client.Tutorial;
using Core.Compliments;
using Ji2.CommonCore.SaveDataContainer;
using Ji2.Models.Analytics;
using Ji2.Presenters.Tutorial;
using Ji2.UI;

namespace Client
{
    public class  Bootstrap : BootstraperBase
    {
        [SerializeField] private LevelsConfig levelsConfig;
        [SerializeField] private ScreenNavigator screenNavigator;
        [SerializeField] private BackgroundService backgroundService;
        [SerializeField] private UpdateService updateService;
        [SerializeField] private ImageCompliments compliments;
        [SerializeField] private AudioService audioService;
        [SerializeField] private TutorialPointerView tutorialPointer;
        [SerializeField] private LevelResultViewConfig levelResultViewConfig;
        

        private AppSession appSession;

        private readonly Context context = Context.GetInstance();

        protected override void Start()
        {
            DontDestroyOnLoad(this);
            InstallCamera();
            InstallAudioService();
            InstallLevelsData();
            InstallNavigator();
            InstallInputService();
            context.Register(updateService);
            context.Register(backgroundService);
            var sceneLoader = new SceneLoader(updateService);

            InstallAnalytics();

            ISaveDataContainer dataContainer = new PlayerPrefsSaveDataContainer();
            context.Register<ISaveDataContainer>(dataContainer);
            context.Register(levelResultViewConfig);
            context.Register(new LevelsLoopProgress(dataContainer, levelsConfig.GetLevelsOrder()));


            context.Register(sceneLoader);
            context.Register<ICompliments>(compliments);

            InstallStateMachine();
            InstallTutorial();
            StartApplication();
        }

        private void InstallTutorial()
        {
            context.Register(tutorialPointer);
            ITutorialFactory factory = new TutorialFactory(context);
            ITutorialStep[] steps = { factory.Create<InitialTutorialStep>() };
            var tutorialService =
                new TutorialService(context.GetService<ISaveDataContainer>(), steps);
            context.Register(tutorialService);
        }

        private void InstallStateMachine()
        {
            StateMachine appStateMachine = new StateMachine(new StateFactory(context));
            context.Register(appStateMachine);
        }

        private void InstallAnalytics()
        {
            var analytics = new Analytics();
            analytics.AddLogger(new YandexMetricaLogger(AppMetrica.Instance));
            context.Register(analytics);
        }

        private void StartApplication()
        {
            var appStateMachine = context.GetService<StateMachine>();
            appStateMachine.Load();
            appSession = new AppSession(appStateMachine);
            appSession.StateMachine.Enter<InitialState>();
        }

        private void InstallCamera()
        {
            context.Register(new CameraProvider());
        }

        private void InstallInputService()
        {
            context.Register(new InputService(updateService));
        }

        private void InstallAudioService()
        {
            audioService.Bootstrap();
            audioService.PlayMusic(AudioClipName.DefaultBackgroundMusic);
            context.Register(audioService);
        }

        private void InstallNavigator()
        {
            screenNavigator.Bootstrap();
            context.Register(screenNavigator);
        }

        private void InstallLevelsData()
        {
            levelsConfig.Bootstrap();
            context.Register(levelsConfig);
        }
    }
}