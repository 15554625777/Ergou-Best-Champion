﻿#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Threading;
#endregion

namespace StonedSeriesAIO
{
    internal class Volibear
    {
        private const string Champion = "Volibear";

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

        private static Obj_AI_Hero Player;

        public Volibear()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 405);
            E = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 125);

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
            

            //Menu Volibear
            Config = new Menu("Stoned 狗熊", "StonedVolibear", true);

            //ts
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
            Config.SubMenu("Combo").AddItem(new MenuItem("CountW", "使用W|敌人血量").SetValue(new Slider(100, 0, 100)));
            Config.SubMenu("Combo").AddItem(new MenuItem("AutoR", "连招自动R")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("CountR", "使用R|范围内敌人").SetValue(new Slider(1, 5, 0)));
            Config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "连招键位!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass Menu
            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassE", "使用E骚扰")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "骚扰键位!").SetValue(new KeyBind(97, KeyBindType.Press)));

            //Drawings
            Config.AddSubMenu(new Menu("范围", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "显示 Q")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "显示 W")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "显示 E")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "显示 R")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "延迟自由圈").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "圈 厚度").SetValue(new Slider(1, 10, 1)));

            Config.AddToMainMenu();

            Game.OnGameUpdate += GameOnGameUpdate;
            Drawing.OnDraw += OnDraw;
            
            Game.PrintChat("<font color='#FF00BF'>Stoned Volibear Loaded By</font> <font color='#FF0000'>The</font><font color='#FFFF00'>Kush</font><font color='#40FF00'>Style</font>");
        }

        private static void GameOnGameUpdate(EventArgs args)
        {
            Player = ObjectManager.Player;

            Orbwalker.SetAttack(true);
            if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (Config.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                Harass();
            }
        
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
            if (target == null) return;

            if (Config.Item("HarassE").GetValue<bool>() && Player.Distance(target) <= E.Range && E.IsReady())
            {
                E.Cast();
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
            if (target == null) return;

            //Combo
            if (Player.Distance(target) <= Q.Range && Q.IsReady() && (Config.Item("UseQCombo").GetValue<bool>()))
            {
                Q.Cast();
            }
            if  (Player.Distance(target) <= E.Range && E.IsReady() && (Config.Item("UseECombo").GetValue<bool>()))
            {
                E.Cast();
            }
            //WLogic
           float health = target.Health;
           float maxhealth = target.MaxHealth;
           float wcount = Config.Item("CountW").GetValue<Slider>().Value;
            if (health < ((maxhealth * wcount)/100))
            {
                if (Config.Item("UseWCombo").GetValue<bool>() && W.IsReady())
                {
                    W.Cast(target);
                }
            }
            if (Config.Item("AutoR").GetValue<bool>() && R.IsReady() && (GetNumberHitByR(target) >= Config.Item("CountR").GetValue<Slider>().Value))
            {
                R.Cast();
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
            }
            
        }

        private static int GetNumberHitByR(Obj_AI_Base target)
        {
            int totalHit = 0;
            foreach (Obj_AI_Hero current in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (current.IsEnemy && Vector3.Distance(Player.ServerPosition, current.ServerPosition) <= R.Range)
                {
                    totalHit = totalHit + 1;
                }
            }
            return totalHit;
        }

        private static void OnDraw(EventArgs args)
        {            
                if (Config.Item("CircleLag").GetValue<bool>())
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
