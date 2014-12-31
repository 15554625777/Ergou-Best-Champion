#region

using LeagueSharp.Common;

#endregion

namespace Yasuo
{
    internal class YasuoMenu
    {
        #region Menu

        public YasuoMenu()
        {
            /* HEADER */
            _menu = new Menu("WorstPing | 亚索", RootName, true);
            TargetSelector.AddToMenu(_menu.AddSubMenu(new Menu("目标 选择", RootName + "_ts")));
            _orbwalker = new Orbwalking.Orbwalker(_menu.AddSubMenu(new Menu("Orbwalker", RootName + "_orb")));

            _menu.AddSubMenu(new Menu("", RootName + ".spacer0"));

            /* COMBO */
            var combo = _menu.AddSubMenu(new Menu("连招 设置", RootName + ".combo"));
            combo.AddItem(new MenuItem(RootName + ComboQ, "使用 斩钢闪 (Q)")).SetValue(true);
            combo.AddItem(new MenuItem(RootName + ComboQWind, "使用 斩钢闪 (Q3)")).SetValue(true);
            combo.AddItem(new MenuItem(RootName + ComboE, "使用 踏前斩 (E)")).SetValue(true);
            combo.AddItem(new MenuItem(RootName + ".combo.spacer0", ""));
            combo.AddItem(new MenuItem(RootName + ComboR, "使用 狂风绝息斩 (R)")).SetValue(true);
            combo.AddItem(new MenuItem(RootName + ComboRMin, "使用大招|敌人数量")).SetValue(new Slider(1, 1, 5));
            combo.AddItem(new MenuItem(RootName + ComboRPer, "使用大招|敌人血量")).SetValue(new Slider(40, 5));

            /* FARMING */
            var farming = _menu.AddSubMenu(new Menu("清线 设置", RootName + ".farming"));
            farming.AddItem(new MenuItem(RootName + FarmingLaneClearQ, "[清线] 使用 斩钢闪 (Q)"))
                .SetValue(true);
            farming.AddItem(new MenuItem(RootName + FarmingLaneClearE, "[清线] 使用 踏前斩 (E)"))
                .SetValue(true);
            farming.AddItem(new MenuItem(RootName + ".farming.spacer0", ""));
            farming.AddItem(new MenuItem(RootName + FarmingLastHitQ, "[补刀] 使用 斩钢闪 (Q)")).SetValue(true);
            farming.AddItem(new MenuItem(RootName + FarmingLastHitE, "[补刀] 使用 踏前斩 (E)")).SetValue(true);
            farming.AddItem(new MenuItem(RootName + ".farming.spacer1", ""));
            farming.AddItem(new MenuItem(RootName + FarmingUseWind, "[同时] 使用 斩钢闪 (Q3)")).SetValue(true);

            /* FLEE */
            var flee = _menu.AddSubMenu(new Menu("逃跑 设置", RootName + ".flee"));
            flee.AddItem(new MenuItem(RootName + FleeMode, "启用 逃跑模式")).SetValue(true);
            flee.AddItem(new MenuItem(RootName + FleeModeKey, "逃跑 键位"))
                .SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press));
            flee.AddItem(new MenuItem(RootName + ".flee.spacer0", ""));
            flee.AddItem(new MenuItem(RootName + FleeE, "使用 踏前斩 (E)")).SetValue(true);
            flee.AddItem(new MenuItem(RootName + FleeW, "使用 风之障壁 (W)")).SetValue(true);

            /* EVADE */
            var evade = _menu.AddSubMenu(new Menu("躲避 设置", RootName + ".evade"));

            /* INTERRUPT */
            var interrupt = _menu.AddSubMenu(new Menu("阻挡 设置", RootName + ".interrupter"));
            interrupt.AddItem(new MenuItem(RootName + InterruptW, "自动 风之障壁 (W)")).SetValue(true);
            interrupt.AddItem(new MenuItem(RootName + InterruptWDelay, "自动 风之障壁 延迟"))
                .SetValue(new Slider(0, 0, 5));
            interrupt.AddItem(new MenuItem(RootName + ".interrupter.space0", ""));
            interrupt.AddItem(new MenuItem(RootName + InterruptActive, "中断 法术")).SetValue(true);

