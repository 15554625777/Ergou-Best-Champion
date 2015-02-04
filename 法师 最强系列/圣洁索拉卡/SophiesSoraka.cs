#region

using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Sophies_Soraka
{
    internal class SophiesSoraka
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;

        public static bool Packets
        {
            get { return Menu.Item("packets").GetValue<bool>(); }
        }

        public static void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Soraka")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 550);
            E = new Spell(SpellSlot.E, 925);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.5f, 300, 1750, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 70f, 1750, false, SkillshotType.SkillshotCircle);

            CreateMenu();

            PrintChat("Loaded ! Definitely created by Sophie AND NOT CHEWYMOON :3");


            Interrupter2.OnInterruptableTarget += InterrupterOnOnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Game.OnGameUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Menu.Item("drawQ").GetValue<bool>();
            var drawW = Menu.Item("drawW").GetValue<bool>();
            var drawE = Menu.Item("drawE").GetValue<bool>();

            var p = ObjectManager.Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawW)
            {
                Render.Circle.DrawCircle(p, W.Range, W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
            }

            if (Menu.Item("autoW").GetValue<bool>())
            {
                AutoW();
            }

            if (Menu.Item("autoR").GetValue<bool>())
            {
                AutoR();
            }
        }

        private static void AutoR()
        {
            if (!R.IsReady())
            {
                return;
            }

            foreach (var friend in
                ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly).Where(x => !x.IsDead).Where(x => !x.IsZombie))
            {
                var friendHealth = (int) friend.Health / friend.MaxHealth * 100;
                var health = Menu.Item("autoRPercent").GetValue<Slider>().Value;

                if (friendHealth <= health)
                {
                    R.Cast(Packets);
                }
            }
        }

        private static void AutoW()
        {
            if (!W.IsReady())
            {
                return;
            }

            var autoWHealth = Menu.Item("autoWHealth").GetValue<Slider>().Value;
            if (ObjectManager.Player.HealthPercentage() < autoWHealth)
            {
                return;
            }

            foreach (var friend in
                from friend in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => !x.IsEnemy)
                        .Where(x => !x.IsMe)
                        .Where(friend => W.IsInRange(friend.ServerPosition, W.Range))
                let friendHealth = friend.Health / friend.MaxHealth * 100
                let healthPercent = Menu.Item("autoWPercent").GetValue<Slider>().Value
                where friendHealth <= healthPercent
                select friend)
            {
                W.CastOnUnit(friend, Packets);
            }
        }

        private static void Combo()
        {
            var useQ = Menu.Item("useQ").GetValue<bool>();
            var useE = Menu.Item("useE").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target == null)
            {
                return;
            }

            if (useQ && Q.IsReady())
            {
                Q.Cast(target, Packets);
            }

            if (useE && E.IsReady())
            {
                E.Cast(target, Packets);
            }
        }

        private static void Harass()
        {
            var useQ = Menu.Item("useQHarass").GetValue<bool>();
            var useE = Menu.Item("useEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target == null)
            {
                return;
            }

            if (useQ && Q.IsReady())
            {
                Q.Cast(target, Packets);
            }

            if (useE && E.IsReady())
            {
                E.Cast(target, Packets);
            }
        }

        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var unit = gapcloser.Sender;

            if (Menu.Item("useQGapcloser").GetValue<bool>() && unit.IsValidTarget(Q.Range) && Q.IsReady())
            {
                Q.Cast(unit, Packets);
            }

            if (Menu.Item("useEGapcloser").GetValue<bool>() && unit.IsValidTarget(E.Range) && E.IsReady())
            {
                E.Cast(unit, Packets);
            }
        }

        private static void InterrupterOnOnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            var unit = sender;
            var spell = args;

            if (Menu.Item("eInterrupt").GetValue<bool>() == false || spell.DangerLevel != Interrupter2.DangerLevel.High)
            {
                return;
            }

            if (!unit.IsValidTarget(E.Range))
            {
                return;
            }
            if (!E.IsReady())
            {
                return;
            }

            E.Cast(unit, Packets);
        }

        private static void CreateMenu()
        {
            Menu = new Menu("花边汉化-圣洁索拉卡", "sSoraka", true);

            // Target Selector
            var tsMenu = new Menu("目标 选择", "ssTS");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalking
            var orbwalkingMenu = new Menu("走 砍", "ssOrbwalking");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkingMenu);
            Menu.AddSubMenu(orbwalkingMenu);

            // Combo
            var comboMenu = new Menu("连 招", "ssCombo");
            comboMenu.AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("骚 扰", "ssHarass");
            harassMenu.AddItem(new MenuItem("useQHarass", "使用 Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("useEHarass", "使用 E").SetValue(true));
            Menu.AddSubMenu(harassMenu);

            // Drawing
            var drawingMenu = new Menu("范 围", "ssDrawing");
            drawingMenu.AddItem(new MenuItem("drawQ", "Q 范围").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "W 范围").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawE", "E 范围").SetValue(true));
            Menu.AddSubMenu(drawingMenu);

            // Misc
            var miscMenu = new Menu("杂 项", "ssMisc");
            miscMenu.AddItem(new MenuItem("packets", "使用 封包").SetValue(true));
            miscMenu.AddItem(new MenuItem("useQGapcloser", "使用Q 丨被突进").SetValue(true));
            miscMenu.AddItem(new MenuItem("useEGapcloser", "使用E 丨被突进").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoW", "自动 使用 W").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoWPercent", "血量 百分比").SetValue(new Slider(50, 1)));
            miscMenu.AddItem(new MenuItem("autoWHealth", "自己 血量百分比").SetValue(new Slider(30, 1)));
            miscMenu.AddItem(new MenuItem("autoR", "自动 使用 R").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoRPercent", "血量 百分比").SetValue(new Slider(15, 1)));
            miscMenu.AddItem(new MenuItem("eInterrupt", "使用 E 打断法术").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            Menu.AddToMainMenu();
        }

        public static void PrintChat(string msg)
        {
            Game.PrintChat("<font color='#3492EB'>Sophie's Soraka:</font> <font color='#FFFFFF'>" + msg + "</font>");
        }
    }
}