
#region
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
#endregion

namespace MasterYiQx
{
    internal class Program
    {
        public const string ChampionName = "MasterYi";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        
        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;
        
        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell E;
        public static Spell W;
        public static Spell R;

        private static readonly SpellSlot SmiteSlot = Player.GetSpellSlot("SummonerSmite");
        private static readonly SpellSlot IgniteSlot = Player.GetSpellSlot("SummonerDot");

        public static Items.Item Tiamat = new Items.Item(3077, 375);
        public static Items.Item Hydra = new Items.Item(3074, 375); 

        public static int DelayTick = 0;
        //Menu
        public static Menu Config;
        public static Menu TargetSelectorMenu;
        public static Menu MenuExtras;
        public static Menu MenuTargetedItems;
        public static Menu MenuNonTargetedItems;
        
        private static void Main(string[] args)
        {
            
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != "MasterYi") return;
            if (Player.IsDead) return;
            
            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 200f);
            R = new Spell(SpellSlot.R);
            
            Q.SetTargetted(0.50f, 75f);
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            
           
            //Create the menu
            Config = new Menu("xQx | 易大师", "MasterYi", true);

            TargetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            Config.AddSubMenu(TargetSelectorMenu);
            
            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Orbwalker.SetAttack(true);
            
            // Combo
            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQComboDontUnderTurret", "禁止塔下 Q")
                .SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));

            Config.SubMenu("Combo")
                  .AddItem(
                       new MenuItem("ComboActive", "连招!").SetValue(new KeyBind("Z".ToCharArray()[0],
                           KeyBindType.Press)));
            
            // Harass
            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarassDontUnderTurret", "禁止塔下 Q")
                .SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassMode", "骚扰 模式: ").SetValue(new StringList(new[] {"Q+W", "Q+E", "Default"})));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("HarassMana", "骚扰最低蓝量: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("Harass")
                  .AddItem(new MenuItem("HarassActive", "骚扰!").SetValue(new KeyBind("C".ToCharArray()[0],
                      KeyBindType.Press)));
            
            // Lane Clear
            Config.AddSubMenu(new Menu("清线", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "使用 Q").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClearDontUnderTurret", "禁止塔下 Q")
                .SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "使用 E").SetValue(false));
            Config.SubMenu("LaneClear")
                .AddItem(new MenuItem("LaneClearMana", "清线最低蓝量: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("LaneClear")
                  .AddItem(new MenuItem("LaneClearActive", "清线!").SetValue(new KeyBind("V".ToCharArray()[0],
                      KeyBindType.Press)));
            
            // Jungling Farm!
            Config.AddSubMenu(new Menu("清野", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJungleFarm", "使用 Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJungleFarm", "使用 E").SetValue(false));
            Config.SubMenu("JungleFarm")
                .AddItem(new MenuItem("JungleFarmMana", "清线最低蓝量: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("JungleFarm")
                .AddItem(new MenuItem("AutoSmite", "自动 惩戒").SetValue<KeyBind>(new KeyBind('N', KeyBindType.Toggle)));
            Config.SubMenu("JungleFarm")
                  .AddItem(new MenuItem("JungleFarmActive", "清野!").SetValue(new KeyBind("V".ToCharArray()[0],
                      KeyBindType.Press)));

            Config.AddSubMenu(new Menu("回复 设置", "HealSettings"));
            Config.SubMenu("HealSettings").AddItem(new MenuItem("HealUseW", "使用 W").SetValue(true));
            Config.SubMenu("HealSettings")
                .AddItem(new MenuItem("HealPercent", "回复最低生命值").SetValue(new Slider(50, 100, 0)));

            Config.SubMenu("HealSettings")
                .AddItem(new MenuItem("JungleFarmMana", "清野最低蓝量:").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("HealSettings")
                .AddItem(new MenuItem("AutoSmite", "自动 惩戒").SetValue<KeyBind>(new KeyBind('N', KeyBindType.Toggle)));
            
            // Extras
            MenuExtras = new Menu("额外", "Extras");
            Config.AddSubMenu(MenuExtras);

            // Extras -> Use Items 
            var menuUseItems = new Menu("使用 物品", "menuUseItems");
            MenuExtras.AddSubMenu(menuUseItems);

            // Extras -> Use Items -> Targeted Items
            MenuTargetedItems = new Menu("攻击 物品", "menuTargetItems");
            menuUseItems.AddSubMenu(MenuTargetedItems);
            MenuTargetedItems.AddItem(new MenuItem("item3153", "破败").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3143", "兰盾").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3144", "比尔吉沃特弯刀").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3146", "科技枪").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3184", "冰锤 ").SetValue(true));
            
            // Extras -> Use Items -> AOE Items
            MenuNonTargetedItems = new Menu("AOE 物品", "menuNonTargetedItems");
            menuUseItems.AddSubMenu(MenuNonTargetedItems);
            MenuNonTargetedItems.AddItem(new MenuItem("item3180", "奥丁面纱").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3131", "神圣之剑").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3074", "贪欲九头蛇").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3142", "幽梦之灵").SetValue(true));
            
            // Drawing
            Config.AddSubMenu(new Menu("范围", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQRange", "Q 范围").SetValue(new Circle(true,
                System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawSmite", "惩戒 范围").SetValue(new Circle(false,
                System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            new PotionManager();
            new AssassinManager();
            Config.AddToMainMenu();


            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            
            Game.PrintChat(String.Format("xQx 鏄撳ぇ甯堜辅鍔犺浇鎴愬姛锛佹眽鍖朾y Bbyyyyy锛丵Q缇や辅361630847"));
        }
        
        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawQRange = Config.Item("DrawQRange").GetValue<Circle>();
            if (drawQRange.Active)
            {
                Utility.DrawCircle(Player.Position, Q.Range, drawQRange.Color);
            } 
        }
        
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Orbwalking.CanMove(100))
                return;

            if (DelayTick - Environment.TickCount <= 250)
            {
                UseSummoners();
                DelayTick = Environment.TickCount;
            }


            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                var assassinRange = TargetSelectorMenu.Item("AssassinRange").GetValue<Slider>().Value;
                Obj_AI_Hero vTarget = null;
                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                enemy =>
                                    enemy.Team != Player.Team && !enemy.IsDead && enemy.IsVisible &&
                                    TargetSelectorMenu.Item("Assassin" + enemy.ChampionName) != null &&
                                    TargetSelectorMenu.Item("Assassin" + enemy.ChampionName).GetValue<bool>())
                            .OrderBy(enemy => enemy.Distance(Game.CursorPos)))
                {
                    vTarget = Player.Distance(enemy) < assassinRange ? enemy : null;
                }
                Combo(vTarget);
            }
            
            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                var existsMana = Player.MaxMana / 100 * Config.Item("HarassMana").GetValue<Slider>().Value;
                if (Player.Mana >= existsMana)
                    Harass();
            }

            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {
                var existsMana = Player.MaxMana / 100 * Config.Item("LaneClearMana").GetValue<Slider>().Value;
                if (Player.Mana >= existsMana)
                    LaneClear();
            }

            if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
            {
                var existsMana = Player.MaxMana / 100 * Config.Item("JungleFarmMana").GetValue<Slider>().Value;
                if (Player.Mana >= existsMana)
                    JungleFarm();
            }

            if (Config.Item("HealUseW").GetValue<KeyBind>().Active)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => !enemy.IsAlly))
                {
                    if (Player.Health > enemy.Health && Player.Level >= enemy.Level)
                        return;
                    if (Player.Distance(enemy) > Q.Range + 200 && enemy.IsValid && enemy.IsVisible)
                        return;
                }

                var existsHp = Player.MaxMana / 100 * Config.Item("HealPercent").GetValue<Slider>().Value;
                if (Player.Health <= existsHp)
                    W.Cast(Player, true);
            }
        }
        
        private static void Combo(Obj_AI_Hero vTarget)
        {
            if (vTarget == null)
                vTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            var useQDontUnderTurret = Config.Item("UseQComboDontUnderTurret").GetValue<bool>();

            if (Q.IsReady() && useQ && vTarget != null)
            {
                if (useQDontUnderTurret)
                {
                    if (!Utility.UnderTurret(vTarget))
                        Q.CastOnUnit(vTarget);
                } else
                {
                    Q.CastOnUnit(vTarget);
                }
            }

            if (Player.Distance(vTarget) <= E.Range + 50)
                UseItems(vTarget, true);
            
            if (vTarget != null)
                UseItems(vTarget);

            if (E.IsReady() && useE && Player.Distance(vTarget) <= E.Range)
                E.Cast();

            if (IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (vTarget != null && Player.GetSummonerSpellDamage(vTarget, Damage.SummonerSpell.Ignite) > vTarget.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, vTarget);
                }
            }

            if (R.IsReady() && useR && vTarget != null)
            {
                if (Utility.CountEnemysInRange((int)Q.Range) >= 2)
                { 
                    R.CastOnUnit(Player);
                }
            }

            if (Tiamat.IsReady() && Player.Distance(vTarget) <= Tiamat.Range)
                Tiamat.Cast();

            if (Hydra.IsReady() && Player.Distance(vTarget) <= Hydra.Range)
                Tiamat.Cast();

        }
        
        private static void Harass()
        {
          
        }

        private static void LaneClear()
        {
            var useQ = Config.Item("UseQLaneClear").GetValue<bool>();
            var useE = Config.Item("UseELaneClear").GetValue<bool>();
            var useQDontUnderTurret = Config.Item("UseQLaneClearDontUnderTurret").GetValue<bool>();

            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All,
                MinionTeam.NotAlly);


            if (allMinions.Count >= 2)
            {
                if (Tiamat.IsReady())
                    Tiamat.Cast();

                if (Hydra.IsReady())
                    Hydra.Cast();
            }

            if (Q.IsReady() && useQ)
            {
                var closestMinion = new Obj_AI_Base();
                if (allMinions.Any())
                {
                    foreach (var minion in allMinions)
                    {
                        if (allMinions.IndexOf(minion) == 0)
                        {
                            closestMinion = minion;
                        }
                        else if (Player.Distance(minion.Position) < Player.Distance(closestMinion.Position))
                        {
                            closestMinion = minion;
                        }
                    }
                    if (!closestMinion.IsValidTarget())
                        return;

                    Q.Cast(closestMinion);
                }
            }
        }

        private static void JungleFarm()
        {
            if (!Config.Item("JungleFarmActive").GetValue<KeyBind>().Active) return;

            var useQ = Config.Item("UseQJungleFarm").GetValue<bool>();
            var useE = Config.Item("UseEJungleFarm").GetValue<bool>();

            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0) return;

            var mob = mobs[0];
            if (useE && E.IsReady() && Player.Distance(mob) < Orbwalking.GetRealAutoAttackRange(Player))
            {
                E.Cast(mob);
            }
            
            if (useQ && Q.IsReady())
            {
                Q.Cast(mob);
            }

            if (mobs.Count >= 2)
            {
                if (Tiamat.IsReady())
                    Tiamat.Cast();

                if (Hydra.IsReady())
                    Hydra.Cast();
            }
        }

        private static InventorySlot GetInventorySlot(int ID)
        {
            return
                ObjectManager.Player.InventoryItems.FirstOrDefault(
                    item =>
                        (item.Id == (ItemId) ID && item.Stacks >= 1) || (item.Id == (ItemId) ID && item.Charges >= 1));
        }

        public static void UseItems(Obj_AI_Hero vTarget, bool useNonTargetedItems = false)
        {
            if (vTarget == null) return;
            foreach (var itemID in from menuItem in MenuTargetedItems.Items
                                   let useItem = MenuTargetedItems.Item(menuItem.Name).GetValue<bool>()
                                   where useItem
                                   select Convert.ToInt16(menuItem.Name.ToString().Substring(4, 4))
                                       into itemID
                                       where Items.HasItem(itemID) && Items.CanUseItem(itemID) && GetInventorySlot(itemID) != null
                                   select itemID)
            {
                Items.UseItem(itemID, vTarget);
            }

            if (!useNonTargetedItems)
                return;

            foreach (var itemID in from menuItem in MenuNonTargetedItems.Items 
                                   let useItem = MenuNonTargetedItems.Item(menuItem.Name).GetValue<bool>() 
                                   where useItem 
                                   select Convert.ToInt16(menuItem.Name.ToString().Substring(4, 4)) 
                                        into itemID 
                                        where Items.HasItem(itemID) && Items.CanUseItem(itemID) && GetInventorySlot(itemID) != null 
                                   select itemID)
            {
                Items.UseItem(itemID);
            }
        }

        private static void UseSummoners()
        {
            if (SmiteSlot == SpellSlot.Unknown)
                return;

            if (!Config.Item("AutoSmite").GetValue<KeyBind>().Active) return;

            string[] monsterNames = { "LizardElder", "AncientGolem", "Worm", "Dragon" };
            var firstOrDefault = Player.Spellbook.Spells.FirstOrDefault(
                spell => spell.Name.Contains("mite"));
            if (firstOrDefault == null) return;

            var vMonsters = MinionManager.GetMinions(Player.ServerPosition, firstOrDefault.SData.CastRange[0],
                MinionTypes.All, MinionTeam.NotAlly);
            foreach (
                var vMonster in
                    vMonsters.Where(
                        vMonster =>
                            vMonster != null && !vMonster.IsDead && !Player.IsDead && !Player.IsStunned &&
                            SmiteSlot != SpellSlot.Unknown &&
                            Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                        .Where(
                            vMonster =>
                                (vMonster.Health < Player.GetSummonerSpellDamage(vMonster, Damage.SummonerSpell.Smite)) &&
                                (monsterNames.Any(name => vMonster.BaseSkinName.StartsWith(name))))) 
            {
                Player.Spellbook.CastSpell(SmiteSlot, vMonster);
            }
        }
    }
}
