using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterSeries.Common.M_Orbwalker;

namespace MasterSeries.Champion
{
    class Talon : Program
    {
        public Talon()
        {
            Q = new Spell(SpellSlot.Q, 300);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 600);
            W.SetSkillshot(0.5f, 0, 902, false, SkillshotType.SkillshotCone);
            E.SetTargetted(0.5f, float.MaxValue);
            R.SetSkillshot(0.5f, 0, 902, false, SkillshotType.SkillshotCircle);

            var ChampMenu = new Menu("插件", Name + "Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    ItemBool(ComboMenu, "Q", "使用 Q");
                    ItemBool(ComboMenu, "W", "使用 W");
                    ItemBool(ComboMenu, "E", "使用 E");
                    ItemBool(ComboMenu, "R", "使用 R");
                    ItemList(ComboMenu, "RMode", "-> 模式", new[] { "能击杀", "# 敌人" });
                    ItemSlider(ComboMenu, "RAbove", "--> 如果敌人超过", 2, 1, 4);
                    ItemBool(ComboMenu, "Item", "使用 物品");
                    ItemBool(ComboMenu, "Ignite", "如果能击杀自动点燃");
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    ItemBool(HarassMenu, "Q", "使用 Q");
                    ItemBool(HarassMenu, "W", "使用 W");
                    ItemBool(HarassMenu, "E", "使用 E");
                    ItemSlider(HarassMenu, "EAbove", "-> 如果血量低于", 20);
                    ItemActive(HarassMenu, "SmallW", "小型骚扰使用W", "H");
                    ChampMenu.AddSubMenu(HarassMenu);
                }
                var ClearMenu = new Menu("清线/清野", "Clear");
                {
                    ItemBool(ClearMenu, "Q", "使用 Q");
                    ItemBool(ClearMenu, "W", "使用 W");
                    ItemBool(ClearMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var MiscMenu = new Menu("额外选项", "Misc");
                {
                    ItemBool(MiscMenu, "WKillSteal", "使用W抢人头");
                    ItemSlider(MiscMenu, "CustomSkin", "失效-换肤", 3, 0, 3).ValueChanged += SkinChanger;
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示范围", "Draw");
                {
                    ItemBool(DrawMenu, "W", "W 范围", false);
                    ItemBool(DrawMenu, "E", "E 范围", false);
                    ItemBool(DrawMenu, "R", "R 范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                Config.AddSubMenu(ChampMenu);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsChannelingImportantSpell() || Player.IsRecalling()) return;
            switch (Orbwalk.CurrentMode)
            {
                case Orbwalk.Mode.Combo:
                    NormalCombo();
                    break;
                case Orbwalk.Mode.Harass:
                    Harass();
                    break;
                case Orbwalk.Mode.LaneClear:
                    //LaneJungClear();
                    break;
                case Orbwalk.Mode.Flee:
                    //WardJump(Game.CursorPos);
                    break;
            }
            if (ItemActive("Harass", "SmallW")) SmallHarass();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (ItemBool("Draw", "W") && W.Level > 0) Utility.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "E") && E.Level > 0) Utility.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "R") && R.Level > 0) Utility.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }

        private void NormalCombo()
        {
            if (targetObj == null) return;
            if (ItemBool("Combo", "E") && E.CanCast(targetObj)) E.CastOnUnit(targetObj, PacketCast());
            if (ItemBool("Combo", "R") && R.CanCast(targetObj)) R.Cast(PacketCast());
            if (ItemBool("Combo", "W") && W.CanCast(targetObj)) W.CastIfHitchanceEquals(targetObj, HitChance.VeryHigh, PacketCast());
            if (ItemBool("Combo", "Item")) UseItem(targetObj);
            if (ItemBool("Combo", "Ignite") && IgniteReady()) CastIgnite(targetObj);
        }

        private void Harass()
        {
            if (targetObj == null) return;
        }

        private void SmallHarass()
        {
            if (targetObj == null || !W.CanCast(targetObj)) return;
            W.CastIfHitchanceEquals(targetObj, HitChance.VeryHigh, PacketCast());
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