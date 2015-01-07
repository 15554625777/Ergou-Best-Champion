using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace D_Nidalee
{
    internal class Program
    {
        public const string ChampionName = "Nidalee";

        public static Orbwalking.Orbwalker Orbwalker;

        public static Spell Q, W, E, R, QC, WC, EC;

        public static List<Spell> SpellList = new List<Spell>();

        private static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        private static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand, _lotis, _zhonya, _dfg;

        public static SpellSlot IgniteSlot;

        public static Menu Config;

        private static Obj_AI_Hero Player;

        public static bool IsHuman;

        public static bool IsCougar;

        public static bool Recall;

        private static readonly float[] HumanQcd = {6, 6, 6, 6, 6};

        private static readonly float[] HumanWcd = {12, 12, 12, 12, 12};

        private static readonly float[] HumanEcd = {13, 12, 11, 10, 9};

        private static readonly float[] CougarQcd, CougarWcd, CougarEcd = {5, 5, 5, 5, 5};

        private static float _humQcd = 0, _humWcd = 0, _humEcd = 0;

        private static float _spidQcd = 0, _spidWcd = 0, _spidEcd = 0;

        private static float _humaQcd = 0, _humaWcd = 0, _humaEcd = 0;

        private static float _spideQcd = 0, _spideWcd = 0, _spideEcd = 0;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;



            Q = new Spell(SpellSlot.Q, 1500f);
            W = new Spell(SpellSlot.W, 900f);
            E = new Spell(SpellSlot.E, 600f);
            WC = new Spell(SpellSlot.W, 750f);
            EC = new Spell(SpellSlot.E, 300f);
            R = new Spell(SpellSlot.R, 0);

            Q.SetSkillshot(0.125f, 40f, 1300, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.500f, 80f, 1450, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            SpellList.Add(QC);
            SpellList.Add(WC);
            SpellList.Add(EC);

            _bilge = new Items.Item(3144, 475f);
            _blade = new Items.Item(3153, 425f);
            _hydra = new Items.Item(3074, 250f);
            _tiamat = new Items.Item(3077, 250f);
            _rand = new Items.Item(3143, 490f);
            _lotis = new Items.Item(3190, 590f);
            _dfg = new Items.Item(3128, 750f);
            _zhonya = new Items.Item(3157, 10);


            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            //D Nidalee
            Config = new Menu("D-豹女", "D-Nidalee", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo
            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用 W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用 R")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQComboCougar", "使用 豹型 Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWComboCougar", "使用 豹型 W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseEComboCougar", "使用 豹型 E")).SetValue(true);
            Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("QHitCombo", "Q 命中率").SetValue(
                        new StringList(new[] {"低", "中", "高", "超高"})));

            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "连招!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Extra
            Config.AddSubMenu(new Menu("治疗", "Heal"));
            Config.SubMenu("Heal").AddItem(new MenuItem("UseAutoE", "对自己使用E")).SetValue(true);
            Config.SubMenu("Heal").AddItem(new MenuItem("HPercent", "血量低于丨使用E")).SetValue(new Slider(40, 1, 100));
            Config.SubMenu("Heal").AddItem(new MenuItem("AllyUseAutoE", "队友血量低于丨使用E")).SetValue(true);
            Config.SubMenu("Heal")
                .AddItem(new MenuItem("AllyHPercent", "Health percent"))
                .SetValue(new Slider(40, 1, 100));

            Config.AddSubMenu(new Menu("物品", "items"));
            Config.SubMenu("items").AddSubMenu(new Menu("进攻 物品", "Offensive"));
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("UseItemsdfg", "使用冥火")).SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("UseItemsignite", "使用 引燃"))
                .SetValue(true);
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Tiamat", "使用 提亚玛特")).SetValue(true);
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Hydra", "使用 九头蛇")).SetValue(true);
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Bilge", "使用 小弯刀")).SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BilgeEnemyhp", "使用小弯刀丨敌人血量").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Bilgemyhp", "使用小弯刀丨自己血量").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Blade", "使用破败")).SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BladeEnemyhp", "使用破败丨敌人血量").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Blademyhp", "使用破败丨自己血量").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items").AddSubMenu(new Menu("防御物品", "Deffensive"));
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omen", "使用 兰盾"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omenenemys", "使用兰盾丨敌人数量").SetValue(new Slider(2, 1, 5)));
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotis", "使用鸟盾"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotisminhp", "使用鸟盾丨队友血量").SetValue(new Slider(35, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Zhonyas", "使用 中亚"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Zhonyashp", "使用中亚丨自己血量").SetValue(new Slider(20, 1, 100)));
            Config.SubMenu("items").AddSubMenu(new Menu("补给", "Potions"));
            Config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usehppotions", "使用 红药/水晶瓶/饼干"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionhp", "使用补给丨自己血量").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usemppotions", "使用 蓝药/水晶瓶/饼干"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionmp", "使用蓝药丨自己蓝量").SetValue(new Slider(85, 1, 100)));

            //Harass
            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "使用 W")).SetValue(true);
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("QHitharass", "Q 命中率").SetValue(
                        new StringList(new[] { "低", "中", "高", "超高" })));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "骚扰（自动）").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("Harrasmana", "骚扰最低蓝量").SetValue(new Slider(60, 1, 100)));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "骚扰 按键").SetValue(new KeyBind("X".ToCharArray()[0],
                        KeyBindType.Press)));


            Config.AddSubMenu(new Menu("清野丨清线", "Farm"));
            Config.SubMenu("Farm").AddSubMenu(new Menu("LastHit", "LastHit"));
            Config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseQLH", "使用 人型 Q")).SetValue(true);
            Config.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(new MenuItem("lastmana", "使用Q最低蓝量").SetValue(new Slider(35, 1, 100)));
            Config.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(
                    new MenuItem("ActiveLast", "补刀!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            Config.SubMenu("Farm").AddSubMenu(new Menu("清野丨清线", "Lane"));
            Config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("farm_E1", "使用 E (人型)")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("UseQLane", "使用 Q (狮型)")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("UseWLane", "使用 W (狮型)")).SetValue(true);
            Config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("UseELane", "使用 E (狮型)")).SetValue(true);
            Config.SubMenu("Farm")
                .SubMenu("Lane")
                .AddItem(
                    new MenuItem("LaneClear", "清线 按键").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm")
                .SubMenu("Lane")
                .AddItem(
                    new MenuItem("farm_R", "使用R切换(自动)").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Config.SubMenu("Farm")
                .SubMenu("Lane")
                .AddItem(new MenuItem("Lane", "自动R最低蓝量").SetValue(new Slider(60, 1, 100)));


            //Kill Steal
            Config.AddSubMenu(new Menu("杂项", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("ActiveKs", "开启 抢人头")).SetValue(true);
            Config.SubMenu("Misc").AddItem(new MenuItem("UseQKs", "使用 Q")).SetValue(true);
            Config.SubMenu("Misc").AddItem(new MenuItem("UseIgnite", "使用 引燃")).SetValue(true);
            Config.SubMenu("Misc")
                .AddItem(new MenuItem("escapeterino", "躲避!!!"))
                .SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Press));

            //Damage after combo:
            MenuItem dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            //Drawings
            Config.AddSubMenu(new Menu("范围选项", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "范围 Q")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "范围 W")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "范围 E")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "自由延迟圈").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawCooldown", "显示冷却时间")).SetValue(true);
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleQuality", "圈 质量").SetValue(new Slider(100, 100, 10)));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleThickness", "圈 厚度").SetValue(new Slider(1, 10, 1)));

            Config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Hero.OnCreate += OnCreateObj;
            Obj_AI_Hero.OnDelete += OnDeleteObj;
            //Game_OnGameEnd += Game_OnGameEnd;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Game.PrintChat("<font color='#881df2'>D-璞瑰コ by Diabaths</font> 鍔犺浇鎴愬姛!姹夊寲by楝肩伒!QQ缇361630847.");
            Game.PrintChat(
                "<font color='#FF0000'>If You like my work and want to support, and keep it always up to date plz donate via paypal in </font> <font color='#FF9900'>ssssssssssmith@hotmail.com</font> (10) S");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("UseAutoE").GetValue<bool>())
            {
                AutoE();
            }
            if (Config.Item("escapeterino").GetValue<KeyBind>().Active)
            {
                Escapeterino();
            }
            AllyAutoE();
            Cooldowns();
            Player = ObjectManager.Player;
            QC = new Spell(SpellSlot.Q, Player.AttackRange + 50);
            Orbwalker.SetAttack(true);

            CheckSpells();
            if (Config.Item("ActiveLast").GetValue<KeyBind>().Active &&
                (100*(Player.Mana/Player.MaxMana)) > Config.Item("lastmana").GetValue<Slider>().Value)
            {
                LastHit();
            }
            if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if ((Config.Item("ActiveHarass").GetValue<KeyBind>().Active ||
                 Config.Item("harasstoggle").GetValue<KeyBind>().Active) &&
                (100*(Player.Mana/Player.MaxMana)) > Config.Item("Harrasmana").GetValue<Slider>().Value)
            {
                Harass();
            }
            if (Config.Item("LaneClear").GetValue<KeyBind>().Active)
            {
                Farm();
            }
            if (Config.Item("ActiveKs").GetValue<bool>())
            {
                KillSteal();
            }
            Usepotion();
        }

        private static void Escapeterino()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (IsHuman)
            {
                if (R.IsReady())
                {
                    R.Cast();
                }
                if (WC.IsReady())
                {
                    WC.Cast(Game.CursorPos);
                }
            }
            else if (IsCougar && WC.IsReady())
            {
                WC.Cast(Game.CursorPos);
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
                //Game.PrintChat("Spell name: " + args.SData.Name.ToString());
                GetCDs(args);
        }

        private static float CalculateCd(float time)
        {
            return time + (time*Player.PercentCooldownMod);
        }

        private static void Cooldowns()
        {
            _humaQcd = ((_humQcd - Game.Time) > 0) ? (_humQcd - Game.Time) : 0;
            _humaWcd = ((_humWcd - Game.Time) > 0) ? (_humWcd - Game.Time) : 0;
            _humaEcd = ((_humEcd - Game.Time) > 0) ? (_humEcd - Game.Time) : 0;
            _spideQcd = ((_spidQcd - Game.Time) > 0) ? (_spidQcd - Game.Time) : 0;
            _spideWcd = ((_spidWcd - Game.Time) > 0) ? (_spidWcd - Game.Time) : 0;
            _spideEcd = ((_spidEcd - Game.Time) > 0) ? (_spidEcd - Game.Time) : 0;
        }

        private static void GetCDs(GameObjectProcessSpellCastEventArgs spell)
        {
            if (IsHuman)
            {
                if (spell.SData.Name == "JavelinToss")
                    _humQcd = Game.Time + CalculateCd(HumanQcd[Q.Level]);
                if (spell.SData.Name == "Bushwhack")
                    _humWcd = Game.Time + CalculateCd(HumanWcd[W.Level]);
                if (spell.SData.Name == "PrimalSurge")
                    _humEcd = Game.Time + CalculateCd(HumanEcd[E.Level]);
            }
            else
            {
                if (spell.SData.Name == "Takedown")
                    _spidQcd = Game.Time + CalculateCd(CougarQcd[QC.Level]);
                if (spell.SData.Name == "Pounce")
                    _spidWcd = Game.Time + CalculateCd(CougarWcd[WC.Level]);
                if (spell.SData.Name == "Swipe")
                    _spidEcd = Game.Time + CalculateCd(CougarEcd[EC.Level]);
            }
        }

        private static HitChance QHitChanceCombo()
        {
            switch (Config.Item("QHitCombo").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static HitChance QHitChanceHarass()
        {
            switch (Config.Item("QHitharass").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.High;
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            Orbwalker.SetAttack((!Q.IsReady() || W.IsReady()));
            var itemsDfg = Config.Item("UseItemsdfg").GetValue<bool>();
            var itemsIgnite = Config.Item("UseItemsignite").GetValue<bool>();
            if (target != null)
            {
                if (_dfg.IsReady() && target.HasBuff("nidaleepassivehunted", true) &&
                    Player.Distance(target) <= _dfg.Range && itemsDfg &&
                    target.Health <= ComboDamage(target))
                {
                    _dfg.Cast(target);
                }
                if (itemsIgnite && IgniteSlot != SpellSlot.Unknown &&
                    Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    if (ComboDamage(target) > target.Health)
                    {
                        Player.Spellbook.CastSpell(IgniteSlot, target);
                    }
                }
                if (Q.IsReady() && IsHuman && Player.Distance(target) <= Q.Range &&
                    Config.Item("UseQCombo").GetValue<bool>())
                {
                    var prediction = Q.GetPrediction(target);
                    if (prediction.Hitchance >= QHitChanceCombo())
                        Q.Cast(prediction.CastPosition);

                }
                if (W.IsReady() && IsHuman && Player.Distance(target) <= W.Range &&
                    Config.Item("UseWCombo").GetValue<bool>())
                {
                    W.Cast(target);
                }

                if (R.IsReady() && IsHuman && Config.Item("UseRCombo").GetValue<bool>() &&
                    Player.Distance(target) <= 625)
                {
                    if (IsHuman)
                    {
                        R.Cast();
                    }

                    if (IsCougar)
                    {
                        if (WC.IsReady() && Config.Item("UseWComboCougar").GetValue<bool>() &&
                            Player.Distance(target) <= WC.Range)
                        {
                            WC.Cast(target);
                        }
                        if (EC.IsReady() && Config.Item("UseEComboCougar").GetValue<bool>() &&
                            Player.Distance(target) <= EC.Range)
                        {
                            EC.Cast(target);
                        }
                        if (QC.IsReady() && Config.Item("UseQComboCougar").GetValue<bool>() &&
                            Player.Distance(target) <= QC.Range)
                        {
                            Orbwalker.SetAttack(true);
                            QC.Cast();
                        }

                    }

                }

                if (IsCougar && Player.Distance(target) < 625)
                {
                    if (IsHuman && R.IsReady())
                    {
                        R.Cast();
                    }

                    if (IsCougar)
                    {
                        if (WC.IsReady() && Config.Item("UseWComboCougar").GetValue<bool>() &&
                            Player.Distance(target) <= WC.Range)
                        {
                            WC.Cast(target);
                        }
                        if (EC.IsReady() && Config.Item("UseEComboCougar").GetValue<bool>() &&
                            Player.Distance(target) <= EC.Range)
                        {
                            EC.Cast(target);
                        }
                        if (QC.IsReady() && Config.Item("UseQComboCougar").GetValue<bool>() &&
                            Player.Distance(target) <= QC.Range)
                        {
                            Orbwalker.SetAttack(true);
                            QC.Cast();
                        }

                    }
                }

                if (R.IsReady() && IsCougar && Config.Item("UseRCombo").GetValue<bool>() &&
                    Player.Distance(target) > WC.Range)
                {
                    R.Cast();
                }
                if (R.IsReady() && IsCougar && Player.Distance(target) > EC.Range &&
                    Config.Item("UseRCombo").GetValue<bool>())
                {
                    R.Cast();
                }
            }

            UseItemes(target);
        }

        private static float ComboDamage(Obj_AI_Base hero)
        {
            var dmg = 0d;
            if (ObjectManager.Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                dmg += Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            if (Items.HasItem(3128) && Items.CanUseItem(3128))
            {
                dmg += Player.GetItemDamage(hero, Damage.DamageItems.Dfg);
                dmg = dmg * 1.2;
            }
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                dmg += Player.GetItemDamage(hero, Damage.DamageItems.Botrk);
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                dmg += Player.GetItemDamage(hero, Damage.DamageItems.Bilgewater);
            if (QC.IsReady())
                dmg += Player.GetSpellDamage(hero, SpellSlot.Q);
            if (EC.IsReady())
                dmg += Player.GetSpellDamage(hero, SpellSlot.E);
            if (WC.IsReady())
                dmg += Player.GetSpellDamage(hero, SpellSlot.W);
            if (Q.IsReady() && !IsCougar)
                dmg += Player.GetSpellDamage(hero, SpellSlot.Q);
            return (float) dmg;
        }

        private static void UseItemes(Obj_AI_Hero target)
        {
            var iBilge = Config.Item("Bilge").GetValue<bool>();
            var iBilgeEnemyhp = target.Health <=
                                (target.MaxHealth*(Config.Item("BilgeEnemyhp").GetValue<Slider>().Value)/100);
            var iBilgemyhp = Player.Health <=
                             (Player.MaxHealth*(Config.Item("Bilgemyhp").GetValue<Slider>().Value)/100);
            var iBlade = Config.Item("Blade").GetValue<bool>();
            var iBladeEnemyhp = target.Health <=
                                (target.MaxHealth*(Config.Item("BladeEnemyhp").GetValue<Slider>().Value)/100);
            var iBlademyhp = Player.Health <=
                             (Player.MaxHealth*(Config.Item("Blademyhp").GetValue<Slider>().Value)/100);
            var iOmen = Config.Item("Omen").GetValue<bool>();
            var iOmenenemys = ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(450)) >=
                              Config.Item("Omenenemys").GetValue<Slider>().Value;
            var iTiamat = Config.Item("Tiamat").GetValue<bool>();
            var iHydra = Config.Item("Hydra").GetValue<bool>();
            var ilotis = Config.Item("lotis").GetValue<bool>();
            var iZhonyas = Config.Item("Zhonyas").GetValue<bool>();
            var iZhonyashp = Player.Health <=
                             (Player.MaxHealth*(Config.Item("Zhonyashp").GetValue<Slider>().Value)/100);
           
            if (Player.Distance(target) <= 450 && iBilge && (iBilgeEnemyhp || iBilgemyhp) && _bilge.IsReady())
            {
                _bilge.Cast(target);

            }
            if (Player.Distance(target) <= 450 && iBlade && (iBladeEnemyhp || iBlademyhp) && _blade.IsReady())
            {
                _blade.Cast(target);

            }
            if (iTiamat && _tiamat.IsReady() && target.IsValidTarget(_tiamat.Range))
            {
                _tiamat.Cast();

            }
            if (iHydra && _hydra.IsReady() && target.IsValidTarget(_hydra.Range))
            {
                _hydra.Cast();

            }
            if (iOmenenemys && iOmen && _rand.IsReady())
            {
                _rand.Cast();

            }
            if (ilotis)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly || hero.IsMe))
                {
                    if (hero.Health <= (hero.MaxHealth*(Config.Item("lotisminhp").GetValue<Slider>().Value)/100) &&
                        hero.Distance(Player.ServerPosition) <= _lotis.Range && _lotis.IsReady())
                        _lotis.Cast();
                }
            }
            if (iZhonyas && iZhonyashp && Utility.CountEnemysInRange(1000) >= 1)
            {
                _zhonya.Cast(Player);

            }
        }
        private static void Usepotion()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var iusehppotion = Config.Item("usehppotions").GetValue<bool>();
            var iusepotionhp = Player.Health <=
                               (Player.MaxHealth * (Config.Item("usepotionhp").GetValue<Slider>().Value) / 100);
            var iusemppotion = Config.Item("usemppotions").GetValue<bool>();
            var iusepotionmp = Player.Mana <=
                               (Player.MaxMana * (Config.Item("usepotionmp").GetValue<Slider>().Value) / 100);
            if (Utility.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (Utility.CountEnemysInRange(800) > 0 ||
                (mobs.Count > 0 && Config.Item("LaneClear").GetValue<KeyBind>().Active && (Items.HasItem(1039) ||
                 SmiteBlue.Any(i => Items.HasItem(i)) || SmiteRed.Any(i => Items.HasItem(i)) || SmitePurple.Any(i => Items.HasItem(i)) ||
                  SmiteBlue.Any(i => Items.HasItem(i)) || SmiteGrey.Any(i => Items.HasItem(i))
                     )))
            {
                if (iusepotionhp && iusehppotion &&
                     !(ObjectManager.Player.HasBuff("RegenerationPotion", true) ||
                       ObjectManager.Player.HasBuff("ItemCrystalFlask", true) ||
                       ObjectManager.Player.HasBuff("ItemMiniRegenPotion", true)))
                {
                    if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }
                    else if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }
                    else if (Items.HasItem(2003) && Items.CanUseItem(2003))
                    {
                        Items.UseItem(2003);
                    }
                }


                if (iusepotionmp && iusemppotion &&
                    !(ObjectManager.Player.HasBuff("FlaskOfCrystalWater", true) ||
                      ObjectManager.Player.HasBuff("ItemCrystalFlask", true) ||
                      ObjectManager.Player.HasBuff("ItemMiniRegenPotion", true)))
                {
                    if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }
                    else if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }
                    else if (Items.HasItem(2004) && Items.CanUseItem(2004))
                    {
                        Items.UseItem(2004);
                    }
                }
            }
        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target != null)
            {
                if (Q.IsReady() && IsHuman && Player.Distance(target) <= Q.Range &&
                    Config.Item("UseQHarass").GetValue<bool>())
                {
                    var prediction = Q.GetPrediction(target);
                    if (prediction.Hitchance >= QHitChanceHarass())
                        Q.Cast(prediction.CastPosition);

                }

                if (W.IsReady() && IsHuman && Player.Distance(target) <= W.Range &&
                    Config.Item("UseWHarass").GetValue<bool>())
                {
                    W.Cast(target);
                }
            }
        }


        private static void Farm()
        {
            foreach (
                Obj_AI_Minion Minion in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            minion =>
                                minion.Team != Player.Team && !minion.IsDead &&
                                Vector2.Distance(minion.ServerPosition.To2D(), Player.ServerPosition.To2D()) < 600f)
                        .OrderBy(minion => Vector2.Distance(minion.Position.To2D(), Player.Position.To2D())))
            {
                if (IsCougar)
                {
                    if (QC.IsReady() && Config.Item("UseQLane").GetValue<bool>() && Player.Distance(Minion) < QC.Range)
                        QC.Cast();
                    else if (WC.IsReady() && Config.Item("UseWLane").GetValue<bool>() && Player.Distance(Minion) > 200f)
                        WC.Cast(Minion);
                    else if (EC.IsReady() && Config.Item("UseELane").GetValue<bool>() &&
                             Player.Distance(Minion) < EC.Range)
                        EC.Cast(Minion);
                }

                else if (R.IsReady() && Config.Item("farm_R").GetValue<KeyBind>().Active)
                    R.Cast();
                else if (E.IsReady() && !Config.Item("farm_R").GetValue<KeyBind>().Active &&
                         Config.Item("farm_E1").GetValue<bool>() &&
                         (100*(Player.Mana/Player.MaxMana)) > Config.Item("Lane").GetValue<Slider>().Value)
                    E.CastOnUnit(Player);
                return;
            }
        }

        private static void AutoE()
        {
            if (Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready && Player.IsMe)
            {

                if (Player.HasBuff("Recall") || Utility.InFountain()) return;

                if (E.IsReady() &&
                    Player.Health <= (Player.MaxHealth*(Config.Item("HPercent").GetValue<Slider>().Value)/100))
                {
                    Player.Spellbook.CastSpell(SpellSlot.E, Player);
                }

            }


        }

        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);
            var useQ = Config.Item("UseQLH").GetValue<bool>();
            foreach (var minion in allMinions)
            {
                if (Q.IsReady() && IsHuman && useQ && Player.Distance(minion) < Q.Range &&
                    minion.Health <= 0.95*Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
            }
        }

        private static void AllyAutoE()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
            {
                if (Player.HasBuff("Recall") || hero.HasBuff("Recall") || Utility.InFountain()) return;
                if (E.IsReady() && Config.Item("AllyUseAutoE").GetValue<bool>() &&
                    (hero.Health/hero.MaxHealth)*100 <= Config.Item("AllyHPercent").GetValue<Slider>().Value &&
                    Utility.CountEnemysInRange(1200) > 0 &&
                    hero.Distance(Player.ServerPosition) <= E.Range)
                {
                    E.Cast(hero);
                }
            }
        }

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var igniteDmg = Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            var QHDmg = Player.GetSpellDamage(target, SpellSlot.Q);

            if (target != null && Config.Item("UseIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (igniteDmg > target.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, target);
                }
            }

            if (Q.IsReady() && Player.Distance(target) <= Q.Range && target != null &&
                Config.Item("UseQKs").GetValue<bool>())
            {
                if (target.Health <= QHDmg)
                {
                    Q.Cast(target);
                }
            }
        }

        private static void CheckSpells()
        {
            if (Player.Spellbook.GetSpell(SpellSlot.Q).Name == "JavelinToss" ||
                Player.Spellbook.GetSpell(SpellSlot.W).Name == "Bushwhack" ||
                Player.Spellbook.GetSpell(SpellSlot.E).Name == "PrimalSurge")
            {
                IsHuman = true;
                IsCougar = false;
            }
            if (Player.Spellbook.GetSpell(SpellSlot.Q).Name == "Takedown" ||
                Player.Spellbook.GetSpell(SpellSlot.W).Name == "Pounce" ||
                Player.Spellbook.GetSpell(SpellSlot.E).Name == "Swipe")
            {
                IsHuman = false;
                IsCougar = true;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var cat = Drawing.WorldToScreen(Player.Position);
            if (Config.Item("CircleLag").GetValue<bool>())
            {
                if (Config.Item("DrawQ").GetValue<bool>() && IsHuman)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value,
                        Config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Config.Item("DrawW").GetValue<bool>() && IsHuman)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value,
                        Config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Config.Item("DrawE").GetValue<bool>() && IsHuman)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value,
                        Config.Item("CircleQuality").GetValue<Slider>().Value);
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
            }
            if (Config.Item("DrawCooldown").GetValue<bool>())
            {
                if (!IsCougar)
                {
                    if (_spideQcd == 0)
                        Drawing.DrawText(cat[0] - 60, cat[1], Color.White, "CQ Rdy");
                    else
                        Drawing.DrawText(cat[0] - 60, cat[1], Color.Orange, "CQ: " + _spideQcd.ToString("0.0"));
                    if (_spideWcd == 0)
                        Drawing.DrawText(cat[0] - 20, cat[1] + 30, Color.White, "CW Rdy");
                    else
                        Drawing.DrawText(cat[0] - 20, cat[1] + 30, Color.Orange, "CW: " + _spideWcd.ToString("0.0"));
                    if (_spideEcd == 0)
                        Drawing.DrawText(cat[0], cat[1], Color.White, "CE Rdy");
                    else
                        Drawing.DrawText(cat[0], cat[1], Color.Orange, "CE: " + _spideEcd.ToString("0.0"));
                }
                else
                {
                    if (_humaQcd == 0)
                        Drawing.DrawText(cat[0] - 60, cat[1], Color.White, "HQ Rdy");
                    else
                        Drawing.DrawText(cat[0] - 60, cat[1], Color.Orange, "HQ: " + _humaQcd.ToString("0.0"));
                    if (_humaWcd == 0)
                        Drawing.DrawText(cat[0] - 20, cat[1] + 30, Color.White, "HW Rdy");
                    else
                        Drawing.DrawText(cat[0] - 20, cat[1] + 30, Color.Orange, "HW: " + _humaWcd.ToString("0.0"));
                    if (_humaEcd == 0)
                        Drawing.DrawText(cat[0], cat[1], Color.White, "HE Rdy");
                    else
                        Drawing.DrawText(cat[0], cat[1], Color.Orange, "HE: " + _humaEcd.ToString("0.0"));
                }
            }
        }


        private static void OnCreateObj(GameObject sender, EventArgs args)
        {
            //Recall
            if (!(sender is Obj_GeneralParticleEmitter)) return;
            var obj = (Obj_GeneralParticleEmitter)sender;
            if (obj != null && obj.IsMe && obj.Name == "TeleportHome")
            {
                Recall = true;
            }
        }

        private static void OnDeleteObj(GameObject sender, EventArgs args)
        {
            //Recall
            if (!(sender is Obj_GeneralParticleEmitter)) return;
            var obj = (Obj_GeneralParticleEmitter)sender;
            if (obj != null && obj.IsMe && obj.Name == "TeleportHome")
            {
                Recall = false;
            }
        }
    }
}
     
   
