using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace PrinceTalon
{
    class MenuHandler
    {
        public static Menu TalonConfig;
        internal static Orbwalking.Orbwalker Orb;
        public static void Init()
        {
        TalonConfig = new Menu(ObjectManager.Player.ChampionName, ObjectManager.Player.ChampionName, true);

        Menu orbwalker = new Menu("走砍", "orbwalker");

        Orb = new Orbwalking.Orbwalker(orbwalker);
        TalonConfig.AddSubMenu(orbwalker);

        var targetselectormenu = new Menu("目標選擇", "Common_TargetSelector");
        TargetSelector.AddToMenu(targetselectormenu);
        TalonConfig.AddSubMenu(targetselectormenu);

        TalonConfig.AddSubMenu(new Menu("連招", "Combo"));
        TalonConfig.SubMenu("Combo").AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
        TalonConfig.SubMenu("Combo").AddItem(new MenuItem("useW", "使用 W").SetValue(true));
        TalonConfig.SubMenu("Combo").AddItem(new MenuItem("useE", "使用 E").SetValue(true));
        TalonConfig.SubMenu("Combo").AddItem(new MenuItem("useR", "使用 R").SetValue(true));

        TalonConfig.AddSubMenu(new Menu("清線", "ClearL"));
        TalonConfig.SubMenu("ClearL").AddItem(new MenuItem("LaneClearQ", "使用 Q").SetValue(true));
        TalonConfig.SubMenu("ClearL").AddItem(new MenuItem("LaneClearW", "使用 W").SetValue(true));
        TalonConfig.SubMenu("ClearL").AddItem(new MenuItem("uselIt", "使用 Items").SetValue(true));
        TalonConfig.SubMenu("ClearL").AddItem(new MenuItem("LaneClearManaPercent", "最低藍量百分比").SetValue(new Slider(30, 0, 100)));

        TalonConfig.AddSubMenu(new Menu("清野", "ClearJ"));
        TalonConfig.SubMenu("ClearJ").AddItem(new MenuItem("usejQ", "使用 Q").SetValue(true));
        TalonConfig.SubMenu("ClearJ").AddItem(new MenuItem("usejW", "使用 W").SetValue(true));
        TalonConfig.SubMenu("ClearJ").AddItem(new MenuItem("usejIt", "使用 物品").SetValue(true));
        TalonConfig.SubMenu("ClearJ").AddItem(new MenuItem("JungleClearManaPercent", "最低藍量百分比").SetValue(new Slider(30, 0, 100)));

        TalonConfig.AddSubMenu(new Menu("不工作", "Harass"));
        TalonConfig.SubMenu("Harass").AddItem(new MenuItem("HarassToggle", "Auto Harass Toggle").SetValue(new KeyBind('T', KeyBindType.Toggle)));
        TalonConfig.SubMenu("Harass").AddItem(new MenuItem("haraW", "Use W").SetValue(true));
        TalonConfig.SubMenu("Harass").AddItem(new MenuItem("HarassManaPercent", "Minimum Mana Percent").SetValue(new Slider(30, 0, 100)));

        TalonConfig.AddSubMenu(new Menu("物品", "Items"));
        TalonConfig.SubMenu("Items").AddItem(new MenuItem("useBilge", "使用 小彎刀").SetValue(true));
        TalonConfig.SubMenu("Items").AddItem(new MenuItem("useBortK", "使用 破敗").SetValue(true));
        TalonConfig.SubMenu("Items").AddItem(new MenuItem("useYoumuu", "使用 幽夢").SetValue(true));
        TalonConfig.SubMenu("Items").AddItem(new MenuItem("useTiamat", "使用 提亞瑪特").SetValue(true));
        TalonConfig.SubMenu("Items").AddItem(new MenuItem("useHydra", "使用 九頭蛇").SetValue(true));

        TalonConfig.AddSubMenu(new Menu("KillSteal", "KS"));
        TalonConfig.SubMenu("KS").AddItem(new MenuItem("KSi", "使用 點燃").SetValue(true));
        TalonConfig.SubMenu("KS").AddItem(new MenuItem("KSq", "使用 Q").SetValue(true));
        TalonConfig.SubMenu("KS").AddItem(new MenuItem("KSw", "使用 W").SetValue(true));

        MenuItem dmgAfterComboItem = new MenuItem("DamageAfterCombo", "連招後顯示傷害").SetValue(true);
        Utility.HpBarDamageIndicator.DamageToUnit = MathHandler.ComboDamage;
        Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
        dmgAfterComboItem.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

        TalonConfig.AddSubMenu(new Menu("顯示範圍", "Drawing"));
        TalonConfig.SubMenu("Drawing").AddItem(new MenuItem("HUD", "動態顯示").SetValue(true));
        TalonConfig.SubMenu("Drawing").AddItem(dmgAfterComboItem);
        TalonConfig.SubMenu("Drawing").AddItem(new MenuItem("drawW", "顯示 W").SetValue(new Circle(true, Color.FromArgb(100, Color.Aqua))));
        TalonConfig.SubMenu("Drawing").AddItem(new MenuItem("drawE", "顯示 E").SetValue(new Circle(true, Color.FromArgb(100, Color.Aqua))));
        TalonConfig.SubMenu("Drawing").AddItem(new MenuItem("drawR", "顯示 R").SetValue(new Circle(true, Color.FromArgb(100, Color.Aqua))));

        TalonConfig.AddItem(new MenuItem("madebyme", "PrincessLeia :)").DontSave());
        TalonConfig.AddToMainMenu();
    }

    }
}
