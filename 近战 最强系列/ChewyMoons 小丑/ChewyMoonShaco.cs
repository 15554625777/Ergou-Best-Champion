#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

#endregion

namespace ChewyMoonsShaco
{
    internal class ChewyMoonShaco
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList;
        public static Items.Item Tiamat;
        public static Items.Item Hydra;

        public static void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Shaco")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 425);
            E = new Spell(SpellSlot.E, 625);

            SpellList = new List<Spell> { Q, E, W };

            CreateMenu();

            Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            Hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();

            Game.OnGameUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += OrbwalkingOnAfterAttack;

            Game.PrintChat("<font color=\"#6699ff\"><b>ChewyMoon's 灏忎笐:</b></font> <font color=\"#FFFFFF\">" +
                           "鍔犺級鎴愬姛锛佹饥鍖朾y浜岀嫍锛丵Q缇361630847" +
                           "</font>");
        }

        private static void OrbwalkingOnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
            {
                return;
            }
            if (!(target is Obj_AI_Hero))
            {
                return;
            }

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Hydra.IsReady())
            {
                Hydra.Cast();
            }
            else if (Tiamat.IsReady())
            {
                Tiamat.Cast();
            }
        }

        private static void CreateMenu()
        {
            (Menu = new Menu("[Chewy's 小丑]", "cmShaco", true)).AddToMainMenu();

            // Target Selector
            var tsMenu = new Menu("目标 选择", "cmShacoTS");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalking
            var orbwalkingMenu = new Menu("走砍", "cmShacoOrbwalkin");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkingMenu);
            Menu.AddSubMenu(orbwalkingMenu);

            // Combo
            var comboMenu = new Menu("连招", "cmShacoCombo");
            comboMenu.AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "使用 W").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useItems", "使用 点燃").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("骚扰", "cmShacoHarass");
            harassMenu.AddItem(new MenuItem("useEHarass", "使用 E").SetValue(true));
            Menu.AddSubMenu(harassMenu);

            // Ks
            var ksMenu = new Menu("抢人头", "cmShacoKS");
            ksMenu.AddItem(new MenuItem("ksE", "使用 E").SetValue(true));
            Menu.AddSubMenu(ksMenu);

            // Drawing
            var drawingMenu = new Menu("范围", "cmShacoDrawing");
            drawingMenu.AddItem(new MenuItem("drawQ", "范围 Q").SetValue(new Circle(true, Color.Khaki)));
            drawingMenu.AddItem(new MenuItem("drawQPos", "范围 Q Pos").SetValue(new Circle(true, Color.Magenta)));
            drawingMenu.AddItem(new MenuItem("drawW", "范围 W").SetValue(new Circle(true, Color.Khaki)));
            drawingMenu.AddItem(new MenuItem("drawE", "范围 E").SetValue(new Circle(true, Color.Khaki)));
            Menu.AddSubMenu(drawingMenu);

            // Misc
            var miscMenu = new Menu("杂项", "cmShacoMisc");
            miscMenu.AddItem(new MenuItem("usePackets", "使用 封包").SetValue(true));
            miscMenu.AddItem(new MenuItem("stuff", "联系作者："));
            miscMenu.AddItem(new MenuItem("stuff2", "你需要添加的内容"));
            miscMenu.AddItem(new MenuItem("stuff3", "联系方式thead或IRC"));
            Menu.AddSubMenu(miscMenu);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var qCircle = Menu.Item("drawQ").GetValue<Circle>();
            var wCircle = Menu.Item("drawW").GetValue<Circle>();
            var eCircle = Menu.Item("drawE").GetValue<Circle>();
            var qPosCircle = Menu.Item("drawQPos").GetValue<Circle>();

            var pos = ObjectManager.Player.Position;

            if (qCircle.Active)
            {
                Render.Circle.DrawCircle(pos, Q.Range, qCircle.Color);
            }

            if (wCircle.Active)
            {
                Render.Circle.DrawCircle(pos, W.Range, wCircle.Color);
            }

            if (eCircle.Active)
            {
                Render.Circle.DrawCircle(pos, E.Range, eCircle.Color);
            }

            if (!qPosCircle.Active)
            {
                return;
            }
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget()))
            {
                Drawing.DrawLine(
                    Drawing.WorldToScreen(enemy.Position), Drawing.WorldToScreen(ShacoUtil.GetQPos(enemy, false)), 2,
                    qPosCircle.Color);
            }
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            if (Menu.Item("ksE").GetValue<bool>())
            {
                KillSecure();
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }
        }

        private static void KillSecure()
        {
            if (!E.IsReady())
            {
                return;
            }

            foreach (var target in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsEnemy)
                    .Where(x => !x.IsDead)
                    .Where(x => x.Distance(ObjectManager.Player) <= E.Range)
                    .Where(target => ObjectManager.Player.GetSpellDamage(target, SpellSlot.E) > target.Health))
            {
                E.CastOnUnit(target, Menu.Item("usePackets").GetValue<bool>());
                return;
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            var useQ = Menu.Item("useQ").GetValue<bool>();
            var useW = Menu.Item("useW").GetValue<bool>();
            var useE = Menu.Item("useE").GetValue<bool>();
            var packets = Menu.Item("usePackets").GetValue<bool>();

            foreach (var spell in SpellList.Where(x => x.IsReady()))
            {
                if (spell.Slot == SpellSlot.Q && useQ)
                {
                    if (!target.IsValidTarget(Q.Range))
                    {
                        continue;
                    }

                    var pos = ShacoUtil.GetQPos(target, true);
                    Q.Cast(pos, packets);
                }

                if (spell.Slot == SpellSlot.W && useW)
                {
                    //TODO: Make W based on waypoints
                    if (!target.IsValidTarget(W.Range))
                    {
                        continue;
                    }

                    var pos = ShacoUtil.GetQPos(target, true, 100);
                    W.Cast(pos, packets);
                }

                if (spell.Slot != SpellSlot.E || !useE)
                {
                    continue;
                }
                if (!target.IsValidTarget(E.Range))
                {
                    continue;
                }

                E.CastOnUnit(target);
            }
        }

        private static void Harass()
        {
            var useE = Menu.Item("useEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (!target.IsValidTarget(E.Range))
            {
                return;
            }

            if (useE && E.IsReady())
            {
                E.CastOnUnit(target);
            }
        }
    }
}