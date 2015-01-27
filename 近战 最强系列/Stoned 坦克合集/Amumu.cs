﻿﻿#region
using System;
using System.Collections;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Collections.Generic;
using System.Threading;
#endregion

namespace StonedSeriesAIO
{
    internal class Amumu
    {
        public Amumu()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private const string Champion = "Amumu";

        private static Orbwalking.Orbwalker Orbwalker;

        private static List<Spell> SpellList = new List<Spell>();

        private static Spell Q;

        private static Spell W;

        private static Spell E;

        private static Spell R;

        private static Menu Config;

        private static Items.Item RDO;

        private static Items.Item DFG;

        private static Items.Item YOY;

        private static Items.Item BOTK;

        private static Items.Item HYD;

        private static Items.Item CUT;

        private static Items.Item TYM;

        private static Obj_AI_Hero Player;

        
        static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != Champion) return;

            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 525);

            Q.SetSkillshot(0.250f, 80, 2000, true, SkillshotType.SkillshotLine);


            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            RDO = new Items.Item(3143, 490f);
            HYD = new Items.Item(3074, 175f);
            DFG = new Items.Item(3128, 750f);
            YOY = new Items.Item(3142, 185f);
            BOTK = new Items.Item(3153, 450f);
            CUT = new Items.Item(3144, 450f);
            TYM = new Items.Item(3077, 175f);

            //Menu Amumu
            Config = new Menu("Stoned 阿木木", "StonedAmumu", true);

            //Ts
            var targetSelectorMenu = new Menu("目标选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //orb
            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo Menu
            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用 W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "使用 物品")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("AutoR", "自动 R")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("CountR", "使用R|范围内敌人").SetValue(new Slider(1, 5, 0)));
            Config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "连招 键位").SetValue(new KeyBind(32, KeyBindType.Press)));

            //JungleClear
            Config.AddSubMenu(new Menu("清野", "Jungle"));
            Config.SubMenu("Jungle").AddItem(new MenuItem("UseQClear", "使用 Q")).SetValue(true);
            Config.SubMenu("Jungle").AddItem(new MenuItem("UseWClear", "使用 W")).SetValue(true);
            Config.SubMenu("Jungle").AddItem(new MenuItem("UseEClear", "使用 E")).SetValue(true);
            Config.SubMenu("Jungle").AddItem(new MenuItem("ActiveClear", "清野 键位").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //WaveClear
            Config.AddSubMenu(new Menu("清线", "Wave"));
            Config.SubMenu("Wave").AddItem(new MenuItem("UseQWave", "使用 Q")).SetValue(true);
            Config.SubMenu("Wave").AddItem(new MenuItem("UseWWave", "使用 W")).SetValue(true);
            Config.SubMenu("Wave").AddItem(new MenuItem("UseEWave", "使用 E")).SetValue(true);
            Config.SubMenu("Wave").AddItem(new MenuItem("ActiveWave", "清线 范围").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));



            //Drawings
            Config.AddSubMenu(new Menu("范围", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "范围 Q")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "范围 W")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "范围 E")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "范围 R")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "延迟自由圈").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "圈 质量").SetValue(new Slider(100, 100, 10)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "圈 厚度").SetValue(new Slider(1, 10, 1)));

            Config.AddToMainMenu();

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;



            Game.PrintChat("<font color='#FF00BF'>Stoned Amumu Loaded By</font> <font color='#FF0000'>The</font><font color='#FFFF00'>Kush</font><font color='#40FF00'>Style</font>");
        }

        private static void OnGameUpdate(EventArgs args)
        {
            Player = ObjectManager.Player;


            Orbwalker.SetAttack(true);
            if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (Config.Item("ActiveClear").GetValue<KeyBind>().Active)
            {
                JungleClear();
            }
            if (Config.Item("ActiveWave").GetValue<KeyBind>().Active)
            {
                WaveClear();
            }

        }

        private static void WaveClear()
        {
            var Minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            var useQ = Config.Item("UseQClear").GetValue<bool>();
            var useW = Config.Item("UseWClear").GetValue<bool>();
            var useE = Config.Item("UseEClear").GetValue<bool>();

            var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            if (minions.Count > 0)
            {
                if (useQ && Q.IsReady() && minions[0].IsValidTarget() && Player.Distance(minions[0]) <= Q.Range)
                {
                    Q.Cast(minions[0].Position);
                }

                if (useW && W.IsReady() && minions[0].IsValidTarget())
                {
                    if (Player.Distance(minions[0]) <= W.Range && (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1))
                    {
                        W.Cast();
                    }
                    else if (Player.Distance(minions[0]) > W.Range && (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2))
                    {
                        W.Cast();
                    }

                }

                if (useE && E.IsReady() && minions[0].IsValidTarget() && Player.Distance(minions[0]) <= E.Range)
                {
                    E.Cast();
                }
            }
        }

        private static void JungleClear() //Credits To Flapperdoodle! 
        {
            var useQ = Config.Item("UseQClear").GetValue<bool>();
            var useW = Config.Item("UseWClear").GetValue<bool>();
            var useE = Config.Item("UseEClear").GetValue<bool>();

            var allminions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (allminions.Count > 0)
            {
                if (useQ && Q.IsReady() && allminions[0].IsValidTarget() && Player.Distance(allminions[0]) <= Q.Range)
                {
                    Q.Cast(allminions[0].Position);
                }

                if (useW && W.IsReady() && allminions[0].IsValidTarget())
                {
                    if (Player.Distance(allminions[0]) <= W.Range && (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1))
                    {
                        W.Cast();
                    }
                    else if (Player.Distance(allminions[0]) > W.Range && (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2))
                    {
                        W.Cast();
                    }

                }

                if (useE && E.IsReady() && allminions[0].IsValidTarget() && Player.Distance(allminions[0]) <= E.Range)
                {
                    E.Cast();
                }
            }
        }



        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;

            //Combo
            if (Player.Distance(target) <= Q.Range && Q.IsReady() && (Config.Item("UseQCombo").GetValue<bool>()))
            {

                Q.Cast(target);

            }
            if (W.IsReady() && (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1) && (Config.Item("UseWCombo").GetValue<bool>()))
                if (Player.ServerPosition.Distance(target.Position) < W.Range)
                {

                    W.Cast();

                }
            if (W.IsReady() && (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2) && (Config.Item("UseWCombo").GetValue<bool>()))
                if (Player.ServerPosition.Distance(target.Position) > W.Range)
                {


                    W.Cast();

                }
            if (Player.Distance(target) <= E.Range && E.IsReady() && (Config.Item("UseECombo").GetValue<bool>()))
            {

                E.Cast();

            }
            if (Config.Item("UseItems").GetValue<bool>())
            {
                if (Player.Distance(target) <= RDO.Range)
                {
                    RDO.Cast(target);
                }
                if (Player.Distance(target) <= HYD.Range)
                {
                    HYD.Cast(target);
                }
                if (Player.Distance(target) <= DFG.Range)
                {
                    DFG.Cast(target);
                }
                if (Player.Distance(target) <= BOTK.Range)
                {
                    BOTK.Cast(target);
                }
                if (Player.Distance(target) <= CUT.Range)
                {
                    CUT.Cast(target);
                }
                if (Player.Distance(target) <= 125f)
                {
                    YOY.Cast();
                }
                if (Player.Distance(target) <= TYM.Range)
                {
                    TYM.Cast(target);
                }
            }
            if (Config.Item("AutoR").GetValue<bool>())
            {

                if (GetNumberHitByR(target) >= Config.Item("CountR").GetValue<Slider>().Value)
                {
                    R.Cast(target, Config.Item("Packet").GetValue<bool>());
                }
            }


        }

        private static int GetNumberHitByR(Obj_AI_Base target) // Credits to Trelli For helping me with this one!
        {
            int totalHit = 0;
            foreach (Obj_AI_Hero current in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (current.IsEnemy && Vector3.Distance(Player.Position, current.Position) <= R.Range)
                {
                    totalHit = totalHit + 1;
                }
            }
            return totalHit;
        }




        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("CircleLag").GetValue<bool>()) // Credits to SKOBOL
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);
                }
                if (Config.Item("DrawW").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);
                }
                if (Config.Item("DrawE").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);
                }
                if (Config.Item("DrawR").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White);
                }
                if (Config.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White);
                }
                if (Config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White);
                }
                if (Config.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.White);
                }

            }
        }
    }
}
