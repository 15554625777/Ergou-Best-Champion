#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Advanced_Turn_Around
{
    internal class Program
    {
        private static readonly List<ChampionInfo> ExistingChampions = new List<ChampionInfo>();
        public static Menu Config;

        private static void Main(string[] args)
        {
            Game.OnGameStart += Game_OnGameStart;
        }

        private static void Game_OnGameStart(EventArgs args)
        {
            AddChampions();

            Config = new Menu("先进-转身", "ATA", true);

            Config.AddItem(new MenuItem("Enable", "启用").SetValue(true));

            Config.AddSubMenu(new Menu("英雄-法术", "CAS"));
            foreach (var champ in ExistingChampions)
            {
                Config.SubMenu("CAS").AddSubMenu(new Menu(champ.CharName + "'s Spells to Avoid", champ.CharName));
                Config.SubMenu("CAS")
                    .SubMenu(champ.CharName)
                    .AddItem(new MenuItem(champ.Key, champ.SpellName).SetValue(true));
            }

            Config.AddToMainMenu();

            Game.PrintChat(
                "<font color=\"#00BFFF\">鍏堣繘杞韩# -</font> <font color=\"#FFFFFF\">鍔犺浇鎴愬姛!姹夊寲by浜岀嫍!QQ缇361630847</font>");

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Config.Item("Enabled").GetValue<bool>() ||
                (ObjectManager.Player.ChampionName == "Teemo" && !ObjectManager.Player.IsTargetable))
            {
                return;
            }

            if (unit == null || unit.Team == ObjectManager.Player.Team)
            {
                return;
            }

            foreach (
                var champ in
                    ExistingChampions.Where(champ => Config.SubMenu(champ.CharName).Item(champ.Key).GetValue<bool>())
                        .Where(champ => args.SData.Name.Contains(champ.Key) &&
                                        (ObjectManager.Player.Distance(unit) <= champ.Range ||
                                         args.Target == ObjectManager.Player)))
            {
                Packet.C2S.Move.Encoded(
                    new Packet.C2S.Move.Struct(
                        ObjectManager.Player.Position.X +
                        ((unit.Position.X - ObjectManager.Player.Position.X)*(champ.Variable)/
                         ObjectManager.Player.Distance(unit)),
                        ObjectManager.Player.Position.Y +
                        ((unit.Position.Y - ObjectManager.Player.Position.Y)*(champ.Variable)/
                         ObjectManager.Player.Distance(unit)))).Send();
            }
        }

        private static void AddChampions()
        {
            ExistingChampions.Add(
                new ChampionInfo
                {
                    CharName = "蛇女",
                    Key = "CassiopeiaPetrifyingGaze",
                    Range = 750,
                    SpellName = "石化凝视 (R)",
                    Variable = -100
                });

            ExistingChampions.Add(
                new ChampionInfo
                {
                    CharName = "小丑",
                    Key = "TwoShivPoison",
                    Range = 625,
                    SpellName = "双面毒刃 (E)",
                    Variable = 100
                });

            ExistingChampions.Add(
                new ChampionInfo
                {
                    CharName = "蛮王",
                    Key = "MockingShout",
                    Range = 850,
                    SpellName = "藐视 (W)",
                    Variable = 100
                });
        }

        internal class ChampionInfo
        {
            public string CharName { get; set; }
            public string Key { get; set; }
            public float Range { get; set; }
            public string SpellName { get; set; }
            public int Variable { get; set; }
        }
    }
}