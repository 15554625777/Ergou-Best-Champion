using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
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

            // OnGapCloser
            AntiGapcloser.OnEnemyGapcloser += BlitzOnGapcloser;

        }

        private void BlitzOnGapcloser(ActiveGapcloser gapcloser)
        {
            if (!_menu.Item("gapcloser").GetValue<bool>()) 
                return;

            foreach (
                var a in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(a => a.IsValidTarget() && a.Team == _player.Team))
            {

                var senderPos = gapcloser.End;
                var validPos = senderPos - Vector3.Normalize(_player.Position - senderPos)*Q.Range;

                if (_player.Distance(validPos) > a.Distance(a.ServerPosition))
                {
                    if (_player.Distance(validPos) > 200f)
                        Q.Cast(senderPos);
                }
            }
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
                    Utility.DrawCircle(_player.Position, spell.Range, circle.Color, 1, 1);
            }

            if (_target.IsValidTarget(Q.Range*2))
                Utility.DrawCircle(_target.Position, _target.BoundingRadius, Color.Red, 10, 1);                    
        }


        private void BlitzOnUpdate(EventArgs args)
        {
            try
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

                if (TargetSelector.GetSelectedTarget() == null || !(_target.Distance(_player.Position) > 750))
                {
                    TheGodHand(_target);
                }

                // powerfist that hoe
                    foreach (
                        var e in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where( e => e.Team != _player.Team && e.IsValidTarget(_player.AttackRange)))
                    {
                        if (_menu.Item("useE").GetValue<bool>() && !Q.IsReady())
                            E.CastOnUnit(_player);
                    }
                
            }
            catch (Exception ex)
            {
                //Game.PrintChat(ex.Message);
                Console.WriteLine(ex);
            }

        }

        private void TheGodHand(Obj_AI_Base target)
        {
            var keydown = _menu.Item("combokey").GetValue<KeyBind>().Active;
            if (TargetSelector.GetSelectedTarget() != null && _target.Distance(_player.Position) > 1000)
                return;

            if (target != null && Q.IsReady())
            {
                PredictionOutput prediction = Q.GetPrediction(target);
                if (keydown)
                {
                    if ((target.Distance(_player.Position) > _menu.Item("dneeded").GetValue<Slider>().Value)
                        && (target.Distance(_player.Position) < _menu.Item("dneeded2").GetValue<Slider>().Value))
                    if (_menu.Item("dograb" + target.SkinName).GetValue<StringList>().SelectedIndex == 0) return;
                    if (prediction.Hitchance == HitChance.High && _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 2)
                            Q.Cast(prediction.CastPosition);
                    else if (prediction.Hitchance == HitChance.Medium && _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 1)
                        Q.Cast(prediction.CastPosition);
                    else if (prediction.Hitchance == HitChance.Low && _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 0)
                        Q.Cast(prediction.CastPosition);      
                    
                }
            }

            foreach (
                   var e in
                       ObjectManager.Get<Obj_AI_Hero>()
                           .Where(
                               e =>
                                   e.Team != _player.Team && !e.IsDead && e.IsValid &&
                                   Vector2.DistanceSquared(_player.Position.To2D(), e.ServerPosition.To2D()) <
                                   Q.Range * Q.Range && _menu.Item("dograb" + e.SkinName).GetValue<StringList>().SelectedIndex == 2))
            {
                if (e.Distance(_player.Position) > _menu.Item("dneeded").GetValue<Slider>().Value)
                {
                    PredictionOutput prediction = Q.GetPrediction(e);
                    if (prediction.Hitchance == HitChance.Immobile && _menu.Item("immobile").GetValue<bool>())
                        Q.Cast(prediction.CastPosition);
                    if (prediction.Hitchance == HitChance.Dashing && _menu.Item("dashing").GetValue<bool>())
                        Q.Cast(prediction.CastPosition);
                }
            }
        }

                        
        private void GodKS(Spell spell)
        {
            if (_menu.Item("killsteal" + spell.Slot).GetValue<bool>() && spell.IsReady())
            {
                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(e => e.Team != _player.Team && e.Distance(_player.Position) < spell.Range))
                {
                    var ksDmg = _player.GetSpellDamage(enemy, spell.Slot);
                    if (ksDmg > enemy.Health)
                    {
                        var po = spell.GetPrediction(enemy);
                        if (po.Hitchance >= HitChance.Medium)
                            spell.Cast(po.CastPosition);
                    }

                }
            }
        }

        private void BlitzMenu()
        {
            _menu = new Menu("Kurisu:»úÆ÷ÈË", "blitz", true);

            var blitzOrb = new Menu("Blitz: ×ß¿³", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(blitzOrb);
            _menu.AddSubMenu(blitzOrb);

            var blitzTS = new Menu("Blitz: Ä¿±êÑ¡Ôñ", "tselect");
            TargetSelector.AddToMenu(blitzTS);
            _menu.AddSubMenu(blitzTS);
            
            var menuD = new Menu("Blitz: ·¶Î§", "drawings");
            menuD.AddItem(new MenuItem("drawQ", "·¶Î§ Q")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            menuD.AddItem(new MenuItem("drawR", "·¶Î§ R")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            _menu.AddSubMenu(menuD);
            
            var menuG = new Menu("Blitz: ÉñÖ®ÊÖ", "autograb");
            menuG.AddItem(new MenuItem("hitchance", "ÃüÖÐÂÊ"))
                .SetValue(new StringList(new[] {"µÍ", "ÖÐ", "¸ß"}, 2));
            menuG.AddItem(new MenuItem("dneeded", "×î½ü¾àÀëQ")).SetValue(new Slider(255, 0, (int)Q.Range));
            menuG.AddItem(new MenuItem("dneeded2", "×îÔ¶¾àÀëQ")).SetValue(new Slider((int)Q.Range, 0, (int)Q.Range));
            menuG.AddItem(new MenuItem("dashing", "×Ô¶¯QÒÆ¶¯µÄµÐÈË")).SetValue(true);
            menuG.AddItem(new MenuItem("immobile", "×Ô¶¯Q²»¶¯µÄµÐÈË")).SetValue(true);
            menuG.AddItem(new MenuItem("hneeded", "²»Q|ÉúÃüÖµµÍÓÚ%")).SetValue(new Slider(0));
            menuG.AddItem(new MenuItem("sep", ""));
            
            foreach (
                var e in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            e =>
                                e.Team != _player.Team))
            {
                menuG.AddItem(new MenuItem("QÉèÖÃ" + e.SkinName, e.SkinName))
                    .SetValue(new StringList(new[] {"½ûÓÃQ ", "Õý³£Q ", "×Ô¶¯Q "}, 1));
            }
            _menu.AddSubMenu(menuG);

            var menuK = new Menu("Blitz: ÇÀÈËÍ·", "blitzks");
            menuK.AddItem(new MenuItem("killstealQ", "Ê¹ÓÃ Q")).SetValue(false);
            menuK.AddItem(new MenuItem("killstealE", "Ê¹ÓÃ E")).SetValue(false);
            menuK.AddItem(new MenuItem("killstealR", "Use R")).SetValue(false);
            _menu.AddSubMenu(menuK);

            _menu.AddItem(new MenuItem("gapcloser", "×Ô¶¯W¸úÉÏµÐÈË")).SetValue(true);
            _menu.AddItem(new MenuItem("interrupt", "ÖÐ¶Ï·¨Êõ")).SetValue(true);
            _menu.AddItem(new MenuItem("useE", "QÖÐ×Ô¶¯E")).SetValue(true);
            _menu.AddItem(new MenuItem("combokey", "Á¬ÕÐ¼üÎ»")).SetValue(new KeyBind(32, KeyBindType.Press));
            _menu.AddToMainMenu();
        }
    }
}
