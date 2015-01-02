#region Includes
using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
#endregion

namespace FreeLux
{
    internal static class FreeLux
    {
        private const string ChampionName = "Lux";
        private static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static SpellSlot IgniteSlot;
        public static Items.Item DeathfireGrasp;
        public static bool PacketCast;
        public static Menu Menu;

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampionName)
                return;

            /* Spells */
            Q = new Spell(SpellSlot.Q, 1175); // Light Binding
            W = new Spell(SpellSlot.W, 1075); // Prismatic Barrier
            E = new Spell(SpellSlot.E, 1100); // Lucient Singularity
            R = new Spell(SpellSlot.R, 3340); // Final Spark

            // Spell.SetSkillshot(float delay, float width, float speed, bool collision, SkillshotType type)
            Q.SetSkillshot(0.25f, 80f, 1200f, true, SkillshotType.SkillshotLine); // to get collision objects
            W.SetSkillshot(0.25f, 150f, 1200f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 275f, 1300f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1.35f, 190f, float.MaxValue, false, SkillshotType.SkillshotLine);

            /* Summoner Spells */
            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            /* Items */
            DeathfireGrasp = new Items.Item(3128, 750);

            /* Menu */
            #region Menu
            Menu = new Menu("Free" + ChampionName, ChampionName, true);

            Menu orbwalkerMenu = new Menu("集成 走砍", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            Menu targetSelectorMenu = new Menu("集成 目标选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Menu.AddSubMenu(targetSelectorMenu);

            Menu comboMenu = new Menu("连招", "Combo");
            comboMenu.AddItem(new MenuItem("comboQ", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboE", "使用 E").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboR", "使用 R").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboOnlyUltToKill", "只有能擊殺才是使用R").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboIgnite", "使用 點燃").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboDFG", "連招中使用冥火").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            Menu laneClearMenu = new Menu("清線", "Lane Clear");
            laneClearMenu.AddItem(new MenuItem("laneClearQ", "使用 Q").SetValue(false));
            laneClearMenu.AddItem(new MenuItem("laneClearE", "使用 E").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("laneClearENumber", "在小兵支持E的寬度最小使用E數量:").SetValue(new Slider(4, 1, 10)));
            laneClearMenu.AddItem(new MenuItem("laneClearMinMana", "清線最低消耗 %").SetValue(new Slider(60)));
            Menu.AddSubMenu(laneClearMenu);

            Menu mixedMenu = new Menu("騷擾/混合", "Mixed");
            mixedMenu.AddItem(new MenuItem("mixedQ", "使用 Q").SetValue(true));
            mixedMenu.AddItem(new MenuItem("mixedE", "使用 E").SetValue(false));
            mixedMenu.AddItem(new MenuItem("mixedQE", "如果能中使用EQ").SetValue(true));
            mixedMenu.AddItem(new MenuItem("mixedMinMana", "騷擾最低消耗 %").SetValue(new Slider(50)));
            Menu.AddSubMenu(mixedMenu);

             Menu autoShieldMenu = new Menu("自動護盾", "Auto Shield");
            autoShieldMenu.AddItem(new MenuItem("selfAutoShield", "自動護盾自己").SetValue(true));
            autoShieldMenu.AddItem(new MenuItem("selfAutoShieldPercentage", "自己自動護盾最低血量 %")).SetValue(new Slider(30));
            autoShieldMenu.AddItem(new MenuItem("selfAutoShieldMinMana", "自己自動護盾最低藍量 %")).SetValue(new Slider(20));
            autoShieldMenu.AddItem(new MenuItem("allyAutoShield", "自動護盾隊友").SetValue(new KeyBind('H', KeyBindType.Toggle)));
            autoShieldMenu.AddItem(new MenuItem("allyAutoShieldPercentage", "隊友使用自動護盾最低血量 %")).SetValue(new Slider(40));
            autoShieldMenu.AddItem(new MenuItem("allyAutoShieldMinMana", "隊友使用自動護盾最低藍量 %")).SetValue(new Slider(40));
            Menu.AddSubMenu(autoShieldMenu);

            Menu potionManagerMenu = new Menu("藥水管理","Potion Manager");
            PotionHelper.AddToMenu(potionManagerMenu);
            Menu.AddSubMenu(potionManagerMenu);

            Menu drawingMenu = new Menu("顯示範圍", "Drawing");
            drawingMenu.AddItem(new MenuItem("drawEnabled", "顯示範圍啟動").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawQ", "顯示 Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "顯示 W").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawE", "顯示 E").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawR", "顯示 R").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawMinimapR", "顯示R在小地圖").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawFullComboKillIndicator", "顯示連招擊殺指示").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawCurrentMode", "顯示當前模式").SetValue(true));
            Menu.AddSubMenu(drawingMenu);

