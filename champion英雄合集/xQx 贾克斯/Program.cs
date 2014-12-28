#region
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
#endregion

namespace JaxQx
{
    internal class Program
    {
        public const string ChampionName = "Jax";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        
        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;
        
        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell E;
        public static Spell W;
        public static Spell R;

        public static string[] xWards =
        {
            "RelicSmallLantern", "RelicLantern", "SightWard", "wrigglelantern",
            "ItemGhostWard", "VisionWard", "BantamTrap", "JackInTheBox", "CaitlynYordleTrap", "Bushwhack"
        };

        public static Map map;

        private static SpellSlot IgniteSlot;
        private static SpellSlot SmiteSlot;
        public static float SmiteRange = 700f;
        public static float wardRange = 600f;
        public static int DelayTick = 0;
        //Menu
        public static Menu Config;
        public static Menu MenuExtras;
        public static Menu MenuTargetedItems;
        public static Menu MenuNonTargetedItems;
        public static Menu TargetSelectorMenu;
        
        private static void Main(string[] args)
        {
            map = new Map();
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != "Jax") return;
            if (Player.IsDead) return;
            
            Q = new Spell(SpellSlot.Q, 680f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 190f);
            R = new Spell(SpellSlot.R);
            
            Q.SetTargetted(0.50f, 75f);
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            
            IgniteSlot = Player.GetSpellSlot("SummonerDot");
            SmiteSlot = Player.GetSpellSlot("SummonerSmite");
            
            //Create the menu
            Config = new Menu("xQx | 贾克斯", "Jax", true);
            
            TargetSelectorMenu = new Menu("目标 选择", "Target Selector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            Config.AddSubMenu(TargetSelectorMenu);
            
            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Orbwalker.SetAttack(true);
            
            // Combo
            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQComboDontUnderTurret", "禁用 塔下 Q")
                .SetValue(true));
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboUseQMinRange", "最小 Q 范围").SetValue(new Slider(250, (int) Q.Range)));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用 W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));

            Config.SubMenu("Combo")
                  .AddItem(
                       new MenuItem("ComboActive", "连招!").SetValue(new KeyBind("Z".ToCharArray()[0],
                           KeyBindType.Press)));
            
            // Harass
            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarassDontUnderTurret", "禁用 塔下 Q")
                .SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassMode", "骚扰 模式: ").SetValue(new StringList(new[] {"Q+W", "Q+E", "Default"})));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("HarassMana", "骚扰最低蓝量: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("Harass")
                  .AddItem(new MenuItem("HarassActive", "骚扰").SetValue(new KeyBind("C".ToCharArray()[0],
                      KeyBindType.Press)));
            
