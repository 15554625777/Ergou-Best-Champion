using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Rumble
{
    internal class Rumble
    {
        #region Spells

        private static void RumbleCombo()
        {
            var target = TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Magical);
            if (!target.IsValidTarget())
            {
                return;
            }

            // ==== [ CHECKS ] ====

            CalculateHeatFunctions(target);

            var hitChance = HitChance.Medium;
            if (Menu.GetMenu() != null && Menu.GetItem(RumbleMenu.MiscHitChance) != null)
            {
                var menuItem = Menu.GetValue<StringList>(RumbleMenu.MiscHitChance);
                Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out hitChance);
            }

            // ==== [  COMBO  ] ====

            /* Q CASTING */
            if (Menu.GetValue<bool>(RumbleMenu.ComboQ) && QSpell.IsReady() && _shouldCastQAndE &&
                target.Distance(PlayerObjAiHero.Position) < 600f && PlayerObjAiHero.IsFacing(target, 600f) &&
                Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250)
            {
                QSpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
            }

            /* E CASTING */
            if (Menu.GetValue<bool>(RumbleMenu.ComboE) && !PlayerObjAiHero.HasBuff("RumbleGrenade") && ESpell.IsReady() &&
                _shouldCastQAndE && target.Distance(PlayerObjAiHero.Position) < 850f &&
                Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250)
            {
                ESpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                _lastETick = Environment.TickCount;
            }

            if (Menu.GetValue<bool>(RumbleMenu.ComboE) && PlayerObjAiHero.HasBuff("RumbleGrenade") && ESpell.IsReady() &&
                target.Distance(PlayerObjAiHero.Position) < 850f)
            {
                var c0 = Menu.GetValue<bool>(RumbleMenu.MiscEmDelay) &&
                         Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250 &&
                         target.Distance(PlayerObjAiHero.Position) >= 300f;

                var c1 = Menu.GetValue<bool>(RumbleMenu.MiscEmDelay) &&
                         Environment.TickCount - _lastETick > 1000 * Menu.GetValue<Slider>(RumbleMenu.MiscEDelay).Value &&
                         target.Distance(PlayerObjAiHero.Position) < 350f;

                var c2 = !Menu.GetValue<bool>(RumbleMenu.MiscEmDelay) &&
                         Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250;

                if (!c0 && !c1 && !c2)
                {
                    return;
                }
                ESpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                _lastETick = Environment.TickCount;
            }

            /* W CASTNG */
            if (Menu.GetValue<bool>(RumbleMenu.ComboW) && WSpell.IsReady() && _shouldCastW &&
                target.Distance(PlayerObjAiHero.Position) < 900f &&
                Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250 &&
                PlayerObjAiHero.Distance(Prediction.GetPrediction(target, 0.250f).UnitPosition) < 900f)
            {
                WSpell.Cast(Menu.GetValue<bool>(RumbleMenu.MiscPackets));
            }

            /* R CASTING */
            if (Menu.GetValue<bool>(RumbleMenu.ComboR))
            {
                var rc0 = (RSpell.IsReady() && ESpell.IsReady() && QSpell.IsReady()) &&
                          PlayerObjAiHero.GetComboDamage(
                              target, new[] { SpellSlot.Q, SpellSlot.E, SpellSlot.E, SpellSlot.R }) > target.Health;

                var rc1 = (RSpell.IsReady() && QSpell.IsReady()) &&
                          PlayerObjAiHero.GetComboDamage(target, new[] { SpellSlot.R, SpellSlot.Q }) > target.Health;

                var rc2 = (RSpell.IsReady() && ESpell.IsReady()) &&
                          PlayerObjAiHero.GetComboDamage(target, new[] { SpellSlot.R, SpellSlot.E, SpellSlot.E }) >
                          target.Health;

                if (!rc0 && !rc1 && !rc2)
                {
                    return;
                }

                CastR(target);
            }

            /* OVERHEAT */
            if (Menu.GetValue<bool>(RumbleMenu.ComboOverheat) && _shouldOverheat &&
                Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250)
            {
                if (Menu.GetValue<bool>(RumbleMenu.ComboQ) && QSpell.IsReady() &&
                    target.Distance(PlayerObjAiHero.Position) < 600f && PlayerObjAiHero.IsFacing(target, 600f))
                {
                    QSpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                }

                if (Menu.GetValue<bool>(RumbleMenu.ComboE) && ESpell.IsReady() &&
                    target.Distance(PlayerObjAiHero.Position) < 850f)
                {
                    ESpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                    _lastETick = Environment.TickCount;
                }

                WSpell.Cast(Menu.GetValue<bool>(RumbleMenu.MiscPackets));
            }
        }

        private static void RumbleHarass()
        {
            var target = TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Magical);
            if (!target.IsValidTarget())
            {
                return;
            }

            // ==== [ CHECKS ] ====

            CalculateHeatFunctions(target);

            var hitChance = HitChance.Medium;
            if (Menu.GetMenu() != null && Menu.GetItem(RumbleMenu.MiscHitChance) != null)
            {
                var menuItem = Menu.GetValue<StringList>(RumbleMenu.MiscHitChance);
                Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out hitChance);
            }

            // ==== [  HARASS  ] ====
            /* Q CASTING */
            if (Menu.GetValue<bool>(RumbleMenu.HarassQ) && QSpell.IsReady() && _shouldCastQAndE &&
                target.Distance(PlayerObjAiHero.Position) < 600f && PlayerObjAiHero.IsFacing(target, 600f) &&
                Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250)
            {
                QSpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
            }

            /* E CASTING */
            if (Menu.GetValue<bool>(RumbleMenu.HarassE) && ESpell.IsReady() && _shouldCastQAndE &&
                target.Distance(PlayerObjAiHero.Position) < 850f &&
                Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250)
            {
                ESpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                _lastETick = Environment.TickCount;
            }

            /* W CASTING */
            if (Menu.GetValue<bool>(RumbleMenu.HarassW) && WSpell.IsReady() && _shouldCastW &&
                target.Distance(PlayerObjAiHero.Position) < 900f &&
                Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250 &&
                PlayerObjAiHero.Distance(Prediction.GetPrediction(target, 0.250f).UnitPosition) < 900f)
            {
                WSpell.Cast(Menu.GetValue<bool>(RumbleMenu.MiscPackets));
            }

            /* OVERHEAT */
            if (Menu.GetValue<bool>(RumbleMenu.HarassOverheat) && _shouldOverheat &&
                Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250)
            {
                if (Menu.GetValue<bool>(RumbleMenu.HarassQ) && QSpell.IsReady() &&
                    target.Distance(PlayerObjAiHero.Position) < 600f && PlayerObjAiHero.IsFacing(target, 600f))
                {
                    QSpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                }

                if (Menu.GetValue<bool>(RumbleMenu.HarassE) && ESpell.IsReady() &&
                    target.Distance(PlayerObjAiHero.Position) < 850f)
                {
                    ESpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                    _lastETick = Environment.TickCount;
                }

                WSpell.Cast(Menu.GetValue<bool>(RumbleMenu.MiscPackets));
            }
        }

        private static void RumbleFlee()
        {
            var target =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(e => e.IsValidTarget() && e.Distance(PlayerObjAiHero.Direction) < 850f)
                    .OrderBy(e => e.Distance(PlayerObjAiHero.Position))
                    .FirstOrDefault();

            if (ESpell.IsReady())
            {
                var hitChance = HitChance.Medium;
                if (Menu.GetMenu() != null && Menu.GetItem(RumbleMenu.MiscHitChance) != null)
                {
                    var menuItem = Menu.GetValue<StringList>(RumbleMenu.MiscHitChance);
                    Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out hitChance);
                }
                ESpell.CastIfHitchanceEquals(target, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
            }

            if (WSpell.IsReady())
            {
                WSpell.Cast(Menu.GetValue<bool>(RumbleMenu.MiscPackets));
            }
        }

        private static void RumbleHeatManager()
        {
            if (PlayerObjAiHero.IsRecalling())
            {
                return;
            }

            if (Menu.GetValue<bool>(RumbleMenu.HmStayInDanger) && PlayerObjAiHero.CountEnemysInRange(1000) < 2)
            {
                if (WSpell.IsReady() && Menu.GetValue<bool>(RumbleMenu.HmW) && PlayerObjAiHero.Mana < 35 &&
                    Environment.TickCount - _lastSavedTick > 750)
                {
                    _lastSavedTick = Environment.TickCount;
                    WSpell.Cast(Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                }
                else if (QSpell.IsReady() && !WSpell.IsReady() && Menu.GetValue<bool>(RumbleMenu.HmQ) &&
                         PlayerObjAiHero.Mana < 35 && Environment.TickCount - _lastSavedTick > 750 && !CheckQCreeps())
                {
                    QSpell.Cast(Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                }
            }
            else if (Menu.GetValue<bool>(RumbleMenu.HmStayInDanger))
            {
                if (!WSpell.IsReady() || !Menu.GetValue<bool>(RumbleMenu.HmW) || !(PlayerObjAiHero.Mana < 35) ||
                    !(Environment.TickCount - _lastSavedTick > 750))
                {
                    return;
                }
                _lastSavedTick = Environment.TickCount;
                WSpell.Cast(Menu.GetValue<bool>(RumbleMenu.HmW));
            }
        }

        private static void KillSteal()
        {
            var hitChance = HitChance.Medium;
            if (Menu.GetMenu() != null && Menu.GetItem(RumbleMenu.MiscHitChance) != null)
            {
                var menuItem = Menu.GetValue<StringList>(RumbleMenu.MiscHitChance);
                Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out hitChance);
            }

            foreach (var enemy in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        e =>
                            e.IsValidTarget() && e.Distance(PlayerObjAiHero.Position) < RSpell.Range &&
                            !IsInvulnerable(e, TargetSelector.DamageType.Magical)))
            {
                if (enemy.Distance(PlayerObjAiHero.Position) < QSpell.Range && QSpell.IsReady() &&
                    Menu.GetValue<bool>(RumbleMenu.KsQ) && Menu.GetValue<bool>(RumbleMenu.KsOverheat) &&
                    _shouldCastQAndE && PlayerObjAiHero.IsFacing(enemy, 600f) &&
                    Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250)
                {
                    if (PlayerObjAiHero.GetSpellDamage(enemy, SpellSlot.Q) / 2 > enemy.Health)
                    {
                        QSpell.CastIfHitchanceEquals(enemy, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                    }
                }

                if (enemy.Distance(PlayerObjAiHero.Position) < ESpell.Range && ESpell.IsReady() &&
                    Menu.GetValue<bool>(RumbleMenu.KsE) && Menu.GetValue<bool>(RumbleMenu.KsOverheat) &&
                    _shouldCastQAndE && Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250)
                {
                    if (PlayerObjAiHero.GetSpellDamage(enemy, SpellSlot.E) > enemy.Health)
                    {
                        ESpell.CastIfHitchanceEquals(enemy, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                    }
                }

                if (enemy.Distance(PlayerObjAiHero.Position) < RSpell.Range && RSpell.IsReady() &&
                    Menu.GetValue<bool>(RumbleMenu.KsR) &&
                    Environment.TickCount - PlayerObjAiHero.LastCastedSpellT() > 250)
                {
                    if (PlayerObjAiHero.GetSpellDamage(enemy, SpellSlot.R) * 1.5 > enemy.Health)
                    {
                        CastR(enemy);
                    }
                }
            }
        }

        private static void CastR(Obj_AI_Base targetObjAiBase)
        {
            var objAiBaseHit = 0;
            var castFromVector2 = new Vector2();
            var castToVector2 = new Vector2();

            if (PlayerObjAiHero.CountEnemysInRange(1200) > 1)
            {
                var rumbleData = MultiR(targetObjAiBase);
                if (rumbleData != null)
                {
                    objAiBaseHit = rumbleData.GetTargetsHit();
                    castFromVector2 = rumbleData.GetFromVector2();
                    castToVector2 = rumbleData.GetToVector2();
                }
            }
            else
            {
                var rumbleData = SingleR(targetObjAiBase);
                if (rumbleData != null)
                {
                    castFromVector2 = rumbleData.GetFromVector2();
                    castToVector2 = rumbleData.GetToVector2();
                }
            }

            if (castFromVector2.IsValid() && castToVector2.IsValid() &&
                (objAiBaseHit > 0 || PlayerObjAiHero.CountEnemysInRange(1200) == 1))
            {
                RSpell.Cast(castFromVector2, castToVector2);
            }
        }

        private static RumbleData MultiR(Obj_AI_Base objAiBase)
        {
            var check0 = MutliRCheckOne(objAiBase);
            if (check0 != null)
            {
                return check0;
            }

            var check1 = MutliRCheckTwo(objAiBase);
            if (check1 != null)
            {
                return check1;
            }

            var check2 = MutliRCheckThree(objAiBase);
            return check2;
        }

        private static RumbleData MutliRCheckOne(Obj_AI_Base objAiBase)
        {
            var castFromVector2 = new Vector2();
            var castToVector2 = new Vector2();
            var targetsHit = 0;
            var pTargetPos = Prediction.GetPrediction(objAiBase, 0.25f, 0f, RSpell.Speed).UnitPosition.To2D();

            foreach (var pEnemyPos in
                from enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(e => e.IsValidTarget() && e.NetworkId != objAiBase.NetworkId)
                let pEnemyPos = Prediction.GetPrediction(enemy, 0.25f).UnitPosition.To2D()
                where
                    Vector2.Distance(enemy.Position.To2D(), objAiBase.Position.To2D()) < 1000f &&
                    Vector2.Distance(pEnemyPos, pTargetPos) < RSpell.Instance.SData.LineWidth
                select pEnemyPos)
            {
                var currentMid = GetMidVector2(pEnemyPos, pTargetPos);
                var midVector = (pEnemyPos - pTargetPos).Normalized();
                var pos = currentMid + midVector * (RSpell.Instance.SData.LineWidth / 2);
                var pos2 = currentMid + midVector * (RSpell.Instance.SData.LineWidth / 2);
                var tempHitCount = CheckHitCount(objAiBase, pos, pos2);

                if (tempHitCount <= targetsHit)
                {
                    continue;
                }

                targetsHit = tempHitCount;
                castFromVector2 = pos;
                castToVector2 = pos2;
            }

            if (targetsHit < 2 || !castFromVector2.IsValid() || !castToVector2.IsValid())
            {
                return null;
            }
            return !CheckWall(castFromVector2.To3D(), castToVector2.To3D())
                ? new RumbleData(castFromVector2, castToVector2, targetsHit)
                : null;
        }

        private static RumbleData MutliRCheckTwo(Obj_AI_Base objAiBase)
        {
            var waypoints = PlayerObjAiHero.GetWaypoints();
            var isLinear = true;

            if (waypoints.Count > 3)
            {
                for (var i = 2; i < waypoints.Count; ++i)
                {
                    var projectionInfo = waypoints[1].ProjectOn(waypoints[waypoints.Count - 1], waypoints[i]);
                    if (!projectionInfo.IsOnSegment || !projectionInfo.SegmentPoint.IsValid() ||
                        !projectionInfo.LinePoint.IsValid())
                    {
                        continue;
                    }

                    if (Vector2.Distance(projectionInfo.SegmentPoint, projectionInfo.LinePoint) > RSpell.Width + 100)
                    {
                        isLinear = false;
                    }
                }
            }

            if (waypoints.Count < 2)
            {
                return null;
            }

            var travelTime = 0f;

            if (isLinear)
            {
                travelTime +=
                    waypoints.Select((t, i) => Vector2.Distance(t, waypoints[i + 1]) / objAiBase.MoveSpeed).Sum();
            }

            var mid = new Vector2(
                (waypoints[1].X + waypoints[waypoints.Count - 1].X) / 2,
                (waypoints[1].Y + waypoints[waypoints.Count - 1].Y) / 2);
            var finalInitialVector = (waypoints[waypoints.Count - 1] - waypoints[1]).Normalized();

            if (Vector2.Distance(waypoints[1], mid) > 425f)
            {
                var pos = waypoints[1] - finalInitialVector * 200;
                var pos2 = mid;
                if (!CheckWall(pos.To3D(), pos2.To3D()))
                {
                    return new RumbleData(pos, pos2, CheckHitCount(objAiBase, pos, pos2));
                }
            }
            else if (Vector2.Distance(waypoints[1], waypoints[waypoints.Count - 1]) < RSpell.Instance.SData.LineWidth &&
                     travelTime >= 0.7f)
            {
                var pos = mid - finalInitialVector * 250;
                var pos2 = mid + finalInitialVector * 450;
                if (!CheckWall(pos.To3D(), pos2.To3D()))
                {
                    return new RumbleData(pos, pos2, CheckHitCount(objAiBase, pos, pos2));
                }
            }
            else if (Vector2.Distance(waypoints[1], waypoints[waypoints.Count - 1]) < RSpell.Instance.SData.LineWidth &&
                     travelTime >= 0.4f)
            {
                var pos = mid - finalInitialVector * 300;
                var pos2 = mid + finalInitialVector * 400;
                if (!CheckWall(pos.To3D(), pos2.To3D()))
                {
                    return new RumbleData(pos, pos2, CheckHitCount(objAiBase, pos, pos2));
                }
            }
            else if (Vector2.Distance(waypoints[1], waypoints[waypoints.Count - 1]) <
                     RSpell.Instance.SData.LineWidth && travelTime >= 0f)
            {
                var pos = mid - finalInitialVector * 350;
                var pos2 = mid + finalInitialVector * 350;
                if (!CheckWall(pos.To3D(), pos2.To3D()))
                {
                    return new RumbleData(pos, pos2, CheckHitCount(objAiBase, pos, pos2));
                }
            }

            return null;
        }

        private static RumbleData MutliRCheckThree(Obj_AI_Base objAiBase)
        {
            var closestEnemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(e => e.IsValidTarget() && e.NetworkId != objAiBase.NetworkId)
                    .OrderBy(e => e.Distance(PlayerObjAiHero.Position))
                    .FirstOrDefault();
            var enemyDistance = closestEnemy.Distance(PlayerObjAiHero.Position);

            if (closestEnemy != null && enemyDistance < 1000f)
            {
                var finalInitialVector = (objAiBase.Position.To2D() - closestEnemy.Position.To2D()).Normalized();
                var midPoint = GetMidVector2(objAiBase.Position.To2D(), closestEnemy.Position.To2D());

                if (!(PlayerObjAiHero.Distance(objAiBase) < RSpell.Range))
                {
                    return null;
                }

                var pos = midPoint + finalInitialVector * 360;
                var pos2 = midPoint - finalInitialVector * 360;
                if (!CheckWall(pos.To3D(), pos2.To3D()))
                {
                    return new RumbleData(pos, pos2, CheckHitCount(objAiBase, pos, pos2));
                }
            }
            else
            {
                var finalInitialVector = (objAiBase.Position.To2D() - PlayerObjAiHero.Position.To2D()).Normalized();
                if (!(PlayerObjAiHero.Distance(objAiBase) < RSpell.Range))
                {
                    return null;
                }

                var pos = objAiBase.Position.To2D() + finalInitialVector * 360;
                var pos2 = objAiBase.Position.To2D() - finalInitialVector * 360;
                if (!CheckWall(pos.To3D(), pos2.To3D()))
                {
                    return new RumbleData(pos, pos2, CheckHitCount(objAiBase, pos, pos2));
                }
            }

            return null;
        }

        private static RumbleData SingleR(Obj_AI_Base objAiBase)
        {
            var check0 = SingleRCheckOne(objAiBase);
            if (check0 != null)
            {
                return check0;
            }

            var check1 = SingleRCheckTwo(objAiBase);
            return check1;
        }

        private static RumbleData SingleRCheckOne(Obj_AI_Base objAiBase)
        {
            var waypoints = PlayerObjAiHero.GetWaypoints();
            var isLinear = true;

            if (waypoints.Count > 3)
            {
                for (var i = 2; i < waypoints.Count; ++i)
                {
                    var projectionInfo = waypoints[1].ProjectOn(waypoints[waypoints.Count - 1], waypoints[i]);
                    if (!projectionInfo.IsOnSegment || !projectionInfo.SegmentPoint.IsValid() ||
                        !projectionInfo.LinePoint.IsValid())
                    {
                        continue;
                    }

                    if (Vector2.Distance(projectionInfo.SegmentPoint, projectionInfo.LinePoint) > RSpell.Width + 100)
                    {
                        isLinear = false;
                    }
                }
            }

            if (waypoints.Count < 2)
            {
                return null;
            }

            var travelTime = 0f;

            if (isLinear)
            {
                travelTime +=
                    waypoints.Select((t, i) => Vector2.Distance(t, waypoints[i + 1]) / objAiBase.MoveSpeed).Sum();
            }

            var mid = new Vector2(
                (waypoints[1].X + waypoints[waypoints.Count - 1].X) / 2,
                (waypoints[1].Y + waypoints[waypoints.Count - 1].Y) / 2);
            var finalInitialVector = (waypoints[waypoints.Count - 1] - waypoints[1]).Normalized();

            if (Vector2.Distance(waypoints[1], mid) > 425f)
            {
                var pos = waypoints[1] - finalInitialVector * 200;
                var pos2 = mid;
                if (!CheckWall(pos.To3D(), pos2.To3D()))
                {
                    return new RumbleData(pos, pos2, CheckHitCount(objAiBase, pos, pos2));
                }
            }
            else if (Vector2.Distance(waypoints[1], waypoints[waypoints.Count - 1]) < RSpell.Instance.SData.LineWidth &&
                     travelTime >= 0.7f)
            {
                var pos = mid - finalInitialVector * 420;
                var pos2 = mid + finalInitialVector * 450;
                if (!CheckWall(pos.To3D(), pos2.To3D()))
                {
                    return new RumbleData(pos, pos2, CheckHitCount(objAiBase, pos, pos2));
                }
            }
            else if (Vector2.Distance(waypoints[1], waypoints[waypoints.Count - 1]) < RSpell.Instance.SData.LineWidth &&
                     travelTime >= 0.4f)
            {
                var pos = mid - finalInitialVector * 440;
                var pos2 = mid + finalInitialVector * 400;
                if (!CheckWall(pos.To3D(), pos2.To3D()))
                {
                    return new RumbleData(pos, pos2, CheckHitCount(objAiBase, pos, pos2));
                }
            }
            else if (Vector2.Distance(waypoints[1], waypoints[waypoints.Count - 1]) <
                     RSpell.Instance.SData.LineWidth && travelTime >= 0f)
            {
                var pos = mid - finalInitialVector * 460;
                var pos2 = mid + finalInitialVector * 350;
                if (!CheckWall(pos.To3D(), pos2.To3D()))
                {
                    return new RumbleData(pos, pos2, CheckHitCount(objAiBase, pos, pos2));
                }
            }

            return null;
        }

        private static RumbleData SingleRCheckTwo(Obj_AI_Base objAiBase)
        {
            var finalInitialVector = (objAiBase.Position.To2D() - PlayerObjAiHero.Position.To2D()).Normalized();
            if (!(PlayerObjAiHero.Distance(objAiBase) < RSpell.Range))
            {
                return null;
            }

            var pos = objAiBase.Position.To2D() + finalInitialVector * 360;
            var pos2 = objAiBase.Position.To2D() - finalInitialVector * 360;
            return !CheckWall(pos.To3D(), pos2.To3D())
                ? new RumbleData(pos, pos2, CheckHitCount(objAiBase, pos, pos2))
                : null;
        }

        private static bool CheckWall(Vector3 vector3, Vector3 otherVector3)
        {
            var endInitialVector = otherVector3 - vector3;
            endInitialVector.Normalize();

            var wallCount = 0;
            for (var i = 0; i < 20; ++i)
            {
                var currentMulti = 60 * i;
                var currentVector = vector3 + endInitialVector * currentMulti;
                if (currentVector.IsWall())
                {
                    ++wallCount;
                }
            }

            return (wallCount >= 8);
        }

        private static int CheckHitCount(Obj_AI_Base objAiBase, Vector2 vector2, Vector2 otherVector2)
        {
            var n = 0;

            if (!objAiBase.IsValidTarget() || !vector2.IsValid() || !otherVector2.IsValid())
            {
                return n;
            }

            var positionMid = new Vector2((vector2.X + otherVector2.X) / 2, (vector2.Y + otherVector2.Y) / 2);
            var endInitialVector = (otherVector2 - vector2).Normalized();
            var extension = RSpell.Instance.SData.LineWidth / 2; // Wall Length?

            var extPos = positionMid + endInitialVector * extension;
            var extPos2 = positionMid - endInitialVector * extension;

            n +=
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(e => e.NetworkId != objAiBase.NetworkId && e.Distance(PlayerObjAiHero.Position) < 1000f)
                    .Select(enemy => Prediction.GetPrediction(enemy, 0.25f).UnitPosition.To2D())
                    .Where(predictedPos => predictedPos.IsValid())
                    .Select(predictedPos => extPos.ProjectOn(extPos2, predictedPos))
                    .Where(
                        projectionInfo =>
                            projectionInfo.IsOnSegment && projectionInfo.SegmentPoint.IsValid() &&
                            projectionInfo.LinePoint.IsValid())
                    .Count(
                        projectionInfo =>
                            Vector2.Distance(projectionInfo.SegmentPoint, projectionInfo.LinePoint) < RSpell.Width + 60);

            return n;
        }

        private static Vector2 GetMidVector2(Vector2 vector2, Vector2 otherVector2)
        {
            return new Vector2((vector2.X + vector2.X) / 2, (vector2.Y + otherVector2.Y) / 2);
        }

        private static bool CheckQCreeps()
        {
            if (!QSpell.IsReady())
            {
                return false;
            }

            return
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(m => m.Distance(PlayerObjAiHero.Position) < 600f)
                    .Any(minion => PlayerObjAiHero.IsFacing(minion, 600f));
        }

        #endregion

        #region Main

        static Rumble()
        {
            Menu = new RumbleMenu();

            PlayerObjAiHero = ObjectManager.Player;

            RSpell = new Spell(SpellSlot.R, 1700f);
            ESpell = new Spell(SpellSlot.E, 850f);
            WSpell = new Spell(SpellSlot.W);
            QSpell = new Spell(SpellSlot.Q, 600f);
        }

        private static void Main()
        {
            if (!PlayerObjAiHero.ChampionName.Equals("Rumble"))
            {
                return;
            }

            ESpell.SetSkillshot(0.25f, 70, 2000, true, SkillshotType.SkillshotLine);
            RSpell.SetSkillshot(1700, 120, 1400, false, SkillshotType.SkillshotLine);

            CustomEvents.Game.OnGameLoad += args =>
            {
                Game.OnGameUpdate += GameOnOnGameUpdate;
                Drawing.OnDraw += DrawingOnOnDraw;
                Game.PrintChat("WorstPing | 鍏板崥 - 鏈烘鍏晫, 鍔犺浇鎴愬姛! 姹夊寲by浜岀嫍!QQ缇36163084.");
            };
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            if (Menu.GetValue<bool>(RumbleMenu.DrawQ))
            {
                Drawing.DrawCircle(PlayerObjAiHero.Position, QSpell.Range, Color.Red);
            }
            if (Menu.GetValue<bool>(RumbleMenu.DrawE))
            {
                Drawing.DrawCircle(PlayerObjAiHero.Position, ESpell.Range, Color.Green);
            }
            if (Menu.GetValue<bool>(RumbleMenu.DrawR))
            {
                Drawing.DrawCircle(PlayerObjAiHero.Position, ESpell.Range, Color.Yellow);
            }
            if (Menu.GetValue<bool>(RumbleMenu.DrawKillText))
            {
                var target = TargetSelector.GetTarget(1200f, TargetSelector.DamageType.Magical, true);
                if (!target.IsValidTarget())
                {
                    return;
                }

                if (PlayerObjAiHero.GetSpellDamage(target, SpellSlot.Q) / 3 > target.Health)
                {
                    Drawing.DrawText(target.Position.X, target.Position.Y, Color.White, "Short Q");
                }
                else if (PlayerObjAiHero.GetSpellDamage(target, SpellSlot.Q) > target.Health)
                {
                    Drawing.DrawText(target.Position.X, target.Position.Y, Color.White, "Q");
                }
                else if (PlayerObjAiHero.GetSpellDamage(target, SpellSlot.R) > target.Health)
                {
                    Drawing.DrawText(target.Position.X, target.Position.Y, Color.White, "R");
                }
                else if (PlayerObjAiHero.GetSpellDamage(target, SpellSlot.E) > target.Health)
                {
                    Drawing.DrawText(target.Position.X, target.Position.Y, Color.White, "E");
                }
                else if (PlayerObjAiHero.GetComboDamage(target, new[] { SpellSlot.Q, SpellSlot.E, SpellSlot.E }) >
                         target.Health)
                {
                    Drawing.DrawText(target.Position.X, target.Position.Y, Color.White, "Q + Twice E");
                }
                else if (
                    PlayerObjAiHero.GetComboDamage(
                        target, new[] { SpellSlot.Q, SpellSlot.E, SpellSlot.E, SpellSlot.R }) > target.Health)
                {
                    Drawing.DrawText(target.Position.X, target.Position.Y, Color.White, "Full Combo");
                }
                else
                {
                    Drawing.DrawText(target.Position.X, target.Position.Y, Color.White, "Not Killable.");
                }
            }
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            switch (Menu.GetOrbwalker().ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    RumbleCombo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    RumbleLaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    RumbleHarass();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    RumbleLastHit();
                    break;
            }
            KillSteal();
            RumbleHeatManager();
        }

        #endregion

        #region Farming

        private static void RumbleLaneClear()
        {
            if (Menu.GetValue<bool>(RumbleMenu.FarmQ) && QSpell.IsReady())
            {
                var hitQ =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Count(
                            m =>
                                m.IsValidTarget() && m.Distance(PlayerObjAiHero.Position) < QSpell.Range &&
                                PlayerObjAiHero.IsFacing(m, 600f));
                if (hitQ >= Menu.GetValue<Slider>(RumbleMenu.FarmMinQ).Value &&
                    (PlayerObjAiHero.Mana < 80 && !Menu.GetValue<bool>(RumbleMenu.FarmOverheat)))
                {
                    QSpell.Cast(Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                }
                else
                {
                    QSpell.Cast(Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                }
            }

            if (Menu.GetValue<bool>(RumbleMenu.FarmE) && ESpell.IsReady())
            {
                var minion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            m =>
                                m.IsValidTarget() && m.Distance(PlayerObjAiHero.Position) < ESpell.Range &&
                                !ESpell.GetPrediction(m).CollisionObjects.Any())
                        .OrderBy(m => m.Health)
                        .FirstOrDefault();

                var hitChance = HitChance.Medium;
                if (Menu.GetMenu() != null && Menu.GetItem(RumbleMenu.MiscHitChance) != null)
                {
                    var menuItem = Menu.GetValue<StringList>(RumbleMenu.MiscHitChance);
                    Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out hitChance);
                }

                if (PlayerObjAiHero.Mana < 80 && !Menu.GetValue<bool>(RumbleMenu.FarmOverheat))
                {
                    ESpell.CastIfHitchanceEquals(minion, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                }
                else
                {
                    ESpell.CastIfHitchanceEquals(minion, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
                }
            }
        }

        private static void RumbleLastHit()
        {
            if (!ESpell.IsReady())
            {
                return;
            }

            var hitChance = HitChance.Medium;
            if (Menu.GetMenu() != null && Menu.GetItem(RumbleMenu.MiscHitChance) != null)
            {
                var menuItem = Menu.GetValue<StringList>(RumbleMenu.MiscHitChance);
                Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out hitChance);
            }

            var minion =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        m =>
                            m.IsValidTarget() && m.Distance(PlayerObjAiHero.Position) < ESpell.Range &&
                            !ESpell.GetPrediction(m).CollisionObjects.Any() &&
                            PlayerObjAiHero.GetSpellDamage(m, SpellSlot.E) > m.Health)
                    .OrderBy(m => m.Health)
                    .FirstOrDefault();

            ESpell.CastIfHitchanceEquals(minion, hitChance, Menu.GetValue<bool>(RumbleMenu.MiscPackets));
        }

        #endregion

        #region Vars

        private static readonly Spell QSpell;
        private static readonly Spell WSpell;
        private static readonly Spell ESpell;
        private static readonly Spell RSpell;

        private static readonly Obj_AI_Hero PlayerObjAiHero;
        private static readonly RumbleMenu Menu;

        private static double _lastSavedTick;
        private static double _lastETick;

        private static bool _shouldOverheat;
        private static bool _shouldCastQAndE;
        private static bool _shouldCastW;

        #endregion

        #region Functions

        private static bool ShouldOverheat(Obj_AI_Base targetObjAiBase)
        {
            return ((PlayerObjAiHero.Mana > 80 && !QSpell.IsReady() && !ESpell.IsReady() &&
                     targetObjAiBase.Distance(PlayerObjAiHero.Position) < 350f) &&
                    ((PlayerObjAiHero.GetAutoAttackDamage(targetObjAiBase, true)) * 3D > targetObjAiBase.Health));
        }

        private static void CalculateHeatFunctions(Obj_AI_Base targetObjAiBase)
        {
            if (!targetObjAiBase.IsValidTarget())
            {
                return;
            }

            if (PlayerObjAiHero.Mana > 80 && ShouldOverheat(targetObjAiBase))
            {
                _shouldOverheat = true;
            }
            else if (PlayerObjAiHero.Mana > 60)
            {
                _shouldCastQAndE = true;
            }
            else if (PlayerObjAiHero.Mana > 0)
            {
                _shouldCastQAndE = true;
                _shouldCastW = true;
            }
        }

        private static bool IsInvulnerable(Obj_AI_Base @base, TargetSelector.DamageType damageType)
        {
            return TargetSelector.IsInvulnerable(@base, damageType);
        }

        #endregion
    }

    internal class RumbleData
    {
        private readonly Vector2 fromVector2;
        private readonly int targetsHit;
        private readonly Vector2 toVector2;

        public RumbleData(Vector2 fromVector2, Vector2 toVector2, int targetsHit)
        {
            this.fromVector2 = fromVector2;
            this.toVector2 = toVector2;
            this.targetsHit = targetsHit;
        }

        public Vector2 GetFromVector2()
        {
            return fromVector2;
        }

        public Vector2 GetToVector2()
        {
            return toVector2;
        }

        public int GetTargetsHit()
        {
            return targetsHit;
        }
    }
}