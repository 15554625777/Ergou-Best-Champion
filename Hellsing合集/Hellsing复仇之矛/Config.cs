using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace Kalista
{
    public class Config
    {
        private static bool initialized = false;
        private const string MENU_TITLE = "[Hellsing] " + Program.CHAMP_NAME;

        private static MenuWrapper _menu;

        private static Dictionary<string, MenuWrapper.BoolLink> _boolLinks = new Dictionary<string, MenuWrapper.BoolLink>();
        private static Dictionary<string, MenuWrapper.CircleLink> _circleLinks = new Dictionary<string, MenuWrapper.CircleLink>();
        private static Dictionary<string, MenuWrapper.KeyBindLink> _keyLinks = new Dictionary<string, MenuWrapper.KeyBindLink>();
        private static Dictionary<string, MenuWrapper.SliderLink> _sliderLinks = new Dictionary<string, MenuWrapper.SliderLink>();

        public static MenuWrapper Menu { get { return _menu; } }

        public static Dictionary<string, MenuWrapper.BoolLink> BoolLinks { get { return _boolLinks; } }
        public static Dictionary<string, MenuWrapper.CircleLink> CircleLinks { get { return _circleLinks; } }
        public static Dictionary<string, MenuWrapper.KeyBindLink> KeyLinks { get { return _keyLinks; } }
        public static Dictionary<string, MenuWrapper.SliderLink> SliderLinks { get { return _sliderLinks; } }

        private static void ProcessLink(string key, object value)
        {
            if (value is MenuWrapper.BoolLink)
                _boolLinks.Add(key, value as MenuWrapper.BoolLink);
            else if (value is MenuWrapper.CircleLink)
                _circleLinks.Add(key, value as MenuWrapper.CircleLink);
            else if (value is MenuWrapper.KeyBindLink)
                _keyLinks.Add(key, value as MenuWrapper.KeyBindLink);
            else if (value is MenuWrapper.SliderLink)
                _sliderLinks.Add(key, value as MenuWrapper.SliderLink);
        }

        public static void Initialize()
        {
            if (initialized)
                return;
            initialized = true;

            // Create menu
            _menu = new MenuWrapper(MENU_TITLE);

            // Combo
            var subMenu = _menu.MainMenu.AddSubMenu("连 招");
            ProcessLink("comboUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("comboUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("comboNumE", subMenu.AddLinkedSlider("E堆叠层数", 5, 1, 20));
            ProcessLink("comboUseItems", subMenu.AddLinkedBool("使用 物品"));
            ProcessLink("comboUseIgnite", subMenu.AddLinkedBool("使用 点燃"));
            ProcessLink("comboActive", subMenu.AddLinkedKeyBind("启用 连招", 32, KeyBindType.Press));

            // Harass
            subMenu = _menu.MainMenu.AddSubMenu("骚 扰");
            ProcessLink("harassUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("harassMana", subMenu.AddLinkedSlider("骚扰最低蓝量 (%)", 30));
            ProcessLink("harassActive", subMenu.AddLinkedKeyBind("启用 骚扰", 'C', KeyBindType.Press));

            // WaveClear
            subMenu = _menu.MainMenu.AddSubMenu("清 线");
            ProcessLink("waveUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("waveNumQ", subMenu.AddLinkedSlider("Q清线|小兵数量", 3, 1, 10));
            ProcessLink("waveUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("waveNumE", subMenu.AddLinkedSlider("E清线|小兵数量", 2, 1, 10));
            ProcessLink("waveMana", subMenu.AddLinkedSlider("清线最低蓝量 (%)", 30));
            ProcessLink("waveActive", subMenu.AddLinkedKeyBind("启用 清线", 'V', KeyBindType.Press));

            // JungleClear
            subMenu = _menu.MainMenu.AddSubMenu("清 野");
            ProcessLink("jungleUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("jungleActive", subMenu.AddLinkedKeyBind("启用 清野", 'V', KeyBindType.Press));

            // Flee
            subMenu = _menu.MainMenu.AddSubMenu("逃 跑");
            ProcessLink("fleeWalljump", subMenu.AddLinkedBool("尝试 跳墙"));
            ProcessLink("fleeAA", subMenu.AddLinkedBool("智能 走A 逃跑"));
            ProcessLink("fleeActive", subMenu.AddLinkedKeyBind("启用 逃跑", 'T', KeyBindType.Press));

            // Misc
            subMenu = _menu.MainMenu.AddSubMenu("杂 项");
            ProcessLink("miscKillstealE", subMenu.AddLinkedBool("使用 E 抢人头"));
            ProcessLink("miscBigE", subMenu.AddLinkedBool("使用 E 抢野"));
            ProcessLink("miscUseR", subMenu.AddLinkedBool("使用 R 拯救你的伴侣"));

            // Spell settings
            subMenu = _menu.MainMenu.AddSubMenu("法术 设置");
            ProcessLink("spellReductionE", subMenu.AddLinkedSlider("E 伤害 复位", 20));

            // Items
            subMenu = _menu.MainMenu.AddSubMenu("物 品");
            ProcessLink("itemsCutlass", subMenu.AddLinkedBool("使用 水银弯刀"));
            ProcessLink("itemsBotrk", subMenu.AddLinkedBool("使用 破败"));
            ProcessLink("itemsYoumuu", subMenu.AddLinkedBool("使用 幽梦"));

            // Drawings
            subMenu = _menu.MainMenu.AddSubMenu("范 围");
            ProcessLink("drawRangeQ", subMenu.AddLinkedCircle("Q 范围", true, Color.FromArgb(150, Color.IndianRed), SpellManager.Q.Range));
            ProcessLink("drawRangeW", subMenu.AddLinkedCircle("W 范围", true, Color.FromArgb(150, Color.MediumPurple), SpellManager.W.Range));
            ProcessLink("drawRangeEsmall", subMenu.AddLinkedCircle("E 范围 (剩下的)", false, Color.FromArgb(150, Color.DarkRed), SpellManager.E.Range - 200));
            ProcessLink("drawRangeEactual", subMenu.AddLinkedCircle("E 范围 (真实)", true, Color.FromArgb(150, Color.DarkRed), SpellManager.E.Range));
            ProcessLink("drawRangeR", subMenu.AddLinkedCircle("R 范围", false, Color.FromArgb(150, Color.Red), SpellManager.R.Range));
        }
    }
}
