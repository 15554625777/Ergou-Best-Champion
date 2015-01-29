using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.Collections.Generic;
using Color = System.Drawing.Color;

namespace KurisuNidalee
{
    //  _____ _   _     _         
    // |   | |_|_| |___| |___ ___ 
    // | | | | | . | .'| | -_| -_|
    // |_|___|_|___|__,|_|___|___|
    // Copyright © Kurisu Solutions 2015

    internal class Program
    {
        private static Menu _mainMenu;
        private static Obj_AI_Base _target;
        private static Orbwalking.Orbwalker _orbwalker;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;
        private static bool _cougarForm;
        private static bool _hasBlue;

        static void Main(string[] args)
        {
            Console.WriteLine("KurisuNidalee injected..");
            CustomEvents.Game.OnGameLoad += Initialize;
        }

        private static readonly Spell Javelin = new Spell(SpellSlot.Q, 1500f);
        private static readonly Spell Bushwack = new Spell(SpellSlot.W, 900f);
        private static readonly Spell Primalsurge = new Spell(SpellSlot.E, 650f);
        private static readonly Spell Takedown = new Spell(SpellSlot.Q, 200f);
        private static readonly Spell Pounce = new Spell(SpellSlot.W, 375f);
        private static readonly Spell Swipe = new Spell(SpellSlot.E, 300f);
        private static readonly Spell Aspectofcougar = new Spell(SpellSlot.R);

        private static readonly List<Spell> HumanSpellList = new List<Spell>();
        private static readonly List<Spell> CougarSpellList = new List<Spell>();
        private static readonly IEnumerable<int> NidaItems = new[] { 3128, 3144, 3153, 3092 };

        private static bool TargetHunted(Obj_AI_Base target)
        {
            return target.HasBuff("nidaleepassivehunted", true);
        }

        private static readonly string[] Jungleminions =
        {
            "SRU_Razorbeak", "SRU_Krug", "Sru_Crab",
            "SRU_Baron", "SRU_Dragon", "SRU_Blue", "SRU_Red", "SRU_Murkwolf", "SRU_Gromp"     
        };

        #region Nidalee: Initialize
        private static void Initialize(EventArgs args)
        {
            // Check champion
            if (Me.ChampionName != "Nidalee")
            {
                return;
            }

            // Load main menu
            NidaMenu();

            // Add drawing skill list
            CougarSpellList.AddRange(new[] { Takedown, Pounce, Swipe });
            HumanSpellList.AddRange(new[] { Javelin, Bushwack, Primalsurge });

            // Set skillshot prediction (i has rito decode now)
            Javelin.SetSkillshot(0.125f, 40f, 1300f, true, SkillshotType.SkillshotLine);
            Bushwack.SetSkillshot(0.50f, 100f, 1500f, false, SkillshotType.SkillshotCircle);
            Swipe.SetSkillshot(0.50f, 375f, 1500f, false, SkillshotType.SkillshotCone);
            Pounce.SetSkillshot(0.50f, 400f, 1500f, false, SkillshotType.SkillshotCone);

            // GameOnGameUpdate Event
            Game.OnGameUpdate += NidaleeOnUpdate;

            // DrawingOnDraw Event
            Drawing.OnDraw += NidaleeOnDraw;

            // OnProcessSpellCast Event
            Obj_AI_Base.OnProcessSpellCast += NidaleeTracker;

            // AntiGapcloer Event
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!_mainMenu.Item("gapcloser").GetValue<bool>())
                return;

            var attacker = gapcloser.Sender;
            if (attacker.IsValidTarget(Javelin.Range))
            {
                if (!_cougarForm)
                {
                    var prediction = Javelin.GetPrediction(attacker);
                    if (prediction.Hitchance != HitChance.Collision && HQ == 0)
                        Javelin.Cast(prediction.CastPosition);

                    if (Aspectofcougar.IsReady())
                        Aspectofcougar.Cast();
                }

                if (_cougarForm)
                {
                    if (attacker.Distance(Me.ServerPosition) <= Takedown.Range && CQ == 0)
                        Takedown.CastOnUnit(Me);
                    if (attacker.Distance(Me.ServerPosition) <= Swipe.Range && CE == 0)
                        Swipe.Cast(attacker.ServerPosition);
                }
            }
        }

