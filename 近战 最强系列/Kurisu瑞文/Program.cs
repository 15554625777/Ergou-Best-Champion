using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using ItemData = LeagueSharp.Common.Data.ItemData;
using SharpDX;

namespace KurisuRiven
{
    internal class Program
    {
        private static Menu _config;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;
        private static Obj_AI_Hero _maintarget;
        private static Orbwalking.Orbwalker _orbwalker;
        private static int _cleavecount;
        private static SpellDataInst _qDataInst;

        static void Main(string[] args)
        {
            Console.WriteLine("Riven injected...");
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Me.ChampionName != "Riven")
            {
                return;
            }

            // load spells
            _w = new Spell(SpellSlot.W, 250f);
            _e = new Spell(SpellSlot.E, 270f);

            _q = new Spell(SpellSlot.Q, 260f);
            _qDataInst = Me.Spellbook.GetSpell(_q.Slot);
            _q.SetSkillshot(0.25f, 100f, 1400f, false, SkillshotType.SkillshotCircle);

            _r = new Spell(SpellSlot.R, 1100f);
            _r.SetSkillshot(0.25f, 150f, 2200f, false, SkillshotType.SkillshotCone);

            // load menu
            Menu();

            // call events
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;

            // moar events xD
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;

            // print chat -- load success
            Game.PrintChat("<font color=\"#7CFC00\"><b>Kurisu鐟炴枃:</b></font> 璇诲彇鎴愬姛锛侊紒锛亅 鍔犺浇鎴愬姛!姹夊寲by浜岀嫍!QQ缇361630847");
        }