            Menu otherMenu = new Menu("額外選項", "Other");
            otherMenu.AddItem(new MenuItem("packetCast", "使用釋放封包?").SetValue(false));
            otherMenu.AddItem(new MenuItem("RKillSteal", "搶人頭使用R").SetValue(true));
            otherMenu.AddItem(new MenuItem("killRecalling", "使用R擊殺回程的敵人").SetValue(true));;
            otherMenu.AddItem(new MenuItem("autoQGapcloser", "突進者自動使用Q中斷").SetValue(true));
            Menu.AddSubMenu(otherMenu);
            Menu.AddToMainMenu();
            #endregion

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;

            GameObject.OnCreate += Actions.GameObject_OnCreate;
            GameObject.OnDelete += Actions.GameObject_OnDelete;
            AntiGapcloser.OnEnemyGapcloser += Actions.AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnTeleport += Actions.Obj_AI_Base_OnTeleport;

            Game.PrintChat("FreeLux loaded. [By:  Feeeez]!");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            PacketCast = Menu.Item("packetCast").GetValue<bool>();
            

            if (Menu.Item("RKillSteal").GetValue<bool>()) Actions.KillSteal();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Actions.Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Actions.LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Actions.Mixed();
                    break;
            }

            // We don't want to interrupt our back ;)
            if (!Player.IsRecalling())
            {
                if (Menu.Item("allyAutoShield").GetValue<KeyBind>().Active)
                    Actions.AutoShieldAlly();
                if (Menu.Item("selfAutoShield").GetValue<bool>())
                    Actions.AutoShieldSelf();
            }

            Console.Clear();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead || !Menu.Item("drawEnabled").GetValue<bool>())
                return;

            bool drawQ = Menu.Item("drawQ").GetValue<bool>();
            bool drawW = Menu.Item("drawW").GetValue<bool>();
            bool drawE = Menu.Item("drawE").GetValue<bool>();
            bool drawR = Menu.Item("drawR").GetValue<bool>();
            bool drawCurrentMode = Menu.Item("drawCurrentMode").GetValue<bool>();

            Color color = Color.Green;
            Vector3 playerPosition = ObjectManager.Player.Position;
            var playerPositionOnScreen = Drawing.WorldToScreen(playerPosition);

            if (drawQ) Utility.DrawCircle(playerPosition, Q.Range, color);
            if (drawW) Utility.DrawCircle(playerPosition, W.Range, color);
            if (drawE) Utility.DrawCircle(playerPosition, E.Range, color);
            if (drawR) Utility.DrawCircle(playerPosition, R.Range, color);

            if (drawCurrentMode)
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 36, playerPositionOnScreen.Y + 41, Color.Black, "Mode: Combo");
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 35, playerPositionOnScreen.Y + 40, Color.Lime, "Mode: Combo");
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 36, playerPositionOnScreen.Y + 41, Color.Black, "Mode: Lane Clear");
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 35, playerPositionOnScreen.Y + 40, Color.Lime, "Mode: Lane Clear");
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 36, playerPositionOnScreen.Y + 41, Color.Black, "Mode: Mixed");
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 35, playerPositionOnScreen.Y + 40, Color.Lime, "Mode: Mixed");
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 36, playerPositionOnScreen.Y + 41, Color.Black, "Mode: Last Hit");
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 35, playerPositionOnScreen.Y + 40, Color.Lime, "Mode: Last Hit");
                        break;
                }
            }

            foreach (var h in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
            {
                var hero = h;
                var enemyPositionOnScreen = Drawing.WorldToScreen(hero.Position);
                if (!hero.IsDead)
                {
                    Drawing.DrawText(
                        enemyPositionOnScreen.X - 36, enemyPositionOnScreen.Y + 41, Color.Black,
                        MathHelper.GetDamageString(hero));
                    Drawing.DrawText(
                        enemyPositionOnScreen.X - 35, enemyPositionOnScreen.Y + 40, Color.OrangeRed,
                        MathHelper.GetDamageString(hero));
                }
            }
            
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            bool drawMinimapR = Menu.Item("drawMinimapR").GetValue<bool>();
            if (Player.Level >= 6 && drawMinimapR)
                Utility.DrawCircle(ObjectManager.Player.Position, R.Range, Color.DeepSkyBlue, 2, 30, true);
        }
    }
}
