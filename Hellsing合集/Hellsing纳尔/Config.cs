using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace Gnar
{
    public class Config
    {
        public const string MENU_NAME = "[Hellsing] " + Program.CHAMP_NAME;
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

            // ----- Combo
            var subMenu = _menu.MainMenu.AddSubMenu("连 招");
            // Mini
            var subSubMenu = subMenu.AddSubMenu("迷你 状态");
            ProcessLink("comboUseQ", subSubMenu.AddLinkedBool("使用 Q"));
            ProcessLink("comboUseE", subSubMenu.AddLinkedBool("使用 E"));
            // Mega
            subSubMenu = subMenu.AddSubMenu("变大 状态");
            ProcessLink("comboUseQMega", subSubMenu.AddLinkedBool("使用 Q"));
            ProcessLink("comboUseWMega", subSubMenu.AddLinkedBool("使用 W"));
            ProcessLink("comboUseEMega", subSubMenu.AddLinkedBool("使用 E"));
            ProcessLink("comboUseRMega", subSubMenu.AddLinkedBool("使用 R"));
            // General
            ProcessLink("comboUseItems", subMenu.AddLinkedBool("使用 物品"));
            ProcessLink("comboUseIgnite", subMenu.AddLinkedBool("使用 点燃"));
            ProcessLink("comboActive", subMenu.AddLinkedKeyBind("启用 连招", 32, KeyBindType.Press));


            // ----- Harass
            subMenu = _menu.MainMenu.AddSubMenu("骚 扰");
            // Mini
            subSubMenu = subMenu.AddSubMenu("迷你 状态");
            ProcessLink("harassUseQ", subSubMenu.AddLinkedBool("使用 Q"));
            // Mega
            subSubMenu = subMenu.AddSubMenu("变大 状态");
            ProcessLink("harassUseQMega", subSubMenu.AddLinkedBool("使用 Q"));
            ProcessLink("harassUseWMega", subSubMenu.AddLinkedBool("使用 W"));
            // General
            ProcessLink("harassActive", subMenu.AddLinkedKeyBind("启用 骚扰", 'C', KeyBindType.Press));


            // ----- WaveClear
            subMenu = _menu.MainMenu.AddSubMenu("清 线");
            // Mini
            subSubMenu = subMenu.AddSubMenu("迷你 状态");
            ProcessLink("waveUseQ", subSubMenu.AddLinkedBool("使用 Q"));
            // Mega
            subSubMenu = subMenu.AddSubMenu("变大 状态");
            ProcessLink("waveUseQMega", subSubMenu.AddLinkedBool("使用 Q"));
            ProcessLink("waveUseWMega", subSubMenu.AddLinkedBool("使用 W"));
            ProcessLink("waveUseEMega", subSubMenu.AddLinkedBool("使用 E"));
            // Gernal
            ProcessLink("waveUseItems", subMenu.AddLinkedBool("使用 物品"));
            ProcessLink("waveActive", subMenu.AddLinkedKeyBind("启用 清线", 'V', KeyBindType.Press));


            // ----- JungleClear
            subMenu = _menu.MainMenu.AddSubMenu("清 野");
            // Mini
            subSubMenu = subMenu.AddSubMenu("迷你 状态");
            ProcessLink("jungleUseQ", subSubMenu.AddLinkedBool("使用 Q"));
            // Mega
            subSubMenu = subMenu.AddSubMenu("变大 状态");
            ProcessLink("jungleUseQMega", subSubMenu.AddLinkedBool("使用 Q"));
            ProcessLink("jungleUseWMega", subSubMenu.AddLinkedBool("使用 W"));
            ProcessLink("jungleUseEMega", subSubMenu.AddLinkedBool("使用 E"));
            // General
            ProcessLink("jungleUseItems", subMenu.AddLinkedBool("使用 物品"));
            ProcessLink("jungleActive", subMenu.AddLinkedKeyBind("启用 清野", 'V', KeyBindType.Press));

            // ----- Flee
            subMenu = _menu.MainMenu.AddSubMenu("逃 跑");
            ProcessLink("fleeNothing", subMenu.AddLinkedBool("技能冷却后 开启"));
            ProcessLink("fleeActive", subMenu.AddLinkedKeyBind("开启 逃跑", 'T', KeyBindType.Press));

            // ----- Items
            subMenu = _menu.MainMenu.AddSubMenu("物 品");
            ProcessLink("itemsTiamat", subMenu.AddLinkedBool("使用 提亚玛特"));
            ProcessLink("itemsHydra", subMenu.AddLinkedBool("使用 九头蛇"));
            ProcessLink("itemsCutlass", subMenu.AddLinkedBool("使用 水银弯刀"));
            ProcessLink("itemsBotrk", subMenu.AddLinkedBool("使用 破败"));
            ProcessLink("itemsYoumuu", subMenu.AddLinkedBool("使用 幽梦"));
            ProcessLink("itemsRanduin", subMenu.AddLinkedBool("使用 蓝盾"));
            ProcessLink("itemsFace", subMenu.AddLinkedBool("使用 山岳之荣"));

            // ----- Drawings
            subMenu = _menu.MainMenu.AddSubMenu("范 围");
            // Mini
            subSubMenu = subMenu.AddSubMenu("迷你 状态");
            ProcessLink("drawRangeQ", subSubMenu.AddLinkedCircle("Q 范围", true, Color.FromArgb(150, Color.IndianRed), SpellManager.QMini.Range));
            ProcessLink("drawRangeE", subSubMenu.AddLinkedCircle("E 范围", true, Color.FromArgb(150, Color.Azure), SpellManager.EMini.Range));
            // Mega
            subSubMenu = subMenu.AddSubMenu("变大 状态");
            ProcessLink("drawRangeQMega", subSubMenu.AddLinkedCircle("Q 范围", true, Color.FromArgb(150, Color.IndianRed), SpellManager.QMega.Range));
            ProcessLink("drawRangeWMega", subSubMenu.AddLinkedCircle("W 范围", false, Color.FromArgb(150, Color.Azure), SpellManager.EMega.Range));
            ProcessLink("drawRangeEMega", subSubMenu.AddLinkedCircle("E 范围", true, Color.FromArgb(150, Color.IndianRed), SpellManager.QMega.Range));
            ProcessLink("drawRangeRMega", subSubMenu.AddLinkedCircle("R 范围", true, Color.FromArgb(150, Color.Azure), SpellManager.EMega.Range));
        }
    }
}
