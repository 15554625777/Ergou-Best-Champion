using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace Rekt_Sai
{
    public class Config
    {
        public const string MENU_NAME = "[Hellsing] "+ "挖掘机";
        private static MenuWrapper _menu;

        private static Dictionary<string, MenuWrapper.BoolLink> _boolLinks = new Dictionary<string, MenuWrapper.BoolLink>();
        private static Dictionary<string, MenuWrapper.CircleLink> _circleLinks = new Dictionary<string, MenuWrapper.CircleLink>();
        private static Dictionary<string, MenuWrapper.KeyBindLink> _keyLinks = new Dictionary<string, MenuWrapper.KeyBindLink>();
        private static Dictionary<string, MenuWrapper.SliderLink> _sliderLinks = new Dictionary<string, MenuWrapper.SliderLink>();

        public static MenuWrapper Menu
        {
            get { return _menu; }
        }

        public static Dictionary<string, MenuWrapper.BoolLink> BoolLinks
        {
            get { return _boolLinks; }
        }
        public static Dictionary<string, MenuWrapper.CircleLink> CircleLinks
        {
            get { return _circleLinks; }
        }
        public static Dictionary<string, MenuWrapper.KeyBindLink> KeyLinks
        {
            get { return _keyLinks; }
        }
        public static Dictionary<string, MenuWrapper.SliderLink> SliderLinks
        {
            get { return _sliderLinks; }
        }

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
            // Create menu
            _menu = new MenuWrapper(MENU_NAME);

            // Combo
            var subMenu = _menu.MainMenu.AddSubMenu("连 招");
            ProcessLink("comboUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("comboUseW", subMenu.AddLinkedBool("使用 W"));
            ProcessLink("comboUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("comboUseQBurrow", subMenu.AddLinkedBool("使用 Q (潜地)"));
            ProcessLink("comboUseEBurrow", subMenu.AddLinkedBool("使用 E (潜地)"));
            ProcessLink("comboUseItems", subMenu.AddLinkedBool("使用 物品"));
            ProcessLink("comboUseIgnite", subMenu.AddLinkedBool("使用 点燃"));
            ProcessLink("comboUseSmite", subMenu.AddLinkedBool("使用 惩戒 （假如有机会）"));
            ProcessLink("comboActive", subMenu.AddLinkedKeyBind("启用 连招", 32, KeyBindType.Press));

            // Harass
            subMenu = _menu.MainMenu.AddSubMenu("骚 扰");
            ProcessLink("harassUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("harassUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("harassUseQBurrow", subMenu.AddLinkedBool("使用 Q (潜地)"));
            ProcessLink("harassUseItems", subMenu.AddLinkedBool("使用 物品"));
            ProcessLink("harassMana", subMenu.AddLinkedSlider("骚扰最低蓝量 (%)", 30));
            ProcessLink("harassActive", subMenu.AddLinkedKeyBind("启用 骚扰", 'C', KeyBindType.Press));

            // WaveClear
            subMenu = _menu.MainMenu.AddSubMenu("清 线");
            ProcessLink("waveUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("waveNumQ", subMenu.AddLinkedSlider("Q清线|小兵数量", 2, 1, 10));
            ProcessLink("waveUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("waveUseQBurrow", subMenu.AddLinkedBool("使用 Q (潜地)"));
            ProcessLink("waveUseItems", subMenu.AddLinkedBool("使用 物品"));
            ProcessLink("waveMana", subMenu.AddLinkedSlider("清线最低蓝量 (%)", 30));
            ProcessLink("waveActive", subMenu.AddLinkedKeyBind("启用 清线", 'V', KeyBindType.Press));

            // JungleClear
            subMenu = _menu.MainMenu.AddSubMenu("清 野");
            ProcessLink("jungleUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("jungleUseW", subMenu.AddLinkedBool("使用 W"));
            ProcessLink("jungleUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("jungleUseQBurrow", subMenu.AddLinkedBool("使用 Q (潜地)"));
            ProcessLink("jungleUseItems", subMenu.AddLinkedBool("使用 物品"));
            ProcessLink("jungleActive", subMenu.AddLinkedKeyBind("启用 清野", 'V', KeyBindType.Press));

            // Flee
            subMenu = _menu.MainMenu.AddSubMenu("逃 跑");
            ProcessLink("fleeNothing", subMenu.AddLinkedBool("技能冷却后 启用"));
            ProcessLink("fleeActive", subMenu.AddLinkedKeyBind("启用 逃跑", 'T', KeyBindType.Press));

            // Items
            subMenu = _menu.MainMenu.AddSubMenu("项 目");
            ProcessLink("itemsTiamat", subMenu.AddLinkedBool("使用 提亚玛特"));
            ProcessLink("itemsHydra", subMenu.AddLinkedBool("使用 九头蛇"));
            ProcessLink("itemsCutlass", subMenu.AddLinkedBool("使用 水银弯刀"));
            ProcessLink("itemsBotrk", subMenu.AddLinkedBool("使用 破败"));
            ProcessLink("itemsRanduin", subMenu.AddLinkedBool("使用 蓝盾"));

            // Drawings
            subMenu = _menu.MainMenu.AddSubMenu("范 围");
            ProcessLink("drawRangeQ", subMenu.AddLinkedCircle("Q 范 围 (潜地)", true, Color.FromArgb(150, Color.IndianRed), SpellManager.QBurrowed.Range));
            ProcessLink("drawRangeE", subMenu.AddLinkedCircle("E 范 围 (潜地)", true, Color.FromArgb(150, Color.Azure), SpellManager.EBurrowed.Range));
        }
    }
}