        #region Riven Menu
        private static void Menu()
        {
            _config = new Menu("Kurisu瑞文", "kurisuriven", true);

            var tsMenu = new Menu("目标选择", "selector");
            TargetSelector.AddToMenu(tsMenu);
            _config.AddSubMenu(tsMenu);

            var owMenu = new Menu("走砍设置", "Orbwalk");
            _orbwalker = new Orbwalking.Orbwalker(owMenu);
            owMenu.AddItem(new MenuItem("fleemode", "逃跑")).SetValue(new KeyBind(65, KeyBindType.Press));
            _config.AddSubMenu(owMenu);

            var drMenu = new Menu("范围显示", "drawings");
            drMenu.AddItem(new MenuItem("drawrange", "显示 W 范围")).SetValue(true);
            drMenu.AddItem(new MenuItem("drawpassive", "显示 伤害计数")).SetValue(true);
            drMenu.AddItem(new MenuItem("drawengage", "显示 连招顺序")).SetValue(true);
            drMenu.AddItem(new MenuItem("drawkill", "显示 击杀提示")).SetValue(true);
            drMenu.AddItem(new MenuItem("drawtarg", "显示 攻击目标")).SetValue(true);
            _config.AddSubMenu(drMenu);

            var cMenu = new Menu("连招设置", "combo");
            cMenu.AddItem(new MenuItem("engage", "连招顺序"))
                .SetValue(new StringList(new[] { "E->R->提亚马特|九头蛇->W", "E->提亚马特|九头蛇->R->W" }));
            cMenu.AddItem(new MenuItem("reckless", "光速QA (启用)")).SetValue(new KeyBind(88, KeyBindType.Toggle));
            cMenu.AddItem(new MenuItem("multir1", "启用光速QA|目标 >=")).SetValue(new Slider(1, 1, 5));
            cMenu.AddItem(new MenuItem("multir2", "启用光速QA|对目标造成伤害 >= ")).SetValue(new Slider(50, 1, 99));
            cMenu.AddItem(new MenuItem("autow", "(W)眩晕|目标 >=")).SetValue(new Slider(2, 2, 5));
            _config.AddSubMenu(cMenu);

            var sMenu = new Menu("技能设置", "Spells");

            var menuQ = new Menu("Q 菜单", "cleave");
            menuQ.AddItem(new MenuItem("usecomboq", "连招 使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("usejungleq", "打野 使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("uselaneq", "清线 使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("qint", "Q3中断法术")).SetValue(true);
            menuQ.AddItem(new MenuItem("prediction", "左右QA封锁走位")).SetValue(true);
            menuQ.AddItem(new MenuItem("keepq", "保持Q使用")).SetValue(true);
            sMenu.AddSubMenu(menuQ);

            var menuW = new Menu("W 菜单", "kiburst");
            menuW.AddItem(new MenuItem("usecombow", "连招 使用")).SetValue(true);
            menuW.AddItem(new MenuItem("usejunglew", "打野 使用")).SetValue(true);
            menuW.AddItem(new MenuItem("uselanew", "清线 使用")).SetValue(true);
            menuW.AddItem(new MenuItem("antigap", "防止突进")).SetValue(true);
            menuW.AddItem(new MenuItem("wint", "中断法术")).SetValue(true);
            sMenu.AddSubMenu(menuW);

            var menuE = new Menu("E 菜单", "valor");
            menuE.AddItem(new MenuItem("usecomboe", "连招 使用")).SetValue(true);
            menuE.AddItem(new MenuItem("usejunglee", "打野 使用")).SetValue(true);
            menuE.AddItem(new MenuItem("uselanee", "清线 使用")).SetValue(true);
            menuE.AddItem(new MenuItem("vhealth", "使用血量 %")).SetValue(new Slider(40, 1));
            sMenu.AddSubMenu(menuE);

            var menuR = new Menu("R 菜单", "blade");
            menuR.AddItem(new MenuItem("user", "连招 使用")).SetValue(true);
            menuR.AddItem(new MenuItem("usews", "使用 光速QA")).SetValue(true);
            menuR.AddItem(new MenuItem("checkover", "检测是否可击杀")).SetValue(false);
            sMenu.AddSubMenu(menuR);

            _config.AddSubMenu(sMenu);

            var mMenu = new Menu("杂项设置", "misc");
            mMenu.AddItem(new MenuItem("useignote", "使用 点燃")).SetValue(true);
            mMenu.AddItem(new MenuItem("useitems", "使用 破败|幽梦")).SetValue(true);
            mMenu.AddItem(new MenuItem("forceaa", "清线 使用QA")).SetValue(false);
            _config.AddSubMenu(mMenu);

            var dMenu = new Menu("调试设置", "debug");
            dMenu.AddItem(new MenuItem("debugtrue", "调试 精准范围")).SetValue(false);
            dMenu.AddItem(new MenuItem("debugdmg", "调试 组合连招")).SetValue(false);
            _config.AddSubMenu(dMenu);
            _config.AddToMainMenu();
        }

        #endregion

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (unit.IsValidTarget(_q.Range))
            {
                if (_q.IsReady() && _cleavecount == 2 && _config.Item("qint").GetValue<bool>())
                    _q.Cast(unit.ServerPosition);
            }

            if (unit.IsValidTarget(_w.Range))
            {
                if (_w.IsReady() && _config.Item("wint").GetValue<bool>())
                    _w.Cast();
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_config.Item("antigap").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(_w.Range) && _w.IsReady())
                {
                    _w.Cast();
                }
            }
        }

        #region Damage Calc
        private static void CheckDamage(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            var ad = Me.GetAutoAttackDamage(target);
            var ignite = Me.GetSpellSlot("summonerdot");

            var tmt = Items.HasItem(3077) && Items.CanUseItem(3077)
                ? Me.GetItemDamage(target, Damage.DamageItems.Tiamat)
                : 0;

            var hyd = Items.HasItem(3074) && Items.CanUseItem(3074)
                ? Me.GetItemDamage(target, Damage.DamageItems.Hydra)
                : 0;

            var bwc = Items.HasItem(3144) && Items.CanUseItem(3144) ?
                Me.GetItemDamage(target, Damage.DamageItems.Bilgewater)
                : 0;

            var brk = Items.HasItem(3153) && Items.CanUseItem(3153)
                ? Me.GetItemDamage(target, Damage.DamageItems.Botrk)
                : 0;

            _rr = Me.GetSpellDamage(target, SpellSlot.R) - 20;
            _ra = ad + (ad * Runicpassive[Me.Level] / 100);
            _rq = _q.IsReady() ? DamageQ(target) : 0;

            _rw = _w.IsReady()
                ? Me.GetSpellDamage(target, SpellSlot.W)
                : 0;

            _ri = Me.Spellbook.CanUseSpell(ignite) == SpellState.Ready
                ? Me.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite)
                : 0;

            _ritems = tmt + hyd + bwc + brk;

            _ua = _r.IsReady()
                ? _ra +
                  Me.CalcDamage(target, Damage.DamageType.Physical,
                      Me.BaseAttackDamage + Me.FlatPhysicalDamageMod * 0.2)
                : _ua;

            _uq = _r.IsReady()
                ? _rq +
                  Me.CalcDamage(target, Damage.DamageType.Physical,
                      Me.BaseAttackDamage + Me.FlatPhysicalDamageMod * 0.2 * 0.7)
                : _uq;

            _uw = _r.IsReady()
                ? _rw +
                  Me.CalcDamage(target, Damage.DamageType.Physical,
                      Me.BaseAttackDamage + Me.FlatPhysicalDamageMod * 0.2 * 1)
                : _uw;

            _rr = _r.IsReady()
                ? Me.GetSpellDamage(target, SpellSlot.R)
                : 0;
        }