            /* MISC */
            var misc = _menu.AddSubMenu(new Menu("杂项 设置", RootName + ".misc"));
            misc.AddItem(new MenuItem(RootName + MiscAutoQ, "自动 Q")).SetValue(true);
            misc.AddItem(new MenuItem(RootName + MiscAutoQUnderTower, "塔下 禁用 Q")).SetValue(true);
            misc.AddItem(new MenuItem(RootName + MiscQRange, "Q 范围")).SetValue(new Slider(475, 0, 475));
            misc.AddItem(new MenuItem(RootName + ".misc.spacer0", ""));
            misc.AddItem(new MenuItem(RootName + MiscAutoR, "自动 狂风绝息斩 (R)")).SetValue(true);
            misc.AddItem(new MenuItem(RootName + MiscRMin, "自动(R)|敌人数量")).SetValue(new Slider(1, 1, 5));
            misc.AddItem(new MenuItem(RootName + ".misc.spacer1", ""));
            misc.AddItem(new MenuItem(RootName + MiscHitChance, "命中率")).SetValue(new StringList(new[] { "低", "正常", "高", "很高" }, 1));
            misc.AddItem(new MenuItem(RootName + MiscPackets, "使用 封包")).SetValue(true);

            /* FOOTER */
            _menu.AddItem(new MenuItem(RootName + "_spacer1", ""));
            _menu.AddItem(new MenuItem(RootName + "_spacer_desc", "亚索 - 疾风剑豪 "));
            _menu.AddToMainMenu();
        }

        #endregion

        #region Fields

        private const string RootName = "worstping_yasuo";

        public const string ComboQ = ".combo.useq";
        public const string ComboQWind = ".combo.useqwind";
        public const string ComboE = ".combo.usee";
        public const string ComboR = ".combo.user";
        public const string ComboRMin = ".combo.usermin";
        public const string ComboRPer = ".combo.userper";

        public const string FarmingLaneClearQ = ".farming.lcq";
        public const string FarmingLaneClearE = ".farming.lce";
        public const string FarmingLastHitQ = ".farming.lhq";
        public const string FarmingLastHitE = ".farming.lhe";
        public const string FarmingUseWind = ".farming.useq3";

        public const string FleeMode = ".flee.active";
        public const string FleeModeKey = ".flee.key";
        public const string FleeE = ".flee.usee";
        public const string FleeW = ".flee.usew";

        public const string InterruptActive = ".interrupt.active";
        public const string InterruptW = ".interrupt.usew";
        public const string InterruptWDelay = ".interrupt.usewdelay";
        public const string InterruptAceInTheHole = ".interrupt.caitlyn.aceinthehole";
        public const string InterruptCrowstorm = ".interrupt.fiddlesticks.crowstorm";
        public const string InterruptDrainChannel = ".interrupt.fiddlesticks.drawinchannel";
        public const string InterruptIdolOfDurand = ".interrupt.galio.idolofdurand";
        public const string InterruptFallenOne = ".interrupt.karthus.falleone";
        public const string InterruptKatarinaR = ".interrupt.katarina.r";
        public const string InterruptNetherGrasp = ".interrupt.malzahar.nethergrasp";
        public const string InterruptBulletTime = ".interrupt.missfortune.bullettime";
        public const string InterruptAbsoluteZero = ".interrupt.nunu.absolutezero";
        public const string InterruptGrandSkyfall = ".interrupt.pantheon.grandskyfall";
        public const string InterruptStandUnited = ".interrupt.shen.standunited";
        public const string InterruptUrgotSwap = ".interrupt.urgot.swap";
        public const string InterruptVarusQ = ".interrupt.varus.q";
        public const string InterruptIfiniteDuress = ".interrupt.warwick.infiniteduress";

        public const string MiscAutoQ = ".misc.autoq";
        public const string MiscAutoQUnderTower = ".misc.autoqundertower";
        public const string MiscQRange = ".misc.qrange";
        public const string MiscAutoR = ".misc.autor";
        public const string MiscRMin = ".misc.autormin";
        public const string MiscHitChance = ".misc.hitchance";
        public const string MiscPackets = ".misc.packets";

        private readonly Menu _menu;
        private readonly Orbwalking.Orbwalker _orbwalker;

        #endregion

        #region Functions

        public Menu GetMenu()
        {
            return _menu;
        }

        public Orbwalking.Orbwalker GetOrbwalker()
        {
            return _orbwalker;
        }

        public T GetValue<T>(string str)
        {
            return _menu.Item(RootName + str).GetValue<T>();
        }

        public MenuItem GetItem(string str)
        {
            return _menu.Item(RootName + str);
        }

        #endregion
    }
}