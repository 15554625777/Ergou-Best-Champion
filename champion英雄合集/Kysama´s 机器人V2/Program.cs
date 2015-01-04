#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Blitzcrank
{
    class Program
    {
        public const string ChampionName = "Blitzcrank";


        public static Orbwalking.Orbwalker Orbwalker;


        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell E;
        public static Spell R;


        public static Menu Config;

        public static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != ChampionName) return;


            Q = new Spell(SpellSlot.Q, 950);
            E = new Spell(SpellSlot.E, Player.AttackRange + 50);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(E);
            SpellList.Add(R);


            Config = new Menu(ChampionName, ChampionName, true);


            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));


            var targetSelectorMenu = new Menu("目标选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);


            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));


            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "使用 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "使用 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "使用 R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("AutoUlt", "自动大招").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("NR", "N. 在敌人范围内自动大招").SetValue(new Slider(1, 5, 0)));

            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "连招键!").SetValue(new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));


            Config.AddSubMenu(new Menu("额外选项", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "自动打断有危险的法术").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("RF", "清线使用R").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealR", "抢人头使用R").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("Qhitch", "Q的命中率")).SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2));
            Config.SubMenu("Misc").AddItem(new MenuItem("APToggle", "自动E击飞").SetValue(true));
            Config.SubMenu("Misc").AddSubMenu(new Menu("Autopull", "自动击飞眩晕"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Config.SubMenu("Misc").SubMenu("AutoPull").AddItem(new MenuItem("AutoPull" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));


            Config.AddSubMenu(new Menu("Drawings", "显示范围"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q 范围").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R 范围").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.AddToMainMenu();



            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Game.PrintChat("<font color='#05F4F4'>Blitzcrank V2 by Kysama [By: Feeeez]  loaded!</font>");
        }


        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("InterruptSpells").GetValue<bool>()) return;

            if (Player.Distance(unit) < R.Range && R.IsReady())
            {
                R.Cast();
            }
        }

        private static void Combo()
        {
            Orbwalker.SetAttack(true);

            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            bool useQ = Config.Item("UseQ").GetValue<bool>();
            bool useE = Config.Item("UseE").GetValue<bool>();
            bool useR = Config.Item("UseR").GetValue<bool>();


            var qLow = Config.Item("Qhitch").GetValue<StringList>().SelectedIndex == 0;
            var qMedium = Config.Item("Qhitch").GetValue<StringList>().SelectedIndex == 1;
            var qHigh = Config.Item("Qhitch").GetValue<StringList>().SelectedIndex == 2;
            var qVeryHigh = Config.Item("Qhitch").GetValue<StringList>().SelectedIndex == 3;

            if (qTarget != null && useQ && Q.IsReady() && Player.Distance(qTarget) < Q.Range)
            {
                if (qLow)
                    Q.Cast(qTarget);
                else if (qMedium)
                    Q.CastIfHitchanceEquals(qTarget, HitChance.Medium);
                else if (qHigh)
                    Q.CastIfHitchanceEquals(qTarget, HitChance.High);
                else if (qVeryHigh)
                    Q.CastIfHitchanceEquals(qTarget, HitChance.VeryHigh);
            }


            if (eTarget != null && useE && E.IsReady())
            {
                if (eTarget.HasBuff("RocketGrab"))
                    E.Cast();
            }


            if (eTarget != null && useE && E.IsReady() && !Q.IsReady())
                E.Cast();


            if (rTarget != null && !Q.IsReady() && useR && R.IsReady())
                R.Cast(rTarget, false, true);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (Player.IsDead) return;
            Orbwalker.SetAttack(true);
            Orbwalker.SetMovement(true);


            var useRks = Config.Item("KillstealR").GetValue<bool>() && R.IsReady();
            var useRF = Config.Item("RF").GetValue<bool>() && R.IsReady();

            if (useRF)
                ULTF();
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();
            if (useRks)
                Killsteal();
            if (Config.Item("AutoUlt").GetValue<bool>() && Utility.CountEnemysInRange((int)R.Range) >= Config.Item("NR").GetValue<Slider>().Value && R.IsReady())
                R.Cast();
            if (Config.Item("APToggle").GetValue<bool>())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                {
                    if (Config.Item("AutoPull" + enemy.BaseSkinName).GetValue<bool>() && Q.IsReady())
                        //foreach (var buff in enemy.Buffs.Where(buff => (buff.Type == (BuffType.Stun) || buff.Type == BuffType.Knockup || buff.Type == BuffType.Snare || buff.Type == BuffType.Suppression)))
                        //    if (buff.EndTime == 0.3 + Q.Delay + (Player.Distance(enemy)/Q.Speed))
                        Q.CastIfHitchanceEquals(enemy, HitChance.Immobile);
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
        }

        private static void Killsteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range)))
            {
                if (R.IsReady() && hero.Distance(ObjectManager.Player) <= R.Range &&
                    Player.GetSpellDamage(hero, SpellSlot.R) >= hero.Health)
                    R.Cast();
            }
        }
        private static void ULTF()
        {

            int minionsInUltimateRange = MinionManager.GetMinions(Player.Position, R.Range).Count;

            if (minionsInUltimateRange > 10)
            {
                R.Cast();
            }
        }
    }
}