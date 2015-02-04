#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

#endregion

namespace Irelia_Reloaded
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        public static void GameOnOnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != "Irelia")
            {
                return;
            }

            // Setup Spells
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, Orbwalking.GetRealAutoAttackRange(Player));
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 1000);

            // Setup Ignite
            IgniteSlot = Player.GetSpellSlot("summonerdot");

            // Add skillshots
            Q.SetTargetted(0f, 2200);
            R.SetSkillshot(0.5f, 120, 1600, false, SkillshotType.SkillshotLine);

            // Create Items
            Botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            Cutlass = ItemData.Bilgewater_Cutlass.GetItem();
            Omen = ItemData.Randuins_Omen.GetItem();

            // Create Menu
            SetupMenu();

            // sexy af color, ty kawaii Kurisu :3
            Game.PrintChat("<font color=\"#7CFC00\"><b>Irelia Reloaded-Loaded</b></font>涓ㄦ眽鍖朆y鑺辫竟! ");

            // Setup Dmg Indicator
            Utility.HpBarDamageIndicator.DamageToUnit = DamageToUnit;
            Utility.HpBarDamageIndicator.Enabled = true;

            // Subscribe to needed events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += InterrupterOnOnPossibleToInterrupt;

            // to get Q tickcount in least amount of lines.
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.SData.Name == "IreliaGatotsu" && sender.IsMe)
            {
                _gatotsuTick = Environment.TickCount;
            }
        }

        private static void InterrupterOnOnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            var spell = args;
            var unit = sender;

            if (spell.DangerLevel != Interrupter2.DangerLevel.High || !unit.CanStunTarget())
            {
                return;
            }

            var interruptE = Menu.Item("interruptE").GetValue<bool>();
            var interruptQe = Menu.Item("interruptQE").GetValue<bool>();

            if (E.IsReady() && E.IsInRange(unit, E.Range) && interruptE)
            {
                E.Cast(unit, Packets);
            }

            if (Q.IsReady() && E.IsReady() && Q.IsInRange(unit, Q.Range) && interruptQe)
            {
                Q.Cast(unit, Packets);

                var timeToArrive = (int) (1000*Player.Distance(unit)/Q.Speed + Q.Delay);
                Utility.DelayAction.Add(timeToArrive, () => E.Cast(unit, Packets));
            }
        }

        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsValidTarget() && Menu.Item("gapcloserE").GetValue<bool>() && E.IsReady())
            {
                E.Cast(gapcloser.Sender);
            }
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = Menu.Item("drawQ").GetValue<bool>();
            var drawE = Menu.Item("drawE").GetValue<bool>();
            var drawR = Menu.Item("drawR").GetValue<bool>();
            var drawStunnable = Menu.Item("drawStunnable").GetValue<bool>();
            var p = Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
            }

            foreach (
                var minion in
                    MinionManager.GetMinions(Q.Range).Where(x => Player.GetSpellDamage(x, SpellSlot.Q) > x.Health))
            {
                Render.Circle.DrawCircle(minion.Position, 65, Color.FromArgb(124, 252, 0), 3);
            }

            if (!drawStunnable)
            {
                return;
            }

            foreach (
                var unit in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.CanStunTarget())
                        .Where(x => x.IsEnemy)
                        .Where(x => !x.IsDead)
                        .Where(x => !x.IsAlly)
                        .Where(x => x.IsVisible))
            {
                var drawPos = Drawing.WorldToScreen(unit.Position);
                var textSize = Drawing.GetTextExtent("Stunnable");
                Drawing.DrawText(drawPos.X - textSize.Width/2f, drawPos.Y, Color.Aqua, "Stunnable");
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            KillSteal();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    WaveClear();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
            }
        }

        private static void KillSteal()
        {
            var useQ = Menu.Item("useQKS").GetValue<bool>();
            var useR = Menu.Item("useRKS").GetValue<bool>();
            var useIgnite = Menu.Item("useIgniteKS").GetValue<bool>();

            if (useQ && Q.IsReady())
            {
                var bestTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsValidTarget(Q.Range))
                        .Where(x => Player.GetSpellDamage(x, SpellSlot.Q) > x.Health)
                        .OrderBy(x => x.Distance(Player))
                        .FirstOrDefault();

                if (bestTarget != null)
                {
                    Q.Cast(bestTarget, Packets);
                }
            }

            if (useR && (R.IsReady() || UltActivated))
            {
                //TODO: Account for all 4 Charges
                var bestTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsValidTarget(R.Range))
                        .Where(x => Player.GetSpellDamage(x, SpellSlot.Q) > x.Health)
                        .OrderBy(x => x.Distance(Player))
                        .FirstOrDefault();

                if (bestTarget != null)
                {
                    R.Cast(bestTarget, Packets);
                }
            }

            if (useIgnite && IgniteSlot.IsReady())
            {
                var bestTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsValidTarget(600))
                        .Where(x => Player.GetSummonerSpellDamage(x, Damage.SummonerSpell.Ignite)/5 > x.Health)
                        .OrderBy(x => x.ChampionsKilled)
                        .FirstOrDefault();

                if (bestTarget != null)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, bestTarget);
                }
            }
        }

        private static void Combo()
        {
            var useQ = Menu.Item("useQ").GetValue<bool>();
            var useW = Menu.Item("useW").GetValue<bool>();
            var useE = Menu.Item("useE").GetValue<bool>();
            var useR = Menu.Item("useR").GetValue<bool>();
            var minQRange = Menu.Item("minQRange").GetValue<Slider>().Value;
            var useEStun = Menu.Item("useEStun").GetValue<bool>();
            var useQGapclose = Menu.Item("useQGapclose").GetValue<bool>();
            var useWBeforeQ = Menu.Item("useWBeforeQ").GetValue<bool>();
            var procSheen = Menu.Item("procSheen").GetValue<bool>();
            var useIgnite = Menu.Item("useIgnite").GetValue<bool>();
            var useRGapclose = Menu.Item("useRGapclose").GetValue<bool>();

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (target == null && useQGapclose)
            {
                    var minionQ =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.IsValidTarget())
                        .Where(x => Player.GetSpellDamage(x, SpellSlot.Q) > x.Health)
                        .FirstOrDefault(
                            x =>
                                x.Distance(TargetSelector.GetTarget(Q.Range * 5, TargetSelector.DamageType.Physical)) <
                                Q.Range);

                    if (minionQ != null)
                    {
                        Q.CastOnUnit(minionQ, Packets);
                        return;
                    }

                if (useRGapclose)
                {
                    var minionR =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(x => x.IsValidTarget())
                            .Where(x => x.Distance(Player) < Q.Range) // Use Q.Range so we follow up with a Q
                            .Where(x => x.CountEnemiesInRange(Q.Range) >= 1)
                            .FirstOrDefault(
                                x =>
                                    x.Health - Player.GetSpellDamage(x, SpellSlot.R) <
                                    Player.GetSpellDamage(x, SpellSlot.Q));

                    if (minionR != null)
                    {
                        R.Cast(minionR, Packets);
                    }
                }
            }

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Botrk.IsReady())
            {
                Botrk.Cast(target);
            }

            if (Cutlass.IsReady())
            {
                Cutlass.Cast(target);
            }

            if (Omen.IsReady() && Omen.IsInRange(target) &&
                target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player))
            {
                Omen.Cast();
            }

            if (useIgnite && target != null && target.IsValidTarget(600) &&
                (IgniteSlot.IsReady() &&
                 Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health))
            {
                Player.Spellbook.CastSpell(IgniteSlot, target);
            }

            if (useWBeforeQ)
            {
                if (useW && W.IsReady())
                {
                    W.Cast(Packets);
                }

                if (useQ && Q.IsReady() && target.Distance(Player.ServerPosition) > minQRange)
                {
                    Q.CastOnUnit(target, Packets);
                }
            }
            else
            {
                if (useQ && Q.IsReady() && target.Distance(Player.ServerPosition) > minQRange)
                {
                    Q.CastOnUnit(target, Packets);
                }

                if (useW && W.IsReady())
                {
                    W.Cast(Packets);
                }
            }

            if (useEStun)
            {
                if (target.CanStunTarget() && useE && E.IsReady())
                {
                    E.Cast(target, Packets);
                }
            }
            else
            {
                if (useE && E.IsReady())
                {
                    E.Cast(target, Packets);
                }
            }

            if (useR && R.IsReady() && !UltActivated)
            {
                R.Cast(target, Packets);
            }

            // Get target that is in the R range
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (useR && UltActivated)
            {
                if (!procSheen)
                {
                    return;
                }

                // Fire Ult if player is out of AA range, with Q not up or not in range
                if (target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player))
                {
                    R.Cast(rTarget, Packets);
                }
                else
                {
                    if (!HasSheenBuff)
                    {
                        R.Cast(rTarget, Packets);
                    }
                }
            }
            else
            {
                R.Cast(rTarget, Packets);
            }
        }

        private static void WaveClear()
        {
            var useQ = Menu.Item("waveclearQ").GetValue<bool>();
            var useQKillable = Menu.Item("waveclearQKillable").GetValue<bool>();
            var useW = Menu.Item("waveclearW").GetValue<bool>();
            var useR = Menu.Item("waveclearR").GetValue<bool>();
            var reqMana = Menu.Item("waveClearMana").GetValue<Slider>().Value;
            var waitTime = Menu.Item("gatotsuTime").GetValue<Slider>().Value;
            var dontQUnderTower = Menu.Item("noQMinionTower").GetValue<bool>();

            if (Player.ManaPercentage() < reqMana)
            {
                return;
            }

            if (useQ && Q.IsReady() && Environment.TickCount - _gatotsuTick >= waitTime * 10)
            {
                if (useQKillable)
                {
                    foreach (var minion in
                        MinionManager.GetMinions(Q.Range).Where(x => Player.GetSpellDamage(x, SpellSlot.Q) > x.Health))
                    {
                        if (dontQUnderTower)
                        {
                            if (!minion.UnderTurret())
                            {
                                Q.Cast(minion, Packets);
                            }
                        }
                        else
                        {
                            Q.Cast(minion, Packets);
                        }
                    }
                }
                else
                {
                    Q.Cast(MinionManager.GetMinions(Q.Range).FirstOrDefault(), Packets);
                }
            }

            if (useW && W.IsReady())
            {
                if (Orbwalker.GetTarget() is Obj_AI_Minion && W.IsInRange(Orbwalker.GetTarget().Position, W.Range))
                {
                    W.Cast(Packets);
                }
            }

            if (!useR || !R.IsReady())
            {
                return;
            }

            // Get best position for ult
            var pos = R.GetLineFarmLocation(MinionManager.GetMinions(R.Range));
            R.Cast(pos.Position, Packets);
        }

        private static void LastHit()
        {
            var useQ = Menu.Item("lastHitQ").GetValue<bool>();
            var waitTime = Menu.Item("gatotsuTime").GetValue<Slider>().Value;
            var manaNeeded = Menu.Item("manaNeededQ").GetValue<Slider>().Value;
            var dontQUnderTower = Menu.Item("noQMinionTower").GetValue<bool>();

            if (useQ && Player.Mana / Player.MaxMana * 100 > manaNeeded &&
                Environment.TickCount - _gatotsuTick >= waitTime * 10)
            {
                foreach (var minion in
                    MinionManager.GetMinions(Q.Range).Where(x => Player.GetSpellDamage(x, SpellSlot.Q) > x.Health))
                {
                    if (dontQUnderTower && !minion.UnderTurret())
                    {
                        Q.Cast(minion, Packets);
                    }
                    else
                    {
                        Q.Cast(minion, Packets);
                    }
                }
            }
        }

        private static void SetupMenu()
        {
            Menu = new Menu("花边汉化-CWM刀妹重做", "cmIreliaReloaded", true);

            // Target Selector
            var tsMenu = new Menu("目标 选择", "cmTS");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalker
            var orbwalkMenu = new Menu("走 砍", "cmOrbwalk");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkMenu);
            Menu.AddSubMenu(orbwalkMenu);

            // Combo
            var comboMenu = new Menu("连 招", "cmCombo");
            comboMenu.AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useQGapclose", "突进时 使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("minQRange", "最小 Q 范围")).SetValue(new Slider(250, 20, 400));
            comboMenu.AddItem(new MenuItem("useW", "使用 W").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useEStun", "只用E 可眩晕").SetValue(false));
            comboMenu.AddItem(new MenuItem("useR", "使用 R").SetValue(true));
            comboMenu.AddItem(new MenuItem("useWBeforeQ", "先Q后W").SetValue(true));
            comboMenu.AddItem(new MenuItem("procSheen", "使用R 激活耀光效果").SetValue(true));
            comboMenu.AddItem(new MenuItem("useRGapclose", "突进时顺手R清兵").SetValue(true));
            comboMenu.AddItem(new MenuItem("useIgnite", "使用 点燃").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // KS
            var ksMenu = new Menu("击 杀", "cmKS");
            ksMenu.AddItem(new MenuItem("useQKS", "使用 Q").SetValue(true));
            ksMenu.AddItem(new MenuItem("useRKS", "使用 R").SetValue(false));
            ksMenu.AddItem(new MenuItem("useIgniteKS", "使用 点燃").SetValue(true));
            Menu.AddSubMenu(ksMenu);

            // Farming
            var farmingMenu = new Menu("打 钱", "cmFarming");
            farmingMenu.AddItem(new MenuItem("lastHitQ", "Q 补刀").SetValue(false));
            farmingMenu.AddItem(new MenuItem("manaNeededQ", "补刀 蓝量 %")).SetValue(new Slider(35));
            farmingMenu.AddItem(new MenuItem("noQMinionTower", "禁止 塔下Q补刀").SetValue(true));
            farmingMenu.AddItem(new MenuItem("gatotsuTime", "Q 延迟")).SetValue(new Slider(35));

            // Wave Clear SubMenu
            var waveClearMenu = new Menu("清 线", "cmWaveClear");
            waveClearMenu.AddItem(new MenuItem("waveclearQ", "使用 Q").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("waveclearQKillable", "仅 使用Q 清线").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("waveclearW", "使用 W").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("waveclearR", "使用 R").SetValue(false));
            waveClearMenu.AddItem(new MenuItem("waveClearMana", "清线 蓝量比 %").SetValue(new Slider(20)));
            farmingMenu.AddSubMenu(waveClearMenu);
            Menu.AddSubMenu(farmingMenu);

            // Drawing
            var drawMenu = new Menu("范 围", "cmDraw");
            drawMenu.AddItem(new MenuItem("drawQ", "Q 范围").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawE", "E 范围").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawR", "R 范围").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawDmg", "显示 连招 伤害").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawStunnable", "显示 眩晕").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawKillableQ", "显示 可被Q击杀 小兵").SetValue(false));
            Menu.AddSubMenu(drawMenu);

            // Misc
            var miscMenu = new Menu("杂 项", "cmMisc");
            miscMenu.AddItem(new MenuItem("packets", "使用 封包").SetValue(true));
            miscMenu.AddItem(new MenuItem("interruptE", "E 打断").SetValue(true));
            miscMenu.AddItem(new MenuItem("interruptQE", "使用 Q+E 打断").SetValue(true));
            miscMenu.AddItem(new MenuItem("gapcloserE", "使用 E 突进").SetValue(true));
            Menu.AddSubMenu(miscMenu);

            Menu.AddToMainMenu();
        }

        private static float DamageToUnit(Obj_AI_Hero hero)
        {
            float dmg = 0;

            var spells = new List<Spell> {Q, W, E, R};
            foreach (var spell in spells.Where(x => x.IsReady()))
            {
                // Account for each blade
                if (spell.Slot == SpellSlot.R)
                {
                    dmg += spell.GetDamage(hero)*4;
                }
                else
                {
                    dmg += spell.GetDamage(hero);
                }
            }

            if (Botrk.IsReady())
            {
                dmg += (float) Player.GetItemDamage(hero, Damage.DamageItems.Botrk);
            }

            if (Cutlass.IsReady())
            {
                dmg += (float) Player.GetItemDamage(hero, Damage.DamageItems.Bilgewater);
            }

            return dmg;
        }

        #region Spells

        private static Spell Q { get; set; }
        private static Spell W { get; set; }
        private static Spell R { get; set; }
        private static Spell E { get; set; }
        private static SpellSlot IgniteSlot { get; set; }

        private static int _gatotsuTick;

        #endregion

        #region Config

        private static bool Packets
        {
            get { return Menu.Item("packets").GetValue<bool>(); }
        }

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static Menu Menu { get; set; }
        private static Orbwalking.Orbwalker Orbwalker { get; set; }

        #endregion

        #region Items

        private static Items.Item Botrk { get; set; }
        private static Items.Item Cutlass { get; set; }
        private static Items.Item Omen { get; set; }

        #endregion

        #region Buffs

        private static bool UltActivated
        {
            get { return Player.HasBuff("ireliatranscendentbladesspell", true); }
        }

        private static bool HasSheenBuff
        {
            get { return Player.HasBuff("sheen", true); }
        }

        #endregion
    }

    // Helpful extension to see if unit is stunnable
    public static class Extension
    {
        public static bool CanStunTarget(this AttackableUnit unit)
        {
            return unit.Health/unit.MaxHealth*100 >
                   ObjectManager.Player.Health/ObjectManager.Player.MaxHealth*100;
        }
    }
}