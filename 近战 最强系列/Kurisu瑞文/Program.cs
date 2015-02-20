using System;
using LeagueSharp.Common;

namespace KurisuRiven
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            //Console.WriteLine("KurisuRiven injected..");
            CustomEvents.Game.OnGameLoad += Base.Initialize;
        }
    }
}