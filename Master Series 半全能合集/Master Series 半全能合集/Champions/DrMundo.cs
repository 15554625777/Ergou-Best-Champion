using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterSeries.Common.M_Orbwalker;

namespace MasterSeries.Champions
{
    class DrMundo : Program
    {
        public DrMundo()
        {
            Q = new Spell(SpellSlot.Q, 1025);
            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 300);
            R = new Spell(SpellSlot.R, 20);
            Q.SetSkillshot(0.5f, 75, 1500, true, SkillshotType.SkillshotLine);

            var ChampMenu = new Menu("插件", Name + "Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    ItemBool(ComboMenu, "Q", "使用 Q");
                    ItemBool(ComboMenu, "W", "使用 W");
                    ItemSlider(ComboMenu, "WAbove", "-> 如果血量超出", 20);
                    ItemBool(ComboMenu, "E", "使用 E");
                    ItemBool(ComboMenu, "Item", "使用物品");
                    ItemBool(ComboMenu, "Ignite", "如果能击杀自动点燃");
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    ItemBool(HarassMenu, "Q", "使用 Q");
                    ItemBool(HarassMenu, "W", "使用 W");
                    ItemSlider(HarassMenu, "WAbove", "-> 如果血量超出", 20);
                    ItemBool(HarassMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(HarassMenu);
                }
                var ClearMenu = new Menu("清线/清野", "Clear");
                {
                    var SmiteMob = new Menu("如果能惩戒击杀野怪", "SmiteMob");
                    { ItemBool(SmiteMob, "Baron", "大龙");
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
                    ItemBool(ClearMenu, "W", "使用 W");
                    ItemSlider(ClearMenu, "WAbove", "-> 如果血量超出", 20);
                    ItemBool(ClearMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var UltiMenu = new Menu("大招", "Ultimate");
                {
                    ItemBool(UltiMenu, "RSurvive", "尝试使用R求生");
                    ItemSlider(UltiMenu, "RUnder", "-> 如果血量少于", 35);
                    ChampMenu.AddSubMenu(UltiMenu);
                }
                var MiscMenu = new Menu("额外选项", "Misc");
                {
                    ItemBool(MiscMenu, "QLastHit", "使用Q补刀");
                    ItemBool(MiscMenu, "QKillSteal", "使用Q抢人头");
                    ItemBool(MiscMenu, "SmiteCol", "自动惩戒碰撞");
                    ItemSlider(MiscMenu, "CustomSkin", "换肤", 7, 0, 7).ValueChanged += SkinChanger;
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示范围", "Draw");
                {
                    ItemBool(DrawMenu, "Q", "Q 范围", false);
                    ItemBool(DrawMenu, "W", "W 范围", false);
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
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LastHit) LastHit();
            if (ItemBool("Ultimate", "RSurvive") && R.IsReady()) TrySurvive(R.Slot, ItemSlider("Ultimate", "RUnder"));
            if (ItemBool("Misc", "QKillSteal")) KillSteal();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (ItemBool("Draw", "Q") && Q.Level > 0) Utility.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "W") && W.Level > 0) Utility.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);
        }

        private void NormalCombo(string Mode)
        {
            if (ItemBool(Mode, "W") && W.IsReady() && Player.HasBuff("BurningAgony") && Player.CountEnemysInRange(500) == 0) W.Cast(PacketCast());
            if (targetObj == null) return;
            if (ItemBool(Mode, "W") && W.IsReady())
            {
                if (Player.HealthPercentage() >= ItemSlider(Mode, "WAbove"))
                {
                    if (Player.Distance3D(targetObj) <= W.Range + 60)
                    {
                        if (!Player.HasBuff("BurningAgony")) W.Cast(PacketCast());
                    }
                    else if (Player.HasBuff("BurningAgony")) W.Cast(PacketCast());
                }
                else if (Player.HasBuff("BurningAgony")) W.Cast(PacketCast());
            }
            if (ItemBool(Mode, "Q") && Q.CanCast(targetObj))
            {
                var QPred = Q.GetPrediction(targetObj);
                if (ItemBool("Misc", "SmiteCol") && QPred.CollisionObjects.Count == 1 && Q.MinHitChance == HitChance.High && CastSmite(QPred.CollisionObjects.First()))
                {
                    Q.Cast(QPred.CastPosition, PacketCast());
                }
                else Q.CastIfHitchanceEquals(targetObj, HitChance.High, PacketCast());
            }
            if (ItemBool(Mode, "E") && E.IsReady() && Orbwalk.InAutoAttackRange(targetObj)) E.Cast(PacketCast());
            if (Mode == "Combo" && ItemBool(Mode, "Item") && Items.CanUseItem(Randuin) && Player.CountEnemysInRange(450) >= 1) Items.UseItem(Randuin);
            if (Mode == "Combo" && ItemBool(Mode, "Ignite") && IgniteReady()) CastIgnite(targetObj);
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            if (minionObj.Count() == 0 && ItemBool("Clear", "W") && W.IsReady() && Player.HasBuff("BurningAgony")) W.Cast(PacketCast());
            foreach (var Obj in minionObj)
            {
                if (SmiteReady() && Obj.Team == GameObjectTeam.Neutral)
                {
                    if ((ItemBool("SmiteMob", "Baron") && Obj.Name.StartsWith("SRU_Baron")) || (ItemBool("SmiteMob", "Dragon") && Obj.Name.StartsWith("SRU_Dragon")) || (!Obj.Name.Contains("Mini") && (
                        (ItemBool("SmiteMob", "Red") && Obj.Name.StartsWith("SRU_Red")) || (ItemBool("SmiteMob", "Blue") && Obj.Name.StartsWith("SRU_Blue")) ||
                        (ItemBool("SmiteMob", "Krug") && Obj.Name.StartsWith("SRU_Krug")) || (ItemBool("SmiteMob", "Gromp") && Obj.Name.StartsWith("SRU_Gromp")) ||
                        (ItemBool("SmiteMob", "Raptor") && Obj.Name.StartsWith("SRU_Razorbeak")) || (ItemBool("SmiteMob", "Wolf") && Obj.Name.StartsWith("SRU_Murkwolf"))))) CastSmite(Obj);
                }
                if (ItemBool("Clear", "E") && E.IsReady() && Orbwalk.InAutoAttackRange(Obj)) E.Cast(PacketCast());
                if (ItemBool("Clear", "W") && W.IsReady())
                {
                    if (Player.HealthPercentage() >= ItemSlider("Clear", "WAbove"))
                    {
                        if (minionObj.Count(i => Player.Distance3D(i) <= W.Range + 60) >= 2 || (Obj.MaxHealth >= 1200 && Player.Distance3D(Obj) <= W.Range + 60))
                        {
                            if (!Player.HasBuff("BurningAgony")) W.Cast(PacketCast());
                        }
                        else if (Player.HasBuff("BurningAgony")) W.Cast(PacketCast());
                    }
                    else if (Player.HasBuff("BurningAgony")) W.Cast(PacketCast());
                }
                if (ItemBool("Clear", "Q") && Q.IsReady() && (CanKill(Obj, Q) || Obj.MaxHealth >= 1200)) Q.CastIfHitchanceEquals(Obj, HitChance.Medium, PacketCast());
            }
        }

        private void LastHit()
        {
            if (!ItemBool("Misc", "QLastHit") || !Q.IsReady()) return;
            foreach (var Obj in MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly).Where(i => CanKill(i, Q)).OrderByDescending(i => i.Distance3D(Player))) Q.CastIfHitchanceEquals(Obj, HitChance.High, PacketCast());
        }

        private void KillSteal()
        {
            if (!Q.IsReady()) return;
            foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsValidTarget(Q.Range) && CanKill(i, Q) && i != targetObj).OrderBy(i => i.Health).OrderBy(i => i.Distance3D(Player)))
            {
                var QPred = Q.GetPrediction(Obj);
                if (ItemBool("Misc", "SmiteCol") && QPred.CollisionObjects.Count == 1 && Q.MinHitChance == HitChance.High && CastSmite(QPred.CollisionObjects.First()))
                {
                    Q.Cast(QPred.CastPosition, PacketCast());
                }
                else Q.CastIfHitchanceEquals(Obj, HitChance.High, PacketCast());
            }
        }
    }
}