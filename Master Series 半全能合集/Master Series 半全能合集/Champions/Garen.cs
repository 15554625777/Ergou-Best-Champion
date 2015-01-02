using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterSeries.Common.M_Orbwalker;

namespace MasterSeries.Champions
{
    class Garen : Program
    {
        public Garen()
        {
            Q = new Spell(SpellSlot.Q, 300);
            W = new Spell(SpellSlot.W, 20);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 400);
            Q.SetTargetted(0.0435f, float.MaxValue);
            R.SetTargetted(0.13f, 900);

            var ChampMenu = new Menu("插件", Name + "Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    ItemBool(ComboMenu, "Q", "使用 Q");
                    ItemBool(ComboMenu, "W", "使用 W");
                    ItemSlider(ComboMenu, "WUnder", "-> 如果血量少于", 60);
                    ItemBool(ComboMenu, "E", "使用 E");
                    ItemBool(ComboMenu, "R", "如果能击杀使用R");
                    ItemBool(ComboMenu, "Item", "使用物品");
                    ItemBool(ComboMenu, "Ignite", "如果能击杀使用点燃");
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    ItemBool(HarassMenu, "Q", "使用 Q");
                    ItemBool(HarassMenu, "W", "使用 W");
                    ItemSlider(HarassMenu, "WUnder", "-> 如果血量少于", 60);
                    ItemBool(HarassMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(HarassMenu);
                }
                var ClearMenu = new Menu("清线/清野", "Clear");
                {
                    ItemBool(ClearMenu, "Q", "使用 Q");
                    ItemList(ClearMenu, "QMode", "-> 模式", new[] { "总是", "能够击杀" });
                    ItemBool(ClearMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var UltiMenu = new Menu("大招", "Ultimate");
                {
                    foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy)) ItemBool(UltiMenu, Obj.ChampionName, "使用 R On " + Obj.ChampionName);
                    ChampMenu.AddSubMenu(UltiMenu);
                }
                var MiscMenu = new Menu("额外选项", "Misc");
                {
                    ItemBool(MiscMenu, "QLastHit", "使用Q补刀");
                    ItemBool(MiscMenu, "WSurvive", "尝试使用W求生");
                    ItemSlider(MiscMenu, "CustomSkin", "换肤", 6, 0, 6).ValueChanged += SkinChanger;
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示范围", "Draw");
                {
                    ItemBool(DrawMenu, "E", "E 范围", false);
                    ItemBool(DrawMenu, "R", "R 范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                Config.AddSubMenu(ChampMenu);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Obj_AI_Base.OnProcessSpellCast += TrySurviveSpellCast;
            Orbwalk.AfterAttack += AfterAttack;
        }

        private void OnGameUpdate(EventArgs args)
        {
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
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.Flee && Q.IsReady()) Q.Cast(PacketCast());
            if (ItemBool("Misc", "WSurvive") && W.IsReady()) TrySurvive(W.Slot);
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (ItemBool("Draw", "E") && E.Level > 0) Utility.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "R") && R.Level > 0) Utility.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (args.SData.IsAutoAttack() && ((Obj_AI_Base)args.Target).IsValidTarget(Orbwalk.GetAutoAttackRange(Player, (Obj_AI_Base)args.Target) + 20) && Q.IsReady())
            {
                if (Orbwalk.CurrentMode == Orbwalk.Mode.Harass && ItemBool("Harass", "Q") && args.Target is Obj_AI_Hero)
                {
                    Q.Cast(PacketCast());
                }
                else if (args.Target is Obj_AI_Minion && CanKill((Obj_AI_Minion)args.Target, Q) && ((Orbwalk.CurrentMode == Orbwalk.Mode.LastHit && ItemBool("Misc", "QLastHit")) || (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear && ItemBool("Clear", "Q") && ItemList("Clear", "QMode") == 1))) Q.Cast(PacketCast());
            }
        }

        private void AfterAttack(AttackableUnit Target)
        {
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo && ItemBool("Combo", "Q") && Q.IsReady() && Target.IsValidTarget(Orbwalk.GetAutoAttackRange(Player, Target) + 20) && Target is Obj_AI_Hero) Q.Cast(PacketCast());
        }

        private void NormalCombo(string Mode)
        {
            if (targetObj == null) return;
            if (ItemBool(Mode, "Q") && Q.IsReady() && Player.Distance3D(targetObj) <= ((Mode == "Harass") ? Orbwalk.GetAutoAttackRange(Player, targetObj) + 20 : 800) && (Mode == "Harass" || (Mode == "Combo" && !Orbwalk.InAutoAttackRange(targetObj))))
            {
                if (Mode == "Harass")
                {
                    Orbwalk.SetAttack(false);
                    Player.IssueOrder(GameObjectOrder.AttackUnit, targetObj);
                    Orbwalk.SetAttack(true);
                }
                else Q.Cast(PacketCast());
            }
            if (ItemBool(Mode, "E") && E.CanCast(targetObj) && !Player.HasBuff("GarenE") && !Player.HasBuff("GarenQBuff")) E.Cast(PacketCast());
            if (ItemBool(Mode, "W") && W.IsReady() && Orbwalk.InAutoAttackRange(targetObj) && Player.HealthPercentage() <= ItemSlider(Mode, "WUnder")) W.Cast(PacketCast());
            if (Mode == "Combo" && ItemBool(Mode, "R") && ItemBool("Ultimate", targetObj.ChampionName) && R.CanCast(targetObj) && CanKill(targetObj, R)) R.CastOnUnit(targetObj, PacketCast());
            if (Mode == "Combo" && ItemBool(Mode, "Item") && Items.CanUseItem(Randuin) && Player.CountEnemysInRange(450) >= 1) Items.UseItem(Randuin);
            if (Mode == "Combo" && ItemBool(Mode, "Ignite") && IgniteReady()) CastIgnite(targetObj);
        }

        private void LaneJungClear()
        {
            foreach (var Obj in MinionManager.GetMinions(700, MinionTypes.All, MinionTeam.NotAlly))
            {
                if (ItemBool("Clear", "Q") && Q.IsReady())
                {
                    switch (ItemList("Clear", "QMode"))
                    {
                        case 0:
                            Q.Cast(PacketCast());
                            break;
                        case 1:
                            if (CanKill(Obj, Q) && Player.Distance3D(Obj) <= Orbwalk.GetAutoAttackRange(Player, Obj) + 20)
                            {
                                Orbwalk.SetAttack(false);
                                Player.IssueOrder(GameObjectOrder.AttackUnit, Obj);
                                Orbwalk.SetAttack(true);
                                break;
                            }
                            break;
                    }
                }
                if (ItemBool("Clear", "E") && E.CanCast(Obj) && !Player.HasBuff("GarenE") && !Player.HasBuff("GarenQBuff")) E.Cast(PacketCast());
            }
        }

        private void LastHit()
        {
            if (!ItemBool("Misc", "QLastHit") || !Q.IsReady()) return;
            foreach (var Obj in MinionManager.GetMinions(Orbwalk.GetAutoAttackRange() + 100, MinionTypes.All, MinionTeam.NotAlly).Where(i => CanKill(i, Q)).OrderByDescending(i => i.Distance3D(Player)))
            {
                Orbwalk.SetAttack(false);
                Player.IssueOrder(GameObjectOrder.AttackUnit, Obj);
                Orbwalk.SetAttack(true);
                break;
            }
        }
    }
}