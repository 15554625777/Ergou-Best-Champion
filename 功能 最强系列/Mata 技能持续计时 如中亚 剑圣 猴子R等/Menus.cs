#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Mata_View
{
    public class Menus
    {
        public static Menu Menu, Skill;
        public static MenuItem testsize;
        public static SkillsList SkillsListAdd;

        public static void Menuadd()
        {
            Menu = new Menu("Mata 技能持续计时", "Mata View", true);
            //SkillsListAdd = new SkillsList();
            var configs = (new Menu("技能计时 配置", "Skills Timer Configs"));
            var activeskill = (new MenuItem("activeskill", "启用 所有技能持续计时").SetValue(true));

            var textmenu = (new Menu("文本显示 配置", "Text Configs"));
            testsize = (new MenuItem("textsize", "文本 尺寸").SetValue(new Slider(16, 30, 10)));
          //  var textcolorenemy = (new MenuItem("textcolorenemy", "Ally Color").SetValue(SharpDX.Color.White));
          //  var textcolorally = (new MenuItem("textcolorally", "Enemy Color").SetValue(Color.White));

            var enemylist = new Menu("敌人计时", "enemylist");
            var allylist = new Menu("队友计时", "allylist");
            var mylist = new Menu("自己计时", "mylist");
            var misclist = new Menu("物品计时", "misclist");
       

            Menu.AddSubMenu(configs);
            Menu.SubMenu("Skills Timer Configs").AddItem(activeskill);

            Menu.AddSubMenu(textmenu);
            Menu.SubMenu("Text Configs").AddItem(testsize);
           // Menu.SubMenu("Text Configs").AddItem(textcolorenemy);
          //  Menu.SubMenu("Text Configs").AddItem(textcolorally);

  
            Skill = new Menu("技能 计时", "Skill Timer", true);
           // Menu.AddSubMenu(Skill);
            Menu.AddSubMenu(enemylist);
            Menu.AddSubMenu(allylist);
            Menu.AddSubMenu(mylist);
            Menu.AddSubMenu(misclist);
  
            foreach (var skill in SkillsList.SkillList0)
            {
                foreach (var herolist in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (herolist.IsEnemy && skill.Champname == herolist.ChampionName)
                    {
                        Menu.SubMenu("enemylist").AddItem(new MenuItem(skill.Name, skill.Displayname).SetValue(true));
                    }
                    else if (herolist.IsAlly && !herolist.IsMe && skill.Champname == herolist.ChampionName)
                    {
                        Menu.SubMenu("allylist").AddItem(new MenuItem(skill.Name, skill.Displayname).SetValue(true));
                    }
                    else if (herolist.IsMe && skill.Champname == herolist.ChampionName)
                    {
                        Menu.SubMenu("mylist").AddItem(new MenuItem(skill.Name, skill.Displayname).SetValue(true));
                    }
                }
               
                   if (skill.Champname == "Misc")
                    {
                        Menu.SubMenu("misclist").AddItem(new MenuItem(skill.Name, skill.Displayname).SetValue(true));
                    }
            }
            Menu.SubMenu("enemylist").AddItem(new MenuItem("activeEnemy", "启用 敌人技能持续计时").SetValue(true));
            Menu.SubMenu("allylist").AddItem(new MenuItem("activeAlly", "启用 队友技能持续计时").SetValue(true));
            Menu.SubMenu("mylist").AddItem(new MenuItem("activeMy", "启用 自己技能持续计时").SetValue(true));
            Menu.SubMenu("misclist").AddItem(new MenuItem("activeMisc", "启用 物品持续计时").SetValue(true));
            Menu.AddToMainMenu();
        }
    }
}
