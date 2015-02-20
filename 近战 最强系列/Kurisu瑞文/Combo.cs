using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace KurisuRiven
{
    internal class Combo
    {
        public static Obj_AI_Hero Target;
        internal static void OnGameUpdate()
        {        
                Target = TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Physical);
            if (Target == null)
                return;
      
            // combo 
            if (Base.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;

            var ignote = Base.Me.GetSpellSlot("summonerdot");
            if (Base.Me.Spellbook.CanUseSpell(ignote) == SpellState.Ready)
            {
                if (Target.Distance(Base.Me.ServerPosition) <= 600 * 600 && Base.CleaveCount <= 1 &&
                    Base.Q.IsReady())
                {
                    var combo = Helpers.GetDmg("P") * 3 + Helpers.GetDmg("Q") * 3 + Helpers.GetDmg("W") +
                        Helpers.GetDmg("ITEMS") + Helpers.GetDmg("I") + Helpers.GetDmg("R");

                    if (Base.GetBool("useignote") && combo >= Target.Health)
                        Base.Me.Spellbook.CastSpell(ignote, Target);

                    else if (Base.GetBool("useignote") && Base.ComboDamage >= Target.Health && Base.R.IsReady())
                        Base.Me.Spellbook.CastSpell(ignote, Target);
                }
            }

            Base.OrbTo(Target);
            if (!Target.IsValidTarget(Base.R.Range + 100)) 
                return;

            // valor
            // engage if target is out of aa range
            if (Base.E.IsReady() && Base.CanE && Base.GetBool("usecomboe") &&
                Target.Distance(Base.Me.ServerPosition, true) > Math.Pow(Base.TrueRange + 100, 2))
            {
                // is target in combo range or low health
                if (Target.Distance(Base.Me.ServerPosition, true) <= Math.Pow(Base.R.Range + 100, 2) ||
                    Base.Me.Health/Base.Me.MaxHealth*100 <= Base.GetSlider("vhealth"))
                {
                    // item handler
                    if (Base.GetBool("useitems"))
                    {
                        if (Items.HasItem(3142) && Items.CanUseItem(3142))
                            Items.UseItem(3142);
                        if (Items.HasItem(3144) && Items.CanUseItem(3144))
                            Items.UseItem(3144, Target);
                        if (Items.HasItem(3153) && Items.CanUseItem(3153))
                            Items.UseItem(3153, Target);
                    }

                    if (Base.GetBool("usecomboe"))
                        Base.E.Cast(Target.ServerPosition);


                    // after dash event
                    if (Base.GetList("engage") == 1)
                    {
                        if (Base.CanHD && Base.HasHD)
                        {
                            if (Base.W.IsReady() && !Base.CanBurst)
                            {
                                Items.UseItem(3077);
                                Items.UseItem(3074);
                            }

                            // used hydra or dont own
                            else
                            {
                                Helpers.CheckR(Target);
                            }
                        }

                        else
                        {
                            Helpers.CheckR(Target);
                        }
                    }
                }
            }

            // kiburst
            // use w if in range
            else if (Base.W.IsReady() && Base.CanW && Base.GetBool("usecombow") &&
                     Target.Distance(Base.Me.ServerPosition, true) <= Math.Pow(Base.W.Range + 25, 2))
            {
                // item handler
                if (Base.GetBool("useitems"))
                {
                    if (Items.HasItem(3142) && Items.CanUseItem(3142))
                        Items.UseItem(3142);

                    if (Target.Distance(Base.Me.ServerPosition, true) <= 450*450)
                    {
                        if (Items.HasItem(3144) && Items.CanUseItem(3144))
                            Items.UseItem(3144, Target);
                        if (Items.HasItem(3153) && Items.CanUseItem(3153))
                            Items.UseItem(3153, Target);
                    }
                }


                if (Base.GetList("engage") == 0)
                {
                    Helpers.CheckR(Target);

                    if (Base.GetBool("usecombow"))
                        Base.W.Cast();

                    // hydra after
                    if (Base.CanHD && Base.HasHD)
                    {
                        Items.UseItem(3077);
                        Items.UseItem(3074);
                    }
                }

                else if (Base.GetList("engage") == 1)
                {
                    // hydra before
                    if (Base.CanHD && Base.HasHD && !Base.CanBurst)
                    {
                        Items.UseItem(3077);
                        Items.UseItem(3074);
                        if (Base.GetBool("usecombow"))
                            Utility.DelayAction.Add(250, () => Base.W.Cast());
                    }

                    // used hydra or dont own
                    else
                    {
                        Helpers.CheckR(Target);
                        if (Base.GetBool("usecombow"))
                            Base.W.Cast();
                    }
                }
            }

            // broken wings
            // use q if in range
            else if (Base.Q.IsReady() && Base.GetBool("usecomboq") &&
                     Target.Distance(Base.Me.ServerPosition, true) <= Math.Pow(Base.Q.Range + 30, 2))
            {
                // item handler
                if (Base.GetBool("useitems"))
                {
                    if (Items.HasItem(3142) && Items.CanUseItem(3142))
                        Items.UseItem(3142);

                    if (Target.Distance(Base.Me.ServerPosition, true) <= 450*450)
                    {
                        if (Items.HasItem(3144) && Items.CanUseItem(3144))
                            Items.UseItem(3144, Target);
                        if (Items.HasItem(3153) && Items.CanUseItem(3153))
                            Items.UseItem(3153, Target);
                    }
                }

                // check ultimate
                Helpers.CheckR(Target);

                if (Base.GetList("engage") == 0 ||
                    Helpers.GetDmg("P", true)*2 + Helpers.GetDmg("Q", true)*1 + Helpers.GetDmg("R") >= Target.Health)
                {
                    if (Items.CanUseItem(3077) || Items.CanUseItem(3074))
                        return;
                }

                if (Base.CanQ && Base.GetBool("usecomboq"))
                    Base.Q.Cast(Target.ServerPosition);
            }

            // gapclose
            else if (Target.Distance(Base.Me.ServerPosition) > Base.TrueRange + 100 && Base.GetBool("qgap"))
            {
                if (Environment.TickCount - Base.LastQ >= 1100 && !Base.DidAA)
                {
                    if (Base.GetBool("usecomboq") && Base.Q.IsReady())
                        Base.Q.Cast(Target.ServerPosition);
                }
            }

        }

        internal static void Flee()
        {
            if (Base.Settings.Item("fleemode").GetValue<KeyBind>().Active)
            {
                if (Base.CanE && Base.E.IsReady())
                {
                    Base.E.Cast(Game.CursorPos);
                }

                if (!Base.E.IsReady() && Environment.TickCount - Base.LastQ >= 300 &&
                    Environment.TickCount - Base.LastE >= 200)
                {
                    Base.Q.Cast(Game.CursorPos);
                }

                if (Base.CanMV)
                {
                    Base.Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }
            }          
        }

        internal static void LaneFarm()
        {
            try
            {
                if (Base.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    var minionList = new[]
                    {
                        "SRU_Razorbeak", "SRU_Krug", "Sru_Crab", "SRU_Baron", "SRU_Dragon",
                        "SRU_Blue", "SRU_Red", "SRU_Murkwolf", "SRU_Gromp"
                    };

                    var smallMinion =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .FirstOrDefault(x => x.Name.Contains("Mini") && !x.Name.StartsWith("Minion") && x.IsValidTarget(700));

                    var bigMinion =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .FirstOrDefault(
                                x =>
                                    !x.Name.Contains("Mini") && !x.Name.StartsWith("Minion") &&
                                    minionList.Any(name => x.Name.StartsWith(name)) && x.IsValidTarget(900));

                    var minion = bigMinion ?? smallMinion;
                    if (minion != null)
                    {
                        Base.OrbTo(minion);
                        if (minionList.Any(x => minion.Name.StartsWith(x) && !minion.Name.Contains("Mini")))
                        {

                            if (Base.GetBool("usejungleq") && Base.Q.IsReady() && Base.CanQ)
                            {
                                if (minion.Distance(Base.Me.ServerPosition, true) <= Math.Pow(Base.Q.Range + 30, 2))
                                    Base.Q.Cast(minion.ServerPosition);
                            }

                            if (Base.GetBool("usejunglew") && Base.W.IsReady() && Base.CanW)
                            {
                                if (minion.Distance(Base.Me.ServerPosition, true) <= Base.W.RangeSqr)
                                    Base.W.Cast();

                            }
                             
                            if (Base.GetBool("usejunglee"))
                            {
                                if (minion.Distance(Base.Me.ServerPosition, true) > Math.Pow(Base.Me.AttackRange, 2) &&
                                    Base.E.IsReady() && Base.CanE)
                                {
                                    Base.E.Cast(Game.CursorPos);
                                }

                                else if (Base.Me.Health / Base.Me.MaxHealth * 100 <= Base.GetSlider("vhealth") &&
                                         Base.E.IsReady() && Base.CanE)
                                {
                                    Base.E.Cast(Game.CursorPos);
                                }
                            }
                        }
                    }

                    else
                    {
                        var newminion = ObjectManager.Get<Obj_AI_Minion>().First(x => x.IsValidTarget(600));
                        if (!Base.Me.ServerPosition.Extend(newminion.ServerPosition, Base.Q.Range).UnderTurret(true))
                        {
                            if (Base.GetBool("uselaneq") && Base.Q.IsReady() && Base.CanQ)
                            {
                                if (newminion.Distance(Base.Me.ServerPosition, true) <= Math.Pow(Base.Q.Range + 200, 2))
                                    Base.Q.Cast(newminion.ServerPosition);
                            }
                        }

                        if (Base.GetBool("uselanee"))
                        {
                            if (Base.E.IsReady() && Base.CanE)
                                Base.E.Cast(Game.CursorPos);
                        }
                    }
                }
            }

            catch (Exception e)
            {
                //Console.WriteLine("Minion died");
            }
            
        }
    }
}
