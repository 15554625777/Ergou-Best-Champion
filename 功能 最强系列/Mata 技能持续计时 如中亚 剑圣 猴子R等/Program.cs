#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Mata_View
{
    internal class Program
    {
       
        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Chat("1.1.1.3 [Jan, 25]");
            Menus.Menuadd();
            DetectObj.DetectObjload();

        }

        private static void Chat(string message)
        {
            Game.PrintChat("<font color=\"#4EE2EC\">Mata View</font> - " + message + " - <font color=\"#B6B6B4\">E2Slayer</font>");
        }

    }
}
