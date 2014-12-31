using System;
using System.Diagnostics;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;

namespace KaiHelper
{
    internal class Program
    {
        public static Menu Config;

        private static void Main(string[] args)
        {
            Config = new Menu("Kai 助手", "KaiHelp", true);
            new SkillBar(Config);
            new GankDetector(Config);
            new WardDetector(Config);
            new HealthTurret(Config);
            new JungleTimer(Config);
            Config.AddToMainMenu();
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            bool hasUpdate = Helper.HasNewVersion(Assembly.GetExecutingAssembly().GetName().Name);
            Game.PrintChat("-------------------------------------------------------------------------------------------"); 
            if (hasUpdate)
            {
                Game.PrintChat("<font color = \"#ff002b\">A new version of KaiHelper is available. Please check for updates!</font>");
            }
            Game.PrintChat("<font color = \"#00FF2B\">Kai 鍔╂墜</font> by <font color = \"#FD00FF\">kaigan</font>");
            Game.PrintChat(
                "<font color = \"#0092FF\">鎰熻濂界敤-璇锋崘璧爘 Paypal:</font> <font color = \"#F0FF00\">ntanphat2406@gmail.com</font>");
            Game.PrintChat("<font color = \"#00FF2B\">Kai 鍔╂墜  鍔犺級鎴愬姛锛佹饥鍖朾y浜岀嫍  浜岀嫍QQ缇361630847</font>");
            Game.PrintChat("-------------------------------------------------------------------------------------------"); 
        }
    }
}