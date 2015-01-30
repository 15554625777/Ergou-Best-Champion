using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace BlackZilean
{
    internal class Program
    {
        // Generic
        public static readonly string ChampName = "Zilean";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        // Spells
        private static readonly List<Spell> SpellList = new List<Spell>();
        private static Spell _q, _w, _e, _r;
        private static SpellSlot _igniteSlot;
        // Menu
        public static Menu Menu;
        private static Orbwalking.Orbwalker _ow;

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
            _q = new Spell(SpellSlot.Q, 700);
            _w = new Spell(SpellSlot.W);
            _e = new Spell(SpellSlot.E, 700);
            _r = new Spell(SpellSlot.R, 900);
            SpellList.AddRange(new[] {_q, _e, _r});

            _igniteSlot = Player.GetSpellSlot("SummonerDot");

            // Create menu
            CreateMenu();

            // Register events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            // Print
            Game.PrintChat(
                String.Format("<font color='#08F5F8'>black鍩哄叞 -</font> <font color='#FFFFFF'>{0} 鍔犺級鎴愬姛锛佹饥鍖朾y浜岀嫍锛丵Q缇361630847!</font>",
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
                (ObjectManager.Player.Mana/ObjectManager.Player.MaxMana*100) >
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

            // AutoUlt
            if (Menu.SubMenu("ult").Item("ultUseR").GetValue<bool>())
            {
                AutoUlt();
            }

            // Misc
            if (Menu.SubMenu("misc").Item("miscEscapeToMouse").GetValue<KeyBind>().Active)
            {
                EscapeToMouse();
            }

            // Killsteal
            Killsteal(target);
        }

        private static void OnCombo(Obj_AI_Hero target)
        {
            var comboMenu = Menu.SubMenu("combo");
            var useQ = comboMenu.Item("comboUseQ").GetValue<bool>() && _q.IsReady();
            var useW = comboMenu.Item("comboUseW").GetValue<bool>() && _w.IsReady();
            var useE = comboMenu.Item("comboUseE").GetValue<bool>() && _e.IsReady();

            if (useQ && Player.Distance(target.Position) < _q.Range)
            {
                _q.Cast(target, Packets());
            }

            if (useW && !_q.IsReady())
            {
                _w.Cast(Player, Packets());
            }

            if (useE && Player.Distance(target.Position) < _e.Range)
            {
                _e.Cast(target, Packets());
            }

            if (useE && Player.Distance(target.Position) > _e.Range)
            {
                _e.Cast(Player, Packets());
            }

            if (target == null || !Menu.Item("miscIgnite").GetValue<bool>() || _igniteSlot == SpellSlot.Unknown ||
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
            var useW = harassMenu.Item("harassUseW").GetValue<bool>() && _w.IsReady();

            if (useQ && Player.Distance(target.Position) < _q.Range)
            {
                _q.Cast(target, Packets());
            }

            if (useW && !_q.IsReady())
            {
                _w.Cast(Player, Packets());
            }
        }

        private static void Killsteal(Obj_AI_Hero target)
        {
            var killstealMenu = Menu.SubMenu("killsteal");
            var useQ = killstealMenu.Item("killstealUseQ").GetValue<bool>() && _q.IsReady();
            var useW = killstealMenu.Item("killstealUseW").GetValue<bool>() && _w.IsReady();

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

            if (useW && !_q.IsReady())
            {
                _w.Cast(Player, Packets());
            }
        }

        private static void WaveClear()
        {
            var waveclearMenu = Menu.SubMenu("waveclear");
            var useQ = waveclearMenu.Item("wcUseQ").GetValue<bool>() && _q.IsReady();
            var useW = waveclearMenu.Item("wcUseW").GetValue<bool>() && _w.IsReady();

            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, _q.Range);

            if (useQ && allMinionsQ.Count > 2)
            {
                foreach (var minion in allMinionsQ.Where(minion => minion.IsValidTarget() &&
                                                                   _q.IsKillable(minion)))
                {
                    _q.CastOnUnit(minion, Packets());
                    return;
                }
            }

            if (useW && !_q.IsReady())
            {
                _w.Cast(Player, Packets());
            }

            var jcreeps = MinionManager.GetMinions(Player.ServerPosition, _e.Range, MinionTypes.All,
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

            if (useW && !_q.IsReady())
            {
                _w.Cast(Player, Packets());
            }
        }

        private static void AutoUlt()
        {
            if (!Menu.Item("ultUseR").GetValue<bool>())
            {
                return;
            }

            foreach (var aChamp in from aChamp in ObjectManager.Get<Obj_AI_Hero>()
                where (aChamp.IsAlly) && (ObjectManager.Player.ServerPosition.Distance(aChamp.Position) < _r.Range)
                where Menu.Item("Ult" + aChamp.BaseSkinName).GetValue<bool>() && _r.IsReady()
                where aChamp.Health < (aChamp.MaxHealth*(Menu.Item("ultPercent").GetValue<Slider>().Value*0.01))
                where (!aChamp.IsDead) && (!aChamp.IsInvulnerable)
                select aChamp)
            {
                _r.CastOnUnit(aChamp, Packets());
            }
        }

        private static void EscapeToMouse()
        {
            var miscMenu = Menu.SubMenu("misc");
            var useE = miscMenu.Item("miscUseE").GetValue<bool>() && _e.IsReady();

            if (useE)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                _e.Cast(Player, Packets());
            }
            else
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (_q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (_w.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
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

        private static void CreateMenu()
        {
            Menu = new Menu("Black基兰", "black" + ChampName, true);

            // Target selector
            var ts = new Menu("目标选择", "ts");
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
            combo.AddItem(new MenuItem("comboUseE", "使用 E").SetValue(true));
            combo.AddItem(new MenuItem("comboActive", "连招 键位!").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Harass
            var harass = new Menu("骚扰", "harass");
            Menu.AddSubMenu(harass);
            harass.AddItem(new MenuItem("harassUseQ", "使用 Q").SetValue(true));
            harass.AddItem(new MenuItem("harassUseW", "使用 W").SetValue(false));
            harass.AddItem(new MenuItem("harassMana", "骚扰最低蓝量").SetValue(new Slider(40, 100, 0)));
            harass.AddItem(new MenuItem("harassActive", "骚扰 键位!").SetValue(new KeyBind('C', KeyBindType.Press)));

            // WaveClear
            var waveclear = new Menu("清线", "waveclear");
            Menu.AddSubMenu(waveclear);
            waveclear.AddItem(new MenuItem("wcUseQ", "使用 Q").SetValue(true));
            waveclear.AddItem(new MenuItem("wcUseW", "使用 W").SetValue(true));
            waveclear.AddItem(new MenuItem("wcMana", "清线最低蓝量").SetValue(new Slider(40, 100, 0)));
            waveclear.AddItem(new MenuItem("wcActive", "清线 键位!").SetValue(new KeyBind('V', KeyBindType.Press)));

            // Killsteal
            var killsteal = new Menu("抢人头", "killsteal");
            Menu.AddSubMenu(killsteal);
            killsteal.AddItem(new MenuItem("killstealUseQ", "使用 Q").SetValue(true));
            killsteal.AddItem(new MenuItem("killstealUseW", "使用 W").SetValue(true));

            // Ult
            var ult = new Menu("R设置", "ult");
            Menu.AddSubMenu(ult);
            ult.AddItem(new MenuItem("ultUseR", "使用 R")).SetValue(true);
            ult.AddItem(new MenuItem("sep0", "========="));
            foreach (var Champ in ObjectManager.Get<Obj_AI_Hero>().Where(champ => champ.IsAlly))
            {
                ult.AddItem(
                    new MenuItem("Ult" + Champ.BaseSkinName, string.Format("Ult {0}", Champ.BaseSkinName)).SetValue(true));
            }
            ult.AddItem(new MenuItem("sep1", "========="));
            ult.AddItem(new MenuItem("ultPercent", "使用R最低血量")).SetValue(new Slider(25, 1));

            // Misc
            var misc = new Menu("杂项", "misc");
            Menu.AddSubMenu(misc);
            misc.AddItem(new MenuItem("miscPacket", "使用 封包").SetValue(true));
            misc.AddItem(new MenuItem("miscIgnite", "使用 点燃").SetValue(true));
            misc.AddItem(
                new MenuItem("miscEscapeToMouse", "逃跑键位").SetValue(new KeyBind('G', KeyBindType.Press)));
            misc.AddItem(new MenuItem("miscUseE", "使用E向鼠标方向连招").SetValue(true));

            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "显示组合连招伤害").SetValue(true);
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
            drawings.AddItem(new MenuItem("drawRangeQ", "Q / E 范围").SetValue(new Circle(true, Color.Aquamarine)));
            //drawings.AddItem(new MenuItem("drawRangeE", "E range").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeR", "R 范围").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(dmgAfterComboItem);

            // Finalizing
            Menu.AddToMainMenu();
        }
    }
}