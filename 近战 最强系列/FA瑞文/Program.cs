using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using SH = FuckingAwesomeRiven.SpellHandler;

namespace FuckingAwesomeRiven
{
    class Program
    {
        public static Obj_AI_Hero Player;


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameStart;
        }

        static void Game_OnGameStart(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Riven")
                return;
            MenuHandler.initMenu();
            CheckHandler.init();
            Player = ObjectManager.Player;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameUpdate += eventArgs => StateHandler.tick();
            Obj_AI_Hero.OnProcessSpellCast += CheckHandler.Obj_AI_Hero_OnProcessSpellCast;
            Drawing.OnDraw += DrawHandler.Draw;
        }


        static void Game_OnGameUpdate(EventArgs args)
        {
            CheckHandler.Checks();
            var Config = MenuHandler.Config;

            if (MenuHandler.getMenuBool("keepQAlive") && SH._spells[SpellSlot.Q].IsReady() && CheckHandler.QCount >= 1 && Environment.TickCount - CheckHandler.LastQ > 3650 && !Player.IsRecalling())
                {
                    SH.CastQ();
                }

            if (Config.Item("normalCombo").GetValue<KeyBind>().Active)
            {
                StateHandler.mainCombo();
            }
            else
            {
                StateHandler.startedR2Combo.state = false;
                StateHandler.startedRCombo.state = false;
            }
            if (Config.Item("burstCombo").GetValue<KeyBind>().Active)
            {
                StateHandler.burstCombo();
            }
            if (Config.Item("jungleCombo").GetValue<KeyBind>().Active)
            {
                StateHandler.JungleFarm();
            }
            if (Config.Item("waveClear").GetValue<KeyBind>().Active)
            {
                StateHandler.laneclear();
            }
            if (Config.Item("lastHit").GetValue<KeyBind>().Active)
            {
                StateHandler.lastHit();
            }
            if (Config.Item("flee").GetValue<KeyBind>().Active)
            {
                StateHandler.flee();
            }

        }  

    }
}