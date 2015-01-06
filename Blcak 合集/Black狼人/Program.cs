using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace BlackWarwick
{
    internal class Program
    {
        // Generic
        public static readonly string ChampName = "Warwick";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        // Spells
        private static readonly List<Spell> SpellList = new List<Spell>();
        private static Spell _q, _w, _e, _r;
        private static SpellSlot _igniteSlot, _smiteSlot;
        // Menu
        public static Menu Menu;
        private static Orbwalking.Orbwalker _ow;
        //Credits to metaphorce
        public static readonly int[] SmiteRed = {3715, 3718, 3717, 3716, 3714};
        public static readonly int[] SmiteBlue = {3706, 3710, 3709, 3708, 3707};

        public static void Main(string[] args)
        {
            // Register events
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            //Champ validation
            if (Player.ChampionName != ChampName)
            {
                return;
            }

            //Define spells
            _q = new Spell(SpellSlot.Q, 400);
            _w = new Spell(SpellSlot.W, 1000);
            _e = new Spell(SpellSlot.E, 1500);
            _r = new Spell(SpellSlot.R, 700);
            SpellList.AddRange(new[] {_q, _w, _e, _r});

            _igniteSlot = Player.GetSpellSlot("SummonerDot");
            _smiteSlot = SpellSlot.Unknown;

            // Finetune spells
            _r.SetTargetted(0.5f, float.MaxValue);

            // Create menu
            CreateMenu();

            // Set smiteslot
            SetSmiteSlot();

            // Register events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            // Print
            Game.PrintChat(
                String.Format("Black鐙间汉涓ㄥ姞杞芥垚鍔燂紒姹夊寲by Bbyyyy锛丵Q缇や辅361630847",
                    ChampName));
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            // Spell ranges
            foreach (var spell in SpellList)
            {
                // Regular spell ranges
                var circleEntry = Menu.Item("drawRange" + spell.Slot).GetValue<Circle>();
                if (circleEntry.Active)
                {
                    Utility.DrawCircle(Player.Position, spell.Range, circleEntry.Color);
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);

            // Combo
            if (Menu.SubMenu("combo").Item("comboActive").GetValue<KeyBind>().Active)
            {
                OnCombo(target);
            }

            // Harass
            if (Menu.SubMenu("harass").Item("harassActive").GetValue<KeyBind>().Active &&
                (Player.Mana/Player.MaxMana*100) >
                Menu.Item("harassMana").GetValue<Slider>().Value)
            {
                OnHarass(target);
            }

            // WaveClear
            if (Menu.SubMenu("waveclear").Item("wcActive").GetValue<KeyBind>().Active &&
                (Player.Mana/Player.MaxMana*100) >
                Menu.Item("wcMana").GetValue<Slider>().Value)
            {
                WaveClear();
            }

            // Killsteal
            Killsteal(target);
        }

        private static void OnCombo(Obj_AI_Hero target)
        {
            var comboMenu = Menu.SubMenu("combo");
            var useQ = comboMenu.Item("comboUseQ").GetValue<bool>() && _q.IsReady();
            var useW = comboMenu.Item("comboUseW").GetValue<bool>() && _w.IsReady();
            var useR = comboMenu.Item("comboUseR").GetValue<bool>() && _r.IsReady();

            if (target.HasBuffOfType(BuffType.Invulnerability))
            {
                return;
            }

            if (useW && Player.Distance(target.Position) < _r.Range)
            {
                _w.Cast(Player, Packets());
            }

            if (Menu.Item("miscSmite").GetValue<bool>() && _smiteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready && useR &&
                Menu.Item("DontUlt" + target.BaseSkinName) != null &&
                Menu.Item("DontUlt" + target.BaseSkinName).GetValue<bool>() == false)
            {
                Player.Spellbook.CastSpell(_smiteSlot, target);
            }

            if (useR && Player.Distance(target.Position) < _r.Range)
            {
                if (Menu.Item("DontUlt" + target.BaseSkinName) != null &&
                    Menu.Item("DontUlt" + target.BaseSkinName).GetValue<bool>() == false)
                    _r.Cast(target, Packets());
            }

            if (useQ && Player.Distance(target.Position) < _q.Range)
            {
                _q.Cast(target, Packets());
            }

            if (!Menu.Item("miscIgnite").GetValue<bool>() || _igniteSlot == SpellSlot.Unknown ||
                Player.Spellbook.CanUseSpell(_igniteSlot) != SpellState.Ready)
            {
                return;
            }

            if (GetComboDamage(target) > target.Health)
            {
                Player.Spellbook.CastSpell(_igniteSlot, target);
            }
        }

        private static void OnHarass(Obj_AI_Hero target)
        {
            var harassMenu = Menu.SubMenu("harass");
            var useQ = harassMenu.Item("harassUseQ").GetValue<bool>() && _q.IsReady();
            if (target.HasBuffOfType(BuffType.Invulnerability))
            {
                return;
            }

            if (useQ && Player.Distance(target.Position) < _q.Range)
            {
                _q.Cast(target, Packets());
            }
        }

        private static void Killsteal(Obj_AI_Hero target)
        {
            var killstealMenu = Menu.SubMenu("killsteal");
            var useQ = killstealMenu.Item("killstealUseQ").GetValue<bool>() && _q.IsReady();
            var useR = killstealMenu.Item("killstealUseR").GetValue<bool>() && _r.IsReady();

            if (target.HasBuffOfType(BuffType.Invulnerability))
            {
                return;
            }

            if (useQ && target.Distance(Player.Position) < _q.Range)
            {
                if (_q.IsKillable(target))
                {
                    _q.Cast(target, Packets());
                }
            }

            if (!useR || !(target.Distance(Player.Position) < _r.Range))
            {
                return;
            }

            if (_r.IsKillable(target))
            {
                _r.Cast(target, Packets());
            }
        }

        private static void WaveClear()
        {
            var waveclearMenu = Menu.SubMenu("waveclear");
            var useQ = waveclearMenu.Item("wcUseQ").GetValue<bool>() && _q.IsReady();
            var useW = waveclearMenu.Item("wcUseW").GetValue<bool>() && _w.IsReady();

            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, _q.Range);

            if (useQ)
            {
                foreach (var minion in allMinionsQ.Where(minion => minion.IsValidTarget() &&
                                                                   _q.IsKillable(minion)))
                {
                    _q.CastOnUnit(minion, Packets());
                    return;
                }
            }

            if (useW && allMinionsQ.Count > 1)
            {
                _w.Cast(Player, Packets());
            }

            var jcreeps = MinionManager.GetMinions(Player.ServerPosition, _q.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (jcreeps.Count <= 0)
            {
                return;
            }

            var jcreep = jcreeps[0];

            if (useQ)
            {
                _q.Cast(jcreep, Packets());
            }

            if (useW)
            {
                _w.Cast(Player, Packets());
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            damage += Player.GetAutoAttackDamage(enemy);

            if (_r.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            if (_q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (_igniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                damage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float) damage;
        }

        private static bool Packets()
        {
            return Menu.Item("miscPacket").GetValue<bool>();
        }

        public static string SmiteType()
        {
            if (SmiteBlue.Any(itemId => Items.HasItem(itemId)))
            {
                return "s5_summonersmiteplayerganker";
            }

            return SmiteRed.Any(itemId => Items.HasItem(itemId)) ? "s5_summonersmiteduel" : "summonersmite";
        }

        public static void SetSmiteSlot()
        {
            foreach (
                var spell in
                    ObjectManager.Player.Spellbook.Spells.Where(
                        spell => String.Equals(spell.Name, SmiteType(), StringComparison.CurrentCultureIgnoreCase)))
            {
                _smiteSlot = spell.Slot;
                return;
            }
        }

        //Credits to metaphorce

        private static void CreateMenu()
        {
            Menu = new Menu("Black" + ChampName, "black" + ChampName, true);

            // Target selector
            var ts = new Menu("目标 选择", "ts");
            Menu.AddSubMenu(ts);
            TargetSelector.AddToMenu(ts);

            // Orbwalker
            var orbwalk = new Menu("走砍", "orbwalk");
            Menu.AddSubMenu(orbwalk);
            _ow = new Orbwalking.Orbwalker(orbwalk);

            // Combo
            var combo = new Menu("连招", "combo");
            Menu.AddSubMenu(combo);
            combo.AddItem(new MenuItem("comboUseQ", "使用 Q").SetValue(true));
            combo.AddItem(new MenuItem("comboUseW", "使用 W").SetValue(true));
            combo.AddItem(new MenuItem("comboUseR", "使用 R").SetValue(true));
            combo.AddItem(new MenuItem("comboActive", "启动 连招!").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Harass
            var harass = new Menu("骚扰", "harass");
            Menu.AddSubMenu(harass);
            harass.AddItem(new MenuItem("harassUseQ", "使用 Q").SetValue(true));
            harass.AddItem(new MenuItem("harassMana", "骚扰最低蓝量").SetValue(new Slider(40, 100, 0)));
            harass.AddItem(new MenuItem("harassActive", "启动 骚扰!").SetValue(new KeyBind('C', KeyBindType.Press)));

            // WaveClear
            var waveclear = new Menu("清线", "waveclear");
            Menu.AddSubMenu(waveclear);
            waveclear.AddItem(new MenuItem("wcUseQ", "使用 Q").SetValue(true));
            waveclear.AddItem(new MenuItem("wcUseW", "使用 W").SetValue(true));
            waveclear.AddItem(new MenuItem("wcMana", "清线最低蓝量").SetValue(new Slider(40, 100, 0)));
            waveclear.AddItem(new MenuItem("wcActive", "启动 清线!").SetValue(new KeyBind('V', KeyBindType.Press)));

            // Killsteal
            var killsteal = new Menu("抢人头", "killsteal");
            Menu.AddSubMenu(killsteal);
            killsteal.AddItem(new MenuItem("killstealUseQ", "使用 Q").SetValue(true));
            killsteal.AddItem(new MenuItem("killstealUseR", "使用 R").SetValue(false));

            // Misc
            var misc = new Menu("杂项", "misc");
            Menu.AddSubMenu(misc);
            misc.AddItem(new MenuItem("miscPacket", "使用 封包").SetValue(true));
            misc.AddItem(new MenuItem("miscSmite", "使用惩戒配合连招").SetValue(true));
            misc.AddItem(new MenuItem("miscIgnite", "使用 引燃").SetValue(true));
            misc.AddItem(new MenuItem("DontUlt", "禁止 使用 R"));
            misc.AddItem(new MenuItem("sep0", "========="));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
            {
                misc.AddItem(new MenuItem("DontUlt" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));
            }
            misc.AddItem(new MenuItem("sep1", "========="));


            //Damage after combo
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "显示连招后伤害").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            // Drawings
            var drawings = new Menu("范围", "drawings");
            Menu.AddSubMenu(drawings);
            drawings.AddItem(new MenuItem("drawRangeQ", "Q 范围").SetValue(new Circle(true, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeW", "W 范围").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeE", "E 范围").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeR", "R 范围").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(dmgAfterComboItem);

            // Finalizing
            Menu.AddToMainMenu();
        }
    }
}