        #endregion

        #region Nidalee: Menu
        private static void NidaMenu()
        {
            _mainMenu = new Menu("Kurisu豹女", "nidalee", true);

            var nidaOrb = new Menu("Nidalee: 走砍设置", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(nidaOrb);

            _mainMenu.AddSubMenu(nidaOrb);

            var nidaTS = new Menu("Nidalee: 目标选择", "target selecter");
            TargetSelector.AddToMenu(nidaTS);
            _mainMenu.AddSubMenu(nidaTS);

            var nidaKeys = new Menu("Nidalee: 键位设置", "keybindongs");
            nidaKeys.AddItem(new MenuItem("usecombo", "连招")).SetValue(new KeyBind(32, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("useharass", "骚扰")).SetValue(new KeyBind(67, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("usejungle", "清野")).SetValue(new KeyBind(86, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("useclear", "清线")).SetValue(new KeyBind(86, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("uselasthit", "补刀")).SetValue(new KeyBind(35, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("useflee", "逃跑模式")).SetValue(new KeyBind(65, KeyBindType.Press));
            _mainMenu.AddSubMenu(nidaKeys);

            var nidaSpells = new Menu("Nidalee: 连招设置", "spells");
            nidaSpells.AddItem(new MenuItem("seth", "命中率: ")).SetValue(new StringList(new[] { "低", "中", "高" }, 2));
            nidaSpells.AddItem(new MenuItem("usehumanq", "使用 标枪投掷(Q)")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("useonhigh", "目标：移动的/固定的")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usehumanw", "使用 丛林伏击(W)")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usehumanwauto", "目标：移动的/固定的")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usecougarq", "使用扑倒(Q)")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usecougarw", "使用猛扑(W)")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usecougare", "使用挥击(E)")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usecougarr", "自动转换形态")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("useitems", "使用物品")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("gapcloser", "防止突进")).SetValue(true);
            
            nidaSpells.AddItem(new MenuItem("javelinks", "标枪投掷(Q) 抢人头")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("ksform", "转换形态 抢人头")).SetValue(true);
            _mainMenu.AddSubMenu(nidaSpells);

            var nidaHeals = new Menu("Nidalee: 治愈设置", "hengine");
            nidaHeals.AddItem(new MenuItem("usedemheals", "启用")).SetValue(true);
            nidaHeals.AddItem(new MenuItem("sezz", "治愈 目标优先级: ")).SetValue(new StringList(new[] { "低血量", "ADC" }));

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
            {
                nidaHeals.AddItem(new MenuItem("heal" + hero.SkinName, hero.SkinName)).SetValue(true);
                nidaHeals.AddItem(new MenuItem("healpct" + hero.SkinName, hero.SkinName + " 治愈目标|血量低于 %")).SetValue(new Slider(50));
            }

            nidaHeals.AddItem(new MenuItem("healmanapct", "治愈最低蓝量 %")).SetValue(new Slider(40));
            _mainMenu.AddSubMenu(nidaHeals);

            var nidaHarass = new Menu("Nidalee: 骚扰设置", "harass");
            nidaHarass.AddItem(new MenuItem("usehumanq2", "使用 标枪投掷(Q)")).SetValue(true);
            nidaHarass.AddItem(new MenuItem("humanqpct", "骚扰最低蓝量 %")).SetValue(new Slider(70));
            _mainMenu.AddSubMenu(nidaHarass);

            var nidaJungle = new Menu("Nidalee: 清野设置", "jungleclear");
            nidaJungle.AddItem(new MenuItem("jghumanq", "使用 标枪投掷(Q)")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jghumanw", "使用 丛林伏击(W)")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougarq", "使用扑倒(Q)")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougarw", "使用猛扑(W)")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougare", "使用挥击(E)")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougarr", "自动转换形态")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgpct", "清野最低蓝量 %")).SetValue(new Slider(25));
            _mainMenu.AddSubMenu(nidaJungle);

            var nidalhit = new Menu("Nidalee: 补刀设置", "lasthit");
            nidalhit.AddItem(new MenuItem("lhhumanq", "使用 标枪投掷(Q)")).SetValue(false);
            nidalhit.AddItem(new MenuItem("lhhumanw", "使用 丛林伏击(W)")).SetValue(false);
            nidalhit.AddItem(new MenuItem("lhcougarq", "使用扑倒(Q)")).SetValue(true);
            nidalhit.AddItem(new MenuItem("lhcougarw", "使用猛扑(W)")).SetValue(true);
            nidalhit.AddItem(new MenuItem("lhcougare", "使用挥击(E)")).SetValue(true);
            nidalhit.AddItem(new MenuItem("lhcougarr", "自动转换形态")).SetValue(false);
            nidalhit.AddItem(new MenuItem("lhpct", "补刀最低蓝量 %")).SetValue(new Slider(55));
            _mainMenu.AddSubMenu(nidalhit);

            var nidalc = new Menu("Nidalee: 清线设置", "laneclear");
            nidalc.AddItem(new MenuItem("lchumanq", "使用 标枪投掷(Q)")).SetValue(false);
            nidalc.AddItem(new MenuItem("lchumanw", "使用 丛林伏击(W)")).SetValue(false);
            nidalc.AddItem(new MenuItem("lccougarq", "使用扑倒(Q)")).SetValue(true);
            nidalc.AddItem(new MenuItem("lccougarw", "使用猛扑(W)")).SetValue(true);
            nidalc.AddItem(new MenuItem("lccougare", "使用挥击(E)")).SetValue(true);
            nidalc.AddItem(new MenuItem("lccougarr", "自动转换形态")).SetValue(false);
            nidalc.AddItem(new MenuItem("lcpct", "清线最低蓝量 %")).SetValue(new Slider(55));
            _mainMenu.AddSubMenu(nidalc);

            var nidaD = new Menu("Nidalee: 范围显示", "drawings");
            nidaD.AddItem(new MenuItem("drawQ", "范围 Q")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            nidaD.AddItem(new MenuItem("drawW", "范围 W")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            nidaD.AddItem(new MenuItem("drawE", "范围 E")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            nidaD.AddItem(new MenuItem("drawline", "显示 攻击目标")).SetValue(true);
            nidaD.AddItem(new MenuItem("drawcds", "显示 技能冷却")).SetValue(true);
            _mainMenu.AddSubMenu(nidaD);

            _mainMenu.AddItem(new MenuItem("useignote", "使用 点燃")).SetValue(true);
            _mainMenu.AddToMainMenu();

            Game.PrintChat("<font color=\"#7CFC00\"><b>Kurisu璞瑰コ:</b></font> 璇诲彇鎴愬姛锛侊紒锛亅 鍔犺浇鎴愬姛!姹夊寲by浜岀嫍!QQ缇361630847");

        }

        #endregion

        #region Nidalee: OnTick
        private static void NidaleeOnUpdate(EventArgs args)
        {
            _hasBlue = Me.HasBuff("crestoftheancientgolem", true);
            _cougarForm = Me.Spellbook.GetSpell(SpellSlot.Q).Name != "JavelinToss";

            _target = TargetSelector.GetSelectedTarget() ??
                     TargetSelector.GetTarget(1500, TargetSelector.DamageType.Magical);

            ProcessCooldowns();
            PrimalSurge();
            Killsteal();

            if (_mainMenu.Item("usecombo").GetValue<KeyBind>().Active)
                UseCombo(_target);
            if (_mainMenu.Item("useharass").GetValue<KeyBind>().Active)
                UseHarass(_target);
            if (_mainMenu.Item("useclear").GetValue<KeyBind>().Active)
                UseLaneFarm();
            if (_mainMenu.Item("usejungle").GetValue<KeyBind>().Active)
                UseJungleFarm();
            if (_mainMenu.Item("uselasthit").GetValue<KeyBind>().Active)
                UseLastHit();
            if (_mainMenu.Item("useflee").GetValue<KeyBind>().Active)
                UseFlee();

            if (Me.HasBuff("Takedown", true))
            {
                Orbwalking.LastAATick = 0;
            }

            if (_mainMenu.Item("usehumanwauto").GetValue<bool>())
            {
                // Human W == 0 -- Bushwack is on CD
                if (HW != 0)
                {
                    return;
                }

                foreach (
                    var targ in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                hero =>
                                    hero.IsValidTarget() && hero.Distance(Me.ServerPosition, true) <= Bushwack.RangeSqr)
                    )
                {
                    var prediction = Bushwack.GetPrediction(targ);
                    if (prediction.Hitchance == HitChance.Immobile)
                    {
                        Bushwack.Cast(prediction.CastPosition);
                    }
                }
            }
        }

        #endregion

        #region Nidalee: Killsteal
        private static void Killsteal()
        {
            if (_mainMenu.Item("javelinks").GetValue<bool>())
            {
                foreach (
                    var targ in
                        ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(Javelin.Range)))
                {
                    var prediction = Javelin.GetPrediction(targ);
                    var hqdmg = Me.GetSpellDamage(targ, SpellSlot.Q);
                    if (targ.Health <= hqdmg && HQ == 0)
                    {                      
                        if (prediction.Hitchance >= HitChance.Medium)
                        {
                            if (_cougarForm && _mainMenu.Item("ksform").GetValue<bool>())
                            {
                                if (Aspectofcougar.IsReady())
                                    Aspectofcougar.Cast();
                            }
                            else
                            {
                                Javelin.Cast(prediction.CastPosition);
                            }
                        }
                    }

                    // use on immoble/dashing (doesn't seem to work)
                    if (_mainMenu.Item("useonhigh").GetValue<bool>())
                    {

                        if (_cougarForm || HQ != 0)
                        {
                            return;
                        }

                        if (prediction.Hitchance == HitChance.Immobile)
                            Javelin.Cast(prediction.CastPosition);

                        if (prediction.Hitchance == HitChance.Dashing)
                            Javelin.Cast(prediction.CastPosition);
                    }
                }
            }
        }

        #endregion

        #region Nidalee : Misc
        private static void UseInventoryItems(IEnumerable<int> items, Obj_AI_Base target)
        {
            if (!_mainMenu.Item("useitems").GetValue<bool>())
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

            return target.Health <= (float)damage * 5;
        }

        private static float CougarDamage(Obj_AI_Base target)
        {
            var damage = 0d;

            if (CQ == 0)
                damage += Me.GetSpellDamage(target, SpellSlot.Q, 1);
            if (CW == 0)
                damage += Me.GetSpellDamage(target, SpellSlot.W, 1);
            if (CE == 0)
                damage += Me.GetSpellDamage(target, SpellSlot.E, 1);

            return (float) damage;
        }

        #endregion

        #region Nidalee : Flee

        private static void UseFlee()
        {
            Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (_cougarForm && CW == 0)
                Pounce.Cast(Game.CursorPos);
            if (!_cougarForm && Aspectofcougar.IsReady() && CW == 0)
                Aspectofcougar.Cast();
        }

        #endregion

        #region Nidalee: SBTW

        private static void UseCombo(Obj_AI_Base target)
        {
            if (TargetSelector.GetSelectedTarget() != null && _target.Distance(Me.ServerPosition, true) > 1500 * 1500)
                return;

            // Cougar combo
            if (_cougarForm && target.IsValidTarget(Javelin.Range))
            {
                UseInventoryItems(NidaItems, target);

                // Check if takedown is ready (on unit)
                if (CQ == 0 && _mainMenu.Item("usecougarq").GetValue<bool>()
                    && target.Distance(Me.ServerPosition, true) <= Takedown.RangeSqr * 2)
                {
                    Takedown.CastOnUnit(Me);
                }

                // Check is pounce is ready 
                else if (CW == 0 && _mainMenu.Item("usecougarw").GetValue<bool>()
                    && target.Distance(Me.ServerPosition, true) > 30 * 30)
                {
                    if (TargetHunted(target) & target.Distance(Me.ServerPosition, true) <= 750*750)
                        Pounce.Cast(target.ServerPosition);
                    else if (target.Distance(Me.ServerPosition, true) <= 400*400)
                        Pounce.Cast(target.ServerPosition);

                }

                else if (target.Distance(Me.ServerPosition, true) > Me.AttackRange*Me.AttackRange)
                {
                    Pounce.Cast(target.ServerPosition);
                }

                // Check if swipe is ready (prediction)
                if (CE == 0 && _mainMenu.Item("usecougare").GetValue<bool>())
                {
                    var prediction = Swipe.GetPrediction(target);
                    if (prediction.Hitchance >= HitChance.Low &&
                        target.Distance(Me.ServerPosition, true) <= Swipe.RangeSqr)
                    {
                        Swipe.Cast(prediction.CastPosition);
                    }
                }

                // force transform if q ready and no collision 
                if (HQ == 0 && _mainMenu.Item("usecougarr").GetValue<bool>())
                {
                    if (!Aspectofcougar.IsReady())
                    {
                        return;
                    }

                    // or return -- stay cougar if we can kill with available spells
                    if (target.Health <= CougarDamage(target) &&
                        target.Distance(Me.ServerPosition, true) <= Swipe.RangeSqr + Pounce.RangeSqr)
                    {
                        return;
                    }

                    var prediction = Javelin.GetPrediction(target);
                    if (prediction.Hitchance >= HitChance.Medium)
                        Aspectofcougar.Cast();
                }

                // Switch to human form if can kill in 5 aa and cougar skill not available      
                if (CW != 0 && CE != 0 && CQ != 0 && target.Distance(Me.ServerPosition, true) > Takedown.RangeSqr && CanKillAA(target)
                    && _mainMenu.Item("usecougarr").GetValue<bool>() && target.Distance(Me.ServerPosition, true) <= Me.AttackRange * Me.AttackRange + 5 * 5)
                {
                    if (Aspectofcougar.IsReady())
                        Aspectofcougar.Cast();
                }

            }

            // Human combo
            if (!_cougarForm && target.IsValidTarget(Javelin.Range))
            {
                // Switch to cougar if target hunted or can kill target 
                if (Aspectofcougar.IsReady() && _mainMenu.Item("usecougarr").GetValue<bool>()
                    && (TargetHunted(target) || target.Health <= CougarDamage(target) && HQ != 0))
                {
                    if (TargetHunted(target) && target.Distance(Me.ServerPosition, true) <= 750*750)
                        Aspectofcougar.Cast();
                    if (target.Health <= CougarDamage(target) && target.Distance(Me.ServerPosition, true) <= 350*350)
                        Aspectofcougar.Cast();
                }

                else if (HQ == 0 && _mainMenu.Item("usehumanq").GetValue<bool>())
                {
                    var prediction = Javelin.GetPrediction(target);
                    switch (_mainMenu.Item("seth").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            if (prediction.Hitchance >= HitChance.Low || prediction.Hitchance == HitChance.VeryHigh)
                                Javelin.Cast(prediction.CastPosition);
                            break;
                        case 1:
                            if (prediction.Hitchance >= HitChance.Medium || prediction.Hitchance == HitChance.VeryHigh)
                                Javelin.Cast(prediction.CastPosition);
                            break;
                        case 2:
                            if (prediction.Hitchance >= HitChance.High || prediction.Hitchance == HitChance.VeryHigh)
                                Javelin.Cast(prediction.CastPosition);
                            break;
                    }
                }

                // Check bushwack and cast underneath targets feet.
                if (HW == 0 && _mainMenu.Item("usehumanw").GetValue<bool>() &&
                         target.Distance(Me.ServerPosition, true) <= Bushwack.RangeSqr)
                {
                    var prediction = Bushwack.GetPrediction(target);
                    if (prediction.Hitchance >= HitChance.Medium)
                    {
                        Bushwack.Cast(prediction.CastPosition);
                    }
                }
            }
        }
        #endregion

        #region Nidalee: Harass
        private static void UseHarass(Obj_AI_Base target)
        {
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var minPercent = _mainMenu.Item("humanqpct").GetValue<Slider>().Value;
            if (!_cougarForm && HQ == 0 && _mainMenu.Item("usehumanq2").GetValue<bool>())
            {
                var prediction = Javelin.GetPrediction(target);
                if (target.Distance(Me.ServerPosition, true) <= Javelin.RangeSqr && actualHeroManaPercent > minPercent)
                {
                    switch (_mainMenu.Item("seth").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            if (prediction.Hitchance >= HitChance.Low)
                                Javelin.Cast(prediction.CastPosition);
                            break;
                        case 1:
                            if (prediction.Hitchance >= HitChance.Medium)
                                Javelin.Cast(prediction.CastPosition);
                            break;
                        case 2:
                            if (prediction.Hitchance >= HitChance.High)
                                Javelin.Cast(prediction.CastPosition);
                            break;
                    }
                }
            }
        }

        #endregion

        #region Nidalee: Heal

        private static void PrimalSurge()
        {
            if (HE != 0 || !_mainMenu.Item("usedemheals").GetValue<bool>() || Me.IsRecalling())
            {
                return;
            }

            var actualHeroManaPercent = (int) ((Me.Mana/Me.MaxMana)*100);
            var selfManaPercent = _mainMenu.Item("healmanapct").GetValue<Slider>().Value;

            Obj_AI_Hero target;
            if (_mainMenu.Item("sezz").GetValue<StringList>().SelectedIndex == 0)
            {
                target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsValidTarget(Primalsurge.Range + 100, false) && hero.IsAlly)
                        .OrderBy(xe => xe.Health/xe.MaxHealth*100).First();
            }
            else
            {
                target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsValidTarget(Primalsurge.Range + 100, false) && hero.IsAlly)
                        .OrderByDescending(xe => xe.FlatPhysicalDamageMod).First();
            }

