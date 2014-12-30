using LeagueSharp.Common;

namespace FreeLux
{
    class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += FreeLux.Game_OnGameLoad;
        }
    }
}