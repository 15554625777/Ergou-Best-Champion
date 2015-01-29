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
    public class Getobj
    {
        public string Name;
        public float Duration;
        public int Realtime;
        public string Champname;
        public string Displayname;

        public Getobj(string name, float duration, int realtime, string champname, string displayname)
        {
            Name = name;
            Duration = duration;
            Realtime = realtime;
            Champname = champname;
            Displayname = displayname;
        }
    }

    public class SkillsList
    {
            public static List<Getobj> SkillList0 = new List<Getobj>();
            public static float Panthtimer = 1.5f;

            static SkillsList()
            {
                //Realtime check 0: off / 1 : Hero Posistion / 2 : Sender Position
                SkillList0.Add(new Getobj("UndyingRage_glow.troy", 5f, 1, "Tryndamere", "Tryndamere R"));
                SkillList0.Add(new Getobj("MonkeyKing_Base_R_Cas.troy", 4f, 1, "MonkeyKing", "Wukong R"));
                SkillList0.Add(new Getobj("nickoftime_tar.troy", 7f, 1, "Zilean", "Zilean R")); // zillean Ult



                SkillList0.Add(new Getobj("Lissandra_Base_R_ring_", 1.5f, 1, "Lissandra", "Lissandra R")); //because of checking self-ult
                SkillList0.Add(new Getobj("Lissandra_Base_R_iceblock.troy", 2.5f, 0, "Lissandra", "Lissandra Self-R"));

               SkillList0.Add(new Getobj("eyeforaneye", 2f, 1, "Kayle", "Kayle R"));

               SkillList0.Add(new Getobj("Karthus_Base_R_Target.troy", 3f, 1, "Karthus", "Karthus R(Target)"));
               SkillList0.Add(new Getobj("Karthus_Base_R_Cas.troy", 3f, 0, "Karthus", "Karthus R"));

               SkillList0.Add(new Getobj("MasterYi_Base_W_Buf.troy", 4f, 0, "MasterYi", "MasterYi W"));

               SkillList0.Add(new Getobj("ShenTeleport_v2.troy", 3f, 0, "Shen" , "Shen R"));



               SkillList0.Add(new Getobj("Shen_StandUnited_shield_v2.troy", 3f, 1, "Shen", "Shen R (Target)"));

               SkillList0.Add(new Getobj("EggTimer.troy", 6f, 1, "Anivia", "Anivia Egg"));
               SkillList0.Add(new Getobj("Passive_Death_Activate.troy", 3f, 1, "Aatrox", "Aatrox Passive"));

               SkillList0.Add(new Getobj("InfiniteDuress_tar.troy", 1.8f, 1, "Warwick", "Warwick R"));

               SkillList0.Add(new Getobj("DiplomaticImmunity_buf.troy", 7f, 1, "Poppy", "Poppy R"));
               SkillList0.Add(new Getobj("olaf_ragnorok_", 6f, 1, "Olaf", "Olaf R"));

               SkillList0.Add(new Getobj("Morgana_base_R_Indicator_Ring.troy", 3f, 1, "Morgana", "Morgana R"));
             //  SkillList0.Add(new Getobj("Crowstorm_", 5f, 1, "FiddleSticks", "FiddleSticks R"));


               SkillList0.Add(new Getobj("w_windwall_enemy", 4f, 2, "Yasuo", "Yasuo W")); //yasuo Yasuo_Skin02_w_windwall_enemy_02.troy 
               SkillList0.Add(new Getobj("Azir_Base_R_SoldierCape_", 5f, 2, "Azir","Azir R")); //Azir_Base_R_SoldierCape_Enemy.troy


             //  SkillList0.Add(new Getobj("talon_ult_sound.troy", 2.5f, 0));

               SkillList0.Add(new Getobj("Zed_Base_W_cloneswap_buf.troy", 4.5f, 0, "Zed", "Zed W"));
               SkillList0.Add(new Getobj("Zed_Base_R_cloneswap_buf.troy", 7.5f, 0,"Zed","Zed R"));
               SkillList0.Add(new Getobj("LeBlanc_Base_W_return_indicator.troy", 4f, 0, "Leblanc", "LeBlanc W"));
               SkillList0.Add(new Getobj("LeBlanc_Base_RW_return_indicator.troy", 4f, 0, "Leblanc", "LeBlanc R"));


               SkillList0.Add(new Getobj("zhonyas_ring_activate.troy", 2.5f, 1, "Misc", "Zhonya Hourglass"));
               SkillList0.Add(new Getobj("LifeAura.troy", 3f, 2, "Misc", "Guardian Angel / Zilean Revive"));
              SkillList0.Add(new Getobj("global_ss_teleport_", 3.5f, 0, "Misc", "Teleport"));
 
               SkillList0.Add(new Getobj("AbsoluteZero2_", 3f, 0, "Nunu", "Nunu R"));
               SkillList0.Add(new Getobj("kennen_ss_aoe_", 3f, 1, "Kennen", "Kennen R"));
               SkillList0.Add(new Getobj("Karthus_Base_W_Post", 5f, 0, "Karthus", "Karthus W"));
                SkillList0.Add(new Getobj("Thresh_Base_Lantern_cas_", 6f, 0, "Thresh","Thresh W"));
                SkillList0.Add(new Getobj("Viktor_Catalyst_", 4f, 0,"Viktor","Viktor W"));
                SkillList0.Add(new Getobj("pirate_cannonBarrage_aoe_indicator_", 7f, 0, "Gangplank", "Gangplank R"));
                SkillList0.Add(new Getobj("Jinx_E_Mine_Ready_", 4.5f, 0, "Jinx", "Jinx E"));
                SkillList0.Add(new Getobj("Zyra_R_cast_", 2f, 0, "Zyra", "Zyra R"));
                SkillList0.Add(new Getobj("Veigar_Base_W_cas_", 1.2f, 0, "Veigar", "Veigar W"));
                SkillList0.Add(new Getobj("Veigar_Base_E_cage_", 3f, 0, "Veigar", "Veigar E"));
                SkillList0.Add(new Getobj("akali_smoke_bomb_tar_team_", 8f, 0, "Akali", "Akali W"));
                SkillList0.Add(new Getobj("Pantheon_Base_R_indicator_", 1.5f, 0, "Pantheon", "Pantheon R")); // Visible = 2.5f / Invisible = 1.5f
            }

            public static Getobj IsObj(string skillname)
            {
                foreach (var proobj in SkillList0.Where(proobj1 => (skillname.ToLower().Contains(proobj1.Name.ToLower()))))
                {
                    var checkenemy = skillname.ToLower().Contains("red");
                    var checkally = skillname.ToLower().Contains("blue") || skillname.ToLower().Contains("green");
                    if (String.Equals(proobj.Name, skillname, StringComparison.CurrentCultureIgnoreCase))
                        return proobj;
                    if (checkenemy)
                        return proobj;
                    if (checkally)
                        return proobj;
                    else
                    {
                        return proobj;
                    }
                }
                return null;
            }

            public static Getobj IsMisc(string sendername)
            {
                foreach (var misc in SkillList0.Where(o => o.Name == sendername && o.Champname == "Misc"))
                {
                    if (Menus.Menu.SubMenu("misclist").Item(misc.Name).GetValue<bool>() &&
                        Menus.Menu.Item("activeMisc").GetValue<bool>())
                    {
                        return null;
                    }
                    else
                    {
                        return misc;
                    }
                }
                return null;
            }
    }


}
