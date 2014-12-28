using System.Linq;

#region

using System;
using System.Collections.Generic;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
#endregion

//Credits: TC Crew :^ )

namespace Katarina
{
    internal class Program
    {
        public const string ChampionName = "Katarina";

        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot IgniteSlot;

        public static Items.Item DFG;

        public static Menu Config;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 675);
            W = new Spell(SpellSlot.W, 375);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 500);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline || Utility.Map.GetMap()._MapType == Utility.Map.MapType.CrystalScar ? new Items.Item(3188, 750) : new Items.Item(3128, 750);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Config = new Menu(ChampionName, ChampionName, true);

            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));

            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("Killsteal", " 启动抢人头").SetValue(false));
            Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboActive", "连招!").SetValue(
                        new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));


            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "使用 W").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(false));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("HarassActive", "骚扰!").SetValue(new KeyBind(88, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("清线", "Farm"));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseQFarm", "使用 Q").SetValue(
                        new StringList(new[] { "控线", "清线", "同时", "禁止" }, 2)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseWFarm", "使用 W").SetValue(
                        new StringList(new[] { "控线", "清线", "同时", "禁止" }, 2)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("FreezeActive", "控线!").SetValue(
                        new KeyBind(Config.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("LaneClearActive", "清线!").SetValue(
                        new KeyBind(Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "显示连招伤害").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            Config.AddSubMenu(new Menu("范围", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("WRange", "W 范围").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("RRange", "R 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings")
                .AddItem(dmgAfterComboItem);
            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.PrintChat(ChampionName + " Tc鍗＄壒鐞冲涓ㄥ姞杞芥垚鍔燂紒姹夊寲by Bbyyyyy锛丵Q缇や辅3161630847!");
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();

                if (menuItem.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("Killsteal").GetValue<bool>())
                ExecuteKillsteal();

            if (ObjectManager.Player.IsChannelingImportantSpell()) return;

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active ||
                Config.Item("HarassActive").GetValue<KeyBind>().Active)
                ExecuteSkills();

            var lc = Config.Item("LaneClearActive").GetValue<KeyBind>().Active;
            if (lc || Config.Item("FreezeActive").GetValue<KeyBind>().Active)
                Farm(lc);
        }


        private static void ExecuteKillsteal()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;

            if (E.IsReady() && E.IsKillable(target, 1) &&
                ObjectManager.Player.Distance(target, false) < E.Range + target.BoundingRadius)
                E.CastOnUnit(target, true);

            if (Q.IsReady() && Q.IsKillable(target, 1) &&
                ObjectManager.Player.Distance(target, false) < Q.Range + target.BoundingRadius)
                Q.CastOnUnit(target, true);

            if (W.IsReady() && W.IsKillable(target) &&
                ObjectManager.Player.Distance(target, false) < W.Range)
                W.Cast();

            if (Q.IsReady() && E.IsReady() &&
                ObjectManager.Player.IsKillable(target,
                    new[] {Tuple.Create(SpellSlot.Q, 1), Tuple.Create(SpellSlot.E, 0)}) &&
                ObjectManager.Player.Distance(target, false) < Q.Range + target.BoundingRadius)
            {
                Q.CastOnUnit(target, true);
                E.CastOnUnit(target, true);
            }

            if (Q.IsReady() && E.IsReady() && W.IsReady() &&
                ObjectManager.Player.IsKillable(target,
                    new[] {Tuple.Create(SpellSlot.Q, 0), Tuple.Create(SpellSlot.E, 0), Tuple.Create(SpellSlot.W, 0)}) &&
                ObjectManager.Player.Distance(target, false) < Q.Range + target.BoundingRadius)
            {
                Q.Cast(target);
                E.Cast(target);
                if (ObjectManager.Player.Distance(target, false) < W.Range)
                {
                    W.Cast();
                }
            }

            if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && ObjectManager.Player.Distance(target, false) < 600)
            {
                if (ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                {
                    ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
                }
            }
        }

        private static void ExecuteSkills()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;

            if ((GetComboDamage(target) > target.Health))
            {
                if (DFG.IsReady() && ObjectManager.Player.Distance(target, false) < DFG.Range + target.BoundingRadius)
                    DFG.Cast(target);

                if (Q.IsReady() && ObjectManager.Player.Distance(target, false) < Q.Range + target.BoundingRadius)
                    Q.CastOnUnit(target, true);

                if (E.IsReady() && ObjectManager.Player.Distance(target, false) < E.Range + target.BoundingRadius)
                    E.CastOnUnit(target, true);

                if (W.IsReady() && ObjectManager.Player.Distance(target, false) < W.Range)
                    W.Cast();

                if (R.IsReady() && ObjectManager.Player.Distance(target, false) < R.Range )
                    R.Cast();

                if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                    ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);

            }
            else if (!(GetComboDamage(target) > target.Health))
            {
                if (Q.IsReady() && ObjectManager.Player.Distance(target, false) < Q.Range)
                    Q.CastOnUnit(target, true);

                if (Config.Item("ComboActive").GetValue<KeyBind>().Active &&
                    E.IsReady() && ObjectManager.Player.Distance(target, false) < E.Range)
                    E.CastOnUnit(target, true);

                if (W.IsReady() && ObjectManager.Player.Distance(target, false) < W.Range)
                    W.Cast();
            }
        }

        private static void Farm(bool laneClear)
        {
            if (!Orbwalking.CanMove(40)) return;
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var useQi = Config.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
            var useWi = Config.Item("UseWFarm").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useW = (laneClear && (useWi == 1 || useWi == 2)) || (!laneClear && (useWi == 0 || useWi == 2));

            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions.Where(minion => minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(ObjectManager.Player.Distance(minion, false) * 1000 / 1400))
                < 0.75 * ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q, 1)))
                {
                    Q.Cast(minion);
                    return;
                }
            }
            else if (useW && W.IsReady())
            {
                if (!allMinions.Any(minion => minion.IsValidTarget(W.Range) && minion.Health < 0.75 * ObjectManager.Player.GetSpellDamage(minion, SpellSlot.W))) return;
                W.Cast();
                return;
            }

            if (!laneClear) return;
            foreach (var minion in allMinions)
            {
                if (useQ)
                    Q.Cast(minion);

                if (useW && ObjectManager.Player.Distance(minion, false) < W.Range)
                    W.Cast(minion);
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady(420))
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (DFG.IsReady())
                damage += ObjectManager.Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (W.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E);

            if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            if (R.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R, 1)*8;
            return (float)damage * (DFG.IsReady() ? 1.2f : 1);
        }
    }
}