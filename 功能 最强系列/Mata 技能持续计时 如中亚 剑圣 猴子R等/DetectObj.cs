#region

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Mata_View
{
    internal class DetectObj
    {
        public static List<ListedText> AllSkills = new List<ListedText>();


        public static bool PanthD, KayleUlt;
        public static float KayleDuration;
        public static Obj_AI_Hero Heropos = new Obj_AI_Hero();


        public static void DetectObjload()
        {
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
        }






        private static void OnProcessSpell(Obj_AI_Base obj, GameObjectProcessSpellCastEventArgs arg)
        {
            if (obj == null || arg == null || obj.Name.Contains("Turret") || obj.Name.Contains("Minion") ||
                !Menus.Menu.Item("activeskill").GetValue<bool>())
                return;

            FakeObjCreate(obj, arg);

        }

        private static void FakeObjCreate(Obj_AI_Base obj, GameObjectProcessSpellCastEventArgs arg)
        {
            if (arg.SData.Name.Contains("JudicatorIntervention"))
            {
                if (Menus.Menu.Item("eyeforaneye").GetValue<bool>())
                {
                    if (obj.Spellbook.GetSpell(SpellSlot.R).Level == 0)
                        return;
                    var level = (int) obj.Spellbook.GetSpell(SpellSlot.R).Level;
                    KayleDuration = 2f + (level - 1) * 0.5f;
                    KayleUlt = true;
                }
            }

            if (arg.SData.Name.Contains("ZhonyasHourglass"))
            {
                if (Menus.Menu.Item("activeMisc").GetValue<bool>())
                {
                    FakeCreateLogic(obj, "zhonyas_ring_activate.troy", 999999998, false, "zhonyas_ring_activate.troy");
                }
            }

            if (arg.SData.Name.Contains("KarthusFallenOne"))
            {
                FakeCreateLogic(obj, "Karthus_Base_R_Cas.troy", 999999997, false, "Karthus_Base_R_Cas.troy");
            }

            if (arg.SData.Name.Contains("infiniteduresschannel"))
            {
                FakeCreateLogic(obj, "InfiniteDuress_tar.troy", 999999996, true, "InfiniteDuress_");
            }

            if (arg.SData.Name.Contains("PantheonRJump"))
                PanthD = true;

            /*
            if (arg.SData.Name.Contains("TalonShadowAssault"))
            {
               FakeCreateLogic(obj, "talon_ult_sound.troy", 999999996, false, "talon_ult_sound.troy");
            }*/
            // talon is kind of buggy



        }

        private static void FakeCreateLogic(Obj_AI_Base obj,
            string createname,
            int fakeNetId,
            bool namecheck,
            string changname)
        {
            if (!Menus.Menu.Item(createname).GetValue<bool>())
                return;
            var ho = SkillsList.IsObj(createname);
            if (ho == null)
                return;

            var misccheck = SkillsList.IsMisc(createname);
            if (misccheck != null)
                return;

            if (namecheck)
                createname = changname;

            var sender = new GameObject();
            foreach (var hero in
                ObjectManager.Get<Obj_AI_Hero>().Where(d => obj.BaseSkinName == d.BaseSkinName))
            {
                AllSkills.Add(
                    new ListedText(
                        fakeNetId, createname, ho.Duration, hero.Position, Game.Time, sender, ho.Realtime, hero));
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {

            // obj_AI_Minion can't type to "ObjectManager.GetUnitByNetworkId<Obj_GeneralParticleEmitter>", so it throw error.
            if (sender == null || args == null || sender.Type != GameObjectType.obj_GeneralParticleEmmiter ||
                !Menus.Menu.Item("activeskill").GetValue<bool>())
                return;
            if (sender.Name.Contains("missile") || sender.Name.Contains("Minion") ||
                sender.Name.Contains("InfiniteDuress_tar") || sender.Name.Contains("teleport_arrive") ||
                sender.Name.Contains("teleport_flash"))
                return;


            var ho =
                SkillsList.IsObj((ObjectManager.GetUnitByNetworkId<Obj_GeneralParticleEmitter>(sender.NetworkId)).Name);
            if (ho == null)
                return;

            var misccheck =
                SkillsList.IsMisc((ObjectManager.GetUnitByNetworkId<Obj_GeneralParticleEmitter>(sender.NetworkId)).Name);
            if (misccheck != null)
                return;



            {
                switch (ho.Realtime)
                {
                    case 0: //Object First Created Posistion
                        var r1 = DivideRealTime.RealTimeDivide(sender, 1);
                        if (r1 == null)
                            return;
                        if (PanthD)
                        {
                            ho.Duration = 2.5f;
                            PanthD = false;
                        }
                        break;
                    case 1: //Realtime on Hero Position / Don't need to check Hero.isenemy
                        var r2 = DivideRealTime.RealTimeDivide(sender, 2);
                        if (r2 == null)
                            return;
                        if (KayleUlt)
                        {
                            if (!Menus.Menu.Item("eyeforaneye").GetValue<bool>())
                                return;
                            ho.Duration = KayleDuration;
                            KayleUlt = false;
                        }
                        break;
                    case 2: //Realtime on Sender Position
                        var r3 = DivideRealTime.RealTimeDivide(sender, 1);
                        if (r3 == null)
                            return;
                        break;

                }
                AllSkills.Add(
                    new ListedText(
                        sender.NetworkId, sender.Name, ho.Duration, sender.Position, Game.Time, sender, ho.Realtime,
                        Heropos));
            }
        }






        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if (sender == null || args == null || sender.Type != GameObjectType.obj_GeneralParticleEmmiter ||
                sender.Name.Contains("missile") || sender.Name.Contains("Minion") ||
                !Menus.Menu.Item("activeskill").GetValue<bool>())
                return;

            //InvalidOperationException is fixed 
            foreach (
                var deleteobj in
                    new List<ListedText>(AllSkills).Where(w => w.NetworkId == sender.NetworkId || w.Name == sender.Name)
                )
            {
                deleteobj.Visible = false;
                AllSkills.Remove(deleteobj);
            }
        }

    }

}

