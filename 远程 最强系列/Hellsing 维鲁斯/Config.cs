using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace Varus
{
    public class Config
    {
        private const string MENU_NAME = "[Hellsing] " + "维鲁斯";

        public static MenuWrapper Menu { get; private set; }

        public static Dictionary<string, MenuWrapper.BoolLink> BoolLinks { get; private set; }
        public static Dictionary<string, MenuWrapper.CircleLink> CircleLinks { get; private set; }
        public static Dictionary<string, MenuWrapper.KeyBindLink> KeyLinks { get; private set; }
        public static Dictionary<string, MenuWrapper.SliderLink> SliderLinks { get; private set; }
        public static Dictionary<string, MenuWrapper.StringListLink> StringListLinks { get; private set; }

        private static void ProcessLink(string key, object value)
        {
            if (value is MenuWrapper.BoolLink)
                BoolLinks.Add(key, value as MenuWrapper.BoolLink);
            else if (value is MenuWrapper.CircleLink)
                CircleLinks.Add(key, value as MenuWrapper.CircleLink);
            else if (value is MenuWrapper.KeyBindLink)
                KeyLinks.Add(key, value as MenuWrapper.KeyBindLink);
            else if (value is MenuWrapper.SliderLink)
                SliderLinks.Add(key, value as MenuWrapper.SliderLink);
            else if (value is MenuWrapper.StringListLink)
                StringListLinks.Add(key, value as MenuWrapper.StringListLink);
        }

        static Config()
        {
            Menu = new MenuWrapper(MENU_NAME);

            BoolLinks = new Dictionary<string, MenuWrapper.BoolLink>();
            CircleLinks = new Dictionary<string, MenuWrapper.CircleLink>();
            KeyLinks = new Dictionary<string, MenuWrapper.KeyBindLink>();
            SliderLinks = new Dictionary<string, MenuWrapper.SliderLink>();
            StringListLinks = new Dictionary<string, MenuWrapper.StringListLink>();

            SetupMenu();
        }

        private static void SetupMenu()
        {
            // ----- Combo
            var subMenu = Menu.MainMenu.AddSubMenu("连招");
            var subSubMenu = subMenu.AddSubMenu("使用 Q");
            ProcessLink("comboUseQ", subSubMenu.AddLinkedBool("启用"));
            ProcessLink("comboFullQ", subSubMenu.AddLinkedBool("Q总是蓄能极限范围"));
            ProcessLink("comboRangeQ", subSubMenu.AddLinkedSlider("蓄能额外范围", 200, 0, 200));
            ProcessLink("comboStacksQ", subSubMenu.AddLinkedSlider("使用Q|W堆叠层数", 3, 0, 3));
            ProcessLink("comboUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("comboUseR", subMenu.AddLinkedKeyBind("使用 R", 'A', KeyBindType.Press));
            ProcessLink("comboActive", subMenu.AddLinkedKeyBind("连招 键位", 32, KeyBindType.Press));

            // ----- Harass
            subMenu = Menu.MainMenu.AddSubMenu("骚扰");
            subSubMenu = subMenu.AddSubMenu("使用 Q");
            ProcessLink("harassUseQ", subSubMenu.AddLinkedBool("启用"));
            ProcessLink("harassFullQ", subSubMenu.AddLinkedBool("Q总是蓄能极限范围"));
            ProcessLink("harassExtraRangeQ", subSubMenu.AddLinkedSlider("蓄能额外范围", 200, 0, 200));
            ProcessLink("harassStacksQ", subSubMenu.AddLinkedSlider("使用Q|W堆叠层数", 0, 0, 3));
            ProcessLink("harassUseE", subMenu.AddLinkedBool("使用 E", false));
            ProcessLink("harassMana", subMenu.AddLinkedSlider("骚扰最低蓝量 (%)", 30));
            ProcessLink("harassActive", subMenu.AddLinkedKeyBind("骚扰 键位", 'C', KeyBindType.Press));

            // ----- WaveClear
            subMenu = Menu.MainMenu.AddSubMenu("清线");
            ProcessLink("waveUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("waveNumE", subMenu.AddLinkedSlider("使用E|命中目标", 3, 1, 10));
            ProcessLink("waveMana", subMenu.AddLinkedSlider("清线最低 (%)", 30));
            ProcessLink("waveActive", subMenu.AddLinkedKeyBind("清线 键位", 'V', KeyBindType.Press));

            // ----- JungleClear
            subMenu = Menu.MainMenu.AddSubMenu("清野");
            ProcessLink("jungleUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("jungleUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("jungleActive", subMenu.AddLinkedKeyBind("清野 键位", 'V', KeyBindType.Press));

            // ----- Flee
            subMenu = Menu.MainMenu.AddSubMenu("逃跑");
            ProcessLink("fleeNothing", subMenu.AddLinkedBool("Nothing yet Kappa"));
            ProcessLink("fleeActive", subMenu.AddLinkedKeyBind("逃跑 键位", 'T', KeyBindType.Press));

            // ----- Drawings
            subMenu = Menu.MainMenu.AddSubMenu("范围");
            ProcessLink("drawRangeQ", subMenu.AddLinkedCircle("Q 范围", true, Color.FromArgb(150, Color.IndianRed), SpellManager.Q.Range));
            ProcessLink("drawRangeQMax", subMenu.AddLinkedCircle("Q 范围 (极限)", true, Color.FromArgb(150, Color.IndianRed), SpellManager.Q.ChargedMaxRange));
            ProcessLink("drawRangeE", subMenu.AddLinkedCircle("E 范围", true, Color.FromArgb(150, Color.IndianRed), SpellManager.E.Range));
            ProcessLink("drawRangeR", subMenu.AddLinkedCircle("R 范围", true, Color.FromArgb(150, Color.DarkRed), SpellManager.R.Range));
        }
    }
}
