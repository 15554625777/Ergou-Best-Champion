using System;
using System.Collections.Generic;
using System.Linq; 
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Mundo
{
    internal class Program
    {
        //      {
        private const string ChampName = "DrMundo";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> Spells = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot IgniteSlot;
        private static Obj_AI_Hero _player;

        public static Menu Config;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }


        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (_player.BaseSkinName != ChampName)
                return;

            Q = new Spell(SpellSlot.Q, 1000);
            Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 160);
            E = new Spell(SpellSlot.E, 0);            
            R = new Spell(SpellSlot.R, 0);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

            Spells.Add(Q);
            Spells.Add(W);
            Spells.Add(E);
            Spells.Add(R);


            Config = new Menu(ChampName, ChampName, true);


            var tsMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);
            Config.AddSubMenu(new Menu("走 砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));


            Config.AddSubMenu(new Menu("连 招", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("comboKey", "按 键").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("combo").AddItem(new MenuItem("autoIgnite", "连招时 自动点燃").SetValue(true));

            Config.AddSubMenu(new Menu("骚 扰", "harass"));
            Config.SubMenu("harass").AddItem(new MenuItem("autoQ", "当目标在范围内 自动q").SetValue(new KeyBind("Z".ToCharArray()[0],KeyBindType.Toggle)));
            Config.SubMenu("harass").AddItem(new MenuItem("harassKey", "按 键").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("打 钱", "farm"));
            Config.SubMenu("farm").AddItem(new MenuItem("farmKey", "按 键").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
//            Config.SubMenu("farm").AddItem(new MenuItem("qFarm", "Farm with (Q)").SetValue(true));
            
            Config.AddSubMenu(new Menu("杂 项", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("usePackets", "使用 封包").SetValue(false));
            Config.SubMenu("misc").AddItem(new MenuItem("autoheal", "残血 自动r").SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Toggle)));
            Config.SubMenu("misc").AddItem(new MenuItem("autohealhp", "当血量低于%自动r").SetValue(new Slider(40, 1)));

            Config.AddSubMenu(new Menu("范 围", "drawing"));
            Config.SubMenu("drawing").AddItem(new MenuItem("mDraw", "禁用 所有范围显示").SetValue(false));
            Config.SubMenu("drawing").AddItem(new MenuItem("Target", "目标 脚下 线圈").SetValue(new Circle(true,Color.FromArgb(255, 255, 0, 0))));
            Config.SubMenu("drawing").AddItem(new MenuItem("QDraw", "Q 范围").SetValue(new Circle(true, Color.FromArgb(255, 178, 0, 0))));

            
            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;


            Game.PrintChat("<font color=\"#00BFFF\">Daimao Mundo by Taerarenai -</font> <font color=\"#FFFFFF\">Loaded</font>");
            Game.PrintChat("<font color=\"#00BFFF\">Daimao 姊﹀涓ㄥ姞杓夋垚鍔燂紒婕㈠寲by 鑺辫竟锛丵Q缇361630847!</font> ");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var comboKey = Config.Item("comboKey").GetValue<KeyBind>().Active;
            var harassKey = Config.Item("harassKey").GetValue<KeyBind>().Active;
            var farmKey = Config.Item("farmKey").GetValue<KeyBind>().Active;
            var autoheal = Config.Item("autoheal").GetValue<KeyBind>().Active;

            if (autoheal)
                AutoHeal();

            if (comboKey && target != null)
                Combo(target);
            else
            {
                if (harassKey && target != null)
                    Harass(target);

                if (farmKey)
                    Farm();

                if (Config.Item("autoQ").GetValue<KeyBind>().Active && target != null &&
                    ObjectManager.Player.Distance(target) <= Q.Range && Q.IsReady())
                    CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Physical, HitChance.High);

            }

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("mDraw").GetValue<bool>())
                return;

            foreach (var spell in Spells.Where(spell => Config.Item(spell.Slot + "Draw").GetValue<Circle>().Active))
            {
                Utility.DrawCircle(ObjectManager.Player.Position, spell.Range,Config.Item(spell.Slot + "Draw").GetValue<Circle>().Color);
            }
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Config.Item("Target").GetValue<Circle>().Active && target != null) Utility.DrawCircle(target.Position, 50, Config.Item("Target").GetValue<Circle>().Color, 1, 50);
        }

                private static void AutoHeal()
        {

            var health = ObjectManager.Player.MaxHealth * (Config.Item("autohealhp").GetValue<Slider>().Value / 100.0);
            if (ObjectManager.Player.Health > health)
                return;

            if (R.IsReady())
                R.Cast();
        }


        private static void Combo(Obj_AI_Base target)
        {
            if (target == null)
                return;

            if (Q.IsReady())
                CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Magical, HitChance.High);

            if (E.IsReady())
                E.Cast();


            if (!Config.Item("autoIgnite").GetValue<bool>())
                return;

            if (IgniteSlot == SpellSlot.Unknown ||
                ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) != SpellState.Ready)
                return;

            if (!(ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) >= target.Health))
                return;

            ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
        }


        private static void Harass(Obj_AI_Base target)
        {
            if (target == null)
                return;


            if (Q.IsReady())
                CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Physical, HitChance.High);
        }


        private static void Farm()
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var mana = ObjectManager.Player.MaxMana*(Config.Item("farmMana").GetValue<Slider>().Value/100.0);
            if (!(ObjectManager.Player.Mana > mana))
                return;

            if (!Config.Item("qFarm").GetValue<bool>() || !Q.IsReady())
                return;

            foreach (var minion in from minion in minions
                let actualHp =(HealthPrediction.GetHealthPrediction(minion,(int) (ObjectManager.Player.Distance(minion)*1000/1500)) <= minion.MaxHealth*0.15)? ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q)*2 : ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) where minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion,(int) (ObjectManager.Player.Distance(minion)*1000/1500)) <= actualHp select minion)
            {

                Q.Cast(minion, Config.Item("usePackets").GetValue<bool>());
                return;
            }
        }


        private static float ComboDamage(Obj_AI_Base target)
        {
            var dmg = 0d;
            if (Q.IsReady()) dmg += (target.Health <= target.MaxHealth*0.15)? (ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q)*2) : ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
            if (W.IsReady()) dmg += ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);
 //           if (E.IsReady()) dmg += ObjectManager.Player.GetSpellDamage(target, SpellSlot.E);
 //           if (R.IsReady()) dmg += ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);
            if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready) dmg += ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            return (float) dmg;
        }



        public static void CastBasicSkillShot(Spell spell, float range, LeagueSharp.Common.TargetSelector.DamageType type, HitChance hitChance)
        {
            var target = LeagueSharp.Common.TargetSelector.GetTarget(range, type);
            if (target == null || !spell.IsReady())
            return;
            spell.UpdateSourcePosition();
            if (spell.GetPrediction(target).Hitchance >= hitChance)
            spell.Cast(target, packets());
        }

         public static bool packets()
        {
            return Config.Item("usePackets").GetValue<bool>();
        }

    }
}