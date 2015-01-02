using System;
using System.Linq;
using System.Collections.Generic;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterSeries.Common.M_Orbwalker;

namespace MasterSeries.Champions
{
    class Shen : Program
    {
        private bool PingCasted = false;

        public Shen()
        {
            Q = new Spell(SpellSlot.Q, 475);
            W = new Spell(SpellSlot.W, 20);
            E = new Spell(SpellSlot.E, 620);
            R = new Spell(SpellSlot.R, 25000);
            Q.SetTargetted(0.5f, 1500);
            E.SetSkillshot(0.5f, 50, float.MaxValue, false, SkillshotType.SkillshotLine);

            Config.SubMenu("OW").SubMenu("模式").AddItem(new MenuItem("OWFlashTaunt", "闪现E", true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            var ChampMenu = new Menu("插件", Name + "Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    ItemBool(ComboMenu, "Q", "使用 Q");
                    ItemBool(ComboMenu, "W", "使用 W");
                    ItemSlider(ComboMenu, "WUnder", "-> 如果血量少于", 20);
                    ItemBool(ComboMenu, "E", "使用 E");
                    ItemBool(ComboMenu, "Item", "使用 物品");
                    ItemBool(ComboMenu, "Ignite", "如果能击杀自动点燃");
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    ItemBool(HarassMenu, "Q", "使用 Q");
                    ItemBool(HarassMenu, "E", "使用 E");
                    ItemSlider(HarassMenu, "EAbove", "-> 如果血量超过", 20);
                    ChampMenu.AddSubMenu(HarassMenu);
                }
                var ClearMenu = new Menu("清线/清野", "Clear");
                {
                    ItemBool(ClearMenu, "Q", "使用 Q");
                    ItemBool(ClearMenu, "W", "使用 W");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var UltiMenu = new Menu("大招", "Ultimate");
                {
                    ItemBool(UltiMenu, "Alert", "警报盟友低血量");
                    ItemSlider(UltiMenu, "HpUnder", "-> 如果血量低于", 30);
                    ItemBool(UltiMenu, "Ping", "-> Ping 回退");
                    ChampMenu.AddSubMenu(UltiMenu);
                }
                var MiscMenu = new Menu("额外选项", "Misc");
                {
                    ItemBool(MiscMenu, "QLastHit", "使用Q补刀");
                    ItemBool(MiscMenu, "EAntiGap", "对突进者使用E");
                    ItemBool(MiscMenu, "EInterrupt", "使用E打断");
                    ItemBool(MiscMenu, "EUnderTower", "如果敌人在塔下使用E");
                    ItemBool(MiscMenu, "WSurvive", "尝试使用W求生");
                    ItemSlider(MiscMenu, "CustomSkin", "失效-换肤", 6, 0, 6).ValueChanged += SkinChanger;
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示范围", "Draw");
                {
                    ItemBool(DrawMenu, "Q", "Q 范围", false);
                    ItemBool(DrawMenu, "E", "E 范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                Config.AddSubMenu(ChampMenu);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += TrySurviveSpellCast;
        }

        private void OnGameUpdate(EventArgs args)
        {
            //Passive: Shen Passive Aura
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsChannelingImportantSpell() || Player.IsRecalling()) return;
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo || Orbwalk.CurrentMode == Orbwalk.Mode.Harass)
            {
                NormalCombo(Orbwalk.CurrentMode.ToString());
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear)
            {
                LaneJungClear();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LastHit)
            {
                LastHit();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.Flee && E.IsReady()) E.Cast(Game.CursorPos, PacketCast());
            if (ItemActive("FlashTaunt")) FlashTaunt();
            if (ItemBool("Ultimate", "Alert")) UltimateAlert();
            if (ItemBool("Misc", "EUnderTower")) AutoEUnderTower();
            if (ItemBool("Misc", "WSurvive") && W.IsReady()) TrySurvive(W.Slot);
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (ItemBool("Draw", "Q") && Q.Level > 0) Utility.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "E") && E.Level > 0) Utility.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
        }

        private void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!ItemBool("Misc", "EAntiGap") || Player.IsDead || !E.CanCast(gapcloser.Sender)) return;
            if (Player.Distance3D(gapcloser.Sender) <= Orbwalk.GetAutoAttackRange(Player, gapcloser.Sender) + 50) E.Cast(gapcloser.Sender.Position.Extend(Player.Position, -100), PacketCast());
        }