            if (!_cougarForm && _mainMenu.Item("heal" + target.SkinName).GetValue<bool>())
            {
                var needed = _mainMenu.Item("healpct" + target.SkinName).GetValue<Slider>().Value;
                var hp = (int)((target.Health / target.MaxHealth) * 100);

                if (actualHeroManaPercent > selfManaPercent && hp <= needed || _hasBlue && hp <= needed)
                    Primalsurge.CastOnUnit(target);
            }
        }



        #endregion

        #region Nidalee: Farm
        private static void UseLaneFarm()
        {
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var minPercent = _mainMenu.Item("lcpct").GetValue<Slider>().Value;

            foreach (
                var m in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            m =>
                                m.IsValidTarget(1500) && Jungleminions.Any(name => !m.Name.StartsWith(name)) &&
                                m.Name.StartsWith("Minion")))
            {

                if (_cougarForm)
                {
                    if ((HQ == 0 && _mainMenu.Item("lchumanq").GetValue<bool>() || CW != 0 && CQ != 0 && CE != 0) &&
                        _mainMenu.Item("lccougarr").GetValue<bool>())
                    {
                        if (Aspectofcougar.IsReady())
                            Aspectofcougar.Cast();
                    }

                    if (m.Distance(Me.ServerPosition, true) <= Swipe.RangeSqr && CE == 0)
                    {
                        if (_mainMenu.Item("lccougare").GetValue<bool>())
                            Swipe.Cast(m.ServerPosition);
                    }


                    if (m.Distance(Me.ServerPosition, true) <= Pounce.RangeSqr && CW == 0)
                    {
                        if (_mainMenu.Item("lccougarw").GetValue<bool>())
                            Pounce.Cast(m.ServerPosition);
                    }

                    if (m.Distance(Me.ServerPosition) <= Takedown.RangeSqr && CQ == 0)
                    {
                        if (_mainMenu.Item("lccougarq").GetValue<bool>())
                            Takedown.CastOnUnit(Me);
                    }
                }
                else
                {
                    if (actualHeroManaPercent > minPercent && HQ == 0)
                    {
                        if (_mainMenu.Item("lchumanq").GetValue<bool>())
                            Javelin.Cast(m.ServerPosition);
                    }

                    if (m.Distance(Me.ServerPosition, true) <= Bushwack.RangeSqr && actualHeroManaPercent > minPercent && HW == 0)
                    {
                        if (_mainMenu.Item("lchumanw").GetValue<bool>())
                            Bushwack.Cast(m.ServerPosition);
                    }

                    if (_mainMenu.Item("lccougarr").GetValue<bool>() && m.Distance(Me.ServerPosition, true) <= Pounce.RangeSqr &&
                        actualHeroManaPercent > minPercent && Aspectofcougar.IsReady())
                    {
                        Aspectofcougar.Cast();
                    }
                }

            }
        }


        private static void UseJungleFarm()
        {
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var minPercent = _mainMenu.Item("jgpct").GetValue<Slider>().Value;

            foreach (var m in
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        m =>
                            m.IsValidTarget(700) && Jungleminions.Any(name => m.Name.StartsWith(name)) &&
                            !m.Name.Contains("Mini")))
            {
                if (_cougarForm)
                {
                    if ((HQ == 0 && _mainMenu.Item("jghumanq").GetValue<bool>() || CW != 0 && CQ != 0 && CE != 0) &&
                        _mainMenu.Item("jgcougarr").GetValue<bool>())
                    {
                        if (Aspectofcougar.IsReady())
                            Aspectofcougar.Cast();
                    }

                    if (m.Distance(Me.ServerPosition, true) <= Swipe.RangeSqr && CE == 0)
                    {
                        if (_mainMenu.Item("jgcougare").GetValue<bool>())
                            Swipe.Cast(m.ServerPosition);
                    }

                    if (m.Distance(Me.ServerPosition, true) <= Pounce.RangeSqr && CW == 0)
                    {
                        if (_mainMenu.Item("jgcougarw").GetValue<bool>())
                            Pounce.Cast(m.ServerPosition);
                    }

                    if (m.Distance(Me.ServerPosition, true) <= Takedown.RangeSqr && CQ == 0)
                    {
                        if (_mainMenu.Item("jgcougarq").GetValue<bool>())
                            Takedown.CastOnUnit(Me);
                    }
                }
                else
                {
                    if (actualHeroManaPercent > minPercent && HQ == 0)
                    {
                        if (_mainMenu.Item("jghumanq").GetValue<bool>())
                        {
                            var prediction = Javelin.GetPrediction(m);
                            if (prediction.Hitchance >= HitChance.Low)
                                Javelin.Cast(m.ServerPosition);
                        }
                    }

                    if (m.Distance(Me.ServerPosition, true) <= Bushwack.RangeSqr && actualHeroManaPercent > minPercent && HW == 0)
                    {
                        if (_mainMenu.Item("jghumanw").GetValue<bool>())
                            Bushwack.Cast(m.ServerPosition);
                    }

                    if (_mainMenu.Item("jgcougarr").GetValue<bool>() && m.Distance(Me.ServerPosition, true) <= Pounce.RangeSqr &&
                        actualHeroManaPercent > minPercent && Aspectofcougar.IsReady() && HQ != 0)
                    {
                        Aspectofcougar.Cast();
                    }
                }
            }
        }

        #endregion

        #region Nidalee: LastHit
        private static void UseLastHit()
        {
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var minPercent = _mainMenu.Item("lhpct").GetValue<Slider>().Value;

            foreach (
                var m in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(m => m.IsValidTarget(Javelin.Range) && Jungleminions.Any(name => !m.Name.StartsWith(name))))
            {
                var cqdmg = Me.GetSpellDamage(m, SpellSlot.Q, 1);
                var cwdmg = Me.GetSpellDamage(m, SpellSlot.W, 1);
                var cedmg = Me.GetSpellDamage(m, SpellSlot.E, 1);
                var hqdmg = Me.GetSpellDamage(m, SpellSlot.Q);

                if (_cougarForm)
                {
                    if (m.Distance(Me.ServerPosition, true) < Swipe.RangeSqr && CE == 0)
                    {
                        if (m.Health <= cedmg && _mainMenu.Item("lhcougare").GetValue<bool>())
                            Swipe.Cast(m.ServerPosition);
                    }


                    if (m.Distance(Me.ServerPosition, true) < Pounce.RangeSqr && CW == 0)
                    {
                        if (m.Health <= cwdmg && _mainMenu.Item("lhcougarw").GetValue<bool>())
                            Pounce.Cast(m.ServerPosition);
                    }

                    if (m.Distance(Me.ServerPosition, true) < Takedown.RangeSqr && CQ == 0)
                    {
                        if (m.Health <= cqdmg && _mainMenu.Item("lhcougarq").GetValue<bool>())
                            Takedown.CastOnUnit(Me);
                    }
                }
                else
                {
                    if (actualHeroManaPercent > minPercent && HQ == 0)
                    {
                        if (m.Health <= hqdmg && _mainMenu.Item("lhhumanq").GetValue<bool>())
                            Javelin.Cast(m.ServerPosition);
                    }

                    if (m.Distance(Me.ServerPosition, true) <= Bushwack.RangeSqr && actualHeroManaPercent > minPercent && HW == 0)
                    {
                        if (_mainMenu.Item("lhhumanw").GetValue<bool>())
                            Bushwack.Cast(m.ServerPosition);
                    }

                    if (_mainMenu.Item("lhcougarr").GetValue<bool>() && m.Distance(Me.ServerPosition, true) <= Pounce.RangeSqr &&
                        actualHeroManaPercent > minPercent && Aspectofcougar.IsReady())
                    {
                        Aspectofcougar.Cast();
                    }
                }
            }
        }

        #endregion

        #region Nidalee: Tracker
        private static void NidaleeTracker(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
                GetCooldowns(args);
        }

        private static readonly float[] HumanQcd = { 6, 6, 6, 6, 6 };
        private static readonly float[] HumanWcd = { 13, 12, 11, 10, 9 };
        private static readonly float[] HumanEcd = { 12, 12, 12, 12, 12 };

        private static float CQRem, CWRem, CERem;
        private static float HQRem, HWRem, HERem;
        private static float CQ, CW, CE;
        private static float HQ, HW, HE;

        private static void ProcessCooldowns()
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

        private static void GetCooldowns(GameObjectProcessSpellCastEventArgs spell)
        {
            if (_cougarForm)
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
                    HQRem = Game.Time + CalculateCd(HumanQcd[Javelin.Level - 1]);
                if (spell.SData.Name == "Bushwhack")
                    HWRem = Game.Time + CalculateCd(HumanWcd[Bushwack.Level - 1]);
                if (spell.SData.Name == "PrimalSurge")
                    HERem = Game.Time + CalculateCd(HumanEcd[Primalsurge.Level - 1]);
            }
        }

        #endregion

        #region Nidalee: On Draw
        private static void NidaleeOnDraw(EventArgs args)
        {
            if (_target != null && _mainMenu.Item("drawline").GetValue<bool>())
            {
                if (Me.IsDead)
                {
                    return;
                }

                Render.Circle.DrawCircle(_target.Position, _target.BoundingRadius - 50, Color.Yellow);
            }

            foreach (var spell in CougarSpellList)
            {
                var circle = _mainMenu.Item("draw" + spell.Slot).GetValue<Circle>();
                if (circle.Active && _cougarForm && !Me.IsDead)
                    Render.Circle.DrawCircle(Me.Position, spell.Range, circle.Color, 2);
            }

            foreach (var spell in HumanSpellList)
            {
                var circle = _mainMenu.Item("draw" + spell.Slot).GetValue<Circle>();
                if (circle.Active && !_cougarForm && !Me.IsDead)
                    Render.Circle.DrawCircle(Me.Position, spell.Range, circle.Color, 2);
            }

            if (!_mainMenu.Item("drawcds").GetValue<bool>()) return;

            var wts = Drawing.WorldToScreen(Me.Position);

            if (!_cougarForm) // lets show cooldown timers for the opposite form :)
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
