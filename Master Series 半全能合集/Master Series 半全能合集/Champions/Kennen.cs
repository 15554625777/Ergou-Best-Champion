using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterSeries.Common.M_Orbwalker;

namespace MasterSeries.Champion
{
    class Kennen : Program
    {
        public Kennen()
        {
            Q = new Spell(SpellSlot.Q, 1050);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 20);
            R = new Spell(SpellSlot.R, 550);
            Q.SetSkillshot(0.125f, 50, 1700, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.125f, 800, 1700, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.125f, 550, 1700, false, SkillshotType.SkillshotCircle);

            var ChampMenu = new Menu("插件", Name + "Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    ItemBool(ComboMenu, "Q", "使用 Q");
                    ItemBool(ComboMenu, "W", "使用 W");
                    ItemList(ComboMenu, "WMode", "-> 模式", new[] { "总是", "智能" });
                    ItemBool(ComboMenu, "R", "使用 R");
                    ItemList(ComboMenu, "RMode", "-> 模式", new[] { "总是", "# 敌人" });
                    ItemSlider(ComboMenu, "RAbove", "--> 如果敌人超过", 2, 1, 4);
                    ItemBool(ComboMenu, "Item", "使用 物品");
                    ItemBool(ComboMenu, "Ignite", "如果能击杀自动点燃");
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    ItemBool(HarassMenu, "Q", "使用 Q");
                    ItemBool(HarassMenu, "W", "使用 W");
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
                    ItemBool(MiscMenu, "QLastHit", "使用Q补刀");
                    ItemBool(MiscMenu, "QKillSteal", "使用Q抢人头");
                    ItemSlider(MiscMenu, "CustomSkin", "失效-换肤", 5, 0, 5).ValueChanged += SkinChanger;
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示范围", "Draw");
                {
                    ItemBool(DrawMenu, "Q", "Q 范围", false);
                    ItemBool(DrawMenu, "W", "W 范围", false);
                    ItemBool(DrawMenu, "R", "R 范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                Config.AddSubMenu(ChampMenu);
            }
        }
    }
}