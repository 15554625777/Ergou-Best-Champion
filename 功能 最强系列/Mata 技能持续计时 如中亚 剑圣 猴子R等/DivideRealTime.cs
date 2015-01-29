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
    public class DivideRealTime
    {

        public static object RealTimeDivide(GameObject sender, int realtimecheck)
        {
            var herof = new Obj_AI_Hero();
                switch (realtimecheck)
                {
                    case 1:
                        foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => (Vector3.Distance(sender.Position, hero.ServerPosition) <= 1000) && sender.Name.ToLower().Contains(hero.ChampionName.ToLower())))
                        {
                            herof = hero;
                            break;
                        }
                    
                        break;
                             case 2:
                        foreach (
                            var hero in
                                ObjectManager.Get<Obj_AI_Hero>()
                                    .Where(o => (Vector3.Distance(sender.Position, o.ServerPosition) <= 100)))
                        {
                            herof = hero;
                            break;
                        }
                        break;
                        // return null;

                }
                foreach (var skill in SkillsList.SkillList0.Where(o => sender.Name.ToLower().Contains(o.Name.ToLower())))
                            {
                                if (herof.ChampionName == "Lissandra" &&
                                    sender.Name.ToLower().Contains("lissandra_base_r_ring_"))
                                    return null;
                                if (!Menus.Menu.Item(skill.Name).GetValue<bool>()) return null;
                                if ((herof.IsEnemy || (sender.Name.ToLower().Contains("red") || sender.Name.ToLower().Contains("enemy"))) && Menus.Menu.Item("activeEnemy").GetValue<bool>())
                                {
                                    DetectObj.Heropos = herof;
                                    return skill;
                                }
                                if ((herof.IsAlly || (sender.Name.ToLower().Contains("green") || sender.Name.ToLower().Contains("blue"))) && !herof.IsMe && Menus.Menu.Item("activeEnemy").GetValue<bool>())
                                {
                                    DetectObj.Heropos = herof;
                                    return skill;
                                }
                                if ((herof.IsMe || sender.Name.ToLower().Contains("green"))&& Menus.Menu.Item("activeMy").GetValue<bool>())
                                {
                                    DetectObj.Heropos = herof;
                                    return skill;
                                }
                                else
                                {
                                    return null;
                                }
                            }
            return null;
        }
    }
    
  

    }

 

