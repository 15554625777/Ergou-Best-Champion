using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace PerplexedEzreal
{
    class SpellManager
    {
        private static Spell _Q, _W, _E, _R;

        public static Spell Q { get { return _Q; } }
        public static Spell W { get { return _W; } }
        public static Spell E { get { return _E; } }
        public static Spell R { get { return _R; } }

        public static void Initialize()
        {
            _Q = new Spell(SpellSlot.Q, 1200);
            _Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);

            _W = new Spell(SpellSlot.W, 1000);
            _W.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);

            _E = new Spell(SpellSlot.E, 475);

            _R = new Spell(SpellSlot.R, float.MaxValue);
            _R.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);
        }

        public static void CastSpell(Spell spell, Obj_AI_Base target, HitChance hitChance, bool packetCast)
        {
            Obj_AI_Hero Player = ObjectManager.Player;
            if (target.IsValidTarget(spell.Range) && spell.GetPrediction(target).Hitchance >= hitChance)
                spell.Cast(target, packetCast);
        }
    }
}
