using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterSeries.Common.M_Orbwalker;

namespace MasterSeries.Champions
{
    class Amumu : Program
    {
        public Amumu()
        {
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 550);
            Q.SetSkillshot(0.5f, 80, 2000, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 550, float.MaxValue, false, SkillshotType.SkillshotCircle);

            var ChampMenu = new Menu("插件", Name + "Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    ItemBool(ComboMenu, "Q", "使用 Q");
                    ItemBool(ComboMenu, "W", "使用 W");
                    ItemSlider(ComboMenu, "WAbove", "-> 如果法力超过", 20);
                    ItemBool(ComboMenu, "E", "使用 E");
                    ItemBool(ComboMenu, "R", "使用 R");
                    ItemList(ComboMenu, "RMode", "-> 模式", new[] { "能击杀", "# 敌人" });
                    ItemSlider(ComboMenu, "RAbove", "--> 如果敌人超过", 2, 1, 4);
                    ItemBool(ComboMenu, "Item", "使用物品");
                    ItemBool(ComboMenu, "Ignite", "如果能击杀自动点燃");
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    ItemBool(HarassMenu, "W", "使用 W");
                    ItemSlider(HarassMenu, "WAbove", "-> 如果法力超过", 20);
                    ItemBool(HarassMenu, "E", "使用 E");
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
                    ItemBool(ClearMenu, "W", "使用 W");
                    ItemSlider(ClearMenu, "WAbove", "-> 如果法力超过", 20);
                    ItemBool(ClearMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var MiscMenu = new Menu("额外选项", "Misc");
                {
                    ItemBool(MiscMenu, "QAntiGap", "对突进者使用Q");
                    ItemBool(MiscMenu, "SmiteCol", "自动惩戒碰撞");
                    ItemSlider(MiscMenu, "CustomSkin", "失效-换肤", 6, 0, 7).ValueChanged += SkinChanger;
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示范围", "Draw");
                {
                    ItemBool(DrawMenu, "Q", "Q 范围", false);
                    ItemBool(DrawMenu, "W", "W 范围", false);
                    ItemBool(DrawMenu, "E", "E 范围", false);
                    ItemBool(DrawMenu, "R", "R 范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                Config.AddSubMenu(ChampMenu);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsChannelingImportantSpell() || Player.IsRecalling()) return;
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo || Orbwalk.CurrentMode == Orbwalk.Mode.Harass)
            {
                NormalCombo(Orbwalk.CurrentMode.ToString());
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear) LaneJungClear();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (ItemBool("Draw", "Q") && Q.Level > 0) Utility.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "W") && W.Level > 0) Utility.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "E") && E.Level > 0) Utility.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "R") && R.Level > 0) Utility.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }

        private void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!ItemBool("Misc", "QAntiGap") || Player.IsDead || !Q.CanCast(gapcloser.Sender) || Player.Distance3D(gapcloser.Sender) > 400) return;
            var QPred = Q.GetPrediction(gapcloser.Sender);
            if (ItemBool("Misc", "SmiteCol") && QPred.CollisionObjects.Count == 1 && Q.MinHitChance == HitChance.High && CastSmite(QPred.CollisionObjects.First()))
            {
                Q.Cast(QPred.CastPosition, PacketCast());
            }
            else Q.Cast(gapcloser.Sender.Position, PacketCast());
        }

        private void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!ItemBool("Misc", "QInterrupt") || !Q.CanCast(unit)) return;
            var QPred = Q.GetPrediction(unit);
            if (ItemBool("Misc", "SmiteCol") && QPred.CollisionObjects.Count == 1 && Q.MinHitChance == HitChance.High && CastSmite(QPred.CollisionObjects.First()))
            {
                Q.Cast(QPred.CastPosition, PacketCast());
            }
            else Q.CastIfHitchanceEquals(unit, HitChance.High, PacketCast());
        }

        private void NormalCombo(string Mode)
        {
            if (ItemBool(Mode, "W") && W.IsReady() && Player.HasBuff("AuraofDespair") && Player.CountEnemysInRange(500) == 0) W.Cast(PacketCast());
            if (targetObj == null) return;
            if (Mode == "Combo" && ItemBool(Mode, "Q") && Q.IsReady())
            {
                var nearObj = ObjectManager.Get<Obj_AI_Base>().Where(i => i.IsValidTarget(Q.Range) && !(i is Obj_AI_Turret) && i.CountEnemysInRange((int)R.Range - 20) >= ItemSlider(Mode, "RAbove")).OrderBy(i => i.CountEnemysInRange((int)R.Range));
                if (ItemBool(Mode, "R") && R.IsReady() && ItemList(Mode, "RMode") == 1 && nearObj.Count() > 0)
                {
                    foreach (var Obj in nearObj)
                    {
                        var QPred = Q.GetPrediction(Obj);
                        if (ItemBool("Misc", "SmiteCol") && QPred.CollisionObjects.Count == 1 && Q.MinHitChance == HitChance.High && CastSmite(QPred.CollisionObjects.First()))
                        {
                            Q.Cast(QPred.CastPosition, PacketCast());
                        }
                        else Q.CastIfHitchanceEquals(Obj, HitChance.High, PacketCast());
                    }
                }
                else if (Q.InRange(targetObj) && (CanKill(targetObj, Q) || !Orbwalk.InAutoAttackRange(targetObj)))
                {
                    var QPred = Q.GetPrediction(targetObj);
                    if (ItemBool("Misc", "SmiteCol") && QPred.CollisionObjects.Count == 1 && Q.MinHitChance == HitChance.High && CastSmite(QPred.CollisionObjects.First()))
                    {
                        Q.Cast(QPred.CastPosition, PacketCast());
                    }
                    else Q.CastIfHitchanceEquals(targetObj, HitChance.High, PacketCast());
                }
            }
            if (ItemBool(Mode, "W") && W.IsReady())
            {
                if (Player.ManaPercentage() >= ItemSlider(Mode, "WAbove"))
                {
                    if (Player.Distance3D(targetObj) <= W.Range + 60)
                    {
                        if (!Player.HasBuff("AuraofDespair")) W.Cast(PacketCast());
                    }
                    else if (Player.HasBuff("AuraofDespair")) W.Cast(PacketCast());
                }
                else if (Player.HasBuff("AuraofDespair")) W.Cast(PacketCast());
            }
            if (ItemBool(Mode, "E") && E.CanCast(targetObj)) E.Cast(PacketCast());
            if (Mode == "Combo" && ItemBool(Mode, "R") && R.IsReady())
            {
                switch (ItemList(Mode, "RMode"))
                {
                    case 0:
                        if (R.InRange(targetObj) && CanKill(targetObj, R)) R.Cast(PacketCast());
                        break;
                    case 1:
                        var Obj = ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsValidTarget(R.Range));
                        if (Obj.Count() > 0 && (Obj.Count() >= ItemSlider(Mode, "RAbove") || (Obj.Count() >= 2 && Obj.Count(i => CanKill(i, R)) >= 1))) R.Cast(PacketCast());
                        break;
                }
            }
            if (Mode == "Combo" && ItemBool(Mode, "Item") && Items.CanUseItem(Randuin) && Player.CountEnemysInRange(450) >= 1) Items.UseItem(Randuin);
            if (Mode == "Combo" && ItemBool(Mode, "Ignite") && IgniteReady()) CastIgnite(targetObj);
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            if (minionObj.Count() == 0 && ItemBool("Clear", "W") && W.IsReady() && Player.HasBuff("AuraofDespair")) W.Cast(PacketCast());
            foreach (var Obj in minionObj)
            {
                if (SmiteReady() && Obj.Team == GameObjectTeam.Neutral)
                {
                    if ((ItemBool("SmiteMob", "Baron") && Obj.Name.StartsWith("SRU_Baron")) || (ItemBool("SmiteMob", "Dragon") && Obj.Name.StartsWith("SRU_Dragon")) || (!Obj.Name.Contains("Mini") && (
                        (ItemBool("SmiteMob", "Red") && Obj.Name.StartsWith("SRU_Red")) || (ItemBool("SmiteMob", "Blue") && Obj.Name.StartsWith("SRU_Blue")) ||
                        (ItemBool("SmiteMob", "Krug") && Obj.Name.StartsWith("SRU_Krug")) || (ItemBool("SmiteMob", "Gromp") && Obj.Name.StartsWith("SRU_Gromp")) ||
                        (ItemBool("SmiteMob", "Raptor") && Obj.Name.StartsWith("SRU_Razorbeak")) || (ItemBool("SmiteMob", "Wolf") && Obj.Name.StartsWith("SRU_Murkwolf"))))) CastSmite(Obj);
                }
                if (ItemBool("Clear", "E") && E.CanCast(Obj)) E.Cast(PacketCast());
                if (ItemBool("Clear", "W") && W.IsReady())
                {
                    if (Player.ManaPercentage() >= ItemSlider("Clear", "WAbove"))
                    {
                        if (minionObj.Count(i => Player.Distance3D(i) <= W.Range + 60) >= 2 || (Obj.MaxHealth >= 1200 && Player.Distance3D(Obj) <= W.Range + 60))
                        {
                            if (!Player.HasBuff("AuraofDespair")) W.Cast(PacketCast());
                        }
                        else if (Player.HasBuff("AuraofDespair")) W.Cast(PacketCast());
                    }
                    else if (Player.HasBuff("AuraofDespair")) W.Cast(PacketCast());
                }
                if (ItemBool("Clear", "Q") && Q.IsReady() && (CanKill(Obj, Q) || !Orbwalk.InAutoAttackRange(Obj))) Q.CastIfHitchanceEquals(Obj, HitChance.Medium, PacketCast());
            }
        }
    }
}