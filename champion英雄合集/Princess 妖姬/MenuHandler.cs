using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Princess_LeBlanc
{
    class MenuHandler
    {
        public static Menu LeBlancConfig;
        internal static Orbwalking.Orbwalker Orb;
        public static void Init()
        {
        LeBlancConfig = new Menu(ObjectManager.Player.ChampionName, ObjectManager.Player.ChampionName, true);

        Menu orbwalker = new Menu("官方集成 走砍", "orbwalker");

        Orb = new Orbwalking.Orbwalker(orbwalker);
        LeBlancConfig.AddSubMenu(orbwalker);

        var targetselectormenu = new Menu("官方集成 目标选择器", "Common_TargetSelector");
        TargetSelector.AddToMenu(targetselectormenu);
        LeBlancConfig.AddSubMenu(targetselectormenu);

        LeBlancConfig.AddSubMenu(new Menu("连招", "Combo"));
        LeBlancConfig.SubMenu("Combo").AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
        LeBlancConfig.SubMenu("Combo").AddItem(new MenuItem("useW", "使用 W").SetValue(true));
        LeBlancConfig.SubMenu("Combo").AddItem(new MenuItem("useE", "使用 E").SetValue(true));
        LeBlancConfig.SubMenu("Combo").AddItem(new MenuItem("useR", "使用 R").SetValue(true));

        LeBlancConfig.AddSubMenu(new Menu("清线技能释放有问题", "ClearL"));
        LeBlancConfig.SubMenu("ClearL").AddItem(new MenuItem("LaneClearQ", "使用 Q").SetValue(true));
        LeBlancConfig.SubMenu("ClearL").AddItem(new MenuItem("LaneClearW", "使用 W").SetValue(true));
        LeBlancConfig.SubMenu("ClearL").AddItem(new MenuItem("LaneClearManaPercent", "控蓝百分比").SetValue(new Slider(30, 0, 100)));

        LeBlancConfig.AddSubMenu(new Menu("清野", "ClearJ"));
        LeBlancConfig.SubMenu("ClearJ").AddItem(new MenuItem("JungleClearQ", "使用 Q").SetValue(true));
        LeBlancConfig.SubMenu("ClearJ").AddItem(new MenuItem("JungleClearW", "使用 W").SetValue(true));
        LeBlancConfig.SubMenu("ClearJ").AddItem(new MenuItem("JungleClearManaPercent", "控蓝百分比").SetValue(new Slider(30, 0, 100)));

        LeBlancConfig.AddSubMenu(new Menu("逃跑", "Flee"));
            LeBlancConfig.SubMenu("Flee")
                .AddItem(new MenuItem("FleeK", "键位").SetValue(new KeyBind('A', KeyBindType.Press)));
        LeBlancConfig.SubMenu("Flee").AddItem(new MenuItem("FleeW", "使用 W").SetValue(true));
        LeBlancConfig.SubMenu("Flee").AddItem(new MenuItem("FleeR", "使用 R").SetValue(true));

        LeBlancConfig.AddSubMenu(new Menu("Items", "Items"));
        LeBlancConfig.SubMenu("Items").AddItem(new MenuItem("useDfg", "使用 冥火").SetValue(true));

        MenuItem dmgAfterComboItem = new MenuItem("DamageAfterCombo", "连招后显示伤害").SetValue(true);
        Utility.HpBarDamageIndicator.DamageToUnit = MathHandler.ComboDamage;
        Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
        dmgAfterComboItem.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

        LeBlancConfig.AddSubMenu(new Menu("显示范围", "Drawing"));
        LeBlancConfig.SubMenu("Drawing").AddItem(new MenuItem("HUD", "动态显示").SetValue(true));
        LeBlancConfig.SubMenu("Drawing").AddItem(dmgAfterComboItem);
        LeBlancConfig.SubMenu("Drawing").AddItem(new MenuItem("drawQ", "显示 Q").SetValue(new Circle(true, Color.FromArgb(100, Color.Aqua))));
        LeBlancConfig.SubMenu("Drawing").AddItem(new MenuItem("drawW", "显示 W").SetValue(new Circle(true, Color.FromArgb(100, Color.Aqua))));
        LeBlancConfig.SubMenu("Drawing").AddItem(new MenuItem("drawE", "显示 E").SetValue(new Circle(true, Color.FromArgb(100, Color.Aqua))));

        LeBlancConfig.AddSubMenu(new Menu("额外选项", "Misc"));
        LeBlancConfig.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "使用E打断突进者").SetValue(true));
        LeBlancConfig.SubMenu("Misc").AddItem(new MenuItem("Gapclose", "突进者使用E中断").SetValue(true));
        LeBlancConfig.SubMenu("Misc").AddItem(new MenuItem("backW", "使用W返回逻辑").SetValue(true));
        LeBlancConfig.SubMenu("Misc")
             .AddItem(
                 new MenuItem("Clone", "被动逻辑").SetValue(
                     new StringList(new[] { "绝不", "自动朝敌人", "自动朝释放者", "自动随机定位", "自动朝鼠标方向" })));

        LeBlancConfig.AddItem(new MenuItem("madebyme", "PrincessLeia :)[By: Feeeez]").DontSave());
        LeBlancConfig.AddToMainMenu();
    }

    }
}
