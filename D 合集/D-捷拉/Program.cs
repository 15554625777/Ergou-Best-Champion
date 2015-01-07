using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace D_Zyra
{
    class Program
    {
        private const string ChampionName = "Zyra";

        private static Orbwalking.Orbwalker _orbwalker;

        private static Spell _q, _w, _e, _r, _passive;

        private static Menu _config;

        private static Obj_AI_Hero _player;

        private static Int32 _lastSkin;

        private static Items.Item _dfg;

        private static SpellSlot _igniteSlot;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            _q = new Spell(SpellSlot.Q, 800);
            _w = new Spell(SpellSlot.W, 825);
            _e = new Spell(SpellSlot.E, 1100);
            _r = new Spell(SpellSlot.R, 700);
            _passive = new Spell(SpellSlot.Q, 1470);

            _q.SetSkillshot(0.8f, 60f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            _e.SetSkillshot(0.5f, 70f, 1400f, false, SkillshotType.SkillshotLine);
            _r.SetSkillshot(0.5f, 500f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            _passive.SetSkillshot(0.5f, 70f, 1400f, false, SkillshotType.SkillshotLine);

            _igniteSlot = _player.GetSpellSlot("SummonerDot");
            _dfg = new Items.Item(3128, 750f);
            //D Zyra
            _config = new Menu("D-捷拉", "D-Zyra", true);

           //TargetSelector
           var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
           TargetSelector.AddToMenu(targetSelectorMenu);
           _config.AddSubMenu(targetSelectorMenu);


            //Orbwalker
            _config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));


            //Combo usedfg, useignite
            _config.AddSubMenu(new Menu("连招", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("usedfg", "使用 冥火")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("useignite", "使用 引燃")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("useQC", "使用 Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("useW_Passive", "显示W位置").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("useEC", "使用 E")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("use_ulti", "使用R丨假如可击杀")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRE", "自动使用R连招")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("MinTargets", "使用R丨最少目标数").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "连招!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //harass
            _config.AddSubMenu(new Menu("骚扰", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("useQH", "使用 Q").SetValue(true));
            _config.SubMenu("Harass").AddItem(new MenuItem("useW_Passiveh", "显示W位置").SetValue(true));
            _config.SubMenu("Harass").AddItem(new MenuItem("useEH", "使用 E").SetValue(true));
            _config.SubMenu("Harass")
                .AddItem(new MenuItem("harassmana", "骚扰最低蓝量").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "骚扰（自动）").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "骚扰!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //Farm
            _config.AddSubMenu(new Menu("清线", "Farm"));
            _config.SubMenu("Farm").AddSubMenu(new Menu("清线", "LaneClear"));
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("useQL", "使用 Q").SetValue(true));
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("useW_Passivel", "显示W位置").SetValue(true));
            _config.SubMenu("Farm").SubMenu("LaneClear").AddItem(new MenuItem("useEL", "使用 E").SetValue(true));
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(new MenuItem("lanemana", "清线最低蓝量").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("Farm")
                .SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("Activelane", "清线!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));
            _config.SubMenu("Farm").AddSubMenu(new Menu("清野", "Jungle"));
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("useQJ", "使用 Q").SetValue(true));
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("useW_Passivej", "显示W位置").SetValue(true));
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("useEJ", "使用 E").SetValue(true));
            _config.SubMenu("Farm")
                .SubMenu("Jungle")
                .AddItem(new MenuItem("junglemana", "清野最低蓝量").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("Farm")
                .SubMenu("Jungle")
                .AddItem(
                    new MenuItem("ActiveJungle", "清野!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Misc
            _config.AddSubMenu(new Menu("杂项", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("usePackets", "使用封包")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("useQkill", "使用 Q 抢人头")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("useEkill", "使用 E 抢人头")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("Inter_E", "E 打断技能")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("Gap_E", "E 防止突进")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("support", "辅助模式")).SetValue(true);

            //Damage after combo:
            MenuItem dmgAfterComboItem = new MenuItem("DamageAfterCombo", "显示组合连招伤害").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };
            //Draw
            _config.AddSubMenu(new Menu("范围选项", "Drawing"));
            _config.SubMenu("Drawing").AddItem(new MenuItem("DrawQ", "范围 Q").SetValue(true));
            _config.SubMenu("Drawing").AddItem(new MenuItem("DrawW", "范围 W").SetValue(true));
            _config.SubMenu("Drawing").AddItem(new MenuItem("DrawE", "范围 E").SetValue(true));
            _config.SubMenu("Drawing").AddItem(new MenuItem("DrawR", "范围 R").SetValue(true));
            _config.SubMenu("Drawing").AddItem(dmgAfterComboItem);
            _config.SubMenu("Drawings").AddItem(new MenuItem("damagetest", "显示伤害文本")).SetValue(true);
            _config.SubMenu("Drawing").AddItem(new MenuItem("CircleLag", "自由延迟圈").SetValue(true));
            _config.SubMenu("Drawing")
                .AddItem(new MenuItem("CircleQuality", "圈 质量").SetValue(new Slider(100, 100, 10)));
            _config.SubMenu("Drawing")
                .AddItem(new MenuItem("CircleThickness", "圈 厚度").SetValue(new Slider(1, 10, 1)));

            _config.AddToMainMenu();

            Game.PrintChat("<font color='#881df2'>D-濠曟媺 by Diabaths</font> 鍔犺浇鎴愬姛!姹夊寲by楝肩伒!QQ缇361630847.");
            Game.PrintChat(
               "<font color='#FF0000'>If You like my work and want to support, and keep it always up to date plz donate via paypal in </font> <font color='#FF9900'>ssssssssssmith@hotmail.com</font> (10) S");
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (ZyraisZombie())
            {
                CastPassive();
                return;
            }
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if ((_config.Item("ActiveHarass").GetValue<KeyBind>().Active ||
                 _config.Item("harasstoggle").GetValue<KeyBind>().Active) &&
                (100*(_player.Mana/_player.MaxMana)) > _config.Item("harassmana").GetValue<Slider>().Value)
            {
                Harass();

            }
            if (_config.Item("Activelane").GetValue<KeyBind>().Active &&
                (100*(_player.Mana/_player.MaxMana)) > _config.Item("lanemana").GetValue<Slider>().Value)
            {
                Laneclear();
            }
            if (_config.Item("ActiveJungle").GetValue<KeyBind>().Active &&
                (100*(_player.Mana/_player.MaxMana)) > _config.Item("junglemana").GetValue<Slider>().Value)
            {
                JungleClear();
            }
            _player = ObjectManager.Player;

            _orbwalker.SetAttack(true);
            KillSteal();
        }
        // princer007  Code
        static int Getallies(float range)
        {
            int allies = 0;
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                if (hero.IsAlly && !hero.IsMe && _player.Distance(hero) <= range) allies++;
            return allies;
        }
        static void Orbwalking_BeforeAttack(LeagueSharp.Common.Orbwalking.BeforeAttackEventArgs args)
        {
            if (Getallies(1000) > 0 && ((Obj_AI_Base)_orbwalker.GetTarget()).IsMinion && /*args.Unit.IsMinion &&*/ _config.Item("support").GetValue<bool>()) args.Process = false;
        }
        private static float ComboDamage(Obj_AI_Hero hero)
        {
            var dmg = 0d;

            if (_q.IsReady())
            {
                if (_w.IsReady())
                {
                    dmg += _player.GetSpellDamage(hero, SpellSlot.Q) + (23 + 6.5*ObjectManager.Player.Level) +
                           (1.2*_player.FlatMagicDamageMod);
                }
                else dmg += _player.GetSpellDamage(hero, SpellSlot.Q);
            }

            if (_e.IsReady())
            {
                if (_w.IsReady())
                {
                    dmg += _player.GetSpellDamage(hero, SpellSlot.E) + (23 + 6.5*ObjectManager.Player.Level) +
                           (1.2*_player.FlatMagicDamageMod);
                }
                else dmg += _player.GetSpellDamage(hero, SpellSlot.E);
            }
            if (_r.IsReady())
                dmg += _player.GetSpellDamage(hero, SpellSlot.R);
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                dmg += _player.GetItemDamage(hero, Damage.DamageItems.Botrk);
            if (Items.HasItem(3146) && Items.CanUseItem(3146))
                dmg += _player.GetItemDamage(hero, Damage.DamageItems.Hexgun);
            if (Items.HasItem(3128) && Items.CanUseItem(3128))
            {
                dmg += _player.GetItemDamage(hero, Damage.DamageItems.Dfg);
                dmg = dmg * 1.2;
            }
            if (ObjectManager.Player.HasBuff("LichBane"))
            {
                dmg += _player.BaseAttackDamage * 0.75 + _player.FlatMagicDamageMod * 0.5;
            }
            if (ObjectManager.Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                dmg += _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            dmg += _player.GetAutoAttackDamage(hero, true);
            return (float) dmg;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var manacheck = _player.Mana >
                            _player.Spellbook.GetSpell(SpellSlot.W).ManaCost +
                            _player.Spellbook.GetSpell(SpellSlot.E).ManaCost;
            var pos = _e.GetPrediction(gapcloser.Sender).CastPosition;
            if (!_config.Item("Gap_E").GetValue<bool>()) return;
            if (manacheck && _e.IsReady() && gapcloser.Sender.IsValidTarget(_e.Range))
            {
                _e.CastIfHitchanceEquals(gapcloser.Sender, HitChance.High, Packets());
                Utility.DelayAction.Add(50, () => _w.Cast(new Vector3(pos.X - 2, pos.Y - 2, pos.Z), Packets()));
                Utility.DelayAction.Add(150, () => _w.Cast(new Vector3(pos.X + 2, pos.Y + 2, pos.Z), Packets()));
            }
            else if (_e.IsReady() && gapcloser.Sender.IsValidTarget(_e.Range))
            {
                _e.CastIfHitchanceEquals(gapcloser.Sender, HitChance.High, Packets());
            }
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            var manacheck = _player.Mana >
                            _player.Spellbook.GetSpell(SpellSlot.W).ManaCost +
                            _player.Spellbook.GetSpell(SpellSlot.E).ManaCost;
            var pos = _e.GetPrediction(unit).CastPosition;
            if (!_config.Item("Inter_E").GetValue<bool>()) return;
            if (manacheck && _e.IsReady() && unit.IsValidTarget(_e.Range))
            {
                _e.CastIfHitchanceEquals(unit, HitChance.High, Packets());
                Utility.DelayAction.Add(50, () => _w.Cast(new Vector3(pos.X - 2, pos.Y - 2, pos.Z), Packets()));
                Utility.DelayAction.Add(150, () => _w.Cast(new Vector3(pos.X + 2, pos.Y + 2, pos.Z), Packets()));
            }
            else if (_e.IsReady() && unit.IsValidTarget(_e.Range))
            {
                _e.CastIfHitchanceEquals(unit, HitChance.High, Packets());
            }
        }

        private static void Combo()
        {
            var usedfg = _config.Item("usedfg").GetValue<bool>();
            var useignite = _config.Item("useignite").GetValue<bool>();
            var target = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
            if (_player.Distance(target) <= _dfg.Range && usedfg &&
                      _dfg.IsReady() && target.Health <= ComboDamage(target))
            {
                _dfg.Cast(target);
            }
            if (useignite && _igniteSlot != SpellSlot.Unknown && _player.Distance(target) <= 600 &&
                   _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                if (target.Health <= ComboDamage(target))
                {
                    _player.Spellbook.CastSpell(_igniteSlot, target);
                }
            }
            if (_config.Item("UseRE").GetValue<bool>() || _config.Item("use_ulti").GetValue<bool>())
                CastREnemy();
            if (_config.Item("useQC").GetValue<bool>())
                CastQEnemy();
            if (_config.Item("useEC").GetValue<bool>())
                CastEEnemy();
           }

        private static void Harass()
        {
            if (_config.Item("useQH").GetValue<bool>())
                CastQEnemy();
            if (_config.Item("useEH").GetValue<bool>())
                CastEEnemy();
        }

        private static void Laneclear()
        {
            if (_config.Item("useQL").GetValue<bool>())
                CastQMinion();
            if (_config.Item("useEL").GetValue<bool>())
                CastEMinion();
        }

        private static void JungleClear()
        {
            if (_config.Item("useQJ").GetValue<bool>())
                CastQjungleMinion();
            if (_config.Item("useEJ").GetValue<bool>())
                CastEjungleMinion();
        }

        private static bool Packets()
        {
            return _config.Item("usePackets").GetValue<bool>();
        }

        private static bool ZyraisZombie()
        {
            return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name ==
                   ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Name ||
                   ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name ==
                   ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name;
        }
        private static void CastEjungleMinion()
        {
            if (!_e.IsReady())
                return;
            var minions = MinionManager.GetMinions(_player.ServerPosition, _e.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (minions.Count == 0)
                return;
            var castPostion =
                MinionManager.GetBestLineFarmLocation(
                    minions.Select(minion => minion.ServerPosition.To2D()).ToList(),
                    _e.Width, _e.Range);
            _e.Cast(castPostion.Position, Packets());
            if (_config.Item("useW_Passivej").GetValue<bool>() && _w.IsReady())
            {
                var pos = castPostion.Position.To3D();
                Utility.DelayAction.Add(50, () => _w.Cast(new Vector3(pos.X - 5, pos.Y - 5, pos.Z), Packets()));
                Utility.DelayAction.Add(150, () => _w.Cast(new Vector3(pos.X + 5, pos.Y + 5, pos.Z), Packets()));
            }
        }
        private static void CastEMinion()
        {
            if (!_e.IsReady())
                return;
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range, MinionTypes.All);
            if (minions.Count == 0)
                return;
            var castPostion =
                MinionManager.GetBestLineFarmLocation(
                    minions.Select(minion => minion.ServerPosition.To2D()).ToList(),
                    _e.Width, _e.Range);
            _e.Cast(castPostion.Position, Packets());
            if (_config.Item("useW_Passivel").GetValue<bool>() && _w.IsReady())
            {
                var pos = castPostion.Position.To3D();
                Utility.DelayAction.Add(50, () => _w.Cast(new Vector3(pos.X - 5, pos.Y - 5, pos.Z), Packets()));
                Utility.DelayAction.Add(150, () => _w.Cast(new Vector3(pos.X + 5, pos.Y + 5, pos.Z), Packets()));
            }
        }

       
        private static void CastQjungleMinion()
        {
            if (!_q.IsReady())
                return;
            var minions = MinionManager.GetMinions(_player.ServerPosition, _q.Range,
               MinionTypes.All,
               MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (minions.Count == 0)
                return;
            var castPostion =
                MinionManager.GetBestCircularFarmLocation(
                    minions.Select(minion => minion.ServerPosition.To2D()).ToList(), _q.Width, _q.Range);
            _q.Cast(castPostion.Position, Packets());
            if (_config.Item("useW_Passivej").GetValue<bool>() && _w.IsReady())
            {
                var pos = castPostion.Position.To3D();
                Utility.DelayAction.Add(50, () => _w.Cast(new Vector3(pos.X - 5, pos.Y - 5, pos.Z), Packets()));
                Utility.DelayAction.Add(150, () => _w.Cast(new Vector3(pos.X + 5, pos.Y + 5, pos.Z), Packets()));
            }
        }
        private static void CastQMinion()
        {
           if (!_q.IsReady())
                return;
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, _q.Range + (_q.Width/2),
                MinionTypes.All, MinionTeam.NotAlly);
            if (minions.Count == 0)
                return;
            var castPostion =
                MinionManager.GetBestCircularFarmLocation(
                    minions.Select(minion => minion.ServerPosition.To2D()).ToList(), _q.Width, _q.Range);
            _q.Cast(castPostion.Position, Packets());
            if (_config.Item("useW_Passivel").GetValue<bool>() && _w.IsReady())
            {
                var pos = castPostion.Position.To3D();
                Utility.DelayAction.Add(50, () => _w.Cast(new Vector3(pos.X - 5, pos.Y - 5, pos.Z), Packets()));
                Utility.DelayAction.Add(150, () => _w.Cast(new Vector3(pos.X + 5, pos.Y + 5, pos.Z), Packets()));
            }
        }

        private static void CastREnemy()
        {
            if (!_r.IsReady())
            {
                return;
            }
            var target = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
            var rpred = _r.GetPrediction(target);
            if (!target.IsValidTarget(_r.Range))
            {
                return;
            }
            if (ComboDamage(target) > target.Health && _config.Item("use_ulti").GetValue<bool>() &&
                GetNumberHitByR(target) >= 1)
            {
                _r.Cast(rpred.CastPosition, Packets());
            }
            if (GetNumberHitByR(target) >= _config.Item("MinTargets").GetValue<Slider>().Value)
            {
                _r.Cast(rpred.CastPosition, Packets());
            }
        }

        private static int GetNumberHitByR(Obj_AI_Hero target)
        {
            int Enemys = 0;
            foreach (Obj_AI_Hero enemys in ObjectManager.Get<Obj_AI_Hero>())
            {
                var pred = _r.GetPrediction(enemys, true);
                if (pred.Hitchance >= HitChance.High && !enemys.IsMe && enemys.IsEnemy && Vector3.Distance(_player.Position, pred.UnitPosition) <= _r.Range)
                {
                    Enemys = Enemys + 1;
                }
            }
            return Enemys;
        }
        private static void CastQEnemy()
        {
            if (!_q.IsReady())
                return;
            var target = TargetSelector.GetTarget(_q.Range + (_q.Width / 2), TargetSelector.DamageType.Magical);
            if (!target.IsValidTarget(_q.Range))
                return;
            _q.CastIfHitchanceEquals(target, HitChance.High, Packets());
            if (_config.Item("useW_Passive").GetValue<bool>() && _w.IsReady())
            {
                var pos = _q.GetPrediction(target).CastPosition;
                Utility.DelayAction.Add(50, () => _w.Cast(new Vector3(pos.X - 2, pos.Y - 2, pos.Z), Packets()));
                Utility.DelayAction.Add(150, () => _w.Cast(new Vector3(pos.X + 2, pos.Y + 2, pos.Z), Packets()));
            }
        }

        private static void CastEEnemy()
        {
            if (!_e.IsReady())
                return;
            var target = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
            if (!target.IsValidTarget(_e.Range))
                return;
            _e.CastIfHitchanceEquals(target, HitChance.High, Packets());
            if (_config.Item("useW_Passive").GetValue<bool>() && _w.IsReady())
            {
                var pos = _e.GetPrediction(target).CastPosition;
                Utility.DelayAction.Add(50, () => _w.Cast(new Vector3(pos.X - 5, pos.Y - 5, pos.Z), Packets()));
                Utility.DelayAction.Add(150, () => _w.Cast(new Vector3(pos.X + 5, pos.Y + 5, pos.Z), Packets()));
            }
        }

        private static void CastPassive()
        {
            if (!_passive.IsReady())
                return;
            var target = TargetSelector.GetTarget(_passive.Range, TargetSelector.DamageType.Magical);
            if (!target.IsValidTarget(_e.Range))
                return;
            _passive.CastIfHitchanceEquals(target, HitChance.High, Packets());
        }

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
            var useq = _config.Item("useQkill").GetValue<bool>();
            var usee = _config.Item("useEkill").GetValue<bool>();
            var whDmg = _player.GetSpellDamage(target, SpellSlot.W);
            var qhDmg = _player.GetSpellDamage(target, SpellSlot.Q);
            var ehDmg = _player.GetSpellDamage(target, SpellSlot.E);
            var emana = _player.Spellbook.GetSpell(SpellSlot.E).ManaCost;
            var wmana = _player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
            var qmana = _player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            if (useq && target.IsValidTarget(_q.Range) && _q.IsReady())
            {
                if (qhDmg >= target.Health && qmana < _player.Mana)
                {
                    _q.CastIfHitchanceEquals(target, HitChance.High, Packets());

                }
                else if (qhDmg + whDmg > target.Health && _player.Mana >= qmana + wmana && _w.IsReady())
                {
                    _q.CastIfHitchanceEquals(target, HitChance.High, Packets());
                    var pos = _e.GetPrediction(target).CastPosition;
                    Utility.DelayAction.Add(50, () => _w.Cast(new Vector3(pos.X - 5, pos.Y - 5, pos.Z), Packets()));
                    Utility.DelayAction.Add(150, () => _w.Cast(new Vector3(pos.X + 5, pos.Y + 5, pos.Z), Packets()));
                }
            }
            if (usee && target.IsValidTarget(_e.Range) && _e.IsReady())
            {
                if (ehDmg >= target.Health && emana < _player.Mana)
                {
                    _e.CastIfHitchanceEquals(target, HitChance.High, Packets());

                }
                else if (ehDmg + whDmg > target.Health && _player.Mana >= emana + wmana && _w.IsReady())
                {
                    _e.CastIfHitchanceEquals(target, HitChance.High, Packets());
                    var pos = _e.GetPrediction(target).CastPosition;
                    Utility.DelayAction.Add(50, () => _w.Cast(new Vector3(pos.X - 5, pos.Y - 5, pos.Z), Packets()));
                    Utility.DelayAction.Add(150, () => _w.Cast(new Vector3(pos.X + 5, pos.Y + 5, pos.Z), Packets()));
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_config.Item("damagetest").GetValue<bool>())
            {
                foreach (
                var enemyVisible in
                ObjectManager.Get<Obj_AI_Hero>().Where(enemyVisible => enemyVisible.IsValidTarget()))
                {
                    if (ComboDamage(enemyVisible) > enemyVisible.Health)
                    {
                        Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)[0] + 50,
                        Drawing.WorldToScreen(enemyVisible.Position)[1] - 40, Color.Red,
                        "Combo=Rekt");
                    }
                    else if (ComboDamage(enemyVisible) + _player.GetAutoAttackDamage(enemyVisible, true) * 2 >
                    enemyVisible.Health)
                    {
                        Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)[0] + 50,
                        Drawing.WorldToScreen(enemyVisible.Position)[1] - 40, Color.Orange,
                        "Combo+AA=Rekt");
                    }
                    else
                        Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)[0] + 50,
                        Drawing.WorldToScreen(enemyVisible.Position)[1] - 40, Color.Green,
                        "Unkillable");
                }
            }
            if (_config.Item("CircleLag").GetValue<bool>())
            {

                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.Gray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _w.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White);
                }

                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.White);
                }
            }
        }
    }
}
       