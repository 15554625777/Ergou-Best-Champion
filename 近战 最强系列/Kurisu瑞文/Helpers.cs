using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;


namespace KurisuRiven
{
    public static class Helpers
    {
        internal static void CheckR(Obj_AI_Base target)
        {
            if (target.IsValidTarget(Base.R.Range + 100))
            {
                if (!Base.R.IsReady() || Base.UltOn || !Base.GetBool("user"))
                    return;

                if (Base.GetList("ultwhen") == 0)
                {
                    if (Base.ComboDamage >= target.Health)
                    {
                        if (Base.CleaveCount <= 1 && Base.Q.IsReady())
                        {
                            Base.R.Cast();
                        }
                    }
                }

                if (Base.GetList("ultwhen") == 1)
                {
                    if (Base.ComboDamage * 1.7 >= target.Health)
                    {
                        if (Base.CleaveCount <= 1 && Base.Q.IsReady())
                        {
                            Base.R.Cast();
                        }
                    }
                }

                if (Base.GetList("wsmode") == 1)
                {
                    var targetList
                        = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(900));

                    var enemies = targetList as Obj_AI_Hero[] ?? targetList.ToArray();
                    if (enemies.Any(huro => Base.ComboDamage >= huro.Health))
                    {
                        if (Base.CleaveCount <= 1 && Base.Q.IsReady())
                            Base.R.Cast();
                    }

                    if (enemies.Count() >= 3 && Base.CleaveCount <= 1 && Base.Q.IsReady())
                    {
                        Base.R.Cast();
                    }
                }
            }
        }

        internal static float GetDmg(string item, bool ulton = false)
        {
            if (Combo.Target == null)
                return 0f;

            var damage = 0f;
            if (item == "P")
            {
                // Passive (Chewy/Crisdmc)
                var ad = (float) Base.Me.GetAutoAttackDamage(Combo.Target);
                var runicpassive = new[] { 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5 };

                damage = ad +
                         (float)
                             ((Base.Me.FlatPhysicalDamageMod + Base.Me.BaseAttackDamage)*runicpassive[Base.Me.Level/3]);

            }

            if (item == "Q" & Base.Q.IsReady())
            {   damage = (float) Base.Me.CalcDamage(Combo.Target, Damage.DamageType.Physical, -10 + (Base.Q.Level*20) +
                    (0.35 + (Base.Q.Level*0.05))*(Base.Me.FlatPhysicalDamageMod + Base.Me.BaseAttackDamage));
            }

            if (item == "W" && Base.W.IsReady())
                damage = (float) Base.Me.GetSpellDamage(Combo.Target, SpellSlot.W);

            if (item == "R" && Base.R.IsReady())
            {
                damage = (float) Base.Me.CalcDamage(Combo.Target, Damage.DamageType.Physical,
                     (new double[] { 80, 80, 120, 160 }[Base.R.Level] +
                          (0.6 * Base.Me.FlatPhysicalDamageMod + Base.Me.BaseAttackDamage) *
                                 ((Combo.Target.MaxHealth - Base.ComboDamage) / Combo.Target.MaxHealth * 2.67 + 1)));             
            }

            if (item == "I" && Base.Me.Spellbook.CanUseSpell(Base.Me.GetSpellSlot("summonerdot")) == SpellState.Ready)
                damage = (float) Base.Me.GetSummonerSpellDamage(Combo.Target, Damage.SummonerSpell.Ignite);

            if (item == "ITEMS")
            {
                var tmt = Items.HasItem(3077) && Items.CanUseItem(3077)
                  ? Base.Me.GetItemDamage(Combo.Target, Damage.DamageItems.Tiamat)
                  : 0;

                var hyd = Items.HasItem(3074) && Items.CanUseItem(3074)
                    ? Base.Me.GetItemDamage(Combo.Target, Damage.DamageItems.Hydra)
                    : 0;

                var bwc = Items.HasItem(3144) && Items.CanUseItem(3144) ?
                    Base.Me.GetItemDamage(Combo.Target, Damage.DamageItems.Bilgewater)
                    : 0;

                var brk = Items.HasItem(3153) && Items.CanUseItem(3153)
                    ? Base.Me.GetItemDamage(Combo.Target, Damage.DamageItems.Botrk)
                    : 0;

                damage = (float) (tmt + hyd + bwc + brk);
            }

            var damagesum = ulton ? damage + (damage*0.25) : damage;
            return (float) damagesum;
        }

        internal static void OnBuffUpdate()
        {
            foreach (var b in Base.Me.Buffs)
            {
                if (b.Name == "RivenTriCleave")
                    Base.CleaveCount = b.Count;

                if (b.Name == "rivenpassiveaaboost")
                    Base.PassiveCount = b.Count;
            }

            if (Base.Me.HasBuff("RivenTriCleave", true) && Environment.TickCount - Base.LastQ >= 3600)
            {
                if (Base.GetBool("keepq") && !Base.Me.IsRecalling())
                {
                    Base.Q.Cast(Game.CursorPos);
                }
            }

            if (!Base.Me.HasBuff("rivenpassiveaaboost", true))
                Utility.DelayAction.Add(1000, () => Base.PassiveCount = 1);

            if (!Base.Me.HasBuff("RivenTriCleave", true))
                Utility.DelayAction.Add(1000, () => Base.CleaveCount = 0);          
 
            // autow
            if (Base.GetBool("autow") && Base.Me.CountEnemiesInRange(Base.W.Range) >= Base.GetSlider("wmin"))
            {
                if (Base.Me.UnderTurret(true) && Base.W.IsReady() && Base.CanW)
                    Base.W.Cast();
            }
        }

        internal static void Windslash()
        {
            // windslash
            if (Base.UltOn && Base.GetBool("usews") && Base.R.IsReady())
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(Base.R.Range + 100)))
                {
                    var de = Base.R.GetPrediction(target, true);
                    if (Base.GetList("wsmode") == 1)
                    {
                        if ((int) (GetDmg("R")/target.MaxHealth*100) >= target.Health/target.MaxHealth*100)
                        {                     
                            if (de.Hitchance >= HitChance.Low && Base.CanWS && Base.R.IsReady())
                                Base.R.Cast(de.CastPosition);
                        }

                        if (target.Health < GetDmg("R", true) + GetDmg("P", true)*1 + GetDmg("Q", true)*2 &&
                            target.Distance(Base.Me.ServerPosition) <= Base.TrueRange + 100)
                        {
                            if (de.Hitchance >= HitChance.Low && Base.CanWS && Base.R.IsReady())
                                Base.R.Cast(de.CastPosition);
                        }

                    }

                    if (GetDmg("R") >= Combo.Target.Health && Base.CanWS)
                    {
                        var po = Base.R.GetPrediction(target, true);
                        if (po.Hitchance >= HitChance.Low)
                            Base.R.Cast(po.CastPosition);
                    }
                }
            }
        }
    }
}
