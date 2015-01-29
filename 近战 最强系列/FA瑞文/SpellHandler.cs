using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LeagueSharp.Common.Data;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using CH = FuckingAwesomeRiven.CheckHandler;

namespace FuckingAwesomeRiven
{
    public static class SpellHandler
    {
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public static Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>()
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q, 300)},
            {SpellSlot.W, new Spell(SpellSlot.W, 250)},
            {SpellSlot.E, new Spell(SpellSlot.E, 325)},
            {SpellSlot.R, new Spell(SpellSlot.R, 900)}
        };

        public enum summonerSpell {Flash, Ignite, Smite}

        public static Dictionary<summonerSpell, SpellSlot> SummonerDictionary =
            new Dictionary<summonerSpell, SpellSlot>()
            {
                { summonerSpell.Flash, Player.GetSpellSlot("SummonerFlash") },
                { summonerSpell.Ignite, Player.GetSpellSlot("SummonerDot") },
            };

        public static int QRange { get { return CheckHandler.RState ? 325 : 300; } }
        public static int WRange { get { return CheckHandler.RState ? 270 : 250; } }

        public static void CastQ(Obj_AI_Base target = null)
        {
            if (target != null)
            {
                _spells[SpellSlot.Q].Cast(target.Position, true);
                Utility.DelayAction.Add(45, () => CheckHandler.ResetQ = true);
            }
            else
            {
                _spells[SpellSlot.Q].Cast(Game.CursorPos, true);
                Utility.DelayAction.Add(48, () => CheckHandler.ResetQ = true);
            }
        }

        public static void CastW(Obj_AI_Hero target = null)
        {
            if (target.IsValidTarget(WRange))
            {
               _spells[SpellSlot.W].Cast();
            }
            if (target == null)
            {
                _spells[SpellSlot.W].Cast();
            }
        }

        public static void CastE(Vector3 pos)
        {
            if (!_spells[SpellSlot.E].IsReady())
                return;
            _spells[SpellSlot.E].Cast(pos);
            CH.LastE = Environment.TickCount;
        }

        public static void CastR()
        {
            _spells[SpellSlot.R].Cast();
        }

        public static void CastR2(Obj_AI_Hero target)
        {
            var r2 = new Spell(SpellSlot.R, 900);
            r2.SetSkillshot(0, 45, 1200, false, SkillshotType.SkillshotCone);
            r2.Cast(target);
        }

        public static void castFlash(Vector3 pos)
        {
            if (!SummonerDictionary[summonerSpell.Flash].IsReady())
                return;
            Player.Spellbook.CastSpell(SummonerDictionary[summonerSpell.Flash], pos);
        }

        public static void castIgnite(Obj_AI_Hero target)
        {
            if (!SummonerDictionary[summonerSpell.Ignite].IsReady())
                return;
            Player.Spellbook.CastSpell(SummonerDictionary[summonerSpell.Ignite], target);
        }


        public static void castItems(Obj_AI_Base target)
        {
            if (!target.IsValidTarget())
                return;
            if (target is Obj_AI_Minion && target.IsValidTarget(300))
            {
                if (ItemData.Tiamat_Melee_Only.GetItem().IsReady())
                    ItemData.Tiamat_Melee_Only.GetItem().Cast();
                if (ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
                    ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
            }
            else
            {
                if (target.IsValidTarget(300))
                {
                    if (ItemData.Tiamat_Melee_Only.GetItem().IsReady())
                        ItemData.Tiamat_Melee_Only.GetItem().Cast();
                    if (ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
                        ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
                }
                if (ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                    ItemData.Youmuus_Ghostblade.GetItem().Cast();
            }
        }

        public static void animCancel(Obj_AI_Base target)
        {
            if (CH.ResetQ)
            {
                var pos1 = target.Position.Extend(Player.Position, target.Distance(Player) + 62);
                var pos2 = target.Position.Extend(Player.Position, target.Distance(Player) - 62);
                Player.IssueOrder(GameObjectOrder.MoveTo, target.IsValidTarget() ? pos1 : pos2);
                CH.ResetQ = false;
            }
        }


        public static void Orbwalk(Obj_AI_Base target = null)
        {
            if (CH.CanMove)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, (target.IsValidTarget(600) && MenuHandler.Config.Item("magnet").GetValue<bool>() && !(target is Obj_AI_Minion) ? Player.Position.Extend(target.Position, Player.Distance(target) - 20) : Game.CursorPos));
            }

            if (target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && CH.CanAA)
            {
                CH.CanMove = false;
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                CH.CanQ = false;
                CH.CanW = false;
                CH.CanE = false;
                CH.CanSR = false;
                CH.LastAA = Environment.TickCount;
            }
        }
    }
}
