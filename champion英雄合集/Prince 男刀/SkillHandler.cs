﻿using LeagueSharp;
using LeagueSharp.Common;
namespace PrinceTalon
{
   internal class SkillHandler
    {
        public static Spell Q, W, E, R;
        public static void Init()
        {
            Q = new Spell(SpellSlot.Q, 125);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 650);

            //W.SetSkillshot(0.7f, 400f, 900f, false, SkillshotType.SkillshotCone);
            W.SetSkillshot(0.25f, 80, 2300, false, SkillshotType.SkillshotLine);
        }
    }
}