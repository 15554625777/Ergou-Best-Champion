using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterSeries.Common.M_Orbwalker;

namespace MasterSeries.Champions
{
    class Tryndamere : Program
    {
        public Tryndamere()
        {
            Q = new Spell(SpellSlot.Q, 20);
            W = new Spell(SpellSlot.W, 830);
            E = new Spell(SpellSlot.E, 670);
            R = new Spell(SpellSlot.R, 20);
            E.SetSkillshot(0.1f, 160, 700, false, SkillshotType.SkillshotLine);

            var ChampMenu = new Menu("插件", Name + "Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    ItemBool(ComboMenu, "Q", "使用 Q");
                    ItemSlider(ComboMenu, "QUnder", "-> 如果血量低于", 40);
                    ItemBool(ComboMenu, "W", "使用 W");
                    ItemBool(ComboMenu, "E", "使用 E");
                    ItemBool(ComboMenu, "Item", "使用 物品");
                    ItemBool(ComboMenu, "Ignite", "如果能击杀自动点燃");
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    ItemBool(HarassMenu, "W", "使用 W");
                    ItemBool(HarassMenu, "E", "使用 E");
                    ItemSlider(HarassMenu, "EAbove", "-> 如果血量超过", 20);
                    ChampMenu.AddSubMenu(HarassMenu);
                }
                var ClearMenu = new Menu("清线/清野", "Clear");
                {
                    var SmiteMob = new Menu("如果惩戒能击杀野怪", "SmiteMob");
                    {
                        ItemBool(SmiteMob, "Baron", "大龙");
                        ItemBool(SmiteMob, "Dragon", "小龙");
                        ItemBool(SmiteMob, "Red", "红BUFF");
                        ItemBool(SmiteMob, "Blue", "蓝BUFF");
                        ItemBool(SmiteMob, "Krug", "石头怪");
                        ItemBool(SmiteMob, "Gromp", "大蛤蟆");
                        ItemBool(SmiteMob, "Raptor", "啄木鸟4兄弟");
                        ItemBool(SmiteMob, "Wolf", "幽灵狼3兄弟");
                        ClearMenu.AddSubMenu(SmiteMob);
                    }
                    ItemBool(ClearMenu, "Q", "使用 Q");
                    ItemSlider(ClearMenu, "QUnder", "-> 如果血量低于", 40);
                    ItemBool(ClearMenu, "E", "使用 E");
                    ItemBool(ClearMenu, "Item", "使用九头蛇");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var MiscMenu = new Menu("额外选项", "Misc");
                {
                    ItemBool(MiscMenu, "EKillSteal", "使用E抢人头");
                    ItemBool(MiscMenu, "QSurvive", "尝试使用Q求生");
                    ItemBool(MiscMenu, "RSurvive", "尝试使用R求生");
                    ItemSlider(MiscMenu, "CustomSkin", "换肤", 4, 0, 6).ValueChanged += SkinChanger;
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示范围", "Draw");
                {
                    ItemBool(DrawMenu, "W", "W 范围", false);
                    ItemBool(DrawMenu, "E", "E 范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                Config.AddSubMenu(ChampMenu);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += TrySurviveSpellCast;
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
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.Flee && E.IsReady()) E.Cast(Game.CursorPos, PacketCast());
            if (ItemBool("Misc", "EKillSteal")) KillSteal();
            if (ItemBool("Misc", "QSurvive") && Q.IsReady()) TrySurvive(Q.Slot);
            if (ItemBool("Misc", "RSurvive") && R.IsReady()) TrySurvive(R.Slot);
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (ItemBool("Draw", "W") && W.Level > 0) Utility.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "E") && E.Level > 0) Utility.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
        }

        private void NormalCombo(string Mode)
        {
            if (targetObj == null) return;
            if (Mode == "Combo" && ItemBool(Mode, "Q") && Q.IsReady() && Player.HealthPercentage() <= ItemSlider(Mode, "QUnder") && Player.CountEnemysInRange(800) >= 1) Q.Cast(PacketCast());
            if (ItemBool(Mode, "W") && W.CanCast(targetObj))
            {
                if (targetObj.IsFacing(Player) && Orbwalk.InAutoAttackRange(targetObj) && (Player.GetAutoAttackDamage(targetObj, true) < targetObj.GetAutoAttackDamage(Player, true) || Player.Health < targetObj.Health))
                {
                    W.Cast(PacketCast());
                }
                else if (!targetObj.IsFacing(Player) && !Orbwalk.InAutoAttackRange(targetObj)) W.Cast(PacketCast());
            }
            if (ItemBool(Mode, "E") && E.CanCast(targetObj) && (CanKill(targetObj, E) || (Player.Distance3D(targetObj) > Orbwalk.GetAutoAttackRange(Player, targetObj) + 20 && (Mode == "Combo" || (Mode == "Harass" && Player.HealthPercentage() >= ItemSlider(Mode, "EAbove")))))) E.Cast(targetObj.Position.Extend(Player.Position, Player.Distance3D(targetObj) <= E.Range - 100 ? -100 : 0), PacketCast());
            if (Mode == "Combo" && ItemBool(Mode, "Item")) UseItem(targetObj);
            if (Mode == "Combo" && ItemBool(Mode, "Ignite") && IgniteReady()) CastIgnite(targetObj);
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly);
            foreach (var Obj in minionObj)
            {
                if (SmiteReady() && Obj.Team == GameObjectTeam.Neutral)
                {
                    if ((ItemBool("SmiteMob", "Baron") && Obj.Name.StartsWith("SRU_Baron")) || (ItemBool("SmiteMob", "Dragon") && Obj.Name.StartsWith("SRU_Dragon")) || (!Obj.Name.Contains("Mini") && (
                        (ItemBool("SmiteMob", "Red") && Obj.Name.StartsWith("SRU_Red")) || (ItemBool("SmiteMob", "Blue") && Obj.Name.StartsWith("SRU_Blue")) ||
                        (ItemBool("SmiteMob", "Krug") && Obj.Name.StartsWith("SRU_Krug")) || (ItemBool("SmiteMob", "Gromp") && Obj.Name.StartsWith("SRU_Gromp")) ||
                        (ItemBool("SmiteMob", "Raptor") && Obj.Name.StartsWith("SRU_Razorbeak")) || (ItemBool("SmiteMob", "Wolf") && Obj.Name.StartsWith("SRU_Murkwolf"))))) CastSmite(Obj);
                }
                if (ItemBool("Clear", "Q") && Q.IsReady() && Player.HealthPercentage() <= ItemSlider("Clear", "QUnder") && (minionObj.Count(i => Orbwalk.InAutoAttackRange(i)) >= 2 || (Obj.MaxHealth >= 1200 && Orbwalk.InAutoAttackRange(Obj)))) Q.Cast(PacketCast());
                if (ItemBool("Clear", "E") && E.IsReady())
                {
                    var posEFarm = E.GetLineFarmLocation(minionObj.ToList());
                    E.Cast(posEFarm.MinionsHit >= 2 ? posEFarm.Position : Obj.Position.Extend(Player.Position, Player.Distance3D(Obj) <= E.Range - 100 ? -100 : 0).To2D(), PacketCast());
                }
                if (ItemBool("Clear", "Item")) UseItem(Obj, true);
            }
        }

