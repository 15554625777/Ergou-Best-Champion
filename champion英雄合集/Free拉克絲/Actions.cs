using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;

namespace FreeLux
{
    static class Actions
    {
        private const string InternalPassiveName = "luxilluminatingfraulein";
        private const string InternalEName = "LuxLightstrike_tar_green";
        private const string InternalEName2 = "LuxLightstrike_tar_red";
        private const string InternalBindingName = "LuxLightBindingMis";
        private const string InternalIgniteName = "SummonerIgnite";
        private const int InternalIgniteRange = 600;
        private static GameObject _luxEObject;

        public static void Combo()
        {
            bool useQ, useE, useR, useDFG, useIgnite, onlyUseRToKill;
            useQ = FreeLux.Menu.Item("comboQ").GetValue<bool>();
            useE = FreeLux.Menu.Item("comboE").GetValue<bool>();
            useR = FreeLux.Menu.Item("comboR").GetValue<bool>();
            useDFG = FreeLux.Menu.Item("comboDFG").GetValue<bool>();
            useIgnite = FreeLux.Menu.Item("comboIgnite").GetValue<bool>();
            onlyUseRToKill = FreeLux.Menu.Item("comboOnlyUltToKill").GetValue<bool>();

            var autoAttackTarget =
                TargetSelector.GetTarget(GetOwnAutoAttackRange(), TargetSelector.DamageType.Magical);
            if (autoAttackTarget != null && autoAttackTarget.GetType() == typeof(Obj_AI_Hero) && HasIllumination(autoAttackTarget))
                return;
            
            if (useDFG && FreeLux.DeathfireGrasp.IsReady())
            {
                var t = TargetSelector.GetTarget(FreeLux.Q.Range, TargetSelector.DamageType.Magical) as Obj_AI_Hero;
                if (MathHelper.GetComboDamage(t) > t.Health && t.IsValidTarget(FreeLux.DeathfireGrasp.Range))
                    FreeLux.DeathfireGrasp.Cast(t);
            }
            
            if (useQ && FreeLux.Q.IsReady())
            {
                var t = TargetSelector.GetTarget(FreeLux.Q.Range, TargetSelector.DamageType.Magical) as Obj_AI_Hero;
                if (t == null)
                    return;
                CastQ(t);
                return;
            }

            if (useE && FreeLux.E.IsReady())
            {
                var t = TargetSelector.GetTarget(FreeLux.E.Range, TargetSelector.DamageType.Magical) as Obj_AI_Hero;
                if (EObjectExists())
                {
                    if (!ObjectManager.Get<Obj_AI_Hero>()
                            .Where(h => h.IsEnemy && !h.IsDead)
                            .Any(e => e.Distance(_luxEObject.Position) <= FreeLux.E.Width*2))
                        return;

                    if (t == null)
                        return;

                    // If they're in our AA range and don't have the illumination passive (we want to auto attack them first), pop E.
                    if (InAutoAttackRange(t) && !HasIllumination(t))
                    {
                        FreeLux.E.Cast(FreeLux.PacketCast);
                        return;
                    }
                    // If they aren't in our AA range, pop E
                    else if (!InAutoAttackRange(t))
                    {
                        FreeLux.E.Cast(FreeLux.PacketCast);
                        return;
                    }
                    return;
                }

                if (t == null)
                    return;
                // Automatically cast E on them if they're bound.
                if (HasLightBinding(t))
                {
                    if (FreeLux.E.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
                // We're holding down spacebar and our Q didn't land or it wasn't up, so try to cast it in a sensible place.
                else if (t.IsValidTarget())
                {
                    //FreeLux.E.CastIfHitchanceEquals(t, HitChance.High, FreeLux.PacketCast);
                    CastE(t);
                    return;
                }
            }

            if (useIgnite && FreeLux.IgniteSlot.IsReady())
            {
                var igniteTarget = TargetSelector.GetTarget(InternalIgniteRange, TargetSelector.DamageType.True) as Obj_AI_Hero;
                if (igniteTarget == null)
                    return;
                // First, check to see if we can kill them with ignite (so we can save our ult)
                if (IsIgniteKillable(FreeLux.Player, igniteTarget))
                {
                    FreeLux.Player.Spellbook.CastSpell(FreeLux.IgniteSlot, igniteTarget);
                    return;
                }
                // Then, check to see if we can kill them with Ignite and then our ult, and do so if we can.
                else if (MathHelper.GetIgniteDamage(igniteTarget) + MathHelper.GetRDamage(igniteTarget) > igniteTarget.Health && useR && FreeLux.R.IsReady())
                {
                    FreeLux.Player.Spellbook.CastSpell(FreeLux.IgniteSlot, igniteTarget);
                    CastR(igniteTarget);
                    return;
                }
            }

            if (useR && FreeLux.R.IsReady())
            {
                // Remember, R automatically pops the illumination passive's damage on a target and then applies it again.
                var rTarget =
                    TargetSelector.GetTarget(FreeLux.R.Range, TargetSelector.DamageType.Magical) as Obj_AI_Hero;
                if (rTarget == null)
                    return;
                // Only cast R if it will kill them (include procing the passive in the damage that is applied in 'killing them')
                // Also, make sure that the damage from the passive wouldn't do enough by itself to kill them
                // And of course, only include the passive damage if they're in range to be auto attacked.
                if (MathHelper.GetRDamage(rTarget) + ((HasIllumination(rTarget) && InAutoAttackRange(rTarget)) ? MathHelper.GetPassiveProcDamage() : 0.0d) >= rTarget.Health &&
                   ((HasIllumination(rTarget) && InAutoAttackRange(rTarget)) ? MathHelper.GetPassiveProcDamage() : 0.0d) >= rTarget.Health &&
                   !IsIgnited(rTarget) &&
                   onlyUseRToKill)
                {
                    CastR(rTarget);
                    return;
                }
                else if (!onlyUseRToKill)
                {
                    CastRForAoeDamage(rTarget);
                    return;
                }
            }

        }

        public static void Mixed()
        {
            bool useQ, useE, useQE;
            useQ = FreeLux.Menu.Item("mixedQ").GetValue<bool>();
            useE = FreeLux.Menu.Item("mixedE").GetValue<bool>();
            useQE = FreeLux.Menu.Item("mixedQE").GetValue<bool>();
            int mixedMinMana = FreeLux.Menu.Item("mixedMinMana").GetValue<Slider>().Value;

            var target = TargetSelector.GetTarget(FreeLux.E.Range, TargetSelector.DamageType.Magical) as Obj_AI_Hero;

            if (target == null)
                return;

            if (EObjectExists())
            {
                FreeLux.E.Cast(FreeLux.PacketCast);
                return;
            }

            if (FreeLux.Player.ManaPercentage() <= mixedMinMana)
                return;
            if (HasIllumination(target))
                return;

            /*if (HasIllumination(target) && Orbwalking.InAutoAttackRange(target))
                FreeLux.Player.IssueOrder(GameObjectOrder.AttackUnit, target);*/

            if (useQ && FreeLux.Q.IsReady() && target.IsValidTarget(FreeLux.Q.Range))
            {
                FreeLux.Q.CastIfHitchanceEquals(target, HitChance.VeryHigh, FreeLux.PacketCast);
                return;
            }


            if (useQE && HasLightBinding(target) && FreeLux.E.IsReady() && target.IsValidTarget(FreeLux.E.Range))
            {
                FreeLux.E.CastIfHitchanceEquals(target, HitChance.High, FreeLux.PacketCast);
                if (EObjectExists())
                {
                    FreeLux.E.Cast(FreeLux.PacketCast);
                    return;
                }
            }
            else if (useE && !useQE && FreeLux.E.IsReady() && target.IsValidTarget(FreeLux.E.Range))
            {
                FreeLux.E.CastIfHitchanceEquals(target, HitChance.High, FreeLux.PacketCast);
                return;
            }

        }

        public static void KillSteal()
        {
            if (FreeLux.R.IsReady())
            {
                var ksTargets =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(a => a.IsEnemy && a.Distance(FreeLux.Player.Position) < FreeLux.R.Range)
                        .OrderBy(a => a.HealthPercentage());
                        //.FirstOrDefault();
                foreach (var target in ksTargets)
                {
                    if (MathHelper.GetRDamage(target) >= target.Health && !IsIgnited(target))
                    {
                        CastR(target);
                        return;
                    }
                }
            }
        }

        public static void LaneClear()
        {
            /*if (!Orbwalking.CanMove(50))
                return;*/
            

            bool useQ, useE;
            useQ = FreeLux.Menu.Item("laneClearQ").GetValue<bool>();
            useE = FreeLux.Menu.Item("laneClearE").GetValue<bool>();
            int useENumber = FreeLux.Menu.Item("laneClearENumber").GetValue<Slider>().Value;

            var allMinionsQ = MinionManager.GetMinions(
                FreeLux.Player.ServerPosition, FreeLux.Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            var rangedMinionsE = MinionManager.GetMinions(
                FreeLux.Player.ServerPosition, FreeLux.E.Range + FreeLux.E.Width, MinionTypes.Ranged,
                MinionTeam.NotAlly);
            var allMinionsE = MinionManager.GetMinions(
                FreeLux.Player.ServerPosition, FreeLux.E.Range + FreeLux.E.Width, MinionTypes.All,
                MinionTeam.NotAlly);

            var minionsAutoAttack = MinionManager.GetMinions(
                FreeLux.Player.Position, GetOwnAutoAttackRange(), MinionTypes.All, MinionTeam.NotAlly);
            foreach (var minion in minionsAutoAttack)
            {
                if (HasIllumination(minion))
                    FreeLux.Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
            }

            if (EObjectExists())
            {
                 FreeLux.E.Cast(FreeLux.PacketCast);
            }

            if (FreeLux.Player.ManaPercentage() <= FreeLux.Menu.Item("laneClearMinMana").GetValue<Slider>().Value)
                return;

            if (useE && FreeLux.E.IsReady() && !EObjectExists())
            {
                var pos1 = FreeLux.E.GetCircularFarmLocation(rangedMinionsE, FreeLux.E.Width);
                var pos2 = FreeLux.E.GetCircularFarmLocation(allMinionsE, FreeLux.E.Width);
                if (pos1.MinionsHit >= useENumber)
                    FreeLux.E.Cast(pos1.Position, FreeLux.PacketCast);
                else if (pos2.MinionsHit >= useENumber)
                    FreeLux.E.Cast(pos2.Position, FreeLux.PacketCast);
            }
            else if (useQ && FreeLux.Q.IsReady())
            {
                var pos = FreeLux.Q.GetLineFarmLocation(allMinionsQ, FreeLux.Q.Width);
                if (pos.MinionsHit == 2)
                    FreeLux.Q.Cast(pos.Position, FreeLux.PacketCast);
            }
        }

        public static void AutoShieldAlly()
        {
            int autoShieldPercentage = FreeLux.Menu.Item("allyAutoShieldPercentage").GetValue<Slider>().Value;
            int autoShieldMinMana = FreeLux.Menu.Item("allyAutoShieldMinMana").GetValue<Slider>().Value;
            if (FreeLux.W.IsReady() &&
                FreeLux.Player.ManaPercentage() >= autoShieldMinMana)
            {
                var leastHealthAllyInRange =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(a => a.IsAlly && a.Distance(FreeLux.Player.Position) < FreeLux.W.Range)
                        .OrderBy(a => a.HealthPercentage())
                        .FirstOrDefault();

                if (leastHealthAllyInRange != null && leastHealthAllyInRange.Health < autoShieldPercentage)
                    FreeLux.W.Cast(leastHealthAllyInRange, FreeLux.PacketCast);
            }
        }

        public static void AutoShieldSelf()
        {
            int autoShieldPercentage = FreeLux.Menu.Item("selfAutoShieldPercentage").GetValue<Slider>().Value;
            int autoShieldMinMana = FreeLux.Menu.Item("selfAutoShieldMinMana").GetValue<Slider>().Value;
            if (FreeLux.W.IsReady() &&
                FreeLux.Player.ManaPercentage() >= autoShieldMinMana &&
                FreeLux.Player.HealthPercentage() <= autoShieldPercentage)
            {
                // Check to see if there is an ally in range that we can shield too, since we're already trying to shield ourself.
                var leastHealthAllyInRange =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(a => a.IsAlly && a.Distance(FreeLux.Player.Position) < FreeLux.W.Range)
                        .OrderBy(a => a.HealthPercentage())
                        .FirstOrDefault();
                if (leastHealthAllyInRange != null)
                    FreeLux.W.Cast(leastHealthAllyInRange, FreeLux.PacketCast);
                else
                {
                    // If not, just go ahead and shield ourself
                    FreeLux.W.Cast(Game.CursorPos, FreeLux.PacketCast);
                }
            }
        }


        public static bool HasIllumination(Obj_AI_Base target)
        {
            return target.HasBuff(InternalPassiveName);
        }

        public static bool HasLightBinding(Obj_AI_Base target)
        {
            return target.HasBuff(InternalBindingName);
        }

        public static bool EObjectExists()
        {
            return (_luxEObject != null);
        }

        public static bool IsIgnited(Obj_AI_Base target)
        {
            return target.HasBuff(InternalIgniteName);
        }

        public static bool InAutoAttackRange(Obj_AI_Base target)
        {
            return FreeLux.Player.Distance(target) <= GetOwnAutoAttackRange();
        }

        public static float GetOwnAutoAttackRange()
        {
            return FreeLux.Player.AttackRange + FreeLux.Player.BoundingRadius;
        }

        private static bool IsIgniteKillable(Obj_AI_Hero source, Obj_AI_Base target)
        {
            return Damage.GetSummonerSpellDamage(source, target, Damage.SummonerSpell.Ignite) >= target.Health;
        }

        public static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains(InternalEName) || sender.Name.Contains(InternalEName2))
                _luxEObject = sender;
        }

        public static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains(InternalEName) || sender.Name.Contains(InternalEName2))
                _luxEObject = null;
        }

        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (FreeLux.Menu.Item("autoQGapcloser").GetValue<bool>())
                FreeLux.Q.Cast(gapcloser.Sender, FreeLux.PacketCast);
        }

        // Q Logic from ChewyMoonsLux/Mid or Feed <3
        // Source: https://github.com/ChewyMoon/ChewyMoonScripts/
        public static Spell.CastStates CastQ(Obj_AI_Hero target, HitChance hitChance = HitChance.Medium)
        {
            var prediction = FreeLux.Q.GetPrediction(target);
            var col = FreeLux.Q.GetCollision(FreeLux.Player.ServerPosition.To2D(), new List<Vector2> { prediction.CastPosition.To2D() });
            var minions = col.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);

            if (minions > 1)
                return Spell.CastStates.Collision;

            /*if (prediction.Hitchance == hitChance)
            {*/
                FreeLux.Q.Cast(prediction.CastPosition, FreeLux.PacketCast);
                return Spell.CastStates.SuccessfullyCasted;
            //}
            //return Spell.CastStates.NotCasted;
        }

        public static Spell.CastStates CastE(Obj_AI_Hero target)
        {
            var prediction = FreeLux.E.GetPrediction(target, true);
            FreeLux.E.Cast(prediction.CastPosition, FreeLux.PacketCast);
            return Spell.CastStates.SuccessfullyCasted;
        }

        public static Spell.CastStates CastR(Obj_AI_Hero target, HitChance hitChance = HitChance.High)
        {
            var prediction = FreeLux.R.GetPrediction(target, true);
            FreeLux.R.Cast(prediction.CastPosition, FreeLux.PacketCast);
            return Spell.CastStates.SuccessfullyCasted;
        }

        public static Spell.CastStates CastRForAoeDamage(Obj_AI_Hero target)
        {
            var prediction = FreeLux.R.GetPrediction(target, true);
            if (prediction.AoeTargetsHitCount > 2)
            {
                FreeLux.R.Cast(prediction.CastPosition, FreeLux.PacketCast);
                return Spell.CastStates.SuccessfullyCasted;
            }
            return Spell.CastStates.NotCasted;
        }

        internal static void Obj_AI_Base_OnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
        {
            if (!FreeLux.Menu.Item("killRecalling").GetValue<bool>())
                return;

            var decoded = Packet.S2C.Teleport.Decoded(sender, args);
            var enemy = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(decoded.UnitNetworkId);

            if (enemy.IsAlly || FreeLux.Player.Distance(enemy) > FreeLux.R.Range || decoded.Type != Packet.S2C.Teleport.Type.Recall || decoded.Status != Packet.S2C.Teleport.Status.Start)
                return;

            if (MathHelper.GetRDamage(enemy) > enemy.Health)
                FreeLux.R.Cast(enemy);
        }
    }
}
