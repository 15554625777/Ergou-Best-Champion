using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Reflection;
using System.Diagnostics;

namespace PerplexedEzreal
{
    class Program
    {
        static Obj_AI_Hero Player = ObjectManager.Player;
        static TargetSelector.DamageType DamageType;
        static System.Version Version = Assembly.GetExecutingAssembly().GetName().Version;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Ezreal")
                return;

            if (Updater.Outdated())
            {
                Game.PrintChat("<font color=\"#ff0000\">Perplexed 浼婃辰鐟炲皵 鐗堟湰杩囦箙! 璇锋洿鏂皘 {0}!</font>", Updater.GetLatestVersion());
                return;
            }

            SpellManager.Initialize();
            Config.Initialize();

            Utility.HpBarDamageIndicator.DamageToUnit = DamageCalc.GetDrawDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            CustomDamageIndicator.Initialize(DamageCalc.GetDrawDamage); //Credits to Hellsing for this! Borrowed it from his Kalista assembly.

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            Game.PrintChat("<font color=\"#ff3300\"><font color=\"#ff3300\">Perplexed 浼婃辰鐟炲皵 ({0})</font> - <font color=\"#ffffff\">鍔犺級鎴愬姛锛佹饥鍖朾y浜岀嫍锛丵Q缇361630847!</font> ({0})</font> - <font color=\"#ffffff\">鍔犺級鎴愬姛锛佹饥鍖朾y浜岀嫍锛丵Q缇361630847!</font>", Version);
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            DamageType = Config.DamageMode == "AD" ? TargetSelector.DamageType.Physical : TargetSelector.DamageType.Magical;
            if (Config.RecallBlock && (Player.HasBuff("Recall") || Player.IsWindingUp))
                return;
            if (Config.UltLowest.Active)
                UltLowest();
            switch (Config.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    if (Config.ToggleAuto.Active)
                        Auto();
                    break;
                default:
                    if(Config.ToggleAuto.Active)
                        Auto();
                    break;
            }
            KillSteal();
        }
        static void UltLowest()
        {
            if (SpellManager.R.IsReady())
            {
                var target = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(Config.UltRange) && hero.IsEnemy).OrderBy(hero => hero.Health).FirstOrDefault();
                Game.PrintChat("Ulting lowest health target: {0}...", target.ChampionName);
                SpellManager.CastSpell(SpellManager.R, target, HitChance.VeryHigh, Config.UsePackets);
            }
        }
        static void Combo()
        {
            if (Config.ComboQ && SpellManager.Q.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.Q.Range, DamageType);
                SpellManager.CastSpell(SpellManager.Q, target, HitChance.High, Config.UsePackets);
            }
            if (Config.ComboW && SpellManager.W.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.W.Range, DamageType);
                SpellManager.CastSpell(SpellManager.W, target, HitChance.High, Config.UsePackets);
            }
        }

        static void Harass()
        {
            if (Config.HarassQ && SpellManager.Q.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.Q.Range, DamageType);
                SpellManager.CastSpell(SpellManager.Q, target, HitChance.High, Config.UsePackets);
            }
            if (Config.HarassW && SpellManager.W.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.W.Range, DamageType);
                SpellManager.CastSpell(SpellManager.W, target, HitChance.High, Config.UsePackets);
            }
        }

        static void LastHit()
        {
            if (Config.LastHitQ && SpellManager.Q.IsReady())
            {
                var minions = MinionManager.GetMinions(Player.ServerPosition, SpellManager.Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                foreach (var minion in minions)
                {
                    if (!minion.IsValidTarget())
                        continue;
                    bool inAARange = Orbwalking.InAutoAttackRange(minion);
                    bool aaKillable = minion.Health <= Player.GetAutoAttackDamage(minion);
                    bool spellKillable = minion.Health <= Player.GetSpellDamage(minion, SpellManager.Q.Slot);
                    var regardlessMobs = minions.Where(mob => mob.Health <= Player.GetAutoAttackDamage(mob));
                    if (regardlessMobs.ToArray().Length > 1)
                    {
                        var regardlessMob = regardlessMobs.FirstOrDefault();
                        SpellManager.Q.Cast(regardlessMob);
                    }
                    else if ((spellKillable && !aaKillable && !inAARange))
                        SpellManager.Q.Cast(minion);
                }
            }
        }

        static void Auto()
        {
            if (Config.AutoQ && SpellManager.Q.IsReady())
            {
                if (Config.ManaER && (SpellManager.R.IsReady() || SpellManager.E.IsReady()) && ((Player.Mana - Player.Spellbook.GetSpell(SpellManager.Q.Slot).ManaCost) < Player.Spellbook.GetSpell(SpellManager.R.Slot).ManaCost) || (Player.Mana - Player.Spellbook.GetSpell(SpellManager.Q.Slot).ManaCost) < Player.Spellbook.GetSpell(SpellManager.E.Slot).ManaCost)
                    return;
                var target = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(SpellManager.Q.Range) && hero.IsEnemy && Config.ShouldAuto(hero.ChampionName)).FirstOrDefault();
                bool bothUnderTurret = target.UnderTurret(true) && Player.UnderTurret(true);
                if (bothUnderTurret)
                {
                    if (Config.AutoTurret)
                        SpellManager.CastSpell(SpellManager.Q, target, HitChance.High, Config.UsePackets);
                    else
                        return;
                }
                else
                    SpellManager.CastSpell(SpellManager.Q, target, HitChance.High, Config.UsePackets);
            }
            if (Config.AutoW && SpellManager.W.IsReady())
            {
                if (Config.ManaER && (SpellManager.R.IsReady() || SpellManager.E.IsReady()) && ((Player.Mana - Player.Spellbook.GetSpell(SpellManager.W.Slot).ManaCost) < Player.Spellbook.GetSpell(SpellManager.R.Slot).ManaCost) || (Player.Mana - Player.Spellbook.GetSpell(SpellManager.W.Slot).ManaCost) < Player.Spellbook.GetSpell(SpellManager.E.Slot).ManaCost)
                    return;
                var target = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(SpellManager.W.Range) && hero.IsEnemy && Config.ShouldAuto(hero.ChampionName)).FirstOrDefault();
                bool bothUnderTurret = target.UnderTurret(true) && Player.UnderTurret(true);
                if (bothUnderTurret)
                {
                    if (Config.AutoTurret)
                        SpellManager.CastSpell(SpellManager.W, target, HitChance.High, Config.UsePackets);
                    else
                        return;
                }
                else
                    SpellManager.CastSpell(SpellManager.W, target, HitChance.High, Config.UsePackets);
            }
        }

        static void KillSteal()
        {
            if (Config.KillSteal && SpellManager.R.IsReady())
            {
                var target = TargetSelector.GetTarget(Config.UltRange, DamageType);
                var ultDamage = DamageCalc.GetUltDamage(target);
                var targetHealth = target.Health;
                if (ultDamage >= targetHealth)
                    SpellManager.CastSpell(SpellManager.R, target, HitChance.VeryHigh, Config.UsePackets);
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.DrawQ)
                Utility.DrawCircle(Player.Position, SpellManager.Q.Range, Config.Settings.Item("drawQ").GetValue<Circle>().Color);
            if (Config.DrawW)
                Utility.DrawCircle(Player.Position, SpellManager.W.Range, Config.Settings.Item("drawW").GetValue<Circle>().Color);
            if (Config.DrawR)
                Utility.DrawCircle(Player.Position, Config.UltRange, Config.Settings.Item("drawR").GetValue<Circle>().Color);
        }
    }
}