        private static float DamageQ(Obj_AI_Base target)
        {
            var dmg = 0d;
            if (_q.IsReady())
            {
                dmg += Me.CalcDamage(target, Damage.DamageType.Physical,
                    -10 + (_q.Level * 20) +
                    (0.35 + (_q.Level * 0.05)) * (Me.FlatPhysicalDamageMod + Me.BaseAttackDamage));
            }

            return (float) dmg;
        }

        private static float Rrb4(Obj_AI_Base target)
        {
            var dmg = 0d;
            var combodamage = _uq*3 + _ua*3 + _uw + _ri + _ritems;
            if (_r.IsReady())
            {
                dmg += Me.CalcDamage(
                    target, Damage.DamageType.Physical,
                    (new double[] { 80, 80, 120, 160 }[_r.Level] +
                     (0.6 * Me.FlatPhysicalDamageMod) * ((target.MaxHealth - combodamage) / target.MaxHealth * 2.67 + 1)));
            }

            return (float) dmg;
        }

        #endregion

        #region Drawings
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Me.IsDead)
            {
                if (_config.Item("drawrange").GetValue<bool>())
                    Render.Circle.DrawCircle(Me.Position, _w.Range, System.Drawing.Color.White, 3);

                if (_config.Item("drawengage").GetValue<bool>())
                    Render.Circle.DrawCircle(Me.Position, Me.AttackRange + _e.Range + 10, System.Drawing.Color.White, 3);

                if (_maintarget.IsValidTarget(900) && _config.Item("drawtarg").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(
                        _maintarget.Position, _maintarget.BoundingRadius - 50, System.Drawing.Color.Yellow, 6);
                }

                if (_config.Item("reckless").GetValue<KeyBind>().Active)
                {
                    var wts = Drawing.WorldToScreen(Me.Position);
                    Drawing.DrawText(wts[0] - 40, wts[1] + 30, System.Drawing.Color.Yellow, "Reckless ON");
                }
                else
                {
                    var wts = Drawing.WorldToScreen(Me.Position);
                    Drawing.DrawText(wts[0] - 25, wts[1] + 30, System.Drawing.Color.LawnGreen, "Only Kill");               
                }

                if (_config.Item("debugtrue").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Me.Position, _truerange, System.Drawing.Color.Yellow, 3);
                }

                if (_config.Item("drawpassive").GetValue<bool>())
                {
                    var wts = Drawing.WorldToScreen(Me.Position);
                    if (Me.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.NotLearned)
                        Drawing.DrawText(wts[0] - 48, wts[1] + 10, System.Drawing.Color.White, "Q: Not Learned!");
                    else if (_qt <= 0)
                        Drawing.DrawText(wts[0] - 30, wts[1] + 10, System.Drawing.Color.White, "Q: Ready");
                    else
                        Drawing.DrawText(
                            wts[0] - 30, wts[1] + 10, System.Drawing.Color.White, "Q: " + _qt.ToString("0.0"));

                }
            }

            if (_config.Item("debugdmg").GetValue<bool>() && _maintarget.IsValidTarget(1000))
            {
                var wts = Drawing.WorldToScreen(_maintarget.Position);
                if (!_r.IsReady())
                    Drawing.DrawText(wts[0] - 75, wts[1] + 40, System.Drawing.Color.Yellow,
                        "Combo Damage: " + (float)(_ra * 3 + _rq * 3 + _rw + Rrb4(_maintarget) + _ri + _ritems));
                else
                    Drawing.DrawText(wts[0] - 75, wts[1] + 40, System.Drawing.Color.Yellow,
                        "Combo Damage: " + (float)(_ua * 3 + _uq * 3 + _uw + Rrb4(_maintarget) + _ri + _ritems));
            }

            if (_maintarget.IsValidTarget(1000) && _config.Item("drawkill").GetValue<bool>())
            {
                var wts = Drawing.WorldToScreen(_maintarget.Position);

                if ((float)(_ra + _rq * 2 + _rw + _ri + _ritems) > _maintarget.Health)
                    Drawing.DrawText(wts[0] - 20, wts[1] + 20, System.Drawing.Color.LawnGreen, "Kill!");
                else if ((float)(_ra * 2 + _rq * 2 + _rw + _ritems) > _maintarget.Health)
                    Drawing.DrawText(wts[0] - 40, wts[1] + 20, System.Drawing.Color.LawnGreen, "Easy Kill!");
                else if ((float)(_ra * 3 + _ra * 3 + _rw + _ri + Rrb4(_maintarget) + _ritems) > _maintarget.Health)
                    Drawing.DrawText(wts[0] - 65, wts[1] + 20, System.Drawing.Color.LawnGreen, "Full Combo Kill!");
                else if ((float)(_ua * 3 + _uq * 3 + _uw + Rrb4(_maintarget) + _ri + _ritems) > _maintarget.Health)
                    Drawing.DrawText(wts[0] - 70, wts[1] + 20, System.Drawing.Color.LawnGreen, "Full Combo Hard Kill!");
                else if ((float)(_ua * 3 + _uq * 3 + _uw + Rrb4(_maintarget) + _ri + _ritems) < _maintarget.Health)
                    Drawing.DrawText(wts[0] - 40, wts[1] + 20, System.Drawing.Color.LawnGreen, "Cant Kill!");
            }
        }

        #endregion

        #region Animations
        private static void Obj_AI_Base_OnPlayAnimation(GameObject sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.Animation.Contains("Idle"))
            {
                _canattack = true;
            }

            if (args.Animation.Contains("Attack"))
            {
                _canmove = false;
                _canattack = false;
                _lastattack = Environment.TickCount;
                _isattacking = true;
                _cancleave = false;
                _cankiburst = false;
                _candash = false;
                _canwindslash = false;
            }

            if (args.Animation.Contains("Spell1a"))
            {
                _qtRem = Game.Time + (13 + (13 * Me.PercentCooldownMod));
            }

            if (args.Animation.Contains("Spell1a") || args.Animation.Contains("Spell1b"))
            {
                _lastcleave = Environment.TickCount;
                _isattacking = false;
                _iscleaving = true;
                _cancleave = false;
                _canmove = false;

                if (_config.Item("fleemode").GetValue<KeyBind>().Active)
                {
                    return;
                }

                if (_maintarget.IsValidTarget(_truerange + 100))
                {
                    Utility.DelayAction.Add(
                        140, () => Me.IssueOrder(GameObjectOrder.MoveTo, new Vector3(_movePos.X, _movePos.Y, _movePos.Z)));
                }
            }

            if (args.Animation.Contains("Spell1c"))
            {
                if (_config.Item("fleemode").GetValue<KeyBind>().Active)
                {
                    return;
                }

                if (_maintarget.IsValidTarget(_truerange + 100))
                {
                    if (args.Animation != "Idle")
                        Me.IssueOrder(GameObjectOrder.MoveTo, new Vector3(_movePos.X, _movePos.Y, _movePos.Z));
                }
            }
        }

        #endregion

        #region OnSpellCast

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            // tickcounts ftw
            switch (args.SData.Name)
            {
                case "RivenMartyr":
                    _lastkiburst = Environment.TickCount;
                    _iskibursting = true;
                    _cankiburst = false;
                    _canmove = false;
                    if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    {
                        Utility.DelayAction.Add(
                            Game.Ping + 75, delegate
                            {
                                if (_q.IsReady() && _cancleave)
                                    _q.Cast(_maintarget.ServerPosition);
                            });
                    }
                    break;
                case "ItemTiamatCleave":
                    _lasthydra = Environment.TickCount;
                    _canattack = true;
                    break;
                case "RivenFeint":
                    _lastdash = Environment.TickCount;
                    _isdashing = true;
                    _canmove = false;
                    _candash = false;
                    if (_config.Item("reckless").GetValue<KeyBind>().Active)
                    {
                        if (_ulton && _canwindslash)
                        {
                            _r.Cast(_maintarget.ServerPosition);
                            Utility.DelayAction.Add(
                                50, delegate
                                {
                                    if (_q.IsReady())
                                    {
                                        _q.Cast(_maintarget.ServerPosition);
                                    }

                                    _canattack = true;
                                    _canmove = true;
                                });
                        }
                    }
                    break;
                case "RivenFengShuiEngine":

                    break;
                case "rivenizunablade":
                    _canwindslash = false;
                    if (_q.IsReady())
                        _q.Cast(_maintarget.ServerPosition);
                    break;
            }
        }

        #endregion

        #region OnGameUpdate
        private static void Game_OnGameUpdate(EventArgs args)
        {
            _maintarget = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
            CheckDamage(_maintarget);
            Kappa();

            _orbwalker.SetAttack(_canattack);
            _orbwalker.SetMovement(_canmove);

            _astime = 1 / (0.318 * Me.AttackSpeedMod);
            _truerange = Me.AttackRange + Me.Distance(Me.BBox.Minimum) + 1;

            _ulton = Me.GetSpell(SpellSlot.R).Name != "RivenFengShuiEngine";
            _hashydra = Items.HasItem(3077) || Items.HasItem(3074);
            _canhydra = !_isattacking && (Items.CanUseItem(3077) || Items.CanUseItem(3074));

            _qt = (_qtRem - Game.Time > 0) ? (_qtRem - Game.Time) : 0;


            if (_maintarget.IsValidTarget(1000))
            {
                _movePos = _maintarget.ServerPosition +
                          Vector3.Normalize(Me.Position - _maintarget.ServerPosition) *
                          (Me.Distance(_maintarget.ServerPosition) + 51);
            }
            else
            {
                _movePos = Game.CursorPos;
            }

            if (_config.Item("fleemode").GetValue<KeyBind>().Active)
            {
                if (_candash && _e.IsReady())
                {
                    _e.Cast(Game.CursorPos);
                }

                if (!_e.IsReady() && Environment.TickCount - _lastcleave >= 300 && Environment.TickCount - _lastdash >= 200)
                {
                    _q.Cast(Game.CursorPos);
                }

                if (_canmove)
                {
                    Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }
            }

            foreach (var b in Me.Buffs)
            {
                if (b.Name == "RivenTriCleave")
                    _cleavecount = b.Count;

                if (b.Name == "rivenpassiveaaboost")
                    _runiccount = b.Count;
            }

            if (Me.HasBuff("RivenTriCleave", true) && Environment.TickCount - _lastcleave >= 3600)
            {
                if (_config.Item("keepq").GetValue<bool>() && !Me.IsRecalling())
                    _q.Cast(Game.CursorPos);
            }

            if (!Me.HasBuff("rivenpassiveaaboost", true))
                Utility.DelayAction.Add(1000, () => _runiccount = 1);

            if (!Me.HasBuff("RivenTriCleave", true))
                Utility.DelayAction.Add(1000, () => _cleavecount = 0);

            if (_config.Item("reckless").GetValue<KeyBind>().Active)
            {
                var wTarget = ObjectManager.Get<Obj_AI_Hero>()
                    .Where(huro => huro.IsValidTarget(_w.Range));

                if (wTarget.Count() >= _config.Item("autow").GetValue<Slider>().Value)
                {
                    if (_cankiburst && _w.IsReady())
                    {
                        _w.Cast();
                    }
                }
            }

            var obj = (_orbwalker.GetTarget() != null ? (Obj_AI_Base)_orbwalker.GetTarget() : _maintarget) ?? Me;

            var time = (int)(Me.AttackCastDelay * 1000) - 100 + Game.Ping / 2 +
                    1000 * (int)Me.Distance(obj.ServerPosition) / (int)Me.BasicAttack.MissileSpeed;

            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                NormalCombo(_maintarget);
            }

            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                LaneClear();
                JungleClear();
            }

            if (!_canwindslash && !_isattacking && _ulton && _r.IsReady())
            {
                _canwindslash = true;
            }

            if (!_candash && !(_iscleaving || _isattacking || _iskibursting) && _e.IsReady())
            {
                _candash = true;
            }

            if (!_cankiburst && !(_iscleaving || _isattacking || _isdashing) && _w.IsReady())
            {
                _cankiburst = true;
            }

            if (!_canmove && !(_isattacking || _iscleaving || _iskibursting || _isdashing) &&
                Environment.TickCount - _lastattack >= time)
            {
                _canmove = true;
            }

            if (!_canattack && !(_iscleaving || _isdashing || _iskibursting) &&
                 Environment.TickCount - _lastattack >= time + 166)
            {
                _canattack = true;
            }

            var time2 = (int) ((_astime*100) - 10 - (Me.Level*8/2) + Game.Ping/2);
            if (_isattacking && Environment.TickCount - _lastattack >= time2)
            {
                _isattacking = false;
                _canmove = true;

                if (_config.Item("usecomboq").GetValue<bool>())
                    _cancleave = true;
                if (_config.Item("usecomboe").GetValue<bool>())
                    _candash = true;
                if (_config.Item("usecombow").GetValue<bool>())
                    _cankiburst = true;
                if (_config.Item("usews").GetValue<bool>())
                    _canwindslash = true;
            }

            if (_iscleaving && Environment.TickCount - _lastcleave >= 273)
            {
                _iscleaving = false;
                _canmove = true;
                _canattack = true;
            }

            if (_iskibursting && Environment.TickCount - _lastkiburst >= 148)
            {
                _iskibursting = false;
                _canattack = true;
                _canmove = true;
            }

            if (_isdashing && Environment.TickCount - _lastdash >= 200)
            {
                _isdashing = false;
                _canmove = true;
            }
        }

        #endregion

        #region KappaKap
        private static void Kappa()
        {
            if (_ulton && _config.Item("usews").GetValue<bool>())
            {
                var needed = _config.Item("multir2").GetValue<Slider>().Value;
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(huro => huro.IsValidTarget(_r.Range + 100))
                        .OrderBy(huro => huro.Health/huro.MaxHealth*100).FirstOrDefault();

                if (target == null)
                {
                    return;
                }

                var po = _r.GetPrediction(target, true);
                if (_config.Item("reckless").GetValue<KeyBind>().Active)
                {
                    if (po.AoeTargetsHitCount >= _config.Item("multir1").GetValue<Slider>().Value)
                    {
                        if (_r.IsReady() && _canwindslash)
                            _r.Cast(po.CastPosition);
                    }

                    if ((int) (_rr/target.MaxHealth*100) >= target.Health/target.MaxHealth*needed)
                    {
                        if (po.Hitchance >= HitChance.Medium && _canwindslash && _r.IsReady())
                            _r.Cast(po.CastPosition);
                    }

                    if (target.Health < _rr + _ua*1 + _uq*2 &&
                        target.Distance(Me.ServerPosition) <= _truerange + 100)
                    {
                        if (po.Hitchance >= HitChance.Medium && _canwindslash && _r.IsReady())
                            _r.Cast(po.CastPosition);
                    }

                }

                if (target.Health <= _rr && _canwindslash)
                {
                    if (po.Hitchance >= HitChance.Medium && _canwindslash)
                        _r.Cast(po.CastPosition);
                }
            }
        }

        #endregion

        #region Minion Clear
        private static void LaneClear()
        {
            var minionListerino = MinionManager.GetMinions(Me.ServerPosition, _e.Range);
            foreach (var minion in minionListerino)
            {
                Orb(minion, "Lane");
                if (_q.IsReady() && _cancleave && minion.Distance(Me.ServerPosition) <= _q.Range)
                {
                    if (_config.Item("uselaneq").GetValue<bool>())
                        _q.Cast(minion.ServerPosition);
                }

                if (_w.IsReady() && _cankiburst)
                {
                    if (minionListerino.Count(m => m.IsValidTarget(_w.Range)) >= 4)
                    {
                        if (_config.Item("uselanew").GetValue<bool>())
                        {
                            Items.UseItem(3077);
                            Items.UseItem(3074);
                            _w.Cast();
                        }
                    }
                }

                if (_e.IsReady() && _candash && minion.IsValidTarget(_e.Range + _q.Range))
                {
                    if (_config.Item("uselanee").GetValue<bool>())
                        _e.Cast(Game.CursorPos);
                }
            }
        }

        private static void JungleClear()
        {
            foreach (var minion in
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        m =>
                            MinionList.Any(x => m.Name.StartsWith(x)) && !m.Name.StartsWith("Minion") &&
                            !m.Name.Contains("Mini")))
            {
                Orb(minion, "Combo");
                if (_cancleave && _q.IsReady() && minion.Distance(Me.ServerPosition) <= _q.Range)
                {
                    if (_config.Item("usejungleq").GetValue<bool>())
                        _q.Cast(minion.ServerPosition);
                }

                if (_cankiburst && _w.IsReady() && minion.Distance(Me.ServerPosition) <= _w.Range)
                {
                    if (_config.Item("usejunglew").GetValue<bool>())
                        _w.Cast();
                }

                if (_e.IsReady() && _candash)
                {
                    if (minion.Distance(Me.ServerPosition) <= _e.Range + _q.Range ||
                        Me.Health / Me.MaxHealth * 100 <= _config.Item("vhealth").GetValue<Slider>().Value)
                    {
                        if (_config.Item("uselanee").GetValue<bool>())
                            _e.Cast(Game.CursorPos);
                    }
                }
            }
        }

        #endregion

        #region Combos
        private static void NormalCombo(Obj_AI_Base target)
        {
            var mode = _config.Item("engage").GetValue<StringList>();
            var healthpercent = Me.Health / Me.MaxHealth * 100;
            if (!target.IsValidTarget(_r.Range * 2))
            {
                return;
            }

            Orb(target, "Combo");

            // valor
            if (_e.IsReady() && _candash && (target.Distance(Me.ServerPosition) > _truerange + 10))
            {
                if (target.Distance(Me.ServerPosition) <= _r.Range + 100 ||
                    healthpercent <= _config.Item("vhealth").GetValue<Slider>().Value &&
                    target.Distance(Me.ServerPosition) <= _r.Range)
                {
                    if (_config.Item("useitems").GetValue<bool>())
                    {
                        ItemData.Youmuus_Ghostblade.GetItem().Cast();
                        ItemData.Blade_of_the_Ruined_King.GetItem().Cast(target);
                        ItemData.Bilgewater_Cutlass.GetItem().Cast(target);
                    }

                    if (_config.Item("usecomboe").GetValue<bool>())
                        _e.Cast(target.ServerPosition);

                    switch (mode.SelectedIndex)
                    {
                        case 0:
                        case 2:
                            CheckR(target);
                            break;
                        case 1:
                            if (_hashydra && _canhydra)
                            {
                                if (_w.IsReady())
                                {
                                    Items.UseItem(3077);
                                    Items.UseItem(3074);
                                }

                                Utility.DelayAction.Add(250, () => CheckR(target));
                            }

                            else
                            {
                                CheckR(target);
                            }
                            break;
                    }
                }
            }

            // kiburst
            else if (_w.IsReady() && _cankiburst && target.Distance(Me.ServerPosition) <= _w.Range + 20)
            {
                switch (mode.SelectedIndex)
                {
                    case 0:
                        if (_canhydra && _hashydra)
                        {
                            Items.UseItem(3077);
                            Items.UseItem(3074);
                        }

                        Utility.DelayAction.Add(
                            130, delegate
                            {
                                if (_config.Item("usecombow").GetValue<bool>())
                                    _w.Cast();
                            });

                        break;
                    case 1:
                        if (_hashydra && _canhydra)
                        {
                            Items.UseItem(3077);
                            Items.UseItem(3074);
                            Utility.DelayAction.Add(200, delegate
                            {
                                CheckR(target);
                                if (_q.IsReady() && _cancleave)
                                    _q.Cast(target.ServerPosition);
                            });

                            if (_config.Item("usecombow").GetValue<bool>())
                                Utility.DelayAction.Add(300, () => _w.Cast());
                        }

                        else
                        {
                            CheckR(target);
                            if (_config.Item("usecombow").GetValue<bool>() && _w.IsReady())
                            {
                                if (target.Distance(Me.ServerPosition) <= _w.Range)
                                    _w.Cast();
                            }
                        }
                        break;

                }


            }

            // cleaves
            else if (_cancleave && _q.IsReady() && target.Distance(Me.ServerPosition) <= _q.Range + 10)
            {
                CheckR(target);
                if (_config.Item("usecomboq").GetValue<bool>())
                {
                    if (_config.Item("prediction").GetValue<bool>())
                    {
                        _q.CastIfHitchanceEquals(target, HitChance.Medium);
                    }

                    else
                    {
                        _q.Cast(target.ServerPosition);
                    }
                }

            }

            // gapclose
            else if (target.Distance(Me.ServerPosition) > _truerange + 101)
            {
                if (!_config.Item("usecomboq").GetValue<bool>())
                {
                    return;
                }

                if (Environment.TickCount - _lastcleave >= 1200 && !_isattacking)
                {
                    if (_e.IsReady())
                    {
                        return;
                    }

                    _q.Cast(target.ServerPosition);
                }
            }

            if (_config.Item("reckless").GetValue<KeyBind>().Active)
            {
                if ((float)_ua * 3 + _uq * 3 + _uw + _ri + _ritems >= target.Health)
                {
                    if (_e.IsReady() && _candash && _cleavecount == 2 && _ulton)
                    {
                        _canmove = false;
                        _canattack = false;
                        _e.Cast(target.ServerPosition);
                    }
                }
            }

            // ignite
            var ignote = Me.GetSpellSlot("summonerdot");
            if ((float)_ua * 3 + _uq * 3 + _uw + Rrb4(target) + _ri + _ritems >= target.Health)
            {
                if (Me.Spellbook.CanUseSpell(ignote) == SpellState.Ready)
                {
                    if (_ulton && target.Distance(Me.ServerPosition) <= 600 && _cleavecount <= 1 && _q.IsReady())
                    {
                        if (_config.Item("useignote").GetValue<bool>())
                            Me.Spellbook.CastSpell(ignote, target);
                    }
                }
            }
        }

        #endregion

        private static void Orb(Obj_AI_Base target, string mode)
        {
            if (target.IsValidTarget(_truerange + 100) && _canattack && _canmove)
            {
                if (mode == "Lane" && !_config.Item("forceaa").GetValue<bool>())
                {
                    return;
                }

                _canmove = false;
                Me.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
        }

        private static void CheckR(Obj_AI_Base target)
        {
            if (target.IsValidTarget(_r.Range + 100))
            {
                if (!_r.IsReady() || _ulton || !_config.Item("user").GetValue<bool>())
                {
                    return;
                }

                if ((float)_ua * 3 + _uq * 3 + _uw + _rr + _ri + _ritems >= target.Health)
                {
                    if (target.Health <= (float) _ra * 2 + _rq * 2 + _rw + _ritems &&
                        _config.Item("checkover").GetValue<bool>())
                    {
                        return;
                    }

                    if (_cleavecount <= 1 && _q.IsReady())
                    {                       
                        _r.Cast();
                    }
                }

                if (_config.Item("reckless").GetValue<KeyBind>().Active)
                {
                    var targetList 
                        = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(900));

                    var enemies = targetList as Obj_AI_Hero[] ?? targetList.ToArray();
                    if (enemies.Any(huro => (float) _ua*3 + _uq*3 + _uw + _rr + _ri + _ritems >= huro.Health))
                    {
                        if (_cleavecount <= 1 && _q.IsReady())
                            _r.Cast();
                    }

                    if (enemies.Count() >= 3 && _cleavecount <= 1 && _q.IsReady())                    
                    {                      
                        _r.Cast();
                    }          
                }
            }
        }

        #region Riven: Fat List Incoming

        private static Spell _q;
        private static Spell _w;
        private static Spell _e;
        private static Spell _r;
        private static Vector3 _movePos;

        private static float _qt, _qtRem;
        private static double _rr, _ri, _ritems;
        private static double _rq, _rw, _ra;
        private static double _uq, _uw, _ua;

        private static bool _isattacking;
        private static bool _iscleaving;
        private static bool _iskibursting;
        private static bool _isdashing;

        private static double _astime;
        private static bool _hashydra;
        private static bool _canhydra;
        private static bool _ulton;
        private static bool _canwindslash = true;
        private static bool _canmove = true;
        private static bool _canattack = true;
        private static bool _cancleave = true;
        private static bool _cankiburst = true;
        private static bool _candash = true;

        private static int _lasthydra;
        private static int _lastattack;
        private static int _lastcleave;
        private static int _lastkiburst;
        private static int _lastdash;
        private static int _runiccount;

        private static float _truerange;
        private static readonly int[] Runicpassive =
        {
            20, 20, 25, 25, 25, 30, 30, 30, 35, 35, 35, 40, 40, 40, 45, 45, 45, 50, 50
        };

        private static readonly string[] MinionList =
        {
            "SRU_Razorbeak", "SRU_Krug", "Sru_Crab", "SRU_Baron", "SRU_Dragon",
            "SRU_Blue", "SRU_Red", "SRU_Murkwolf", "SRU_Gromp"
        };

        #endregion
    }
}