            // Lane Clear
            Config.AddSubMenu(new Menu("清线", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "使用 Q").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClearDontUnderTurret", "禁用 塔下 Q")
                .SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseWLaneClear", "使用 W").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "使用 E").SetValue(false));
            Config.SubMenu("LaneClear")
                .AddItem(new MenuItem("LaneClearMana", "清线最低蓝量: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("LaneClear")
                  .AddItem(new MenuItem("LaneClearActive", "清线").SetValue(new KeyBind("V".ToCharArray()[0],
                      KeyBindType.Press)));
            
            // Jungling Farm
            Config.AddSubMenu(new Menu("清野", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJungleFarm", "使用 Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJungleFarm", "使用 W").SetValue(false));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJungleFarm", "使用 E").SetValue(false));
            Config.SubMenu("JungleFarm")
                .AddItem(new MenuItem("JungleFarmMana", "清野最低蓝量: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("JungleFarm")
                .AddItem(new MenuItem("AutoSmite", "自动 惩戒").SetValue(new KeyBind('N', KeyBindType.Toggle)));

            Config.SubMenu("JungleFarm")
                  .AddItem(new MenuItem("JungleFarmActive", "清野").SetValue(new KeyBind("V".ToCharArray()[0],
                      KeyBindType.Press)));
            
            // Extra
            MenuExtras = new Menu("额外", "Extras");
            Config.AddSubMenu(MenuExtras);
            MenuExtras.AddItem(new MenuItem("InterruptSpells", "中断法术").SetValue(true));

            Config.AddSubMenu(new Menu("顺眼", "WardJump"));
            Config.SubMenu("WardJump")
                .AddItem(new MenuItem("Ward", "顺眼"))
                .SetValue(new KeyBind('T', KeyBindType.Press));
            
            // Extras -> Use Items 
            Menu menuUseItems = new Menu("使用物品", "menuUseItems");
            Config.SubMenu("Extras").AddSubMenu(menuUseItems);

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
            MenuNonTargetedItems.AddItem(new MenuItem("item3074", "贪欲九天蛇").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3077", "提亚马特").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3142", "幽梦之灵").SetValue(true));
            
            // Drawing
            Config.AddSubMenu(new Menu("范围", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("DrawQRange", "Q 范围").SetValue(new Circle(true,
                        System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("DrawQMinRange", "最小 Q 范围").SetValue(new Circle(true,
                        System.Drawing.Color.GreenYellow)));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("DrawWard", "顺眼 范围").SetValue(new Circle(false,
                        System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("DrawSmiteRange", "惩戒 范围").SetValue(new Circle(false, System.Drawing.Color.Indigo)));

            new PotionManager();
            new AssassinManager();
            Config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            Game.PrintChat(
                String.Format(
                    "<font color='#70DBDB'>xQx | </font> <font color='#FFFFFF'>{0}</font> <font color='#70DBDB'> Loaded!</font>",
                    ChampionName));
        }
        
        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawQRange = Config.Item("DrawQRange").GetValue<Circle>();
            if (drawQRange.Active)
            {
                Utility.DrawCircle(Player.Position, Q.Range, drawQRange.Color, 1, 15);
            }

            var drawWard = Config.Item("DrawWard").GetValue<Circle>();
            if (drawWard.Active)
            {
                Utility.DrawCircle(Player.Position, wardRange, drawWard.Color, 1, 15);
            }

            var drawSmiteRange = Config.Item("DrawSmiteRange").GetValue<Circle>();
            if (drawSmiteRange.Active && Config.Item("AutoSmite").GetValue<KeyBind>().Active)
            {
                Utility.DrawCircle(Player.Position, SmiteRange, drawWard.Color, 1, 15);
            }

            var drawMinQRange = Config.Item("DrawQMinRange").GetValue<Circle>();
            if (drawMinQRange.Active)
            {
                var minQRange = Config.Item("ComboUseQMinRange").GetValue<Slider>().Value;
                Utility.DrawCircle(Player.Position, minQRange, drawMinQRange.Color, 1, 15);
            }

        }
        private static Obj_AI_Hero GetEnemy
        {
            get
            {
                var assassinRange = TargetSelectorMenu.Item("AssassinSearchRange").GetValue<Slider>().Value;

                var vEnemy = ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        enemy =>
                            enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible &&
                            TargetSelectorMenu.Item("Assassin" + enemy.ChampionName) != null &&
                            TargetSelectorMenu.Item("Assassin" + enemy.ChampionName).GetValue<bool>() &&
                            ObjectManager.Player.Distance(enemy) < assassinRange);

                if (TargetSelectorMenu.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex == 1)
                {
                    vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
                }

                Obj_AI_Hero[] objAiHeroes = vEnemy as Obj_AI_Hero[] ?? vEnemy.ToArray();

                Obj_AI_Hero t = !objAiHeroes.Any()
                    ? TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical)
                    : objAiHeroes[0];

                return t;
            }
        }
        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
         //   if (sender.Name.Contains("Missile") || sender.Name.Contains("Minion"))
        }

        public static void Obj_AI_Base_OnProcessSpellCast(LeagueSharp.Obj_AI_Base obj,
            LeagueSharp.GameObjectProcessSpellCastEventArgs arg)
        {
            if (!xWards.ToList().Contains(arg.SData.Name)) return;

            Jumper.testSpellCast = arg.End.To2D();
            Polygon pol;
            if ((pol = map.getInWhichPolygon(arg.End.To2D())) != null)
            {
                Jumper.testSpellProj = pol.getProjOnPolygon(arg.End.To2D());
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
            
            if (Config.Item("Ward").GetValue<KeyBind>().Active)
            {
                Jumper.wardJump(Game.CursorPos.To2D());
            }
            
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo(GetEnemy);            
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
                var existsMana = Player.MaxMana/100*Config.Item("JungleFarmMana").GetValue<Slider>().Value;
                if (Player.Mana >= existsMana)
                    JungleFarm();
            }
        }

        private static void Combo(Obj_AI_Hero t)
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();

            var minQRange = Config.Item("ComboUseQMinRange").GetValue<Slider>().Value;
            var useQDontUnderTurret = Config.Item("UseQComboDontUnderTurret").GetValue<bool>();

            if (Q.IsReady() && useQ && Player.Distance(t) >= minQRange && ObjectManager.Player.Distance(t) <= Q.Range)
            {
                if (E.IsReady())
                    E.Cast();

                if (useQDontUnderTurret)
                {
                    if (!Utility.UnderTurret(t))
                        Q.Cast(t);
                }
                else
                {
                    Q.Cast(t);
                }
            }

            if (ObjectManager.Player.Distance(t) <= E.Range)
                UseItems(t);

            if (W.IsReady() && useW && ObjectManager.Player.Distance(t) <= W.Range) 
                W.Cast();

            if (E.IsReady() && useE && ObjectManager.Player.Distance(t) <= E.Range)
                E.Cast();

            if (IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite) > t.Health &&
                    ObjectManager.Player.Distance(t) <= 500) 
                {
                    Player.Spellbook.CastSpell(IgniteSlot, t);
                }
            }

            if (R.IsReady() && useR)
            {
                if (Player.Distance(t) < Player.AttackRange)
                {
                    if (Utility.CountEnemysInRange((int) Orbwalking.GetRealAutoAttackRange(ObjectManager.Player)) >= 2 ||
                        t.Health > Player.Health) 
                    {
                        R.CastOnUnit(Player);
                    }
                }
            }
        }
        private static void Harass()
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useQDontUnderTurret = Config.Item("UseQHarassDontUnderTurret").GetValue<bool>();

            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            switch (Config.Item("HarassMode").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    {
                        if (Q.IsReady() && W.IsReady() && qTarget != null && useQ && useW)
                        {
                            if (useQDontUnderTurret)
                            {
                                if (!Utility.UnderTurret(qTarget))
                                {
                                    Q.Cast(qTarget);
                                    W.Cast();
                                }
                            }
                            else
                            {
                                Q.Cast(qTarget);
                                W.Cast();
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        if (Q.IsReady() && E.IsReady() && qTarget != null && useQ && useE)
                        {
                            if (useQDontUnderTurret)
                            {
                                if (!Utility.UnderTurret(qTarget))
                                {
                                    Q.Cast(qTarget);
                                    E.Cast();
                                }
                            }
                            else
                            {
                                Q.Cast(qTarget);
                                E.Cast();
                            }
                        }
                        break;
                    }
                case 2:
                    {
                        if (Q.IsReady() && useQ && qTarget != null && useQ)
                        {
                            if (useQDontUnderTurret)
                            {
                                if (!Utility.UnderTurret(qTarget))
                                    Q.Cast(qTarget);
                            }
                            else
                                Q.Cast(qTarget);
                            UseItems(qTarget);
                        }

                        if (W.IsReady() && useW && eTarget != null)
                        {
                            W.Cast();
                        }

                        if (E.IsReady() && useE && eTarget != null)
                        {
                            E.CastOnUnit(Player);
                        }
                        break;
                    }
            }
        }

        private static void LaneClear()
        {
            var useQ = Config.Item("UseQLaneClear").GetValue<bool>();
            var useW = Config.Item("UseWLaneClear").GetValue<bool>();
            var useE = Config.Item("UseELaneClear").GetValue<bool>();
            var useQDontUnderTurret = Config.Item("UseQLaneClearDontUnderTurret").GetValue<bool>();

            var vMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            foreach (var vMinion in vMinions)
            {
                if (useQ && Q.IsReady() && Player.Distance(vMinion) > Orbwalking.GetRealAutoAttackRange(Player))
                {
                    if (useQDontUnderTurret)
                    {
                        if (!Utility.UnderTurret(vMinion))
                            Q.Cast(vMinion);
                    }
                    else
                        Q.Cast(vMinion);
                }

                if (useW && W.IsReady() && Player.Distance(vMinion) < E.Range)
                    W.Cast();

                if (useE && E.IsReady() && Player.Distance(vMinion) < E.Range)
                    E.CastOnUnit(Player);
            }
        }

        private static void JungleFarm()
        {
            var useQ = Config.Item("UseQJungleFarm").GetValue<bool>();
            var useW = Config.Item("UseWJungleFarm").GetValue<bool>();
            var useE = Config.Item("UseEJungleFarm").GetValue<bool>();

            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0) return;

            if (Q.IsReady() && useQ && Player.Distance(mobs[0]) > Player.AttackRange)
                Q.Cast(mobs[0]);

            if (W.IsReady() && useW)
                W.Cast();

            if (E.IsReady() && useE)
                E.CastOnUnit(Player);
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base vTarget, InterruptableSpell args)
        {
            var interruptSpells = Config.Item("InterruptSpells").GetValue<KeyBind>().Active;
            if (!interruptSpells)
                return;
            if (!E.IsReady())
                return;

            if (Player.Distance(vTarget) < Q.Range && Player.Distance(vTarget) > E.Range && Q.IsReady())
            {
                E.Cast();
                Q.Cast(vTarget);
            }

            if (Player.Distance(vTarget) <= E.Range)
            {
                E.Cast();
            }
        }
        
        private static InventorySlot GetInventorySlot(int ID)
        {
            return
                ObjectManager.Player.InventoryItems.FirstOrDefault(
                    item =>
                        (item.Id == (ItemId) ID && item.Stacks >= 1) || (item.Id == (ItemId) ID && item.Charges >= 1));
        }
        
        public static void UseItems(Obj_AI_Hero vTarget)
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
