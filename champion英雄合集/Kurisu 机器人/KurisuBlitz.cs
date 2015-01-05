using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace KurisuBlitz
{
    //   _____       _    _____           _ 
    //  |   __|___ _| |  |  |  |___ ___ _| |
    //  |  |  | . | . |  |     | .'|   | . |
    //  |_____|___|___|  |__|__|__,|_|_|___|
    //  Copyright © Kurisu Solutions 2014          
                                                        
    internal class KurisuBlitz
    {
        
        private static Menu _menu;
        private static Obj_AI_Hero _target;
        private static Orbwalking.Orbwalker _orbwalker;
        private static readonly Obj_AI_Hero _player = ObjectManager.Player;

        private static readonly Spell Q = new Spell(SpellSlot.Q, 925f);
        private static readonly Spell E = new Spell(SpellSlot.E, _player.AttackRange);
        private static readonly Spell R = new Spell(SpellSlot.R, 550f);

        private static readonly List<Spell> blitzDrawingList = new List<Spell>();
        //private static List<InterruptableSpell> blitzInterruptList = new List<InterruptableSpell>();

        public KurisuBlitz()
        {           
            Console.WriteLine("Blitzcrank assembly is loading...");
            CustomEvents.Game.OnGameLoad += BlitzOnLoad;
        }

        private void BlitzOnLoad(EventArgs args)
        {
            if (_player.ChampionName != "Blitzcrank") 
                return;

            // Set Q Prediction
            Q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);
            
            // Drawing List
            blitzDrawingList.Add(Q);
            blitzDrawingList.Add(R);

            // Load Menu
            BlitzMenu();

            // Load Drawings
            Drawing.OnDraw += BlitzOnDraw;

            // OnUpdate
            Game.OnGameUpdate += BlitzOnUpdate;

            // Interrupter
            Interrupter.OnPossibleToInterrupt += BlitzOnInterrupt;

        }

        private void BlitzOnInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (_menu.Item("interrupt").GetValue<bool>())
            {   if (unit.Distance(_player.Position) < Q.Range)
                    Q.Cast(unit);        
                else if (unit.Distance(_player.Position) < R.Range)
                    R.Cast();
            }
        }

        private void BlitzOnDraw(EventArgs args)
        {
            foreach (var spell in blitzDrawingList)
            {
                var circle = _menu.SubMenu("drawings").Item("draw" + spell.Slot).GetValue<Circle>();
                if (circle.Active)
                    Utility.DrawCircle(_player.Position, spell.Range, circle.Color, 5, 1);
            }

            if (_target.IsValidTarget(Q.Range*2))
                Utility.DrawCircle(_target.Position, _target.BoundingRadius, Color.Red, 10, 1);                    
        }


        private static void BlitzOnUpdate(EventArgs args)
        {

            _target = TargetSelector.GetSelectedTarget()
                ?? TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);

            // do KS
            GodKS(Q);
            GodKS(R);
            GodKS(E);

            var actualHealthSetting = _menu.Item("hneeded").GetValue<Slider>().Value;
            var actualHealthPercent = (int) (_player.Health/_player.MaxHealth*100);

            if (actualHealthPercent < actualHealthSetting) 
                return;

            // use the god hand
            GodHand(_target);


            // powerfist that hoe
            foreach (
                var e in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(e => e.Team != _player.Team && e.IsValidTarget(_player.AttackRange)))
            {
                if (_menu.Item("useE").GetValue<bool>() && !Q.IsReady())
                    E.CastOnUnit(_player);
            }
            
        }

        private static void GodHand(Obj_AI_Base target)
        {
            if (TargetSelector.GetSelectedTarget() != null && _target.Distance(_player.Position) > 1000)
                return;

            if (target.IsValidTarget() && Q.IsReady())
            {
                var prediction = Q.GetPrediction(target);
                if (_menu.Item("combokey").GetValue<KeyBind>().Active)
                {
                    if ((target.Distance(_player.Position) > _menu.Item("dneeded").GetValue<Slider>().Value)
                        && (target.Distance(_player.Position) < _menu.Item("dneeded2").GetValue<Slider>().Value))

                    {
                        if (_menu.Item("dograb" + target.SkinName).GetValue<StringList>().SelectedIndex == 0) 
                            return;

                        if (prediction.Hitchance >= HitChance.High &&
                            _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 2)
                        {
                            Q.Cast(prediction.CastPosition);
                        }

                        else if (prediction.Hitchance >= HitChance.Medium && 
                                 _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 1)
                        {
                            Q.Cast(prediction.CastPosition);
                        }

                        else if (prediction.Hitchance >= HitChance.Low && 
                                 _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 0)
                        {
                            Q.Cast(prediction.CastPosition);   
                        }   
                    }                
                }
            }

            foreach (
                var e in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            e => e.IsValidTarget(Q.Range) &&
                                _menu.Item("dograb" + e.SkinName).GetValue<StringList>().SelectedIndex == 2)) 
            {
                if (e.Distance(_player.Position) > _menu.Item("dneeded").GetValue<Slider>().Value)
                {
                    var prediction = Q.GetPrediction(e);
                    if (prediction.Hitchance == HitChance.Immobile && 
                        _menu.Item("immobile").GetValue<bool>())
                    {
                        Q.Cast(prediction.CastPosition);
                    }

                    if (prediction.Hitchance == HitChance.Dashing &&
                        _menu.Item("dashing").GetValue<bool>())
                    {
                        Q.Cast(prediction.CastPosition);
                    }
                }
            }
        }

                        
        private static void GodKS(Spell spell)
        {
            if (_menu.Item("killsteal" + spell.Slot).GetValue<bool>() && spell.IsReady())
            {
                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(e => e.IsValidTarget(spell.Range)))
                {
                    var ksDmg = _player.GetSpellDamage(enemy, spell.Slot);
                    if (ksDmg > enemy.Health)
                    {
                        if (spell.Slot.ToString() == "Q")
                        {
                            var po = spell.GetPrediction(enemy);
                            if (po.Hitchance >= HitChance.Medium)
                                spell.Cast(po.CastPosition);
                        }

                        else
                        {
                            spell.CastOnUnit(_player);
                        }
                    }
                }
            }
        }

        private static void BlitzMenu()
        {
            _menu = new Menu("Kurisu: »úÆ÷ÈË", "blitz", true);

            var blitzOrb = new Menu("Blitz: ×ß¿³", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(blitzOrb);
            _menu.AddSubMenu(blitzOrb);

            var blitzTS = new Menu("Blitz: Ä¿±êÑ¡Ôñ", "tselect");
            TargetSelector.AddToMenu(blitzTS);
            _menu.AddSubMenu(blitzTS);
            
            var menuD = new Menu("Blitz: ·¶Î§", "drawings");
            menuD.AddItem(new MenuItem("drawQ", "ÏÔÊ¾ Q")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            menuD.AddItem(new MenuItem("drawR", "ÏÔÊ¾ R")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            _menu.AddSubMenu(menuD);
            
            var menuG = new Menu("Blitz: ÉñÖ®ÊÖ", "autograb");
            menuG.AddItem(new MenuItem("hitchance", "ÃüÖÐÂÊ"))
                .SetValue(new StringList(new[] {"µÍ", "ÖÐ", "¸ß"}, 2));
            menuG.AddItem(new MenuItem("dneeded", "Ê¹ÓÃ Q ×îÐ¡¾àÀë")).SetValue(new Slider(255, 0, (int)Q.Range));
            menuG.AddItem(new MenuItem("dneeded2", "Ê¹ÓÃ Q ×î´óÐ¡¾àÀë")).SetValue(new Slider((int)Q.Range, 0, (int)Q.Range));
            menuG.AddItem(new MenuItem("dashing", "×Ô¶¯QÒÆ¶¯µÄÄ¿±ê")).SetValue(true);
            menuG.AddItem(new MenuItem("immobile", "×Ô¶¯Q²»¶¯µÄÄ¿±ê")).SetValue(true);
            menuG.AddItem(new MenuItem("hneeded", "½ûÓÃQ|ÉúÃüÖµµÍÓÚ %")).SetValue(new Slider(0));
            menuG.AddItem(new MenuItem("sep", ""));
            
            foreach (
                var e in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            e =>
                                e.Team != _player.Team))
            {
                menuG.AddItem(new MenuItem("dograb" + e.SkinName, e.SkinName))
                    .SetValue(new StringList(new[] {"½ûÓÃ Q ", "Õý³£ Q ", "×Ô¶¯ Q "}, 1));
            }

            _menu.AddSubMenu(menuG);

            var menuK = new Menu("Blitz: ÇÀÈËÍ·", "blitzks");
            menuK.AddItem(new MenuItem("killstealQ", "Ê¹ÓÃ Q")).SetValue(false);
            menuK.AddItem(new MenuItem("killstealE", "Ê¹ÓÃ E")).SetValue(false);
            menuK.AddItem(new MenuItem("killstealR", "Ê¹ÓÃ R")).SetValue(false);
            _menu.AddSubMenu(menuK);

            _menu.AddItem(new MenuItem("interrupt", "ÖÐ¶Ï·¨Êõ")).SetValue(true);
            _menu.AddItem(new MenuItem("useE", "Q¹ýÀ´×Ô¶¯E")).SetValue(true);
            _menu.AddItem(new MenuItem("combokey", "Á¬ÕÐ ¼üÎ»")).SetValue(new KeyBind(32, KeyBindType.Press));
            _menu.AddToMainMenu();
        }
    }
}
