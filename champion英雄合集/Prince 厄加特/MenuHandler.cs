using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Prince_Urgot
{
    internal class MenuHandler
    {
        public static Menu _uMenu;
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static readonly StringList HitChanceList = new StringList(new[] { "低", "中", "高", "非常高" });
        internal static Orbwalking.Orbwalker Orb;
        public static void Init()
        {
            _uMenu = new Menu("Prince " + Player.ChampionName, Player.ChampionName, true);

            Menu orbwalker = new Menu("走砍", "orbwalker");
            Orb = new Orbwalking.Orbwalker(orbwalker);
            _uMenu.AddSubMenu(orbwalker);

            Menu ts = _uMenu.AddSubMenu(new Menu("目標選擇", "Target Selector")); ;

            TargetSelector.AddToMenu(ts);

            Menu comboMenu = _uMenu.AddSubMenu(new Menu("連招", "Combo"));
            comboMenu.AddItem(new MenuItem("ComboQ", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("ComboW", "使用 W").SetValue(true));
            comboMenu.AddItem(new MenuItem("ComboE", "使用 E").SetValue(true));

            Menu laneClearMenu = _uMenu.AddSubMenu(new Menu("清線", "LaneClear"));
            laneClearMenu.AddItem(new MenuItem("LaneClearQ", "使用 Q").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("LaneClearE", "使用 E").SetValue(false));
            laneClearMenu.AddItem(new MenuItem("LaneClearQManaPercent", "最低Q藍量百分比").SetValue(new Slider(30, 0, 100)));

            Menu lastHitMenu = _uMenu.AddSubMenu(new Menu("補尾刀", "LastHit"));
            lastHitMenu.AddItem(new MenuItem("lastHitQ", "使用 Q").SetValue(true));
            lastHitMenu.AddItem(new MenuItem("lastHitQManaPercent", "最低藍量Q百分比").SetValue(new Slider(30, 0, 100)));

            Menu harassMenu = _uMenu.AddSubMenu(new Menu("騷擾", "Harass"));
            harassMenu.AddItem(new MenuItem("HarassActive", "騷擾1").SetValue(new KeyBind('C', KeyBindType.Press)));
            harassMenu.AddItem(new MenuItem("HarassToggle", "騷擾2").SetValue(new KeyBind('T', KeyBindType.Toggle)));
            harassMenu.AddItem(new MenuItem("haraQ", "需要EBUFF支持Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("haraE", "自動EBUFF").SetValue(true));
            harassMenu.AddItem(new MenuItem("HaraManaPercent", "最低藍量百分比").SetValue(new Slider(30, 0, 100)));

            Menu itemsMenu = _uMenu.AddSubMenu(new Menu("物品", "Items"));
            itemsMenu.AddItem(new MenuItem("useMura", "自動啟動藍量控制").SetValue(new KeyBind('A', KeyBindType.Toggle)));

            Menu preMenu = _uMenu.AddSubMenu(new Menu("預判設置", "Prediction"));
            preMenu.AddItem(new MenuItem("preE", "命中幾率 E").SetValue(HitChanceList));
            preMenu.AddItem(new MenuItem("preQ", "命中幾率 Q").SetValue(HitChanceList));

            Menu drawMenu = _uMenu.AddSubMenu(new Menu("顯示範圍", "Drawing"));
            drawMenu.AddItem(new MenuItem("drawQ", "顯示 Q").SetValue(new Circle(true, Color.FromArgb(100, Color.Aqua))));
            drawMenu.AddItem(new MenuItem("drawEe", "顯示 E").SetValue(new Circle(true, Color.FromArgb(100, Color.Aqua))));
            drawMenu.AddItem(new MenuItem("drawE", "顯示E擊殺後Q範圍").SetValue(new Circle(true, Color.FromArgb(100, Color.Aqua))));
            drawMenu.AddItem(new MenuItem("HUD", "平视显示器").SetValue(true));
            drawMenu.AddItem(new MenuItem("hitbye", "顯示擊中敵人圓圈使用E").SetValue(true));
            drawMenu.AddItem(new MenuItem("drawR", "顯示 R").SetValue(new Circle(true, Color.FromArgb(100, Color.Red))));

            Menu miscMenu = _uMenu.AddSubMenu(new Menu("額外選項", "Misc"));
            miscMenu.AddItem(new MenuItem("autoR", "[測試]如果敵人在塔範圍使用R換位").SetValue(false));
            miscMenu.AddItem(new MenuItem("autoInt", "自動R打斷目標施法").SetValue(false));
            miscMenu.AddItem(new MenuItem("KillQ", "搶人頭使用Q").SetValue(true));
            miscMenu.AddItem(new MenuItem("KillI", "搶人頭使用點燃").SetValue(true));

            _uMenu.AddItem(new MenuItem("Packet", "釋放封包模式").SetValue(true));

            _uMenu.AddItem(new MenuItem("madebyme", "PrincessLeia :)").DontSave());

            _uMenu.AddToMainMenu();
    }

    }
}
