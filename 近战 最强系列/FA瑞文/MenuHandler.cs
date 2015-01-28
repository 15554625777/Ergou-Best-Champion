using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using SharpDX;

namespace FuckingAwesomeRiven
{
    class MenuHandler
    {

        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;

        public static void initMenu()
        {
            Config = new Menu("花边汉化-FA瑞文", "KappaChino", true);

            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("走 砍", "OW")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("目标 选择", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("连 招", "Combo"));
            combo.AddItem(new MenuItem("xdxdxdxd", "正常 连招!"));

            var enabledCombos = combo.AddSubMenu(new Menu("击杀 连招", "Killable Combos"));
            enabledCombos.AddItem(new MenuItem("dfsjknfsj", "有R 连招方式"));
            enabledCombos.AddItem(new MenuItem("QWR2KS", "Q - W - R2").SetValue(true));
            enabledCombos.AddItem(new MenuItem("QR2KS", "Q - R2").SetValue(true));
            enabledCombos.AddItem(new MenuItem("WR2KS", "W - R2").SetValue(true));
            enabledCombos.AddItem(new MenuItem("dfdgdgdfggfdsj", "击杀连招设置"));
            enabledCombos.AddItem(new MenuItem("dfgdhnfdsf", "没有R 连招方式"));
            enabledCombos.AddItem(new MenuItem("QWKS", "Q - W").SetValue(true));
            enabledCombos.AddItem(new MenuItem("QKS", "Q").SetValue(true));
            enabledCombos.AddItem(new MenuItem("WKS", "W").SetValue(true));

            combo.AddItem(new MenuItem("CQ", "使用 Q").SetValue(true));
            combo.AddItem(new MenuItem("UseQ-GC", "使用Q丨突进").SetValue(true));
            combo.AddItem(new MenuItem("Use R2", "使用 R2").SetValue(true));
            combo.AddItem(new MenuItem("CW", "使用 W").SetValue(true));
            combo.AddItem(new MenuItem("CE", "使用 E").SetValue(true));
            combo.AddItem(new MenuItem("UseE-AA", "假如敌人离开攻击范围丨仅使用E").SetValue(true));
            combo.AddItem(new MenuItem("UseE-GC", "使用E丨突进").SetValue(true));
            combo.AddItem(new MenuItem("CR", "使用 R").SetValue(true));
            combo.AddItem(new MenuItem("CR2", "使用 第二段R").SetValue(true));
            combo.AddItem(new MenuItem("magnet", "黏住 目标").SetValue(false));
            combo.AddItem(new MenuItem("bdsfdsf", "-- 爆发 连招"));
            combo.AddItem(new MenuItem("BFl", "使用闪现 连招").SetValue(false));
			combo.AddItem(new MenuItem("bdsfds1f", "有闪现时:E-R-闪现-W-Q"));
            combo.AddItem(new MenuItem("bdsfdsfddd", "无闪现:E-R-W-Q"));

            var farm = Config.AddSubMenu(new Menu("打 钱", "Farming"));
            farm.AddItem(new MenuItem("fnjdsjkn", "补刀 设置"));
            farm.AddItem(new MenuItem("QLH", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("WLH", "使用 W").SetValue(true));
            farm.AddItem(new MenuItem("10010321223", "清野 设置"));
            farm.AddItem(new MenuItem("QJ", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("WJ", "使用 W").SetValue(true));
            farm.AddItem(new MenuItem("EJ", "使用 E").SetValue(true));
            farm.AddItem(new MenuItem("5622546001", "清线 设置"));
            farm.AddItem(new MenuItem("QWC", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("QWC-LH", "Q 补刀").SetValue(true));
            farm.AddItem(new MenuItem("QWC-AA", "Q -> 平A").SetValue(true));
            farm.AddItem(new MenuItem("WWC", "使用 W").SetValue(true));


            var draw = Config.AddSubMenu(new Menu("范 围", "Draw"));

            draw.AddItem(new MenuItem("DQ", "Q 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DW", "W 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DE", "E 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DR", "R 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DD", "显示 伤害[不久]").SetValue(new Circle(false, System.Drawing.Color.White)));

            var misc = Config.AddSubMenu(new Menu("杂 项", "Misc"));
            misc.AddItem(new MenuItem("keepQAlive", "保持 Q 激活").SetValue(true));
            misc.AddItem(new MenuItem("QFlee", "Q 逃跑").SetValue(true));
            misc.AddItem(new MenuItem("EFlee", "E 逃跑").SetValue(true));

            var Keybindings = Config.AddSubMenu(new Menu("按键 设置", "KB"));
            Keybindings.AddItem(new MenuItem("normalCombo", "正常连招 按键").SetValue(new KeyBind(32, KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("burstCombo", "爆发连招 按键").SetValue(new KeyBind('M', KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("jungleCombo", "清野 按键").SetValue(new KeyBind('C', KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("waveClear", "清线 按键").SetValue(new KeyBind('C', KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("lastHit", "补刀 按键").SetValue(new KeyBind('X', KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("flee", "逃跑 按键").SetValue(new KeyBind('Z', KeyBindType.Press)));

            var Info = Config.AddSubMenu(new Menu("信 息", "info"));
            Info.AddItem(new MenuItem("Msddsds", "假如你喜欢FA瑞文可以利用 paypal 捐赠"));
            Info.AddItem(new MenuItem("Msdsddsd", "你可以把钱转到这个帐号:"));
            Info.AddItem(new MenuItem("Msdsadfdsd", "jayyeditsdude@gmail.com"));
            Info.AddItem(new MenuItem("debug", "开启 调试")).SetValue(false);
			
            Config.AddItem(new MenuItem("Mgdgdfgsd", "版本: 0.0.2 测试"));
            Config.AddItem(new MenuItem("Msd", "作者 By FluxySenpai"));
            Config.AddItem(new MenuItem("M1sd", "花边汉化-FA瑞文!"));
            Config.AddToMainMenu();
        }

        public static bool getMenuBool(String s)
        {
            return Config.Item(s).GetValue<bool>();
        }
    }
}
