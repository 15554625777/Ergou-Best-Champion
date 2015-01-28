using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SH = FuckingAwesomeRiven.SpellHandler;

namespace FuckingAwesomeRiven
{
    class CheckHandler
    {

        public static int LastQ, LastQ2, LastW, LastE, LastAA, LastPassive, LastFR, lastTiamat, lastR2;
        public static bool CanQ, CanW, CanE, CanR, CanAA, CanMove, CanSR, MidQ, MidW, MidE, MidAa, RState, BurstFinished, ResetQ;
        public static int PassiveStacks, QCount, FullComboState;

        public static void init()
        {
            CanAA = true;
            CanMove = true;
            CanQ = true;
            CanW = true;
            CanE = true;
            CanR = true;
            RState = false;

            LastQ = Environment.TickCount;
            LastQ2 = Environment.TickCount;
            LastW = Environment.TickCount;
            LastE = Environment.TickCount;
            LastAA = Environment.TickCount;
            LastPassive = Environment.TickCount;
            LastFR = Environment.TickCount;
        }

        public static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spell = args.SData;
            if (!sender.IsMe)
                return;
            if (!MidQ && spell.Name.Contains("RivenBasicAttack"))
            {
                LastAA = Environment.TickCount;
                LastPassive = Environment.TickCount;

                if (PassiveStacks >= 1)
                {
                    PassiveStacks = PassiveStacks - 1;
                }
                MidAa = true;
                CanMove = false;
                CanAA = false;
            }

            if (spell.Name.Contains("RivenTriCleave"))
            {
                LastQ = Environment.TickCount;
                LastPassive = Environment.TickCount;

                if (PassiveStacks <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }

                if (QCount <= 1)
                {
                    LastQ2 = Environment.TickCount;
                    QCount = QCount + 1;
                }
                else if (QCount == 2)
                {
                    QCount = 0;
                }
                Utility.DelayAction.Add(350, Orbwalking.ResetAutoAttackTimer);
                MidQ = true;
                CanMove = false;
                CanQ = false;
                FullComboState = 0;
                BurstFinished = true;
            }

            if (spell.Name.Contains("RivenMartyr"))
            {
                LastW = Environment.TickCount;
                LastPassive = Environment.TickCount;

                if (LastPassive <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }

                MidW = true;
                CanW = false;
                FullComboState = 2;
            }

            if (spell.Name.Contains("RivenFeint"))
            {
                LastE = Environment.TickCount;
                PassiveStacks = Environment.TickCount;

                if (LastPassive <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }

                MidE = true;
                CanE = false;
            }

            if (spell.Name.Contains("RivenFengShuiEngine"))
            {
                LastFR = Environment.TickCount;
                LastPassive = Environment.TickCount;

                if (PassiveStacks <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }

                RState = true;
                FullComboState = 1;
            }

            if (spell.Name.Contains("rivenizunablade"))
            {
                LastPassive = Environment.TickCount;

                if (PassiveStacks <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }
                lastR2 = Environment.TickCount;
                RState = false;
                CanSR = false;
                FullComboState = 3;
            }
        }

        public static void Checks()
        {
            if (MidQ && Environment.TickCount - LastQ >= 270)
            {
                MidQ = false;
                CanMove = true;
                CanAA = true;
            }

            if (MidW && Environment.TickCount - LastW >= 266.7)
            {
                MidW = false;
                CanMove = true;
            }

            if (MidE && Environment.TickCount - LastE >= 500)
            {
                MidE = false;
                CanMove = true;
            }

            if (PassiveStacks != 0 && Environment.TickCount - LastPassive >= 5000)
            {
                PassiveStacks = 0;
            }

            if (QCount != 0 && Environment.TickCount - LastQ >= 4000)
            {
                QCount = 0;
            }

            if (!CanW && !(MidAa || MidQ || MidE) && SH._spells[SpellSlot.W].IsReady())
            {
                CanW = true;
            }

            if (!CanE && !(MidAa || MidQ || MidW) && SH._spells[SpellSlot.E].IsReady())
            {
                CanE = true;
            }

            if (RState && Environment.TickCount - LastFR >= 15000)
            {
                RState = false;
            }

            if (MidAa && Environment.TickCount + Game.Ping / 2 >= LastAA + ObjectManager.Player.AttackCastDelay * 1000)
            {
                CanMove = true;
                CanQ = true;
                CanW = true;
                CanE = true;
                CanSR = true;
                MidAa = false;
            }
            if (!(MidAa || MidQ || MidE || MidW) &&
                Environment.TickCount + Game.Ping / 2 >= LastAA + ObjectManager.Player.AttackCastDelay * 1000)
            {
                CanMove = true;
            }

            if (!CanAA && !(MidQ || MidE || MidW) &&
                Environment.TickCount + Game.Ping / 2 + 25 >= LastAA + ObjectManager.Player.AttackDelay * 1000)
            {
                CanAA = true;
            }
        }

    }
}
