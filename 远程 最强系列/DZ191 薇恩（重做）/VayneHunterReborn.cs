using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using VayneHunter_Reborn.Utility;
using Color = System.Drawing.Color;

namespace VayneHunter_Reborn
{
    class VayneHunterReborn
    {
        public static Menu Menu;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Orbwalking.Orbwalker Orbwalker;
        private static Vector3 predictedPosition;
        private static Vector3 predictedEndPosition;
        private readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q) },
            { SpellSlot.W, new Spell(SpellSlot.W) },
            { SpellSlot.E, new Spell(SpellSlot.E, 590f) },
            { SpellSlot.R, new Spell(SpellSlot.R) }
        };
        private static Notification condemnNotification = new Notification("Condemned: ",5500);
        public VayneHunterReborn()
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Vayne")
            {
                return;
            }
            Menu = new Menu("DZ191 薇恩（重做）","VHR",true);
            var owMenu = new Menu("[VHR] 走砍", "dz191.vhr.orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(owMenu);
            Menu.AddSubMenu(owMenu);

            var tgMenu = new Menu("[VHR] 目标选择", "dz191.vhr.targetselector");
            TargetSelector.AddToMenu(tgMenu);
            Menu.AddSubMenu(tgMenu);

            var comboMenu = new Menu("[VHR] 连招", "dz191.vhr.combo");
            comboMenu.AddModeMenu(Mode.Combo,new []{SpellSlot.Q,SpellSlot.E,SpellSlot.R},new []{true,true,false});
            comboMenu.AddManaManager(Mode.Combo, new[] { SpellSlot.Q, SpellSlot.E, SpellSlot.R }, new[] { 25,20,20 });
            comboMenu.AddItem(new MenuItem("dz191.vhr.combo.r.minenemies", "使用R|敌人数量").SetValue(new Slider(2, 1, 5)));
            Menu.AddSubMenu(comboMenu);
            var harassMenu = new Menu("[VHR] 骚扰", "dz191.vhr.harass");
            harassMenu.AddModeMenu(Mode.Harrass, new[] { SpellSlot.Q, SpellSlot.E }, new[] { true, true });
            harassMenu.AddManaManager(Mode.Harrass, new[] { SpellSlot.Q, SpellSlot.E}, new[] { 25, 20 });
            Menu.AddSubMenu(harassMenu);
            var farmMenu = new Menu("[VHR] 清线", "dz191.vhr.farm");
            farmMenu.AddModeMenu(Mode.Farm, new[] { SpellSlot.Q}, new[] { true, true });
            farmMenu.AddManaManager(Mode.Farm, new[] { SpellSlot.Q }, new[] { 40 });
            Menu.AddSubMenu(farmMenu);
            
            var miscMenu = new Menu("[VHR] 杂项", "dz191.vhr.misc");
            var miscQMenu = new Menu("杂项 - 闪避突袭", "dz191.vhr.misc.tumble");
            {
                miscQMenu.AddItem(new MenuItem("dz191.vhr.misc.tumble.smartq", "尝试优先QE").SetValue(false));
                miscQMenu.AddItem(new MenuItem("dz191.vhr.misc.tumble.noqenemies", "禁用Q向敌方阵营").SetValue(true));
                miscQMenu.AddItem(new MenuItem("dz191.vhr.misc.tumble.noaastealth", "禁用走A|R刷新被动").SetValue(false));
                miscQMenu.AddItem(new MenuItem("dz191.vhr.misc.tumble.walltumble", "Q翻墙键位").SetValue(new KeyBind("Y".ToCharArray()[0],KeyBindType.Press)));
            }
            var miscEMenu = new Menu("杂项 - 恶魔审判", "dz191.vhr.misc.condemn");
            {
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.enextauto", "启用自动E墙").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.condemnmethod", "审判 模式").SetValue(new StringList(new []{"VH Reborn","Marksman/Gosu"})));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.pushdistance", "E推开敌人距离").SetValue(new Slider(375, 350, 500)));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.condemnturret", "尝试E向炮塔").SetValue(false));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.condemnflag", "E向 J4 标记").SetValue(true));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.autoe", "自动 E").SetValue(false));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.eks", "智能E击杀").SetValue(true));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.ethird", "骚扰中使用E叠加W三次被动").SetValue(false));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.noeturret", "在敌方塔下禁用E").SetValue(true));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.lowlifepeel", "当虚弱时E开敌人").SetValue(false));
            }
            var miscGeneralSubMenu = new Menu("杂项 - 常规", "dz191.vhr.misc.general");
            {
                miscGeneralSubMenu.AddItem(new MenuItem("dz191.vhr.misc.general.antigp", "防止突进")).SetValue(true);
                miscGeneralSubMenu.AddItem(new MenuItem("dz191.vhr.misc.general.interrupt", "中断法术").SetValue(true));
                miscGeneralSubMenu.AddItem(new MenuItem("dz191.vhr.misc.general.specialfocus", "优先攻击W2次被动目标").SetValue(false));
            }
            miscMenu.AddSubMenu(miscQMenu);
            miscMenu.AddSubMenu(miscEMenu);
            miscMenu.AddSubMenu(miscGeneralSubMenu);
            Menu.AddSubMenu(miscMenu);

            var drawMenu = new Menu("[VHR] 显示", "dz191.vhr.drawing");
            drawMenu.AddDrawMenu(_spells,Color.Red);
            drawMenu.AddItem(new MenuItem("dz191.vhr.drawing.drawstun", "显示 范围").SetValue(true));
            drawMenu.AddItem(new MenuItem("dz191.vhr.drawing.drawspots", "显示 翻墙点").SetValue(true));
            Menu.AddSubMenu(drawMenu);

            Menu.AddToMainMenu();
            Game.PrintChat("<b><font color='#FF0000'>[VH]</font></b><font color='#FFFFFF'> DZ191 钖囨仼锛堥噸鍋氾級 鍔犺浇鎴愬姛!姹夊寲by浜岀嫍!QQ缇361630847! Version: {0} </font>",Assembly.GetExecutingAssembly().GetName().Version);
            SetUpEvents();
            SetUpSkills();
            Console.Clear();
        }

        void SetUpSkills()
        {
            _spells[SpellSlot.E].SetTargetted(0.35f,2200f);
        }

        void SetUpEvents()
        {
            Cleanser.OnLoad();
            PotionManager.OnLoad(Menu);
            ItemManager.OnLoad(Menu);

            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harrass();
                    break;

            }
            OnUpdateFunctions();
        }

        private void OnUpdateFunctions()
        {        
            #region Auto E
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.autoe"))
            {
                Obj_AI_Hero target;
                if (CondemnCheck(ObjectManager.Player.ServerPosition, out target))
                {
                    if (target.IsValidTarget(_spells[SpellSlot.E].Range) && (target is Obj_AI_Hero))
                    {
                        _spells[SpellSlot.E].Cast(target);
                    }
                }
            }
            #endregion

            #region Focus 2 W stacks
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.general.specialfocus"))
            {
                var target = HeroManager.Enemies.Find(en => en.IsValidTarget(ObjectManager.Player.AttackRange) && en.Has2WStacks());
                if (target != null)
                {
                   // Orbwalker.ForceTarget(target);
                  //  Hud.SelectedUnit = target;
                }
            }
            #endregion

            #region Disable AA Stealth
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble.noaastealth"))
            {
                Orbwalker.SetAttack(!Helpers.IsPlayerFaded());
            }
            #endregion

            #region Condemn KS
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.eks"))
            {
                var target = HeroManager.Enemies.Find(en => en.IsValidTarget(_spells[SpellSlot.E].Range) && en.Has2WStacks());
                if (target != null && target.Health + 20 <=(_spells[SpellSlot.E].GetDamage(target) + _spells[SpellSlot.W].GetDamage(target)) && (target is Obj_AI_Hero))
                {
                    _spells[SpellSlot.E].Cast(target);
                }
            }
            #endregion

            #region WallTumble

            if (Menu.Item("dz191.vhr.misc.tumble.walltumble").GetValue<KeyBind>().Active)
            {
                Vector2 MidWallQPos = new Vector2(6707.485f, 8802.744f);
                Vector2 drakeWallQPos = new Vector2(11514, 4462);
                Vector2 baseWallQPos = new Vector2(12171.35f, 10381.68f);

                if (ObjectManager.Player.Distance(baseWallQPos) < ObjectManager.Player.Distance(drakeWallQPos) && ObjectManager.Player.Distance(baseWallQPos) < ObjectManager.Player.Distance(MidWallQPos))
                {
                    WallTumble2();
                }
                else
                {
                    WallTumble();
                }
    
            }
            #endregion

            #region Low Life Peel

            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.lowlifepeel") &&ObjectManager.Player.HealthPercentage() <= 20)
            {
                var meleeEnemies = ObjectManager.Player.GetEnemiesInRange(350f).FindAll(m => m.IsMelee());
                if (meleeEnemies.Any())
                {
                    var mostDangerous = meleeEnemies.OrderByDescending(m => m.GetAutoAttackDamage(ObjectManager.Player)).First();
                    if (mostDangerous is Obj_AI_Hero)
                    {
                        _spells[SpellSlot.E].Cast(mostDangerous);
                    }
                }
            }
            #endregion

        }

        private void Combo()
        {
            if (_spells[SpellSlot.E].IsEnabledAndReady(Mode.Combo))
            {
                Obj_AI_Hero target;
                if (CondemnCheck(ObjectManager.Player.ServerPosition, out target))
                {
                    if (target.IsValidTarget(_spells[SpellSlot.E].Range) && (target is Obj_AI_Hero))
                    {
                        _spells[SpellSlot.E].Cast(target);
                       //Notifications.AddNotification(new Notification("Condemned " + target.ChampionName, 1000));
                    }
                }
            }
        }

        private void Harrass()
        {
            if (_spells[SpellSlot.E].IsEnabledAndReady(Mode.Harrass))
            {
                var possibleTarget = HeroManager.Enemies.Find(enemy => enemy.IsValidTarget(_spells[SpellSlot.E].Range) && enemy.Has2WStacks());
                if (possibleTarget != null && MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.ethird") && (possibleTarget is Obj_AI_Hero))
                {
                    _spells[SpellSlot.E].Cast(possibleTarget);
                }

                Obj_AI_Hero target;
                if (CondemnCheck(ObjectManager.Player.ServerPosition, out target))
                {
                    if (target.IsValidTarget(_spells[SpellSlot.E].Range) && (target is Obj_AI_Hero))
                    {
                        _spells[SpellSlot.E].Cast(target);
                    }
                }
            }
        }

        private void Farm()
        {
            if (!_spells[SpellSlot.Q].IsEnabledAndReady(Mode.Farm))
            {
                return;
            }
            var minionsInRange = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Player.AttackRange).FindAll(m => m.Health <= Player.GetAutoAttackDamage(m) + _spells[SpellSlot.Q].GetDamage(m)).ToList();
            if (!minionsInRange.Any())
            {
                return;
            }
            if (minionsInRange.Count > 1)
            {
                var firstMinion = minionsInRange.OrderBy(m => m.HealthPercentage()).First();
                CastTumble(firstMinion);
                Orbwalker.ForceTarget(firstMinion);
            }
        }

        void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!(target is Obj_AI_Base) || !unit.IsMe)
            {
                return;
            }
            var tg = (Obj_AI_Base) target;
            if (MenuHelper.getKeybindValue("dz191.vhr.misc.condemn.enextauto") &&
                _spells[SpellSlot.E].CanCast(tg) && (tg is Obj_AI_Hero))
            {
                _spells[SpellSlot.E].Cast(tg);
                Menu.Item("dz191.vhr.misc.condemn.enextauto").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle));
            }
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (_spells[SpellSlot.Q].IsEnabledAndReady(Mode.Combo))
                    {
                        CastQ(tg);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (_spells[SpellSlot.Q].IsEnabledAndReady(Mode.Harrass))
                    {
                        CastQ(tg);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Farm();
                    break;
                default:
                    return;
            }
        }


        void Drawing_OnDraw(EventArgs args)
        {
            var drawE = Menu.Item("VayneDrawE").GetValue<Circle>();
            var midWallQPos = new Vector2(6707.485f, 8802.744f);
            var drakeWallQPos = new Vector2(11514, 4462);
            if (drawE.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position,_spells[SpellSlot.E].Range,drawE.Color);
            }
            if (MenuHelper.isMenuEnabled("dz191.vhr.drawing.drawstun"))
            {
                Obj_AI_Hero myTarget;
                if (CondemnCheck(ObjectManager.Player.ServerPosition, out myTarget))
                {
                    if (myTarget != null)
                    {
                        Drawing.DrawText(myTarget.Position.X, myTarget.Position.Y, Color.Aqua, "Stunnable!");
                    }
                }
            }
            if (MenuHelper.isMenuEnabled("dz191.vhr.drawing.drawspots"))
            {
                if (ObjectManager.Player.Distance(midWallQPos) <= 1500f)
                {
                    Render.Circle.DrawCircle(midWallQPos.To3D2(), 65f, Color.AliceBlue);
                }
                if (ObjectManager.Player.Distance(drakeWallQPos) <= 1500f)
                {
                    Render.Circle.DrawCircle(drakeWallQPos.To3D2(), 65f, Color.AliceBlue);
                }
            }
           // Render.Circle.DrawCircle(predictedEndPosition, 65, drawE.Color);
           // Render.Circle.DrawCircle(predictedPosition, 65, Color.LightBlue);
        }

        void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.general.antigp"))
            {
               
                if (gapcloser.Sender.IsValidTarget(_spells[SpellSlot.E].Range) && gapcloser.End.Distance(ObjectManager.Player.ServerPosition) <= 375f && (gapcloser.Sender is Obj_AI_Hero))
                {
                    _spells[SpellSlot.E].Cast(gapcloser.Sender);
                }
            }
        }

        void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.general.interrupt"))
            {
                if (args.DangerLevel == Interrupter2.DangerLevel.High && sender.IsValidTarget(_spells[SpellSlot.E].Range) && (sender is Obj_AI_Hero))
                {
                    _spells[SpellSlot.E].Cast(sender);
                }
            }
        }
        

        #region Tumble Region
        private void CastQ(Obj_AI_Base target)
        {
            if (Environment.TickCount - _spells[SpellSlot.E].LastCastAttemptT <= 265)
            {
                return;
            }
            var myPosition = Game.CursorPos;
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble.smartq") && _spells[SpellSlot.E].IsReady()) 
            {
                const int currentStep = 35;
                var direction = ObjectManager.Player.Direction.To2D().Perpendicular();
                for (var i = 0; i < 360; i += currentStep)
                {
                    var angleRad = Geometry.DegreeToRadian(i);
                    var rotatedPosition = ObjectManager.Player.Position.To2D() + (300f * direction.Rotated(angleRad));
                    Obj_AI_Hero myTarget;
                    if (CondemnCheck(rotatedPosition.To3D(), out myTarget) && Helpers.OkToQ(rotatedPosition.To3D()))
                    {
                        myPosition = rotatedPosition.To3D();
                        break;
                    }
                }
            }
            if (_spells[SpellSlot.R].IsEnabledAndReady(Mode.Combo) && ObjectManager.Player.CountEnemiesInRange(Orbwalking.GetRealAutoAttackRange(null)) >= MenuHelper.getSliderValue("dz191.vhr.combo.r.minenemies"))
            {
                _spells[SpellSlot.R].Cast();
            }
            CastTumble(myPosition,target);
        }

        void CastTumble(Obj_AI_Base target)
        {
            if (!_spells[SpellSlot.Q].IsReady())
            {
                return;
            }
            var posAfterTumble =
                ObjectManager.Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), 300).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if (distanceAfterTumble < 550 * 550 && distanceAfterTumble > 100 * 100)
            {
                if (!Helpers.OkToQ(posAfterTumble) && MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble.noqenemies"))
                {
                    return;
                }
                _spells[SpellSlot.Q].Cast(Game.CursorPos);
            }
        }
        void CastTumble(Vector3 pos, Obj_AI_Base target)
        {
            if (!_spells[SpellSlot.Q].IsReady())
            {
                return;
            }
            var posAfterTumble =
                ObjectManager.Player.ServerPosition.To2D().Extend(pos.To2D(), 300).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if (distanceAfterTumble < 550 * 550 && distanceAfterTumble > 100 * 100)
            {
                if (!Helpers.OkToQ(posAfterTumble) && MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble.noqenemies"))
                {
                    return;
                }
                _spells[SpellSlot.Q].Cast(pos);
            }
        }

        #endregion

        #region E Region

        bool CondemnCheck(Vector3 fromPosition, out Obj_AI_Hero tg)
        {
            if (fromPosition.UnderTurret(true) && MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.noeturret") || !_spells[SpellSlot.E].IsReady())
            {
                tg = null;
                return false;
            }
            switch (Menu.Item("dz191.vhr.misc.condemn.condemnmethod").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    //VHReborn Condemn Code
                    foreach (var target in HeroManager.Enemies.Where(h => h.IsValidTarget(_spells[SpellSlot.E].Range) && !h.HasBuffOfType(BuffType.SpellShield) && !h.HasBuffOfType(BuffType.SpellImmunity)))
                    {
                        var pushDistance = MenuHelper.getSliderValue("dz191.vhr.misc.condemn.pushdistance");
                        var targetPosition = _spells[SpellSlot.E].GetPrediction(target).UnitPosition;
                        var myDelay = (int)Math.Ceiling(fromPosition.Distance(target.ServerPosition) / _spells[SpellSlot.E].Speed * 1000 +50);
                        // var targetPosition = Prediction.GetPrediction(target, myDelay).UnitPosition;
                        //var targetPosition = target.Position;
                        var finalPosition = targetPosition.Extend(fromPosition, -pushDistance);
                        var numberOfChecks = (float)Math.Ceiling(pushDistance / 30f);
                        for (var i = 1; i <= 30; i++)
                        {
                            //var extendedPosition = targetPosition.Extend(fromPosition, -(i * 30));
                            var v3 = (targetPosition - fromPosition).Normalized();
                            var extendedPosition = targetPosition + v3 * (numberOfChecks * i); 
                            //var extendedPosition2 = targetPosition.Extend(fromPosition, -(i * target.BoundingRadius + target.BoundingRadius/4));
                            //var extendedPosition3 = targetPosition.Extend(fromPosition, -(i * target.BoundingRadius - target.BoundingRadius / 4));
                            var underTurret = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.condemnturret") && (Helpers.UnderAllyTurret(finalPosition) || Helpers.IsFountain(finalPosition));
                            var j4Flag = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.condemnflag") && (Helpers.IsJ4FlagThere(extendedPosition, target));

                            if (extendedPosition.IsWall() || j4Flag || underTurret)
                            {
                                condemnNotification.Text = "Condemned " + target.ChampionName;
                                //Game.PrintChat("Condemned " + target.ChampionName +" His BBox is "+ target.BoundingRadius +" "+ extendedPosition.IsWall() +" "+ j4Flag +" "+ underTurret);
                                predictedEndPosition = extendedPosition;
                                predictedPosition = targetPosition;
                                Notifications.AddNotification(condemnNotification);
                                tg = target;
                                return true;
                            }
                        }
                    }
                    break;
                case 1:
                    //Marksman/Gosu Condemn Code
                    foreach (var target in HeroManager.Enemies.Where(h => h.IsValidTarget(_spells[SpellSlot.E].Range)))
                    {
                        var pushDistance = MenuHelper.getSliderValue("dz191.vhr.misc.condemn.pushdistance");
                        var targetPosition = _spells[SpellSlot.E].GetPrediction(target).UnitPosition;
                        var finalPosition = targetPosition.Extend(fromPosition, -pushDistance);
                        var finalPosition2 = targetPosition.Extend(fromPosition, -(pushDistance/2));
                        var underTurret = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.condemnturret") && (finalPosition.UnderTurret(false) || Helpers.IsFountain(finalPosition));
                        var j4Flag = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.condemnflag") && (Helpers.IsJ4FlagThere(finalPosition, target) || Helpers.IsJ4FlagThere(finalPosition2, target));
                        if (finalPosition.IsWall() || finalPosition2.IsWall() || underTurret || j4Flag)
                        {
                            tg = target;
                            return true;
                        }
                    }
                    break;
            }
            tg = null;
            return false;
        }
        #endregion

        #region WallTumble
        void WallTumble()
        {
            Vector2 midWallQPos = new Vector2(6707.485f, 8802.744f);
            Vector2 drakeWallQPos = new Vector2(11514, 4462);
            Vector2 baseWallQPos = new Vector2(12171.35f, 10381.68f);

            if (Player.Distance(midWallQPos) >= Player.Distance(drakeWallQPos))
            {
                if (Player.Position.X < 12000 || Player.Position.X > 12070 || Player.Position.Y < 4800 ||
                    Player.Position.Y > 4872)
                {
                    Helpers.MoveToLimited(new Vector2(12050, 4827).To3D());
                }
                else
                {
                    Helpers.MoveToLimited(new Vector2(12050, 4827).To3D());
                    _spells[SpellSlot.Q].Cast(drakeWallQPos, true);
                }
            }
            else
            {
                if (Player.Position.X < 6908 || Player.Position.X > 6978 || Player.Position.Y < 8917 ||
                    Player.Position.Y > 8989)
                {
                    Helpers.MoveToLimited(new Vector2(6958, 8944).To3D());
                }
                else
                {
                    Helpers.MoveToLimited(new Vector2(6958, 8944).To3D());
                    _spells[SpellSlot.Q].Cast(midWallQPos, true);
                }
            }
        }

        void WallTumble2()
        {
            Vector2 baseWallQPos = new Vector2(12171.35f, 10381.68f);

            if (Player.Position.X < 11905 || Player.Position.X > 11985 || Player.Position.Y < 9990 ||
                Player.Position.Y > 10050)
            {
                Helpers.MoveToLimited(new Vector2(11935, 10010).To3D());
            }
            else
            {
                Helpers.MoveToLimited(new Vector2(11935, 10010).To3D());
                _spells[SpellSlot.Q].Cast(baseWallQPos, true);
            }

        }


        #endregion


    }
}
