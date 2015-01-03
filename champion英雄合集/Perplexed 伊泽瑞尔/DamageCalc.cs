using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace PerplexedEzreal
{
    class DamageCalc
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;

        public static float GetComboDamage(Obj_AI_Hero target)
        {
            double dmg = Player.GetAutoAttackDamage(target);

            if (SpellManager.Q.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.Q);
            if (SpellManager.W.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.W);
            if (SpellManager.E.IsReady())
                dmg += Player.GetSpellDamage(target, SpellSlot.E);

            dmg += GetUltDamage(target);

            return (float)dmg;
        }

        public static float GetDrawDamage(Obj_AI_Hero target)
        {
            double dmg = Config.DrawAADmg ? Player.GetAutoAttackDamage(target) : 0;

            if (SpellManager.Q.IsReady() && Config.DrawQDmg)
                dmg += Player.GetSpellDamage(target, SpellSlot.Q);
            if (SpellManager.W.IsReady() && Config.DrawWDmg)
                dmg += Player.GetSpellDamage(target, SpellSlot.W);
            if (SpellManager.E.IsReady() && Config.DrawEDmg)
                dmg += Player.GetSpellDamage(target, SpellSlot.E);

            dmg += Config.DrawRDmg ? GetUltDamage(target) : 0;

            return (float)dmg;
        }

        public static float GetUltDamage(Obj_AI_Hero target)
        {
            Spell R = SpellManager.R;

            if (!R.IsReady())
                return 0f;

            float reduction = 1f - ((R.GetCollision(Player.Position.To2D(), new List<Vector2>() { target.Position.To2D() }).Count) / 10);
            reduction = reduction < 0.3f ? 0.3f : reduction;

            return (float)Player.GetSpellDamage(target, SpellManager.R.Slot) * reduction;
        }
    }
}
