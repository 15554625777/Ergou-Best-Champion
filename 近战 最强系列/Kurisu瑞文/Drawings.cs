using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace KurisuRiven
{
    class Drawings
    {
        internal static void OnDraw(EventArgs args)
        {
            var combo = Helpers.GetDmg("P")*3 + Helpers.GetDmg("Q")*3 + Helpers.GetDmg("W") +
                        Helpers.GetDmg("ITEMS") + Helpers.GetDmg("I") + Helpers.GetDmg("R");

            var comboult = Helpers.GetDmg("P", true)*3 + Helpers.GetDmg("Q", true)*3 + Helpers.GetDmg("W", true) +
                           Helpers.GetDmg("ITEMS", true) + Helpers.GetDmg("I") + Helpers.GetDmg("R");

            if (!Base.Me.IsDead)
            {
                if (Base.GetBool("drawengage"))
                {
                    Render.Circle.DrawCircle(Base.Me.Position,
                        Base.Me.AttackRange + Base.E.Range + 10, Color.White, 3);
                }

                if (Combo.Target.IsValidTarget(900) && Base.GetBool("drawtarg"))
                {
                    Render.Circle.DrawCircle(
                        Combo.Target.Position, Combo.Target.BoundingRadius - 50, Color.DarkOrange, 6);
                }

                if (Base.GetBool("debugtrue"))
                {
                    Render.Circle.DrawCircle(Base.Me.Position, Base.TrueRange, Color.DarkOrange, 3);
                }
            }

            if (Base.GetBool("debugdmg") && Combo.Target.IsValidTarget(1000))
            {
                var wts = Drawing.WorldToScreen(Combo.Target.Position);

                if (!Base.R.IsReady())
                    Drawing.DrawText(wts[0] - 75, wts[1] + 40, Color.DarkOrange,
                        "Combo Damage: " + combo);
                else
                    Drawing.DrawText(wts[0] - 75, wts[1] + 40, Color.DarkOrange,
                        "Combo Damage: " + comboult);
            }

            if (Combo.Target.IsValidTarget(1000) && Base.GetBool("drawkill"))
            {
                var wts = Drawing.WorldToScreen(Combo.Target.Position);

                if (Base.CanBurst)
                {
                    Drawing.DrawText(wts[0] - 65, wts[1] + 20, Color.White, "Burst Combo Kill!");
                }

                else if ((Helpers.GetDmg("P") + Helpers.GetDmg("Q")*2 + Helpers.GetDmg("W") + Helpers.GetDmg("I") +
                         Helpers.GetDmg("ITEMS")) > Combo.Target.Health)
                {
                    Drawing.DrawText(wts[0] - 20, wts[1] + 20, Color.White, "Kill!");
                }

                else if ((Helpers.GetDmg("P")*2 + Helpers.GetDmg("Q")*2 + Helpers.GetDmg("W") +
                         Helpers.GetDmg("ITEMS")) > Combo.Target.Health)
                {
                    Drawing.DrawText(wts[0] - 40, wts[1] + 20, Color.White, "Easy Kill!");
                }

                else if ((Helpers.GetDmg("P")*3 + Helpers.GetDmg("Q")*3 + Helpers.GetDmg("W") +
                         Helpers.GetDmg("I") + Helpers.GetDmg("R") + Helpers.GetDmg("ITEMS")) > Combo.Target.Health)
                {
                    Drawing.DrawText(wts[0] - 65, wts[1] + 20, Color.White,
                        "Full Combo Kill!");
                }

                else if ((Helpers.GetDmg("P", true)*3 + Helpers.GetDmg("Q", true)*3 + Helpers.GetDmg("W", true) + 
                    Helpers.GetDmg("R") + Helpers.GetDmg("I") + Helpers.GetDmg("ITEMS")) > Combo.Target.Health)
                {
                    Drawing.DrawText(wts[0] - 70, wts[1] + 20, Color.White,
                        "Full Combo Hard Kill!");
                }

                else if ((Helpers.GetDmg("P", true)*3 + Helpers.GetDmg("Q", true)*3 + Helpers.GetDmg("ITEMS")) < Combo.Target.Health)
                {
                    Drawing.DrawText(wts[0] - 40, wts[1] + 20, Color.Red, "Cant Kill!");
                }
            }           
        }
    }
}
