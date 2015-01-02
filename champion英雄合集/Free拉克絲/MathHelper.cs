using LeagueSharp;
using LeagueSharp.Common;

namespace FreeLux
{
    static class MathHelper
    {
        private const int InternalBasicAttackRange = 550;
        private const int InternalIgniteRange = 600;

        private static Obj_AI_Hero Player
        {
            get { return FreeLux.Player; }
        }

        public static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (FreeLux.Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (FreeLux.E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (FreeLux.R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

            if (Items.HasItem(FreeLux.DeathfireGrasp.Id))
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg);
                damage = damage * 1.2;
            }

            if (ObjectManager.Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                damage += GetIgniteDamage(enemy);
            }

            //damage += Player.GetAutoAttackDamage(enemy, true) * 2;

            if (Actions.HasIllumination(enemy))
            {
                damage += GetPassiveProcDamage();
            }

            return (float) damage;
        }

        public static string GetDamageString(Obj_AI_Base enemy)
        {
            double qDamage, eDamage, rDamage, iDamage;
            string str = "";

            qDamage = (FreeLux.Q.IsReady()) ? Player.GetSpellDamage(enemy, SpellSlot.Q) : 0.0d;
            eDamage = (FreeLux.E.IsReady()) ? Player.GetSpellDamage(enemy, SpellSlot.E) : 0.0d;
            rDamage = (FreeLux.R.IsReady()) ? Player.GetSpellDamage(enemy, SpellSlot.R) : 0.0d;
            iDamage = (FreeLux.IgniteSlot.IsReady()) ? Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite) : 0.0d;
            bool debug = false;

            if (enemy.Health < qDamage)
                str = "Q Kill! " + ((debug) ? ((int)(qDamage)).ToString() : "");
            else if (enemy.Health < eDamage)
                str = "E Kill! " + ((debug) ? ((int)(eDamage)).ToString() : "");
            else if (enemy.Health < rDamage)
                str = "R Kill! " + ((debug) ? ((int)(rDamage)).ToString() : "");
            else if (enemy.Health < iDamage)
                str = "Ignite Kill! " + ((debug) ? ((int)(iDamage)).ToString() : "");
            else if (enemy.Health < qDamage + iDamage)
                str = "Q+Ignite Kill! " + ((debug) ? ((int)(qDamage + iDamage)).ToString() : "");
            else if (enemy.Health < eDamage + iDamage)
                str = "E+Ignite Kill! " + ((debug) ? ((int)(eDamage + iDamage)).ToString() : "");
            else if (enemy.Health < rDamage + iDamage)
                str = "R+Ignite Kill! " + ((debug) ? ((int)(rDamage + iDamage)).ToString() : "");
            else if (enemy.Health < qDamage + eDamage)
                str = "Q+E Kill! " + ((debug) ? ((int)(qDamage + eDamage)).ToString() : "");
            else if (enemy.Health < qDamage + rDamage)
                str = "Q+R Kill! " + ((debug) ? ((int)(qDamage + rDamage)).ToString() : "");
            else if (enemy.Health < eDamage + rDamage)
                str = "E+R Kill! " + ((debug) ? ((int)(eDamage + rDamage)).ToString() : "");
            else if (enemy.Health < qDamage + eDamage + iDamage)
                str = "Q+E+Ignite Kill! " + ((debug) ? ((int)(qDamage + eDamage + iDamage)).ToString() : "");
            else if (enemy.Health < qDamage + rDamage + iDamage)
                str = "Q+R+Ignite Kill! " + ((debug) ? ((int)(qDamage + rDamage + iDamage)).ToString() : "");
            else if (enemy.Health < eDamage + rDamage + iDamage)
                str = "E+R+Ignite Kill! " + ((debug) ? ((int)(eDamage + rDamage + iDamage)).ToString() : "");
            else if (enemy.Health < qDamage + eDamage + rDamage)
                str = "Q+E+R Kill! " + ((debug) ? ((int)(qDamage + eDamage + rDamage)).ToString() : "");
            else if (enemy.Health < qDamage + eDamage + rDamage + iDamage)
                str = "Q+E+R+Ignite Kill! " + ((debug) ? ((int)(qDamage + eDamage + rDamage + iDamage)).ToString() : "");
            else if (enemy.Health < GetComboDamage(enemy))
                str = "Full Combo Kill! " + ((debug) ? ((int)GetComboDamage(enemy)).ToString() : "");
            else
                str = "Harass! " + ((debug) ? ((int)GetComboDamage(enemy)).ToString() : "");
            return str;
        }

        public static double GetPassiveProcDamage()
        {
            return 10 + 8 * Player.Level + (Player.FlatMagicDamageMod + Player.BaseAbilityDamage) * 0.2d;
        }

        public static double GetIgniteDamage(Obj_AI_Base enemy)
        {
            return Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
        }

        public static double GetRDamage(Obj_AI_Hero enemy)
        {
            return Player.GetSpellDamage(enemy, SpellSlot.R);
        }
    }
}
