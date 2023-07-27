using System.Collections.Generic;
using JetBrains.Annotations;
using Ji2Core.Core.Audio;

namespace Client.Views
{
    [UsedImplicitly]
    public class SoundNamesCollection : ISoundNamesCollection
    {
        public const string BackgroundMusic = "BackgroundMusic";
        public const string ButtonTap = "ButtonTap";
        public const string Swap = "Swap";
        public const string TileSet = "TileSet";
        public const string TileTap = "TileTap";
        public const string Win = "Win";

        public IEnumerable<string> SoundsList
        {
            get
            {
                yield return BackgroundMusic;
                yield return ButtonTap;
                yield return Swap;
                yield return TileSet;
                yield return TileTap;
                yield return Win;
            }
        }
    }
}