#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Mid_or_Feed.Champions
{
    internal class Lux : Plugin
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static GameObject EGameObject;

        public Lux()
        {
            Q = new Spell(SpellSlot.Q, 1300);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 3340);

            Q.SetSkillshot(0.25f, 70, 1200, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.5f, 150, 1200, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 275, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1, 190, float.MaxValue, false, SkillshotType.SkillshotLine);

            GameObject.OnCreate += delegate(GameObject sender, EventArgs args)
            {
                //Noticed a different E game object name while looking through Kurisu's oracle, credits to him.
                if (sender.Name.Contains("LuxLightstrike_tar_green") || sender.Name.Contains("LuxLightstrike_tar_red"))
                    EGameObject = sender;
            };

            GameObject.OnDelete += delegate(GameObject sender, EventArgs args)
            {
                //Noticed a different E game object name while looking through Kurisu's oracle, credits to him.
                if (sender.Name.Contains("LuxLightstrike_tar_green") || sender.Name.Contains("LuxLightstrike_tar_red"))
                    EGameObject = null;
            };

            Game.OnGameUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
            Obj_AI_Base.OnTeleport += ObjAiHeroOnOnTeleport;           
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;

            PrintChat("Lux loaded!");
        }

        private void ObjAiHeroOnOnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
        {
            if (!GetBool("rKSRecall"))
                return;

            var decoded = Packet.S2C.Teleport.Decoded(sender, args);
            var hero = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(decoded.UnitNetworkId);

            if (hero.IsAlly || decoded.Type != Packet.S2C.Teleport.Type.Recall || decoded.Status != Packet.S2C.Teleport.Status.Start)
                return;

            var rDamage = Player.GetSpellDamage(hero, SpellSlot.R);
            if (rDamage > hero.Health)
                R.Cast(hero);

        }

        public static bool EActivated
        {
            get { return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1 || EGameObject != null; }
        }

        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!GetBool("qGapcloser"))
                return;

            Q.Cast(gapcloser.Sender, Packets);
        }


        public static bool HasPassive(Obj_AI_Hero hero)
        {
            return hero.HasBuff("luxilluminatingfraulein");
        }

        private void GameOnOnGameUpdate(EventArgs args)
        {
            switch (OrbwalkerMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    DoHarass();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    DoCombo();
                    break;
            }

            if (GetBool("autoW"))
                AutoW();

            if (GetBool("rKS"))
                ItsKillSecure();

            if (GetBool("stealBlue"))
                StealBlue();

            if (GetBool("stealRed"))
                StealRed();
        }

        private void StealBlue()
        {
            if (!R.IsReady()) return;

            var blueBuffs = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Name.ToUpper().Equals("SRU_BLUE"));
            foreach (
                var blueBuff in
                    blueBuffs.Where(
                        blueBuff => Player.GetSpellDamage(blueBuff, SpellSlot.R) > blueBuff.Health))
            {
                R.Cast(blueBuff, Packets);
            }
        }

        private void StealRed()
        {
            if (!R.IsReady()) return;

            var redBuffs = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Name.ToUpper().Equals("SRU_RED"));
            foreach (
                var redBuff in
                    redBuffs.Where(
                        redBuff => Player.GetSpellDamage(redBuff, SpellSlot.R) > redBuff.Health))
            {
                R.Cast(redBuff, Packets);
            }
        }

        private void ItsKillSecure()
        {
            if (!R.IsReady())
                return;
            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsValidTarget())
                        .Where(x => !x.IsZombie)
                        .Where(enemy => Player.GetDamageSpell(enemy, SpellSlot.R).CalculatedDamage > enemy.Health))
            {
                R.Cast(enemy, Packets);
                return;
            }
        }

        private void AutoW()
        {
            if (!W.IsReady() || Player.IsRecalling())
                return;

            foreach (
                var ally in from ally in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly).Where(x => !x.IsDead)
                    let allyPercent = ally.Health/ally.MaxHealth*100
                    let healthPercent = GetValue<Slider>("autoWPercent").Value
                    where healthPercent >= allyPercent
                    select ally)
            {
                W.Cast(ally, Packets);
                return;
            }
        }

        private void CastQ(Obj_AI_Base target)
        {
            var input = Q.GetPrediction(target);
            var col = Q.GetCollision(Player.ServerPosition.To2D(), new List<Vector2> {input.CastPosition.To2D()});
            var minions = col.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);

            if (minions <= 1)
                Q.Cast(input.CastPosition);
        }

        private void CastE(Obj_AI_Hero target)
        {
            if (EActivated)
            {
                if (
                    !ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsEnemy)
                        .Where(x => !x.IsDead)
                        .Any(enemy => enemy.Distance(EGameObject.Position) <= E.Width)) return;

                var isInAaRange = Player.Distance(target) <= Orbwalking.GetRealAutoAttackRange(Player);

                if (isInAaRange && !HasPassive(target))
                    E.Cast();

                // Pop E if the target is out of AA range
                if (!isInAaRange)
                    E.Cast();
            }
            else
            {
                E.Cast(target);
            }
        }

        private void DoCombo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            var useQ = GetBool("useQ");
            var useW = GetBool("useW");
            var useE = GetBool("useE");
            var useR = GetBool("useR");
            var useRKillable = GetBool("useRKillable");

            if (useQ && !HasPassive(target) && Q.IsReady())
            {
                CastQ(target);
            }

            if (useW && W.IsReady())
            {
                W.Cast(Game.CursorPos);
            }

            if (useE && E.IsReady())
            {
                CastE(target);
            }

            if (useR && R.IsReady())
            {
                R.Cast(target);
            }

            if (!useRKillable) return;
            var killable = Player.GetSpellDamage(target, SpellSlot.R)> target.Health;
            if (killable && R.IsReady())
                R.Cast(target);
        }

        private void DoHarass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            var useQ = GetBool("useQHarass");
            var useE = GetBool("useEHarass");

            if (useQ && !HasPassive(target) && Q.IsReady())
            {
                CastQ(target);
            }

            if (!useE || !E.IsReady()) return;
            CastE(target);
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = GetBool("drawQ");
            var drawW = GetBool("drawW");
            var drawE = GetBool("drawE");
            var drawR = GetBool("drawR");
            var drawRMinimap = GetBool("drawRMinimap");

            var p = Player.Position;

            if (drawQ)
            {
                Utility.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawW)
            {
                Utility.DrawCircle(p, W.Range, W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Utility.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Utility.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawRMinimap)
            {
                Utility.DrawCircle(p, R.Range, R.IsReady() ? Color.Aqua : Color.Red, 2, 30, true);
            }
        }

        public override float GetComboDamage(Obj_AI_Hero target)
        {
            double dmg = 0;
            var p = Player;

            if (Q.IsReady())
                dmg += p.GetDamageSpell(target, SpellSlot.Q).CalculatedDamage;

            if (E.IsReady())
                dmg += p.GetDamageSpell(target, SpellSlot.E).CalculatedDamage;

            if (R.IsReady())
                dmg += p.GetDamageSpell(target, SpellSlot.R).CalculatedDamage;

            return (float) dmg;
        }

        public override void Combo(Menu comboMenu)
        {
            comboMenu.AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "使用 W").SetValue(false));
            comboMenu.AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "使用 R").SetValue(false));
            comboMenu.AddItem(new MenuItem("useRKillable", "使用R 可击杀").SetValue(true));

            comboMenu.Item("useRKillable").ValueChanged += delegate(object sender, OnValueChangeEventArgs args)
            {
                if (args.GetNewValue<bool>() && GetBool("useR"))
                    Menu.Item("useR").SetValue(false);
            };

            comboMenu.Item("useR").ValueChanged += delegate(object sender, OnValueChangeEventArgs args)
            {
                if (args.GetNewValue<bool>() && GetBool("useRKillable"))
                    Menu.Item("useRKillable").SetValue(false);
            };
        }

        public override void Harass(Menu harassMenu)
        {
            harassMenu.AddItem(new MenuItem("useQHarass", "使用 Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("useEHarass", "使用 E").SetValue(true));
        }

        public override void ItemMenu(Menu itemsMenu)
        {
            //itemsMenu.AddItem(new MenuItem("useDFG", "Use DFG").SetValue(true));
        }

        public override void Misc(Menu miscMenu)
        {
            miscMenu.AddItem(new MenuItem("rKS", "使用R抢人头").SetValue(true));
            miscMenu.AddItem(new MenuItem("rKSRecall", "对进入战争迷雾的敌人使用R").SetValue(true));
            miscMenu.AddItem(new MenuItem("qGapcloser", "Q 防止突进").SetValue(true));
            miscMenu.AddItem(new MenuItem("stealBlue", "R偷 蓝 buff").SetValue(true));
            miscMenu.AddItem(new MenuItem("stealRed", "R偷 红 Buff").SetValue(false));
            miscMenu.AddItem(new MenuItem("autoW", "自动 W").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoWPercent", "自动W|血量百分比").SetValue(new Slider(15, 1)));
        }

        public override void Drawings(Menu drawingMenu)
        {
            drawingMenu.AddItem(new MenuItem("drawQ", "范围 Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "范围 W").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawE", "范围 E").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawR", "范围 R").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawRMinimap", "范围 R(小地图)").SetValue(true));
        }
    }
}