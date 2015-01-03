using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace xc_TwistedFate
{
    internal class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Orbwalking.Orbwalker Orbwalker;
        private static Spell Q, W;
        private static Items.Item Dfg;
        private static Menu Menu;
        private static SpellSlot SFlash;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "TwistedFate")
                return;

            SFlash = Player.GetSpellSlot("SummonerFlash");

            Q = new Spell(SpellSlot.Q, 1450);
            Q.SetSkillshot(0.25f, 40f, 1000f, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 1000);

            Dfg = new Items.Item((int)ItemId.Deathfire_Grasp, Orbwalking.GetRealAutoAttackRange(Player));

            Menu = new Menu("[xcsoft] 卡牌", "xcoft_TF", true);

            Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("走砍", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            Menu ts = Menu.AddSubMenu(new Menu("目标选择", "Target Selector"));
            TargetSelector.AddToMenu(ts);

            var wMenu = new Menu("选牌 [连招不会使用到]", "pickcard");
            wMenu.AddItem(new MenuItem("selectgold", "选择 黄牌").SetValue(new KeyBind("W".ToCharArray()[0], KeyBindType.Press)));
            wMenu.AddItem(new MenuItem("selectblue", "选择 蓝牌").SetValue(new KeyBind("E".ToCharArray()[0], KeyBindType.Press)));
            wMenu.AddItem(new MenuItem("selectred", "选择 红牌").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Menu.AddSubMenu(wMenu);

            var comboMenu  = new Menu("连招 设置", "comboop");
            comboMenu.AddItem(new MenuItem("cconly", "只对眩晕等状态目标Q (不推介)").SetValue(false));
            comboMenu.AddItem(new MenuItem("usepacket", "使用 封包 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("ignoreshield", "忽略 有护盾目标 (不推介)").SetValue(false));
            Menu.AddSubMenu(comboMenu);

            var AdditionalsMenu = new Menu("常规 设置", "additionals");
            AdditionalsMenu.AddItem(new MenuItem("goldR", "使用大招时自动选择黄牌(gate)").SetValue(true));
            Menu.AddSubMenu(AdditionalsMenu);

            var lasthitMenu = new Menu("补刀 设置", "lasthitset");
            lasthitMenu.AddItem(new MenuItem("lasthitUseW", "使用 W (蓝牌)").SetValue(false));
            lasthitMenu.AddItem(new MenuItem("lasthitbluemana", "蓝牌补刀最低蓝量 % <").SetValue(new Slider(20, 0, 100)));
            Menu.AddSubMenu(lasthitMenu);

            var laneclearMenu = new Menu("清线 设置", "laneclearset");
            laneclearMenu.AddItem(new MenuItem("laneclearUseW", "使用 W (蓝牌 默认红牌)").SetValue(true));
            laneclearMenu.AddItem(new MenuItem("laneclearbluemana", " 蓝牌代替红牌清线|蓝量低于 % ").SetValue(new Slider(20, 0, 100)));
            laneclearMenu.AddItem(new MenuItem("laneclearmc", "红牌清线|小兵数量 >=").SetValue(new Slider(3, 2, 5)));
            Menu.AddSubMenu(laneclearMenu);

            var Drawings = new Menu("范围 设置", "Drawings");
            Drawings.AddItem(new MenuItem("AAcircle", "平A 范围").SetValue(true));
            Drawings.AddItem(new MenuItem("FAAcircle", "闪现 + 平A 范围").SetValue(true));
            Drawings.AddItem(new MenuItem("Qcircle", "Q 范围").SetValue(new Circle(true, Color.LightSkyBlue)));
            Drawings.AddItem(new MenuItem("Rcircle", "R 范围").SetValue(new Circle(true, Color.LightSkyBlue)));
            Drawings.AddItem(new MenuItem("RcircleMap", "R 范围 (小地图)").SetValue(new Circle(true, Color.White)));
            Drawings.AddItem(new MenuItem("drawMinionLastHit", "小兵补刀标记").SetValue(new Circle(true, Color.GreenYellow)));
            Drawings.AddItem(new MenuItem("drawMinionNearKill", "小兵补刀提示").SetValue(new Circle(true, Color.Gray)));
            Menu.AddSubMenu(Drawings);

            var predMenu = new Menu("预测", "pred");
            predMenu.AddItem(new MenuItem("kappa", "可能是最强的卡牌"));
            Menu.AddSubMenu(predMenu);

            var havefun = new MenuItem("Have fun!", "玩得 开心 !");
            Menu.AddItem(havefun);

            Menu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.BeforeAttack += OrbwalkingOnBeforeAttack;

            Game.PrintChat("<font color = \"#33CCCC\">[xcsoft] 鍗＄墝 -</font> 鍔犺浇鎴愬姛!姹夊寲by浜岀嫍!QQ缇361630847!");
        }

        private static void OrbwalkingOnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                args.Process = CardSelector.Status != SelectStatus.Selecting && Environment.TickCount - CardSelector.LastWSent > 300;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                Combo();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                Harras();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                Lasthit();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                LaneClear();

            if (Menu.Item("selectgold").GetValue<KeyBind>().Active)
                CardSelector.StartSelecting(Cards.Yellow);

            if (Menu.Item("selectblue").GetValue<KeyBind>().Active)
                CardSelector.StartSelecting(Cards.Blue);

            if (Menu.Item("selectred").GetValue<KeyBind>().Active)
                CardSelector.StartSelecting(Cards.Red);
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "gate" && Menu.Item("goldR").GetValue<bool>())
                CardSelector.StartSelecting(Cards.Yellow);
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var Qcircle = Menu.Item("Qcircle").GetValue<Circle>();
            if (Q.IsReady() && Qcircle.Active)
                Utility.DrawCircle(Player.Position, Q.Range, Qcircle.Color);

            Color temp = Color.Gold;

            if (Menu.Item("AAcircle").GetValue<bool>())
            {
                if (W.IsReady())
                {
                    var wName = Player.Spellbook.GetSpell(SpellSlot.W).Name;

                    if (wName == "goldcardlock") temp = Color.Gold;
                    else if (wName == "bluecardlock") temp = Color.Blue;
                    else if (wName == "redcardlock") temp = Color.Red;
                    else if (wName == "PickACard") temp = Color.LightGreen;

                    Utility.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player), temp);
                }
                else
                    Utility.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player), Color.Gray);
            }

            if (Menu.Item("FAAcircle").GetValue<bool>())
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(Player) + 400, TargetSelector.DamageType.Magical, false);

                if (target != null && SFlash != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SFlash) == SpellState.Ready)
                {
                    Utility.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player) + 400, Color.Gold);//AA+Flash Range

                    if (!target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
                    {
                        Utility.DrawCircle(target.Position, 50, Color.Gold);

                        var targetpos = Drawing.WorldToScreen(target.Position);

                        Drawing.DrawText(targetpos[0] - 60, targetpos[1] + 20, Color.Gold, "Flash+Stun possible");
                    }

                }
                else
                    Utility.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player) + 400, Color.Gray);//AA+Flash Range
            }

            var drawMinionLastHit = Menu.Item("drawMinionLastHit").GetValue<Circle>();
            var drawMinionNearKill = Menu.Item("drawMinionNearKill").GetValue<Circle>();
            if (drawMinionLastHit.Active || drawMinionNearKill.Active)
            {
                var xMinions =
                    MinionManager.GetMinions(ObjectManager.Player.Position, ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + 300, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

                foreach (var xMinion in xMinions)
                {
                    if (drawMinionLastHit.Active && ObjectManager.Player.GetAutoAttackDamage(xMinion, true) >= xMinion.Health)
                    {
                        Utility.DrawCircle(xMinion.Position, xMinion.BoundingRadius, drawMinionLastHit.Color);
                    }
                    else if (drawMinionNearKill.Active && ObjectManager.Player.GetAutoAttackDamage(xMinion, true) * 2 >= xMinion.Health)
                    {
                        Utility.DrawCircle(xMinion.Position, xMinion.BoundingRadius, drawMinionNearKill.Color);
                    }
                }
            }
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var Rcircle = Menu.Item("Rcircle").GetValue <Circle>();

            if (Rcircle.Active) 
                Utility.DrawCircle(Player.Position, 5500, Rcircle.Color);

            var Rcirclemap = Menu.Item("RcircleMap").GetValue<Circle>();

            if (Rcirclemap.Active) 
            Utility.DrawCircle(Player.Position, 5500, Rcirclemap.Color, 1, 21, true);
        }

        static void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(1450, TargetSelector.DamageType.Magical, Menu.Item("ignoreshield").GetValue<bool>());
            
            if (Dfg.IsReady())
            {
                if (target.IsValidTarget(Dfg.Range))
                    Dfg.Cast(target);
            }

            if (W.IsReady())
            {
                if (target.IsValidTarget(W.Range) && target is Obj_AI_Hero)
                    CardSelector.StartSelecting(Cards.Yellow);
            }

            if (Q.IsReady())
            {
                if (target.IsValidTarget(Q.Range) && target is Obj_AI_Hero)
                {
                    var pred = Q.GetPrediction(target);

                    if (Menu.Item("cconly").GetValue<bool>())
                    {
                        if (pred.Hitchance == HitChance.VeryHigh || pred.Hitchance == HitChance.Immobile || pred.Hitchance == HitChance.Dashing)
                        {
                            foreach (var buff in target.Buffs)
                            {
                                if (buff.Type == BuffType.Stun || buff.Type == BuffType.Taunt || buff.Type == BuffType.Snare || buff.Type == BuffType.Suppression || buff.Type == BuffType.Charm || buff.Type == BuffType.Fear || buff.Type == BuffType.Flee || buff.Type == BuffType.Slow)
                                    Q.Cast(target, Menu.Item("usepacket").GetValue<bool>());
                            }
                        } 
                    }
                    else if (pred.Hitchance == HitChance.High || pred.Hitchance == HitChance.Dashing || pred.Hitchance == HitChance.Immobile)
                        Q.Cast(target);
                    
                }
            }
        }

        static void Harras()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Q.IsReady())
            {
                if (target.IsValidTarget(Q.Range) && Q.GetPrediction(target).Hitchance == HitChance.High)
                    Q.Cast(target);
            }
        }

        static void Lasthit()
        {
            if (W.IsReady())
            {
                if (Menu.Item("lasthitUseW").GetValue<bool>())
                {
                    if (Utility.ManaPercentage(Player) < Menu.Item("lasthitbluemana").GetValue<Slider>().Value)
                    {
                        var xMinions = MinionManager.GetMinions(Player.Position, 700, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

                        foreach (var xMinion in xMinions)
                        {
                            if (Player.GetAutoAttackDamage(xMinion, true) * 3 >= xMinion.Health)
                            {
                                CardSelector.StartSelecting(Cards.Blue);
                            }
                        }
                    }
                }
            }
        }

        static void LaneClear()
        {
            if (W.IsReady())
            {
                int minionsInWRange = MinionManager.GetMinions(Player.Position, W.Range).Count;

                if (Menu.Item("laneclearUseW").GetValue<bool>())
                {
                    if (Utility.ManaPercentage(Player) > Menu.Item("laneclearbluemana").GetValue<Slider>().Value)
                    {
                        if (minionsInWRange >= Menu.Item("laneclearmc").GetValue<Slider>().Value)
                            CardSelector.StartSelecting(Cards.Red);
                        else
                            CardSelector.StartSelecting(Cards.Blue);
                    }
                    else
                        CardSelector.StartSelecting(Cards.Blue);
                }
            }
        }
    }
}
