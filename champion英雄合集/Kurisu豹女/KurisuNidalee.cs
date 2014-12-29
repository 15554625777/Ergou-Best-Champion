using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace KurisuNidalee
{
    //  _____ _   _     _         
    // |   | |_|_| |___| |___ ___ 
    // | | | | | . | .'| | -_| -_|
    // |_|___|_|___|__,|_|___|___|
    // Copyright © Kurisu Solutions 2014

    internal class KurisuNidalee
    {

        private static Menu MainMenu;
        private static Obj_AI_Base Target;
        private static Orbwalking.Orbwalker Orb;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;
        private static bool CougarForm;

        public KurisuNidalee()
        {
            Console.WriteLine("KurisuNidalee is loading...");
            CustomEvents.Game.OnGameLoad += Initialize;
        }

        // human form
        private static Spell javelin = new Spell(SpellSlot.Q, 1500f);
        private static Spell bushwack = new Spell(SpellSlot.W, 900f);
        private static Spell primalsurge = new Spell(SpellSlot.E, 650f);

        // cougar form
        private static Spell takedown = new Spell(SpellSlot.Q, 200f);
        private static Spell pounce = new Spell(SpellSlot.W, 375f);
        private static Spell swipe = new Spell(SpellSlot.E, 300f);
        private static Spell aspectofcougar = new Spell(SpellSlot.R);

        private static readonly SpellDataInst NidaData = Me.Spellbook.GetSpell(SpellSlot.Q);
        private static readonly List<Spell> CougarList = new List<Spell>();
        private static readonly List<Spell> HumanList = new List<Spell>();
        private static IEnumerable<int> NidaItems = new[] {3128, 3144, 3153, 3092};

        private static bool Packets()
        {
            return MainMenu.Item("usepackets").GetValue<bool>();
        }

        private static bool TargetHunted(Obj_AI_Base target)
        {
            return target.HasBuff("nidaleepassivehunted", true);
        }
        
        private static readonly string[] jungleminions =
        {
            "SRU_Razorbeak", "SRU_Krug", "Sru_Crab",
            "SRU_Baron", "SRU_Dragon", "SRU_Blue", "SRU_Red", "SRU_Murkwolf", "SRU_Gromp"     
        };


        #region Nidalee: Initialize
        private void Initialize(EventArgs args)
        {
            // Check champion
            if (Me.ChampionName != "Nidalee") 
                return;

            // Load main menu
            NidaMenu();

            // Add drawing skill list
            CougarList.AddRange(new[] { takedown, pounce, swipe });
            HumanList.AddRange(new[] { javelin, bushwack, primalsurge });

            // Set skillshot prediction
            javelin.SetSkillshot(0.125f, 40f, 1300f, true, SkillshotType.SkillshotLine);
            bushwack.SetSkillshot(0.50f, 100f, 1500f, false, SkillshotType.SkillshotCircle);
            swipe.SetSkillshot(0.50f, 375f, 1500f, false, SkillshotType.SkillshotCone);
            pounce.SetSkillshot(0.50f, 400f, 1500f, false, SkillshotType.SkillshotCone);

            // GameOnGameUpdate Event
            Game.OnGameUpdate += NidaleeOnUpdate;

            // DrawingOnDraw Event
            Drawing.OnDraw += NidaleeOnDraw;

            // OnProcessSpellCast Event
            Obj_AI_Base.OnProcessSpellCast += NidaleeTracker;

        }

        #endregion

        #region Nidalee: Menu
        private static void NidaMenu()
        {
            MainMenu = new Menu("Kurisu: ±™≈Æ", "nidalee", true);

            var nidaOrb = new Menu("±™≈Æ: ◊ﬂø≥", "orbwalker");
            Orb = new Orbwalking.Orbwalker(nidaOrb);
           
            MainMenu.AddSubMenu(nidaOrb);

            var nidaTS = new Menu("±™≈Æ: ƒø±Í—°‘Ò", "target selecter");
            TargetSelector.AddToMenu(nidaTS);
            MainMenu.AddSubMenu(nidaTS);

            var nidaKeys = new Menu("±™≈Æ: º¸Œª…Ë÷√", "keybindongs");
            nidaKeys.AddItem(new MenuItem("usecombo", "¡¨’–")).SetValue(new KeyBind(32, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("useharass", "…ß»≈")).SetValue(new KeyBind(67, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("usejungle", "«Â“∞")).SetValue(new KeyBind(86, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("useclear", "«Âœﬂ")).SetValue(new KeyBind(86, KeyBindType.Press));
            MainMenu.AddSubMenu(nidaKeys);

            var nidaSpells = new Menu("±™≈Æ: ¡¨’–…Ë÷√", "spells");
            
            nidaSpells.AddItem(new MenuItem("usehumanq", " π”√±Í«πÕ∂÷¿")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("seth", "√¸÷–¬ ")).SetValue(new StringList(new[] { "µÕ|", "÷–|", "∏ﬂ|" }, 2));
            nidaSpells.AddItem(new MenuItem("usehumanw", " π”√¥‘¡÷∑¸ª˜")).SetValue(true);
            nidaSpells.AddItem(new MenuItem(" ", " "));
            nidaSpells.AddItem(new MenuItem("usecougarq", " π”√∆Àµπ")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usecougarw", " π”√√Õ∆À")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("setp", "◊Ó–°√Õ∆Àæ‡¿Î")).SetValue(new Slider(100, 15, 300));
            nidaSpells.AddItem(new MenuItem("usecougare", " π”√±™–ŒÃ¨")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usecougarr", "◊‘∂Ø±™–ŒÃ¨«Â±¯«Â“∞")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("useitems", " π”√ŒÔ∆∑")).SetValue(true);
            MainMenu.AddSubMenu(nidaSpells);

            var nidaHeals = new Menu("±™≈Æ: ÷Œ”˙…Ë÷√", "hengine");
            nidaHeals.AddItem(new MenuItem("usedemheals", "∆Ù”√")).SetValue(true);
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
            {
                nidaHeals.AddItem(new MenuItem("heal" + hero.SkinName, hero.SkinName)).SetValue(true);
                nidaHeals.AddItem(new MenuItem("healpct" + hero.SkinName, hero.SkinName + " heal %")).SetValue(new Slider(50));
            }

            nidaHeals.AddItem(new MenuItem("healmanapct", "Ω˚”√÷Œ”˙£®ƒß∑®÷µ£©%")).SetValue(new Slider(40));
            MainMenu.AddSubMenu(nidaHeals);

            var nidaHarass = new Menu("±™≈Æ: …ß»≈…Ë÷√", "harass");
            nidaHarass.AddItem(new MenuItem("usehumanq2", " π”√±Í«πÕ∂÷¿")).SetValue(true);
            nidaHarass.AddItem(new MenuItem("humanqpct", "Ω˚”√Q£®∑®¡¶÷µ£©%")).SetValue(new Slider(70));
            MainMenu.AddSubMenu(nidaHarass);

            var nidaClear = new Menu("±™≈Æ: «Âœﬂ…Ë÷√", "laneclear");
            nidaClear.AddItem(new MenuItem("clearhumanq", " π”√±Í«πÕ∂÷¿")).SetValue(false);
            nidaClear.AddItem(new MenuItem(" ", " "));
            nidaClear.AddItem(new MenuItem("clearcougarq", " π”√Õ∆µΩ")).SetValue(true);
            nidaClear.AddItem(new MenuItem("clearcougarw", " π”√√Õ∆À")).SetValue(true);
            nidaClear.AddItem(new MenuItem("clearcougare", " π”√±™–ŒÃ¨")).SetValue(true);
            nidaClear.AddItem(new MenuItem("clearcougarr", "◊‘∂Ø±™–ŒÃ¨«Âœﬂ")).SetValue(false);
            nidaClear.AddItem(new MenuItem("clearpct", "Ω˚”√«Âœﬂ£®∑®¡¶÷µ£©%")).SetValue(new Slider(55));
            MainMenu.AddSubMenu(nidaClear);

            var nidaJungle = new Menu("±™≈Æ: «Â“∞…Ë÷√", "jungleclear");
            nidaJungle.AddItem(new MenuItem("jghumanq", " π”√±Í«πÕ∂÷¿")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jghumanw", " π”√¥‘¡÷∑¸ª˜")).SetValue(true);
            nidaJungle.AddItem(new MenuItem(" ", " "));
            nidaJungle.AddItem(new MenuItem("jgcougarq", " π”√Õ∆µΩ")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougarw", " π”√√Õ∆À")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougare", " π”√±™–ŒÃ¨")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougarr", "◊‘∂Ø±™–ŒÃ¨«Â“∞")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgrpct", "Ω˚”√«Â“∞£®∑®¡¶÷µ£©%")).SetValue(new Slider(55, 0, 100));
            MainMenu.AddSubMenu(nidaJungle);

            var nidaD = new Menu("±™≈Æ: ∑∂Œß…Ë÷√", "drawings");
            nidaD.AddItem(new MenuItem("drawQ", "∑∂Œß Q")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            nidaD.AddItem(new MenuItem("drawW", "∑∂Œß W")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            nidaD.AddItem(new MenuItem("drawE", "∑∂Œß E")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            nidaD.AddItem(new MenuItem("drawcds", "œ‘ æººƒ‹¿‰»¥")).SetValue(true);
            MainMenu.AddSubMenu(nidaD);

            MainMenu.AddItem(new MenuItem("useignote", " π”√µ„»º")).SetValue(true);
            MainMenu.AddItem(new MenuItem("usepackets", " π”√∑‚∞¸")).SetValue(true);
            MainMenu.AddToMainMenu();

            Game.PrintChat("<font color=\"#FFAF4D\">[</font><font color=\"#FFA333\">Ë±πÂ•≥</font><font color=\"#FFAF4D\">]</font><font color=\"#FF8C00\"> - <u>the Bestial Huntress Rev106Âä†ËΩΩÊàêÂäü!Ê±âÂåñby‰∫åÁãó!QQÁæ§|361630847  </u>  </font>- Kurisu");

        }

        #endregion

        #region Nidalee: OnTick
        private void NidaleeOnUpdate(EventArgs args)
        {
            CougarForm = NidaData.Name != "JavelinToss";
            Target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Magical);

            ProcessCooldowns();
            PrimalSurge();

            if (MainMenu.Item("usecombo").GetValue<KeyBind>().Active)
                UseCombo(Target);
            if (MainMenu.Item("useharass").GetValue<KeyBind>().Active)
                UseHarass(Target);
            if (MainMenu.Item("useclear").GetValue<KeyBind>().Active)
                UseLaneclear();
            if (MainMenu.Item("usejungle").GetValue<KeyBind>().Active )
                UseJungleclear();

        }

        #endregion

        #region Nidalee : Misc

        private void UseInventoryItems(IEnumerable<int> items, Obj_AI_Base target)
        {
            if (!MainMenu.Item("useitems").GetValue<bool>())
                return;

            foreach (var i in items.Where(x => Items.CanUseItem(x) && Items.HasItem(x)))
            {
                if (target.IsValidTarget(800))
                {
                    if (i == 3092)
                        Items.UseItem(i, target.ServerPosition);
                    else
                    {
                        Items.UseItem(i);
                        Items.UseItem(i, target);
                    }
                }
            }
        }

        private static bool CanKillAA(Obj_AI_Base target)
        {
            var damage = 0d;

            if (target.IsValidTarget(Me.AttackRange + 30))
                damage = Me.GetAutoAttackDamage(target);

            return target.Health <= (float) damage*5;
        }

        private static float CougarDamage(Obj_AI_Base target)
        {
            var damage = 0d;

            if (CQ < 1)
                damage += Me.GetSpellDamage(target, SpellSlot.Q, 1);
            if (CW < 1)
                damage += Me.GetSpellDamage(target, SpellSlot.W, 1);
            if (CE < 1)
                damage += Me.GetSpellDamage(target, SpellSlot.E, 1);

            return (float) damage;
        }

        #endregion

        #region Nidalee: SBTW

        private void UseCombo(Obj_AI_Base target)
        {
            var minPounce 
                = MainMenu.Item("setp").GetValue<Slider>().Value;
            var hitchance 
                = MainMenu.Item("seth").GetValue<StringList>().SelectedIndex;
  
            // Cougar combo
            if (CougarForm && target.IsValidTarget(javelin.Range))
            {             
                UseInventoryItems(NidaItems, target);

                // Check if takedown is ready (on unit)
                if (CQ == 0
                    && MainMenu.Item("usecougarq").GetValue<bool>()
                    && target.Distance(Me.ServerPosition) <= takedown.Range)
                {
                    takedown.CastOnUnit(Me, Packets());
                }

                // Check is pounce is ready 
                if (CW == 0
                    && MainMenu.Item("usecougarw").GetValue<bool>()
                    && target.Distance(Me.ServerPosition) > minPounce)
                {
                    if (TargetHunted(target) & target.Distance(Me.ServerPosition) <= 750)
                        pounce.Cast(target.ServerPosition, Packets()); 
                    if (!TargetHunted(target) && target.Distance(Me.ServerPosition) <= 350)
                        pounce.Cast(target.ServerPosition, Packets());
                }

                // Check if swipe is ready (prediction)
                if (CE == 0 
                    && MainMenu.Item("usecougare").GetValue<bool>())
                {
                    var prediction = swipe.GetPrediction(target);
                    if (prediction.Hitchance >= HitChance.Medium && target.Distance(Me.Position) <= swipe.Range)
                        swipe.Cast(prediction.CastPosition, Packets());
                }


                // force transform if q ready and no collision
                if (HQ == 0)
                {
                    var prediction = javelin.GetPrediction(target);
                    if (prediction.Hitchance != HitChance.Collision)
                        aspectofcougar.Cast();              
                }

                // Switch to human form no cougar spell are ready and or q not in range and can kill in 5 aa       
                if (CW > 1 && CE > 1 && (CQ > 1 || target.Distance(Me.ServerPosition) > takedown.Range) && CanKillAA(target) 
                    && MainMenu.Item("usecougarr").GetValue<bool>() && target.Distance(Me.ServerPosition) <= Me.AttackRange + 5)
                {
                    if (aspectofcougar.IsReady())
                        aspectofcougar.Cast();
                }

                // Switch to human form if cougar W/E are not ready and Q is not ready or not in range and Q ready
                if (CW > 1 && CE > 1 && (CQ > 1 || target.Distance(Me.ServerPosition) > takedown.Range) && HQ < 1
                    && MainMenu.Item("usecougarr").GetValue<bool>() && target.Distance(Me.ServerPosition) <= javelin.Range)
                {
                    var prediction = javelin.GetPrediction(target);
                    if (prediction.Hitchance != HitChance.Collision)
                        aspectofcougar.Cast();
                }
            }

            // Human combo
            if (!CougarForm && target.IsValidTarget(javelin.Range))
            {
                // Switch to cougar if target hunted or can kill target 
                if (aspectofcougar.IsReady() 
                    && MainMenu.Item("usecougarr").GetValue<bool>() 
                    && (TargetHunted(target) || target.Health <= CougarDamage(target)))
                {

                    if (TargetHunted(target) && target.Distance(Me.ServerPosition, true) <= 750*750)
                        aspectofcougar.Cast();
                    if (target.Health <= CougarDamage(target))
                        aspectofcougar.Cast();
                }

                if (HQ == 0
                    && MainMenu.Item("usehumanq").GetValue<bool>())
                {
                    var prediction = javelin.GetPrediction(target);
                    switch (hitchance)
                    {
                        case 0:
                            if (prediction.Hitchance == HitChance.Low)
                                javelin.Cast(prediction.CastPosition, Packets());
                            break;
                        case 1:
                            if (prediction.Hitchance == HitChance.Medium)
                                javelin.Cast(prediction.CastPosition, Packets());
                            break;
                        case 2:
                            if (prediction.Hitchance == HitChance.High)
                                javelin.Cast(prediction.CastPosition, Packets());
                            break;
                    }
                }

                // Check bushwack and cast underneath targets feet.
                if (HW == 0
                    && MainMenu.Item("usehumanw").GetValue<bool>()
                    && target.Distance(Me.Position) <= bushwack.Range)
                {
                    bushwack.Cast(target.Position, Packets());
                }
            }
        }
        #endregion

        #region Nidalee: Harass

        private void UseHarass(Obj_AI_Base target)
        {
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var minPercent = MainMenu.Item("humanqpct").GetValue<Slider>().Value;
            if (!CougarForm && HQ == 0 && MainMenu.Item("usehumanq2").GetValue<bool>())
            {
                var prediction = javelin.GetPrediction(target);
                if (target.Distance(Me.ServerPosition) <= javelin.Range && actualHeroManaPercent > minPercent)
                {
                    switch (MainMenu.Item("seth").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            if (prediction.Hitchance == HitChance.Low)
                                javelin.Cast(prediction.CastPosition, Packets());
                            break;
                        case 1:
                            if (prediction.Hitchance == HitChance.Medium)
                                javelin.Cast(prediction.CastPosition, Packets());
                            break;
                        case 2:
                            if (prediction.Hitchance >= HitChance.High)
                                javelin.Cast(prediction.CastPosition, Packets());
                            break;
                    }
                }                    
            }
        }

        #endregion

        #region Nidalee: PrimalSurge

        private void PrimalSurge()
        {
            if (HE > 1 || !MainMenu.Item("usedemheals").GetValue<bool>()) 
                return;

            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var selfManaPercent = MainMenu.Item("healmanapct").GetValue<Slider>().Value;

            foreach (
                var hero in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsValidTarget(primalsurge.Range, false) && hero.IsAlly)) 
            {

                if (!CougarForm && MainMenu.Item("heal" + hero.SkinName).GetValue<bool>() && !Me.HasBuff("Recall"))
                {
                    var needed = MainMenu.Item("healpct" +hero.SkinName).GetValue<Slider>().Value;
                    var hp = (int)((hero.Health / hero.MaxHealth) * 100);

                    if (actualHeroManaPercent > selfManaPercent && hp <= needed)
                        primalsurge.CastOnUnit(hero, Packets());
                }
            }
        }

        #endregion

        #region Nidalee: Jungleclear

        private void UseJungleclear()
        {
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var minPercent = MainMenu.Item("jgrpct").GetValue<Slider>().Value;

            foreach (
                var m in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(m => m.IsValidTarget(1500) && jungleminions.Any(name => m.Name.StartsWith(name) && !name.Contains("Mini")))) 
            {
                if (CougarForm)
                {
                    if (MainMenu.Item("jgcougare").GetValue<bool>() && m.Distance(Me.Position) < swipe.Range)
                        if (swipe.IsReady())
                            swipe.Cast(m.Position);
                    if (MainMenu.Item("jgcougarw").GetValue<bool>() && m.Distance(Me.Position) < pounce.Range)
                        if (pounce.IsReady())
                            pounce.Cast(m.Position);
                    if (MainMenu.Item("jgcougarq").GetValue<bool>() && m.Distance(Me.Position) < takedown.Range)
                        if (takedown.IsReady())
                            takedown.CastOnUnit(Me);
                }
                else
                {
                    if (MainMenu.Item("jghumanq").GetValue<bool>() && actualHeroManaPercent > minPercent)
                        if (javelin.IsReady())
                            javelin.Cast(m.Position);
                    if (MainMenu.Item("jghumanw").GetValue<bool>() && m.Distance(Me.Position) < bushwack.Range && actualHeroManaPercent > minPercent)
                        if (bushwack.IsReady())
                            bushwack.Cast(m.Position);
                    if (!javelin.IsReady() && MainMenu.Item("jgcougarr").GetValue<bool>() && m.Distance(Me.Position) < pounce.Range && actualHeroManaPercent > minPercent)
                        if (aspectofcougar.IsReady())
                            aspectofcougar.Cast();
                }
            }
        }

        #endregion

        #region Nidalee: Laneclear

        private void UseLaneclear()
        {
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var minPercent = MainMenu.Item("clearpct").GetValue<Slider>().Value;

            foreach (
                var m in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(m => m.IsValidTarget(javelin.Range) && jungleminions.Any(name => !m.Name.StartsWith(name)))) 
            {
                if (CougarForm)
                {
                    if (MainMenu.Item("clearcougare").GetValue<bool>() && m.Distance(Me.Position) < swipe.Range)
                        if (swipe.IsReady())
                            swipe.Cast(m);
                    if (MainMenu.Item("clearcougarw").GetValue<bool>() && m.Distance(Me.Position) < pounce.Range)
                        if (pounce.IsReady())
                            pounce.Cast(m.Position);
                    if (MainMenu.Item("clearcougarq").GetValue<bool>() && m.Distance(Me.Position) < takedown.Range)
                        if (takedown.IsReady())
                            takedown.CastOnUnit(Me);
                }
                else
                {
                    if (actualHeroManaPercent > minPercent
                        && MainMenu.Item("clearhumanq").GetValue<bool>() 
                        && javelin.IsReady())
                    {
                        javelin.Cast(m.Position);
                    }

                    if ((!javelin.IsReady() || !MainMenu.Item("clearhumanq").GetValue<bool>())
                        && MainMenu.Item("clearcougarr").GetValue<bool>()
                        && m.Distance(Me.Position) < pounce.Range)
                    {
                        aspectofcougar.Cast();
                    }
                }
            }
        }

        #endregion
         
        #region Nidalee: Tracker

        // timer trackers credits to detuks or whom he got it from
        private void NidaleeTracker(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
                GetCooldowns(args);
        }

        private static readonly float[] humanQcd = { 6, 6, 6, 6, 6 };
        private static readonly float[] humanWcd = { 13, 12, 11, 10, 9 };
        private static readonly float[] humanEcd = { 12, 12, 12, 12, 12 };

        private static float CQRem, CWRem, CERem;
        private static float HQRem, HWRem, HERem;
        private static float CQ, CW, CE;
        private static float HQ, HW, HE;

        private void ProcessCooldowns()
        {
            if (Me.IsDead) 
                return;

            CQ = ((CQRem - Game.Time) > 0) ? (CQRem - Game.Time) : 0;
            CW = ((CWRem - Game.Time) > 0) ? (CWRem - Game.Time) : 0;
            CE = ((CERem - Game.Time) > 0) ? (CERem - Game.Time) : 0;
            HQ = ((HQRem - Game.Time) > 0) ? (HQRem - Game.Time) : 0;
            HW = ((HWRem - Game.Time) > 0) ? (HWRem - Game.Time) : 0;
            HE = ((HERem - Game.Time) > 0) ? (HERem - Game.Time) : 0;
        }

        private static float CalculateCd(float time)
        {
            return time + (time * Me.PercentCooldownMod);
        }

        private void GetCooldowns(GameObjectProcessSpellCastEventArgs spell)
        {
            if (CougarForm)
            {
                if (spell.SData.Name == "Takedown")
                    CQRem = Game.Time + CalculateCd(5);
                if (spell.SData.Name == "Pounce")
                    CWRem = Game.Time + CalculateCd(5);
                if (spell.SData.Name == "Swipe")
                    CERem = Game.Time + CalculateCd(5);
            }
            else
            {
                if (spell.SData.Name == "JavelinToss")
                    HQRem = Game.Time + CalculateCd(humanQcd[javelin.Level-1]);
                if (spell.SData.Name == "Bushwhack")
                    HWRem = Game.Time + CalculateCd(humanWcd[bushwack.Level-1]);
                if (spell.SData.Name == "PrimalSurge")
                    HERem = Game.Time + CalculateCd(humanEcd[primalsurge.Level-1]);
            }
        }

        #endregion

        #region Nidalee: On Draw
        private void NidaleeOnDraw(EventArgs args)
        {

            if (Target != null) Utility.DrawCircle(Target.Position, Target.BoundingRadius, Color.Red, 1, 1);

            foreach (var spell in CougarList)
            {
                var circle = MainMenu.Item("draw" + spell.Slot).GetValue<Circle>();
                if (circle.Active && CougarForm && !Me.IsDead)
                    Utility.DrawCircle(Me.Position, spell.Range, circle.Color, 1, 1);
            }

            foreach (var spell in HumanList)
            {
                var circle = MainMenu.Item("draw" + spell.Slot).GetValue<Circle>();
                if (circle.Active && !CougarForm && !Me.IsDead)
                    Utility.DrawCircle(Me.Position, spell.Range, circle.Color, 1, 1);
            }

            if (!MainMenu.Item("drawcds").GetValue<bool>()) return;

            var wts = Drawing.WorldToScreen(Me.Position);

            if (!CougarForm) // lets show cooldown timers for the opposite form :)
            {
                if (Me.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q: Null");
                else if (CQ == 0)
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q: Ready");
                else
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.Orange, "Q: " + CQ.ToString("0.0"));
                if (Me.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W: Null");
                else if (CW == 0)
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W: Ready");
                else
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.Orange, "W: " + CW.ToString("0.0"));
                if (Me.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0], wts[1], Color.White, "E: Null");
                else if (CE == 0)
                    Drawing.DrawText(wts[0], wts[1], Color.White, "E: Ready");
                else
                    Drawing.DrawText(wts[0], wts[1], Color.Orange, "E: " + CE.ToString("0.0"));

            }
            else
            {
                if (Me.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q: Null");
                else if (HQ == 0)
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q: Ready");
                else
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.Orange, "Q: " + HQ.ToString("0.0"));
                if (Me.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W: Null");
                else if (HW == 0)
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W: Ready");
                else
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.Orange, "W: " + HW.ToString("0.0"));
                if (Me.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0], wts[1], Color.White, "E: Null");
                else if (HE == 0)
                    Drawing.DrawText(wts[0], wts[1], Color.White, "E: Ready");
                else
                    Drawing.DrawText(wts[0], wts[1], Color.Orange, "E: " + HE.ToString("0.0"));

            }
        }

        #endregion
    }
}
