#region

using System;
using System.Linq;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Yasuo
{
    internal class Yasuo
    {
        #region Fields

        private static readonly Obj_AI_Base Player;
        private static readonly YasuoMenu Menu;

        private static readonly Spell QSpell;
        private static readonly Spell QWindSpell;
        private static readonly Spell WSpell;
        private static readonly Spell ESpell;
        private static readonly Spell RSpell;

        private static int _knockedUpUnits;
        private static int _lastCommand;

        private const int CommandDelay = 250;

        #endregion

        #region Loader

        private static void Main(string[] args)
        {
            if (args != null)
            {
                CustomEvents.Game.OnGameLoad += eventArgs =>
                {
                    Game.OnGameUpdate += GameOnOnGameUpdate;
                    Drawing.OnDraw += DrawingOnOnDraw;
                    GameObject.OnCreate += GameObjectOnOnCreate;
                    GameObject.OnDelete += GameObjectOnOnDelete;
                    Obj_AI_Base.OnProcessSpellCast += ObjAiBaseOnOnProcessSpellCast;
                    Game.PrintChat("WorstPing | 浜氱储 - 鐤鹃鍓戣豹, 鍔犺浇鎴愬姛! 姹夊寲by浜岀嫍!QQ缇36163084");
                };
            }
        }

        #region EventHandlers

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            switch (Menu.GetOrbwalker().ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Farming(true);
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farming(false);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    //Harass();
                    break;
            }

            if (Menu.GetOrbwalker().ActiveMode == Orbwalking.OrbwalkingMode.None &&
                Menu.GetValue<bool>(YasuoMenu.FleeMode) && Menu.GetValue<KeyBind>(YasuoMenu.FleeModeKey).Active)
            {
                Flee();
                return;
            }

            if (((Obj_AI_Hero) Player).IsRecalling())
            {
                return;
            }

            Interrupt();
            Killsteal();
            Auto();
        }

        private static Obj_AI_Base _drawCircle;

        private static void DrawingOnOnDraw(EventArgs args)
        {
            if (_drawCircle.IsValidTarget())
            {
                LeagueSharp.Common.Utility.DrawCircle(_drawCircle.Position, _drawCircle.BoundingRadius, Color.Red);
            }
        }

        private static void GameObjectOnOnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Yasuo_base_R_i"))
            {
                ++_knockedUpUnits;
            }
        }

        private static void GameObjectOnOnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Yasuo_base_R_i"))
            {
                --_knockedUpUnits;
            }
        }

        private static void ObjAiBaseOnOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            string[] baseSkinName =
            {
                "Caitlyn", "Nunu", "FiddleSticks", "Fiddlesticks", "Galio", "Karthus", "Katarina", "Malzahar",
                "MissFortune", "Pantheon", "Shen", "Urgot", "Warwick", "Varus"
            };
            string[] skillsStrings =
            {
                "CaitlynAceintheHole", "AbsoluteZero", "Crowstorm", "DrainChannel",
                "GalioIdolOfDurand", "FallenOne", "KatarinaR", "AlZaharNetherGrasp", "MissFortuneBulletTime",
                "Pantheon_GrandSkyfall_Jump", "ShenStandUnited", "UrgotSwap2", "InfiniteDuress", "VarusQ"
            };

            if (Menu.GetValue<bool>(YasuoMenu.InterruptActive))
            {
                for (var i = 0; i < baseSkinName.Length; ++i)
                {
                    if (sender.BaseSkinName == baseSkinName[i] && args.SData.Name == skillsStrings[i])
                    {
                        if (Player.Distance(sender.Position) <= ESpell.Range && Player.HasWhirlwind())
                        {
                            ESpell.Cast(sender, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                            LeagueSharp.Common.Utility.DelayAction.Add(
                                300, () => QSpell.Cast(sender, Menu.GetValue<bool>(YasuoMenu.MiscPackets)));
                        }
                    }
                }
            }
        }

        #endregion

        static Yasuo()
        {
            Player = ObjectManager.Player;

            Menu = new YasuoMenu();

            QSpell = new Spell(SpellSlot.Q, 475f);
            QWindSpell = new Spell(SpellSlot.Q, 900f);
            QSpell.SetSkillshot(0.36f, 350f, 20000f, false, SkillshotType.SkillshotLine);
            QWindSpell.SetSkillshot(0.36f, 120f, 1200f, true, SkillshotType.SkillshotLine);
            WSpell = new Spell(SpellSlot.W, 400f);
            ESpell = new Spell(SpellSlot.E, 475f);
            RSpell = new Spell(SpellSlot.R, 1200f);

            _knockedUpUnits = 0;
            _lastCommand = 0;
        }

        #endregion

        #region SpellHandler

        #region Combo

        private static void Combo()
        {
            if (_lastCommand.GetTickCount() < CommandDelay)
            {
                return;
            }

            #region Last Breath (R)

            if (Menu.GetValue<bool>(YasuoMenu.ComboR) && RSpell.IsReady())
            {
                if (Menu.GetValue<Slider>(YasuoMenu.ComboRMin).Value <= _knockedUpUnits)
                {
                    RSpell.Cast();
                }
            }

            #endregion

            var target = TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Physical);
            if (target == null)
            {
                return;
            }

            #region Steel Tempest (Q)

            if (Menu.GetValue<bool>(YasuoMenu.ComboQ) &&
                (Player.HasWhirlwind() ? QWindSpell.IsReady() : QSpell.IsReady()) &&
                target.Distance(Player.Position) <= (Player.HasWhirlwind() ? QWindSpell.Range : QSpell.Range))
            {
                var hitChance = HitChance.Medium;
                if (Menu != null && Menu.GetItem(YasuoMenu.MiscHitChance) != null)
                {
                    var menuItem = Menu.GetValue<StringList>(YasuoMenu.MiscHitChance);
                    Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out hitChance);
                }

                if (Player.HasWhirlwind())
                {
                    QWindSpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                    _lastCommand = Environment.TickCount;
                }
                else
                {
                    QSpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                    _lastCommand = Environment.TickCount;
                }
            }

            #endregion

            #region Sweeping Blade (E)

            if (Menu.GetValue<bool>(YasuoMenu.ComboE) && ESpell.IsReady())
            {
                var dashTarget =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(
                            t =>
                                t.IsValidTarget() && t.IsDashable() && t.Distance(Player.Position) <= ESpell.Range &&
                                Game.CursorPos.CountEnemysInRange(250) > 0)
                        .OrderBy(
                            t =>
                                Player.GetDashingEnd(t).IsValid() &&
                                (Game.CursorPos.Distance(Player.GetDashingEnd(t).To3D()) <
                                 (Game.CursorPos.Distance(Player.Position))) &&
                                (target.Distance(Player.GetDashingEnd(t)) < target.Distance(Player.Position)) &&
                                target.Distance(Player.Position) < Player.GetAutoAttackRange())
                        .ThenBy(t => t.Distance(Player.Position))
                        .LastOrDefault();

                if (dashTarget != null)
                {
                    _drawCircle = dashTarget;
                    if (dashTarget == target && target.Distance(Player.Position) > Player.GetAutoAttackRange())
                    {
                        ESpell.CastOnUnit(dashTarget, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                        _lastCommand = Environment.TickCount;
                    }
                    else
                    {
                        var dashEnd = Player.GetDashingEnd(dashTarget);
                        if (dashEnd.IsValid() && (target.Distance(dashEnd)) < (target.Distance(Player.Position)))
                        {
                            ESpell.CastOnUnit(dashTarget, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                            _lastCommand = Environment.TickCount;
                        }
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Farming

        private static void Farming(bool laneclear)
        {
            #region Last Hit

            if (QSpell.IsReady() &&
                ((Menu.GetValue<bool>(YasuoMenu.FarmingUseWind) && Player.HasWhirlwind()) || !Player.HasWhirlwind()))
            {
                var killableMinion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(m => m.IsValidTarget() && m.Health < Player.GetSpellDamage(m, QSpell.Instance.Name))
                        .OrderBy(m => m.Health)
                        .FirstOrDefault();
                if (killableMinion != null)
                {
                    QSpell.Cast(killableMinion, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                    return;
                }
            }
            else if (ESpell.IsReady())
            {
                var killableMinion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(m => m.IsValidTarget() && m.Health < Player.GetSpellDamage(m, ESpell.Instance.Name) && !m.UnderTurret())
                        .OrderBy(m => m.Health)
                        .FirstOrDefault();
                if (killableMinion != null)
                {
                    ESpell.Cast(killableMinion, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                    return;
                }
            }

            #endregion

            #region Lane Clear
            var minions =
                ObjectManager.Get<Obj_AI_Minion>().Count(m => m.IsValidTarget() && m.Distance(Player.Position) < 1200f);
            if (ESpell.IsReady() && QSpell.IsReady() &&
                ((Menu.GetValue<bool>(YasuoMenu.FarmingUseWind) && Player.HasWhirlwind()) || !Player.HasWhirlwind()))
            {
                var shouldEQ =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .FirstOrDefault(m => m.IsValidTarget() && m.CountEnemysInRange(450) > 3);
                if (shouldEQ != null)
                {
                    ESpell.Cast(shouldEQ, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                    LeagueSharp.Common.Utility.DelayAction.Add(
                        400, () => QSpell.Cast(Menu.GetValue<bool>(YasuoMenu.MiscPackets)));
                }
            }
            else if (QSpell.IsReady() &&
                     ((Menu.GetValue<bool>(YasuoMenu.FarmingUseWind) && Player.HasWhirlwind()) || !Player.HasWhirlwind()))
            {
                var minion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(m => m.IsValidTarget() && m.Distance(Player.Position) < QSpell.Range)
                        .OrderBy(m => m.Health)
                        .LastOrDefault();
                if (minion != null)
                {
                    QSpell.Cast(minion, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                }
            }
            #endregion
        }

        #endregion

        #region Flee

        private static void Flee()
        {
            #region Sweeping Blade (E)

            if (Menu.GetValue<bool>(YasuoMenu.FleeE) && ESpell.IsReady())
            {
                var minion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            m =>
                                m.IsValidTarget() && m.Distance(Player.Position) < ESpell.Range && m.IsDashable() &&
                                !Player.GetDashingEnd(m).To3D().UnderTurret(true))
                        .OrderBy(m => m.Distance(Game.CursorPos))
                        .FirstOrDefault();
                if (minion.IsValidTarget())
                {
                    ESpell.Cast(minion);
                    return;
                }
            }

            #endregion

            MoveTo(Game.CursorPos, Player.BoundingRadius * 2f);
        }

        #region MoveTo

        public static int LastMoveCommandT;
        private const int Delay = 150;
        private const float MinDistance = 400;
        private static readonly Random Random = new Random(DateTime.Now.Millisecond);
        public static Vector3 LastMoveCommandPosition = Vector3.Zero;

        private static void MoveTo(Vector3 position,
            float holdAreaRadius = 0,
            bool overrideTimer = false,
            bool useFixedDistance = true,
            bool randomizeMinDistance = true)
        {
            if (Environment.TickCount - LastMoveCommandT < Delay && !overrideTimer)
            {
                return;
            }

            LastMoveCommandT = Environment.TickCount;

            if (Player.ServerPosition.Distance(position) < holdAreaRadius)
            {
                if (Player.Path.Count() <= 1)
                {
                    return;
                }
                Player.IssueOrder(GameObjectOrder.HoldPosition, Player.ServerPosition);
                LastMoveCommandPosition = Player.ServerPosition;
                return;
            }

            var point = position;
            if (useFixedDistance)
            {
                point = Player.ServerPosition +
                        (randomizeMinDistance ? (Random.NextFloat(0.6f, 1) + 0.2f) * MinDistance : MinDistance) *
                        (position.To2D() - Player.ServerPosition.To2D()).Normalized().To3D();
            }
            else
            {
                if (randomizeMinDistance)
                {
                    point = Player.ServerPosition +
                            (Random.NextFloat(0.6f, 1) + 0.2f) * MinDistance *
                            (position.To2D() - Player.ServerPosition.To2D()).Normalized().To3D();
                }
                else if (Player.ServerPosition.Distance(position) > MinDistance)
                {
                    point = Player.ServerPosition +
                            MinDistance * (position.To2D() - Player.ServerPosition.To2D()).Normalized().To3D();
                }
            }

            Player.IssueOrder(GameObjectOrder.MoveTo, point);
            LastMoveCommandPosition = point;
        }

        #endregion

        #endregion

        #region Killsteal

        private static void Killsteal()
        {
            var target =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        e =>
                            e.IsValidTarget() &&
                            e.Distance(Player.Position) < (Player.HasWhirlwind() ? QWindSpell.Range : QSpell.Range))
                    .OrderBy(e => e.Health)
                    .FirstOrDefault();

            if (target == null)
            {
                return;
            }

            #region Steel Tempest (Q/3Q)

            if (QSpell.IsReady() && Player.GetSpellDamage(target, QSpell.Instance.Name) > target.Health)
            {
                if (Menu == null || Menu.GetItem(YasuoMenu.MiscHitChance) == null)
                {
                    return;
                }

                var menuItem = Menu.GetValue<StringList>(YasuoMenu.MiscHitChance);
                HitChance hitChance;
                Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out hitChance);
                if (!Player.HasWhirlwind())
                {
                    QSpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                }
                else
                {
                    QWindSpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                }
            }
                #endregion
                #region Sweeping Blade (E)

            else if (ESpell.IsReady() && Player.GetSpellDamage(target, ESpell.Instance.Name) > target.Health)
            {
                ESpell.Cast(target);
            }
                #endregion
                #region Steel Tempest (Q/3Q) + Sweepeing Blade (E)

            else if ((QSpell.IsReady() && ESpell.IsReady()) &&
                     (Player.GetSpellDamage(target, ESpell.Instance.Name) +
                      Player.GetSpellDamage(target, QSpell.Instance.Name)) > target.Health &&
                     target.Distance(Player.Position) < ESpell.Range)
            {
                ESpell.Cast(target);
                LeagueSharp.Common.Utility.DelayAction.Add(
                    500, () =>
                    {
                        if (Menu == null || Menu.GetItem(YasuoMenu.MiscHitChance) == null)
                        {
                            return;
                        }

                        var menuItem = Menu.GetValue<StringList>(YasuoMenu.MiscHitChance);
                        HitChance hitChance;
                        Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out hitChance);
                        QSpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                    });
            }

            #endregion
        }

        #endregion

        #region Auto

        private static void Auto()
        {
            var target = TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Physical);
            if (target == null || Menu == null)
            {
                return;
            }

            if (QSpell.IsReady() && Menu.GetValue<bool>(YasuoMenu.MiscAutoQ) &&
                Player.Distance(target.Position) <= (Player.HasWhirlwind() ? QWindSpell.Range : QSpell.Range))
            {
                if ((target.UnderTurret() && Menu.GetValue<bool>(YasuoMenu.MiscAutoQUnderTower)) ||
                    !target.UnderTurret())
                {
                    var hitChance = HitChance.Medium;
                    if (Menu != null && Menu.GetItem(YasuoMenu.MiscHitChance) != null)
                    {
                        var menuItem = Menu.GetValue<StringList>(YasuoMenu.MiscHitChance);
                        Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out hitChance);
                    }

                    if (Player.HasWhirlwind())
                    {
                        QWindSpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                    }
                    else
                    {
                        QSpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(YasuoMenu.MiscPackets));
                    }
                }
            }

            if (RSpell.IsReady() && Menu.GetValue<bool>(YasuoMenu.MiscAutoR) &&
                _knockedUpUnits >= Menu.GetValue<Slider>(YasuoMenu.MiscRMin).Value)
            {
                RSpell.Cast(Menu.GetValue<bool>(YasuoMenu.MiscPackets));
            }
        }

        #endregion

        #region Interrupt

        private static void Interrupt()
        {
            
        }

        #endregion

        #endregion
    }

    #region Utility

    public static class Utility
    {
        public static bool HasWhirlwind(this Obj_AI_Base objAiBase)
        {
            return objAiBase.HasBuff("YasuoQ3W");
        }

        public static bool CanDashOn(this Obj_AI_Base objAiBase)
        {
            return !objAiBase.HasBuff("YasuoDashWrapper");
        }

        public static bool IsDashable(this Obj_AI_Base objAiBase)
        {
            return objAiBase.CanDashOn() &&
                   (objAiBase.Type == GameObjectType.obj_AI_Hero || objAiBase.Type == GameObjectType.obj_AI_Minion);
        }

        public static Vector2 GetDashingEnd(this Obj_AI_Base objAiBase, Obj_AI_Base unitObjAiBase)
        {
            var vector3 = new Vector2(
                unitObjAiBase.Position.X - objAiBase.Position.X, unitObjAiBase.Position.Y - objAiBase.Position.Y);
            var abs = Math.Sqrt(vector3.X * vector3.X + vector3.Y * vector3.Y);
            return new Vector2(
                (float) (objAiBase.Position.X + (475f * (vector3.X / abs))),
                (float) (objAiBase.Position.Y + (475f * (vector3.Y / abs))));
        }

        public static int GetTickCount(this int int32)
        {
            return (Environment.TickCount - int32);
        }

        public static float GetAutoAttackRange(this Obj_AI_Base objAiBase)
        {
            return (objAiBase.AttackRange + objAiBase.BoundingRadius);
        }
    }

    #endregion
}