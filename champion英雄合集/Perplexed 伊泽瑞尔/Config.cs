using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace PerplexedEzreal
{
    class Config
    {
        public static Menu Settings = new Menu("Perplexed Ezreal", "menu", true);
        public static Orbwalking.Orbwalker Orbwalker;

        public static void Initialize()
        {
            //Orbwalker
            Settings.AddSubMenu(new Menu("走砍", "orbMenu"));
            Orbwalker = new Orbwalking.Orbwalker(Settings.SubMenu("orbMenu"));
            //Target Selector
            Settings.AddSubMenu(new Menu("目标 选择", "ts"));
            TargetSelector.AddToMenu(Settings.SubMenu("ts"));
            //Combo
            Settings.AddSubMenu(new Menu("连招", "menuCombo"));
            Settings.SubMenu("menuCombo").AddItem(new MenuItem("comboQ", "使用 Q").SetValue(true));
            Settings.SubMenu("menuCombo").AddItem(new MenuItem("comboW", "使用 W").SetValue(true));
            //Harass
            Settings.AddSubMenu(new Menu("骚扰", "menuHarass"));
            Settings.SubMenu("menuHarass").AddItem(new MenuItem( "harassQ", "使用 Q").SetValue(true));
            Settings.SubMenu("menuHarass").AddItem(new MenuItem("harassW", "使用 W").SetValue(true));
            //Last Hit
            Settings.AddSubMenu(new Menu("补刀", "menuLastHit"));
            Settings.SubMenu("menuLastHit").AddItem(new MenuItem("lastHitQ", "使用 Q").SetValue(true));
             //Auto
            Settings.AddSubMenu(new Menu("自动", "menuAuto"));
            Settings.SubMenu("menuAuto").AddItem(new MenuItem("toggleAuto", "自动选择目标").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
            Settings.SubMenu("menuAuto").AddSubMenu(new Menu("自动设置", "autoSettings"));
            foreach(Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValid && hero.IsEnemy))
                Settings.SubMenu("menuAuto").SubMenu("autoSettings").AddItem(new MenuItem("auto" + hero.ChampionName, hero.ChampionName).SetValue(true));
            Settings.SubMenu("menuAuto").AddItem(new MenuItem("autoQ", "自动 Q").SetValue(true));
            Settings.SubMenu("menuAuto").AddItem(new MenuItem("autoW", "自动 W").SetValue<bool>(false));
            Settings.SubMenu("menuAuto").AddItem(new MenuItem("manaER", "存储法力值使用 E/R").SetValue(true));
            Settings.SubMenu("menuAuto").AddItem(new MenuItem("autoTurret", "自动攻击塔下敌人").SetValue<bool>(false));
            //Ultimate
            Settings.AddSubMenu(new Menu("大招设置", "menuUlt"));
            Settings.SubMenu("menuUlt").AddItem(new MenuItem("ultLowest", "手动大招（自动低血量目标）").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Settings.SubMenu("menuUlt").AddItem(new MenuItem("ks", "击杀使用R").SetValue(true));
            Settings.SubMenu("menuUlt").AddItem(new MenuItem("ultRange", "大招 使用 范围").SetValue<Slider>(new Slider(1000, 1000, 5000)));
            //Drawing
            Settings.AddSubMenu(new Menu("显示", "menuDrawing"));
            Settings.SubMenu("menuDrawing").AddSubMenu(new Menu("伤害指示器", "menuDamage"));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawAADmg", "显示自动攻击伤害").SetValue(true));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawQDmg", "显示 Q 伤害").SetValue(true));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawWDmg", "显示 W 伤害").SetValue(true));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawEDmg", "显示 E 伤害").SetValue(true));
            Settings.SubMenu("menuDrawing").SubMenu("menuDamage").AddItem(new MenuItem("drawRDmg", "显示 R 伤害").SetValue(true));
            Settings.SubMenu("menuDrawing").AddItem(new MenuItem("drawQ", "显示 Q 范围").SetValue(new Circle(true, Color.Yellow)));
            Settings.SubMenu("menuDrawing").AddItem(new MenuItem("drawW", "显示 W 范围").SetValue(new Circle(true, Color.Yellow)));
            Settings.SubMenu("menuDrawing").AddItem(new MenuItem("drawR", "显示 R 范围").SetValue(new Circle(true, Color.Yellow)));
            //Other
            Settings.AddItem(new MenuItem("dmgMode", "伤害 模式").SetValue(new StringList(new string[] { "AD", "AP" })));
            Settings.AddItem(new MenuItem("recallBlock", "阻止 回程").SetValue(true));
            Settings.AddItem(new MenuItem("usePackets", "使用 封包").SetValue(true));
            //Finish
            Settings.AddToMainMenu();
        }

        public static bool ComboQ { get { return Settings.Item("comboQ").GetValue<bool>(); } }
        public static bool ComboW { get { return Settings.Item("comboW").GetValue<bool>(); } }

        public static bool HarassQ { get { return Settings.Item("harassQ").GetValue<bool>(); } }
        public static bool HarassW { get { return Settings.Item("harassW").GetValue<bool>(); } }

        public static bool LastHitQ { get { return Settings.Item("lastHitQ").GetValue<bool>(); } }

        public static KeyBind UltLowest { get { return Settings.Item("ultLowest").GetValue<KeyBind>(); } }
        public static bool KillSteal { get { return Settings.Item("ks").GetValue<bool>(); } }
        public static int UltRange { get { return Settings.Item("ultRange").GetValue<Slider>().Value; } }

        public static KeyBind ToggleAuto { get { return Settings.Item("toggleAuto").GetValue<KeyBind>(); } }
        public static bool ShouldAuto(string championName)
        {
            return Settings.Item("auto" + championName).GetValue<bool>();
        }
        public static bool AutoQ { get { return Settings.Item("autoQ").GetValue<bool>(); } }
        public static bool AutoW { get { return Settings.Item("autoW").GetValue<bool>(); } }
        public static bool ManaER { get { return Settings.Item("manaER").GetValue<bool>(); } }
        public static bool AutoTurret { get { return Settings.Item("autoTurret").GetValue<bool>(); } }

        public static bool DrawAADmg { get { return Settings.Item("drawAADmg").GetValue<bool>(); } }
        public static bool DrawQDmg { get { return Settings.Item("drawQDmg").GetValue<bool>(); } }
        public static bool DrawWDmg { get { return Settings.Item("drawWDmg").GetValue<bool>(); } }
        public static bool DrawEDmg { get { return Settings.Item("drawEDmg").GetValue<bool>(); } }
        public static bool DrawRDmg { get { return Settings.Item("drawRDmg").GetValue<bool>(); } }
        public static bool DrawQ { get { return Settings.Item("drawQ").GetValue<Circle>().Active; } }
        public static bool DrawW { get { return Settings.Item("drawW").GetValue<Circle>().Active; } }
        public static bool DrawR { get { return Settings.Item("drawR").GetValue<Circle>().Active; } }

        public static string DamageMode { get { return Settings.Item("dmgMode").GetValue<StringList>().SelectedValue; } }
        public static bool RecallBlock { get { return Settings.Item("recallBlock").GetValue<bool>(); } }
        public static bool UsePackets { get { return Settings.Item("usePackets").GetValue<bool>(); } }
    }
}
