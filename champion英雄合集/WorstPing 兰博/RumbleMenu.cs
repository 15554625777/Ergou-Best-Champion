using LeagueSharp.Common;

namespace Rumble
{
    internal class RumbleMenu
    {
        public RumbleMenu()
        {
            /* HEADER */
            _menu = new Menu("WorstPing | 兰博", RootName, true);
            TargetSelector.AddToMenu(_menu.AddSubMenu(new Menu("目标选择", RootName + "_ts")));
            _orbwalker = new Orbwalking.Orbwalker(_menu.AddSubMenu(new Menu("走砍", RootName + "_orb")));

            /* COMBO */
            var combo = _menu.AddSubMenu(new Menu("连招", RootName + "_combo"));
            combo.AddItem(new MenuItem(RootName + ComboQ, "使用 Q")).SetValue(true);
            combo.AddItem(new MenuItem(RootName + ComboW, "使用 W")).SetValue(true);
            combo.AddItem(new MenuItem(RootName + ComboE, "使用 E")).SetValue(true);
            combo.AddItem(new MenuItem(RootName + ComboR, "使用 R")).SetValue(false);
            combo.AddItem(new MenuItem(RootName + ComboRMin, "使用R最少敌人")).SetValue(new Slider(2, 1, 5));
            combo.AddItem(new MenuItem(RootName + ComboOverheat, "允许 过热")).SetValue(true);

            /* HARASS */
            var harass = _menu.AddSubMenu(new Menu("骚扰", RootName + "_harass"));
            harass.AddItem(new MenuItem(RootName + HarassQ, "使用 Q")).SetValue(true);
            harass.AddItem(new MenuItem(RootName + HarassW, "使用 W")).SetValue(true);
            harass.AddItem(new MenuItem(RootName + HarassE, "使用 E")).SetValue(true);
            harass.AddItem(new MenuItem(RootName + HarassOverheat, "允许 过热")).SetValue(true);

            /* FARMING */
            var farm = _menu.AddSubMenu(new Menu("清线", RootName + "_farm"));
            farm.AddItem(new MenuItem(RootName + FarmQ, "使用 Q")).SetValue(true);
            farm.AddItem(new MenuItem(RootName + FarmMinQ, "使用Q最低敌人")).SetValue(new Slider(3, 1, 8));
            farm.AddItem(new MenuItem(RootName + FarmE, "使用 E")).SetValue(true);
            farm.AddItem(new MenuItem(RootName + FarmOverheat, "允许 过热")).SetValue(true);

            /* DRAWING */
            var drawing = _menu.AddSubMenu(new Menu("范围", RootName + "_drawing"));
            drawing.AddItem(new MenuItem(RootName + DrawQ, "显示 Q 范围")).SetValue(true);
            drawing.AddItem(new MenuItem(RootName + DrawE, "显示 E 范围")).SetValue(true);
            drawing.AddItem(new MenuItem(RootName + DrawR, "显示 R 范围")).SetValue(true);
            drawing.AddItem(new MenuItem(RootName + DrawKillText, "显示 击杀 文本")).SetValue(true);

            /* KILL STEAL */
            var ks = _menu.AddSubMenu(new Menu("抢人头", RootName + "_ks"));
            ks.AddItem(new MenuItem(RootName + KsQ, "显示 Q")).SetValue(true);
            ks.AddItem(new MenuItem(RootName + KsE, "显示 E")).SetValue(true);
            ks.AddItem(new MenuItem(RootName + KsR, "显示 R")).SetValue(false);
            ks.AddItem(new MenuItem(RootName + KsOverheat, "允许过热 抢人头")).SetValue(true);

            /* HEAT MANAGER */
            var hm = _menu.AddSubMenu(new Menu("过热控制", RootName + "_hm"));
            hm.AddItem(new MenuItem(RootName + HmStayInDanger, "启用 过热控制")).SetValue(true);
            hm.AddItem(new MenuItem(RootName + HmQ, "显示 Q")).SetValue(true);
            hm.AddItem(new MenuItem(RootName + HmW, "显示 W")).SetValue(true);

            /* MISC */
            var misc = _menu.AddSubMenu(new Menu("杂项", RootName + "_misc"));
            misc.AddItem(new MenuItem(RootName + MiscMinWGapcloser, "护盾(W)突进 范围"))
                .SetValue(new Slider(725, 0, 1450));
            misc.AddItem(new MenuItem(RootName + MiscAutoE, "自动 鱼叉(E)")).SetValue(true);
            misc.AddItem(new MenuItem(RootName + MiscEDelay, "使用 鱼叉(E) 延迟"))
                .SetValue(new Slider(1, 0, 3));
            misc.AddItem(new MenuItem(RootName + MiscEmDelay, "Delay Harpoons(E) at Melee Range")).SetValue(true);
            misc.AddItem(new MenuItem(RootName + MiscKeepInR, "Keep in Equalizer (R) for"))
                .SetValue(new Slider(2, 0, 5));
            misc.AddItem(new MenuItem(RootName + MiscHitChance, "命中率"))
                .SetValue(new StringList(new[] { "低", "正常", "高", "很高" }, 1));
            misc.AddItem(new MenuItem(RootName + MiscPackets, "使用 封包")).SetValue(true);

            /* FOOTER */
            _menu.AddItem(new MenuItem(RootName + "_spacer0", ""));
            _menu.AddItem(new MenuItem(RootName + "_spacer_desc", "兰博 - 机械公敌"));
            _menu.AddToMainMenu();
        }

        #region Vars

        private const string RootName = "worstping_kennen";

        public static readonly string ComboQ = "_combo_useq";
        public static readonly string ComboW = "_combo_usew";
        public static readonly string ComboE = "_combo_usee";
        public static readonly string ComboR = "_combo_user";
        public static readonly string ComboRMin = "combo_usermin";
        public static readonly string ComboOverheat = "combo_overheat";

        public static readonly string HarassQ = "_harass_useq";
        public static readonly string HarassW = "_harass_usew";
        public static readonly string HarassE = "_harass_usee";
        public static readonly string HarassOverheat = "_harass_overheat";

        public static readonly string FarmQ = "_farm_useq";
        public static readonly string FarmMinQ = "_farm_minq";
        public static readonly string FarmE = "_farm_usee";
        public static readonly string FarmOverheat = "_farm_overheat";

        public static readonly string DrawQ = "_drawing_q";
        public static readonly string DrawE = "_drawing_e";
        public static readonly string DrawR = "_drawing_r";
        public static readonly string DrawKillText = "_drawing_kt";

        public static readonly string KsQ = "_ks_useq";
        public static readonly string KsE = "_ks_usee";
        public static readonly string KsR = "_ks_user";
        public static readonly string KsOverheat = "_ks_overheat";

        public static readonly string HmStayInDanger = "_hm_stayindanger";
        public static readonly string HmQ = "_hm_useq";
        public static readonly string HmW = "_hm_usew";

        public static readonly string MiscMinWGapcloser = "_misc_minwgapcloser";
        public static readonly string MiscAutoE = "_misc_autoe";
        public static readonly string MiscEDelay = "_misc_edelay";
        public static readonly string MiscEmDelay = "_misc_emdelay";
        public static readonly string MiscKeepInR = "_misc_keepinr";
        public static readonly string MiscHitChance = "_misc_hitchance";
        public static readonly string MiscPackets = "_misc_packets";

        private readonly Menu _menu;
        private readonly Orbwalking.Orbwalker _orbwalker;

        #endregion

        #region Get Methods

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