using System.Collections.Generic;
using JetBrains.Annotations;
using Ji2Core.Core.Audio;

namespace Client
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

        public IEnumerable<string> SoundsList => _names;

        private readonly string[] _names =
        {
            BackgroundMusic,
            ButtonTap,
            Swap,
            TileSet,
            TileTap,
            Win
        };
    }
}