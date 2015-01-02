using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterSeries.Common.M_Orbwalker;

namespace MasterSeries.Champions
{
    class Maokai : Program
    {
        public Maokai()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 525);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 475);
            Q.SetSkillshot(0.2222f, 110, 1100, false, SkillshotType.SkillshotLine);
            W.SetTargetted(0.5f, 1000);
            E.SetSkillshot(0.25f, 225, 1750, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 475, float.MaxValue, false, SkillshotType.SkillshotCircle);

            var ChampMenu = new Menu("插件", Name + "Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    ItemBool(ComboMenu, "Q", "使用 Q");
                    ItemBool(ComboMenu, "W", "使用 W");
                    ItemBool(ComboMenu, "E", "使用 E");
                    ItemBool(ComboMenu, "R", "使用 R");
                    ItemBool(ComboMenu, "RKill", "-> 如果能击杀将不使用R");
                    ItemSlider(ComboMenu, "RAbove", "-> 如果R2低于MP将取消", 20);
                    ItemBool(ComboMenu, "Item", "使用 物品");
                    ItemBool(ComboMenu, "Ignite", "如果能击杀自动点燃");
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    ItemBool(HarassMenu, "Q", "使用 Q");
                    ItemBool(HarassMenu, "W", "使用 W");
                    ItemSlider(HarassMenu, "WAbove", "-> 如果血量超过", 20);
                    ItemBool(HarassMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(HarassMenu);
                }
                var ClearMenu = new Menu("清线/清野", "Clear");
                {
                    var SmiteMob = new Menu("如果惩戒能击杀野怪", "SmiteMob");
                    {
                        ItemBool(SmiteMob, "Baron", "大龙");
                        ItemBool(SmiteMob, "Dragon", "小龙");
                        ItemBool(SmiteMob, "Red", "红BUFF");
                        ItemBool(SmiteMob, "Blue", "蓝BUFF");
                        ItemBool(SmiteMob, "Krug", "石头怪");
                        ItemBool(SmiteMob, "Gromp", "大蛤蟆");
                        ItemBool(SmiteMob, "Raptor", "啄木鸟4兄弟");
                        ItemBool(SmiteMob, "Wolf", "幽灵狼3兄弟");
                        ClearMenu.AddSubMenu(SmiteMob);
                    }
                    ItemBool(ClearMenu, "Q", "使用 Q");
                    ItemBool(ClearMenu, "W", "使用 W");
                    ItemBool(ClearMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var MiscMenu = new Menu("额外选项", "Misc");
                {
                    ItemBool(MiscMenu, "QKillSteal", "使用Q抢人头");
                    ItemBool(MiscMenu, "QAntiGap", "突进者使用Q中断");
                    ItemBool(MiscMenu, "QInterrupt", "使用Q打断");
                    ItemBool(MiscMenu, "WUnderTower", "如果敌人在塔下使用W");
                    ItemSlider(MiscMenu, "CustomSkin", "失效-换肤", 5, 0, 5).ValueChanged += SkinChanger;
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("显示范围", "Draw");
                {
                    ItemBool(DrawMenu, "Q", "Q 范围", false);
                    ItemBool(DrawMenu, "W", "W 范围", false);
                    ItemBool(DrawMenu, "E", "E 范围", false);
                    ItemBool(DrawMenu, "R", "R 范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                Config.AddSubMenu(ChampMenu);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsChannelingImportantSpell() || Player.IsRecalling()) return;
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo || Orbwalk.CurrentMode == Orbwalk.Mode.Harass)
            {
                NormalCombo(Orbwalk.CurrentMode.ToString());
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear) LaneJungClear();
            if (ItemBool("Misc", "QKillSteal")) KillSteal();
            if (ItemBool("Misc", "WUnderTower")) AutoWUnderTower();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (ItemBool("Draw", "Q") && Q.Level > 0) Utility.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "W") && W.Level > 0) Utility.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "E") && E.Level > 0) Utility.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
            if (ItemBool("Draw", "R") && R.Level > 0) Utility.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }

        private void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!ItemBool("Misc", "QAntiGap") || Player.IsDead || !Q.CanCast(gapcloser.Sender)) return;
            if (Player.Distance3D(gapcloser.Sender) <= 100) Q.Cast(gapcloser.Sender.Position, PacketCast());
        }

        private void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!ItemBool("Misc", "QInterrupt") || Player.IsDead || !Q.IsReady()) return;
            if (Player.Distance3D(unit) > 100 && W.CanCast(unit) && Player.Mana >= Q.Instance.ManaCost + W.Instance.ManaCost)
            {
                W.CastOnUnit(unit, PacketCast());
                return;
            }
            if (Q.InRange(unit) && Player.Distance3D(unit) <= 100) Q.Cast(unit.Position, PacketCast());
        }

        private void NormalCombo(string Mode)
        {
            if (targetObj == null) return;
            if (ItemBool(Mode, "E") && E.CanCast(targetObj)) E.Cast(targetObj.Position.Extend(Player.Position, Player.Distance3D(targetObj) <= E.Range - 100 ? -100 : 0), PacketCast());
            if (Mode == "Combo" && ItemBool(Mode, "R") && R.CanCast(targetObj))
            {
                if (ItemBool(Mode, "RKill") && Player.HasBuff("MaokaiDrain") && (CanKill(targetObj, R, GetRDmg(targetObj)) || ObjectManager.Get<Obj_AI_Hero>().Count(i => i.IsValidTarget(R.Range) && CanKill(i, R, GetRDmg(i)) && i != targetObj) >= 1)) R.Cast(PacketCast());
                if (Player.ManaPercentage() >= ItemSlider(Mode, "RAbove"))
                {
                    if (!Player.HasBuff("MaokaiDrain")) R.Cast(PacketCast());
                }
                else if (Player.HasBuff("MaokaiDrain")) R.Cast(PacketCast());
            }
            if (ItemBool(Mode, "W") && W.CanCast(targetObj) && (Mode == "Combo" || (Mode == "Harass" && Player.HealthPercentage() >= ItemSlider(Mode, "WAbove")))) W.CastOnUnit(targetObj, PacketCast());
            if (ItemBool(Mode, "Q") && Q.CanCast(targetObj)) Q.Cast(targetObj.Position, PacketCast());
            if (Mode == "Combo" && ItemBool(Mode, "Item") && Items.CanUseItem(Randuin) && Player.CountEnemysInRange(450) >= 1) Items.UseItem(Randuin);
            if (Mode == "Combo" && ItemBool(Mode, "Ignite") && IgniteReady()) CastIgnite(targetObj);
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            foreach (var Obj in minionObj)
            {
                if (SmiteReady() && Obj.Team == GameObjectTeam.Neutral)
                {
                    if ((ItemBool("SmiteMob", "Baron") && Obj.Name.StartsWith("SRU_Baron")) || (ItemBool("SmiteMob", "Dragon") && Obj.Name.StartsWith("SRU_Dragon")) || (!Obj.Name.Contains("Mini") && (
                        (ItemBool("SmiteMob", "Red") && Obj.Name.StartsWith("SRU_Red")) || (ItemBool("SmiteMob", "Blue") && Obj.Name.StartsWith("SRU_Blue")) ||
                        (ItemBool("SmiteMob", "Krug") && Obj.Name.StartsWith("SRU_Krug")) || (ItemBool("SmiteMob", "Gromp") && Obj.Name.StartsWith("SRU_Gromp")) ||
                        (ItemBool("SmiteMob", "Raptor") && Obj.Name.StartsWith("SRU_Razorbeak")) || (ItemBool("SmiteMob", "Wolf") && Obj.Name.StartsWith("SRU_Murkwolf"))))) CastSmite(Obj);
                }
                if (ItemBool("Clear", "E") && E.IsReady() && (minionObj.Count >= 2 || Obj.MaxHealth >= 1200))
                {
                    var posEFarm = E.GetCircularFarmLocation(minionObj);
                    E.Cast(posEFarm.MinionsHit >= 2 ? posEFarm.Position : Obj.Position.To2D(), PacketCast());
                }
                if (ItemBool("Clear", "W") && W.CanCast(Obj) && (CanKill(Obj, W, 0, W.GetDamage(Obj) > 300 ? Player.CalcDamage(Obj, Damage.DamageType.Magical, 300) : W.GetDamage(Obj)) || Obj.MaxHealth >= 1200)) W.CastOnUnit(Obj, PacketCast());
                if (ItemBool("Clear", "Q") && Q.IsReady())
                {
                    var posQFarm = Q.GetLineFarmLocation(minionObj.Where(i => i.IsValidTarget(Q.Range)).ToList());
                    if (posQFarm.MinionsHit >= 2)
                    {
                        Q.Cast(posQFarm.Position, PacketCast());
                    }
                    else if (Q.InRange(Obj)) Q.Cast(Obj.Position, PacketCast());
                }
            }
        }

        private void KillSteal()
        {
            if (!Q.IsReady()) return;
            foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsValidTarget(Q.Range) && CanKill(i, Q) && i != targetObj).OrderBy(i => i.Health).OrderBy(i => i.Distance3D(Player))) Q.Cast(Obj.Position, PacketCast());
        }

        private void AutoWUnderTower()
        {
            if (Player.UnderTurret() || !W.IsReady()) return;
            foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsValidTarget(W.Range)).OrderBy(i => i.Distance3D(Player)))
            {
                var TowerObj = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(i => i.IsValidTarget(950, false) && i.IsAlly);
                if (TowerObj != null && Obj.Distance3D(TowerObj) <= 950) W.CastOnUnit(Obj, PacketCast());
            }
        }

        private double GetRDmg(Obj_AI_Base Target)
        {
            return Player.CalcDamage(Target, Damage.DamageType.Magical, new double[] { 100, 150, 200 }[R.Level - 1] + 0.5 * Player.FlatMagicDamageMod + R.Instance.Ammo);
        }
    }
}