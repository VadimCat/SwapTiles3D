using Ji2Core.Core;
using Ji2Core.Core.SaveDataContainer;
using Ji2Core.Core.ScreenNavigation;

namespace Client.Utils
{
    public static class ContextExtensions
    {
        public static ScreenNavigator ScreenNavigator(this Context context)
        {
            return context.GetService<ScreenNavigator>();
        }
        
        public static SceneLoader SceneLoader(this Context context)
        {
            return context.GetService<SceneLoader>();
        }
        
        public static ISaveDataContainer ISaveDataContainer(this Context context)
        {
            return context.GetService<ISaveDataContainer>();
        }
        
        public static LevelsLoopProgress LevelsLoopProgress(this Context context)
        {
            return context.GetService<LevelsLoopProgress>();
        }
    }
}