        private void KillSteal()
        {
            if (!E.IsReady()) return;
            foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsValidTarget(E.Range) && CanKill(i, E) && i != targetObj).OrderBy(i => i.Health).OrderBy(i => i.Distance3D(Player))) E.Cast(Obj.Position.Extend(Player.Position, Player.Distance3D(Obj) <= E.Range - 100 ? -100 : 0), PacketCast());
        }

        private void UseItem(Obj_AI_Base Target, bool IsFarm = false)
        {
            if (Items.CanUseItem(Bilgewater) && Player.Distance3D(Target) <= 450 && !IsFarm) Items.UseItem(Bilgewater, Target);
            if (Items.CanUseItem(BladeRuined) && Player.Distance3D(Target) <= 450 && !IsFarm) Items.UseItem(BladeRuined, Target);
            if (Items.CanUseItem(Tiamat) && IsFarm ? Player.Distance3D(Target) <= 350 : Player.CountEnemysInRange(350) >= 1) Items.UseItem(Tiamat);
            if (Items.CanUseItem(Hydra) && IsFarm ? Player.Distance3D(Target) <= 350 : (Player.CountEnemysInRange(350) >= 2 || (Player.GetAutoAttackDamage(Target, true) < Target.Health && Player.CountEnemysInRange(350) == 1))) Items.UseItem(Hydra);
            if (Items.CanUseItem(Randuin) && Player.CountEnemysInRange(450) >= 1 && !IsFarm) Items.UseItem(Randuin);
            if (Items.CanUseItem(Youmuu) && Player.CountEnemysInRange(350) >= 1 && !IsFarm) Items.UseItem(Youmuu);
        }
    }
}