        private void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!ItemBool("Misc", "EInterrupt") || Player.IsDead || !E.CanCast(unit)) return;
            E.Cast(unit.Position.Extend(Player.Position, Player.Distance3D(unit) <= E.Range - 100 ? -100 : 0), PacketCast());
        }

        private void NormalCombo(string Mode)
        {
            if (targetObj == null) return;
            if (Mode == "Combo" && ItemBool(Mode, "Item"))
            {
                if (Items.CanUseItem(Deathfire) && Player.Distance3D(targetObj) <= 750) Items.UseItem(Deathfire, targetObj);
                if (Items.CanUseItem(Blackfire) && Player.Distance3D(targetObj) <= 750) Items.UseItem(Blackfire, targetObj);
                if (Items.CanUseItem(Randuin) && Player.CountEnemysInRange(450) >= 1) Items.UseItem(Randuin);
            }
            if (ItemBool(Mode, "E") && E.CanCast(targetObj) && (Mode == "Combo" || (Mode == "Harass" && Player.HealthPercentage() >= ItemSlider(Mode, "EAbove")))) E.Cast(targetObj.Position.Extend(Player.Position, Player.Distance3D(targetObj) <= E.Range - 100 ? -100 : 0), PacketCast());
            if (ItemBool(Mode, "Q") && Q.CanCast(targetObj)) Q.CastOnUnit(targetObj, PacketCast());
            if (Mode == "Combo" && ItemBool(Mode, "W") && W.IsReady() && Orbwalk.InAutoAttackRange(targetObj) && Player.HealthPercentage() <= ItemSlider(Mode, "WUnder")) W.Cast(PacketCast());
            if (Mode == "Combo" && ItemBool(Mode, "Ignite") && IgniteReady()) CastIgnite(targetObj);
        }

        private void LaneJungClear()
        {
            foreach (var Obj in MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly))
            {
                if (ItemBool("Clear", "Q") && Q.IsReady()) Q.CastOnUnit(Obj, PacketCast());
                if (ItemBool("Clear", "W") && W.IsReady() && Orbwalk.InAutoAttackRange(Obj)) W.Cast(PacketCast());
            }
        }

        private void LastHit()
        {
            if (!ItemBool("Misc", "QLastHit") || !Q.IsReady()) return;
            foreach (var Obj in MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly).Where(i => CanKill(i, Q)).OrderByDescending(i => i.Distance3D(Player))) Q.CastOnUnit(Obj, PacketCast());
        }

        private void FlashTaunt()
        {
            CustomOrbwalk(targetObj);
            if (targetObj == null || !E.IsReady()) return;
            if (E.InRange(targetObj))
            {
                E.Cast(targetObj.Position.Extend(Player.Position, Player.Distance3D(targetObj) <= E.Range - 100 ? -100 : 0), PacketCast());
            }
            else if (Player.Distance3D(targetObj) <= E.Range + 385 && FlashReady()) CastFlash(targetObj.Position);
        }

        private void UltimateAlert()
        {
            if (!R.IsReady() || PingCasted) return;
            foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsValidTarget(float.MaxValue, false) && i.IsAlly && !i.IsMe && i.CountEnemysInRange(800) >= 1 && i.HealthPercentage() <= ItemSlider("Ultimate", "HpUnder")))
            {
                Game.PrintChat("<font color = \'{0}'>-></font> <font color = \'{1}'>{2}</font>: <font color = \'{3}'>In Dangerous</font>", HtmlColor.BlueViolet, HtmlColor.Gold, Obj.ChampionName, HtmlColor.Cyan);
                if (ItemBool("Ultimate", "Ping"))
                {
                    for (var i = 0; i < 3; i++) Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(Obj.Position.X, Obj.Position.Y, Obj.NetworkId, 0, Packet.PingType.Fallback)).Process();
                    PingCasted = true;
                    Utility.DelayAction.Add(5000, () => PingCasted = false);
                }
            }
        }

        private void AutoEUnderTower()
        {
            if (Player.UnderTurret() || !E.IsReady()) return;
            foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsValidTarget(E.Range)).OrderBy(i => i.Distance3D(Player)))
            {
                var TowerObj = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(i => i.IsValidTarget(950, false) && i.IsAlly);
                if (TowerObj != null && Obj.Distance3D(TowerObj) <= 950) E.Cast(Obj.Position.Extend(Player.Position, Player.Distance3D(Obj) <= E.Range - 100 ? -100 : 0), PacketCast());
            }
        }
    }
}