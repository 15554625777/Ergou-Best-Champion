using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Obama_Leona
{
    class Program
    {
        internal static Menu _menu;
        internal static Obj_AI_Hero _player;
        internal static Orbwalking.Orbwalker Orbwalker;
        internal static Spell Q,W,E,R;
        internal static Spell[] SpellList = { E, R };

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 1200);
            E.SetSkillshot(0.25f, 100f, 2000f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            _menu = new Menu("Obama's 雷欧娜","obama_leona",true);

            var targetSelectorMenu = new Menu("目标选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _menu.AddSubMenu(targetSelectorMenu);

            var orbwalking = _menu.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalking);

            var combo = new Menu("连招", "combo");
            combo.AddItem(new MenuItem("useComboItem", "使用 弯刀/破败").SetValue(true));
            combo.AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            combo.AddItem(new MenuItem("useW", "使用 W").SetValue(true));
            combo.AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            combo.AddItem(new MenuItem("useR", "使用 R").SetValue(true));
            combo.AddItem(new MenuItem("useR_x", "使用 R|敌人数量").SetValue(new Slider(2,1,5)));

            var flee = new Menu("逃跑", "flee");
            flee.AddItem(new MenuItem("useFleeItem", "使用 弯刀/破败").SetValue(true));
            flee.AddItem(new MenuItem("useQflee", "使用 Q 如果敌方追赶").SetValue(true));
            flee.AddItem(new MenuItem("flee_key","逃跑 键位").SetValue(new KeyBind("G".ToCharArray()[0],KeyBindType.Press)));

            var clearJg = new Menu("清野|清线", "jungle");
            clearJg.AddItem(new MenuItem("useQ_jg", "使用 Q").SetValue(true));
            clearJg.AddItem(new MenuItem("useW_jg", "使用 W").SetValue(true));
            clearJg.AddItem(new MenuItem("useW_x", "使用W|小兵数量").SetValue(new Slider(3, 1, 20)));
            clearJg.AddItem(new MenuItem("useE_jg", "使用 E").SetValue(true));

            var misc = new Menu("杂项", "misc");
            misc.AddItem(new MenuItem("interrupt", "中断 法术").SetValue(true));
            misc.AddItem(new MenuItem("blockGapClosers", "防止 突进").SetValue(true));
            misc.AddItem(new MenuItem("packetCast", "使用 封包").SetValue(false));
            misc.AddItem(new MenuItem("usecb", "使用 弯刀/破败").SetValue(true));

            var pred = new Menu("预判", "pred");
            pred.AddItem(new MenuItem("predE", "E 预判").SetValue(new StringList(new[] { "低", "中", "高" }, 1)));
            pred.AddItem(new MenuItem("predR", "R 预判").SetValue(new StringList(new[] { "低", "中", "高"  })));

            var draw = new Menu("范围", "draw");
            draw.AddItem(new MenuItem("DrawE", "E 范围").SetValue(new Circle(false, Color.Blue)));
            draw.AddItem(new MenuItem("DrawR", "R 范围").SetValue(new Circle(false, Color.Red)));
            draw.AddItem(new MenuItem("HPB", "血量伤害 指示器").SetValue(new Circle(false, Color.Gray)));

            _menu.AddSubMenu(combo);
            _menu.AddSubMenu(clearJg);
            _menu.AddSubMenu(flee);
            _menu.AddSubMenu(misc);
            _menu.AddSubMenu(pred);
            _menu.AddSubMenu(draw);

            _menu.AddToMainMenu();
            Game.PrintChat("[!] Obama's |闆锋濞渱 鍔犺級鎴愬姛锛佹饥鍖朾y浜岀嫍  浜岀嫍QQ缇361630847!!");

            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var source = gapcloser.Sender;
            if (!_menu.Item("blockGapClosers").GetValue<bool>() || !source.IsValidTarget(Q.Range + 50) || !Q.IsReady())
                return;
            Q.Cast(PacketCast);
            _player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
        }
        static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!_menu.Item("interrupt").GetValue<bool>() || !Q.IsReady() || !unit.IsValidTarget(Q.Range + 50))
                return;
            Q.Cast(PacketCast);
            _player.IssueOrder(GameObjectOrder.AttackUnit, unit);
        }
        static bool CanCast(Obj_AI_Base target, int skill = 1)
        {
            var pred = skill == 1 ? E.GetPrediction(target) : R.GetPrediction(target);
            var getPredictionValue = skill == 1 ? _menu.Item("predE").GetValue<StringList>().SelectedIndex : _menu.Item("predR").GetValue<StringList>().SelectedIndex;
            var cast = false;
            switch (getPredictionValue)
            {
                case 0: // Low
                    if (pred.Hitchance >= HitChance.Low) cast = true;
                    break;
                case 1: // Medium
                    if (pred.Hitchance >= HitChance.Medium) cast = true;
                    break;
                case 2: // High
                    if (pred.Hitchance >= HitChance.High) cast = true;
                    break;
            }
            if (!cast && pred.Hitchance == HitChance.Immobile) cast = true;
            return cast;
        }

        static void OnCombo()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical); // AD Leona, not AP! Select target for AD dmg!
            if (target == null)
                return;
            var useW = _menu.Item("useW").GetValue<bool>();
            var useE = _menu.Item("useE").GetValue<bool>();
            var useR = _menu.Item("useR").GetValue<bool>();
            var useRx = _menu.Item("useR_x").GetValue<Slider>().Value;
            if ((E.IsReady() && useE) || (E.IsReady() && useE && _player.FlatMovementSpeedMod < (target.FlatMovementSpeedMod + 10)))
            {
                if (CanCast(target))
                    E.Cast(target, PacketCast);
            }
            if (useW && target.IsValidTarget(275)) W.Cast(PacketCast);
            if (useR && useRx >= target.CountEnemysInRange(300) && CanCast(target,2)) R.Cast(target,PacketCast);

            if (_menu.Item("useComboItem").GetValue<bool>() && target.IsValidTarget(500f) && _menu.Item("usecb").GetValue<bool>() && (Items.HasItem(3144) || Items.HasItem(3153)))
            {
                var distance = _player.Distance(target) > _player.GetRealAutoAttackRange()+50;
                if (Items.CanUseItem(3144) && distance) // cutlass
                    Items.UseItem(3144,target);
                if (Items.CanUseItem(3153) && distance && _player.Health + _player.GetItemDamage(target,Damage.DamageItems.Botrk) < _player.MaxHealth)
                    Items.UseItem(3153,target);
            }
        }

        static void OnClear()
        {
            var useW = _menu.Item("useW_jg").GetValue<bool>();
            var useWe = _menu.Item("useW_x").GetValue<Slider>().Value;
            var useE = _menu.Item("useE_jg").GetValue<bool>();
            var allmobs = MinionManager.GetMinions(500f);
            var neutralMobs = MinionManager.GetMinions(500f, MinionTypes.All, MinionTeam.Neutral);

            if (allmobs.Count <= 0 && neutralMobs.Count <= 0) return;
            if (useW && W.IsReady() && MinionManager.GetMinions(300f).Count >= useWe) W.Cast(PacketCast);
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValid);
            foreach (var minion in minions.Where(minion => useE && E.IsReady()))
                E.Cast(minion, PacketCast);
        }

        private static void OnFlee()
        {
            var target = TargetSelector.GetTarget(_player.GetRealAutoAttackRange()+40, TargetSelector.DamageType.Magical);
            var cast = false;
            if (_menu.Item("useFleeItem").GetValue<bool>() && (Items.HasItem(3144) && Items.CanUseItem(3144)) || (Items.HasItem(3153) && Items.HasItem(3153)))
            {
                Items.UseItem(Items.HasItem(3144) ? 3144 : 3153);
                cast = true;
            }
            if (target.IsFacing(_player) && Q.IsReady() && !cast)
                Q.Cast(PacketCast);
            Orbwalking.Orbwalk(Q.IsReady() && !cast ? target : null, Game.CursorPos);
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (Q.IsReady())
                        Q.Cast(PacketCast);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (_menu.Item("useQ_jg").GetValue<bool>() && Q.IsReady()) Q.Cast(PacketCast);
                    break;
            }
        }

        static bool PacketCast 
        {
            get { return _menu.Item("packetCast").GetValue<bool>(); }
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    OnClear();
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    if (_menu.Item("flee_key").GetValue<KeyBind>().Active)
                        OnFlee();
                    break;
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var item = _menu.Item("Draw" + spell.Slot).GetValue<Circle>();
                if (item.Active && spell.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, item.Color);
            }
            if (_menu.Item("HPB").GetValue<Circle>().Active)
            {
                Utility.HpBarDamageIndicator.Color = _menu.Item("HPB").GetValue<Circle>().Color;
                Utility.HpBarDamageIndicator.DamageToUnit = GetTotalDamage;
                Utility.HpBarDamageIndicator.Enabled = true;
            }
        }

        static float GetTotalDamage(Obj_AI_Hero target)
        {
            var damage = 0d;
            if (Q.IsReady())
                damage += _player.GetSpellDamage(target, SpellSlot.Q);
            if (W.IsReady())
                damage += _player.GetSpellDamage(target, SpellSlot.W);
            if (E.IsReady())
                damage += _player.GetSpellDamage(target, SpellSlot.E);
            if (R.IsReady())
                damage += _player.GetSpellDamage(target, SpellSlot.R);
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += _player.GetItemDamage(target, Damage.DamageItems.Bilgewater);
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += _player.GetItemDamage(target, Damage.DamageItems.Botrk);
            damage += GetSheenDamage();
            return (float)damage;
        }
        static double GetSheenDamage(bool simulate = false)
        {
            if (simulate)
                return Items.HasItem(3057)
                    ? ObjectManager.Player.BaseAttackDamage
                    : (Items.HasItem(3078) ? ObjectManager.Player.BaseAttackDamage * 2 : 0);
            else if (Items.HasItem(3057) && ObjectManager.Player.HasBuff("Sheen")) // sheen
                return ObjectManager.Player.BaseAttackDamage;
            else if (Items.HasItem(3078) && ObjectManager.Player.HasBuff("Sheen")) // trinity
                return ObjectManager.Player.BaseAttackDamage * 2;
            else
                return 0;
        }
    }
}
