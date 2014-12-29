using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Zyra : Champion
    {
        public Zyra()
        {
            LoadSpell();
            LoadMenu();
        }

        private void LoadSpell()
        {
            P = new Spell(SpellSlot.Q, 1470);
            Q = new Spell(SpellSlot.Q, 800);
            W = new Spell(SpellSlot.W, 825);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 700);

            P.SetSkillshot(0.5f, 70f, 1400f, false, SkillshotType.SkillshotLine);
            Q.SetSkillshot(0.8f, 60f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 70f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 500f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        private void LoadMenu()
        {
            //key
            var key = new Menu("键位", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招!").SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "骚扰 (自动)!").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "清线!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                //key.AddItem(new MenuItem("qAA", "Auto Q Enemy During AA Windup").SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("Escape", "使用 E 逃跑").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            //Combo menu:
            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
                combo.AddItem(new MenuItem("qHit", "Q/E 命中敌人数量").SetValue(new Slider(3, 1, 4)));
                combo.AddItem(new MenuItem("UseWCombo", "使用 W").SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));
                combo.AddItem(new MenuItem("R_Min", "R 敌人数量, 6 = 关").SetValue(new Slider(3, 1, 6)));
                combo.AddItem(new MenuItem("ignite", "使用 点燃").SetValue(true));
                combo.AddItem(new MenuItem("igniteMode", "点燃 模式").SetValue(new StringList(new[] { "连招", "抢人头" })));
                //add to menu
                menu.AddSubMenu(combo);
            }
            //Harass menu:
            var harass = new Menu("骚扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
                harass.AddItem(new MenuItem("qHit2", "Q/E 命中敌人数量").SetValue(new Slider(3, 1, 4)));
                harass.AddItem(new MenuItem("UseWHarass", "使用 W").SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(true));
                AddManaManagertoMenu(harass, "Harass", 30);
                //add to menu
                menu.AddSubMenu(harass);
            }
            //Farming menu:
            var farm = new Menu("清线", "Farm");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用 Q").SetValue(false));
                farm.AddItem(new MenuItem("UseWFarm", "使用 W").SetValue(false));
                farm.AddItem(new MenuItem("UseEFarm", "使用 E").SetValue(false));
                AddManaManagertoMenu(farm, "LaneClear", 30);
                //add to menu
                menu.AddSubMenu(farm);
            }

            //Misc Menu:
            var misc = new Menu("杂项", "Misc");
            {
                misc.AddItem(new MenuItem("E_GapCloser", "使用 E 防止突进").SetValue(true));
                misc.AddItem(new MenuItem("mana", "点燃前检测魔法是否足够连招").SetValue(true));
                misc.AddItem(new MenuItem("smartKS", "智能 抢人头").SetValue(true));
                misc.AddItem(new MenuItem("Auto_Bloom", "Auto bloom Plant if Enemy near").SetValue(true));
                //add to menu
                menu.AddSubMenu(misc);
            }

            //Drawings menu:
            var drawing = new Menu("范围", "Drawings");
            {
                drawing.AddItem(new MenuItem("Draw_Disabled", "禁用 所有").SetValue(false));
                drawing.AddItem(new MenuItem("Draw_Q", "显示 Q 范围").SetValue(true));
                drawing.AddItem(new MenuItem("Draw_W", "显示 W 范围").SetValue(true));
                drawing.AddItem(new MenuItem("Draw_E", "显示 E 范围").SetValue(true));
                drawing.AddItem(new MenuItem("Draw_R", "显示 R 范围").SetValue(true));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示组合范围连招").SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示组合填充伤害").SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
                drawing.AddItem(drawComboDamageMenu);
                drawing.AddItem(drawFill);
                DamageIndicator.DamageToUnit = GetComboDamage;
                DamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
                DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
                DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;
                drawComboDamageMenu.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                    };
                drawFill.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                        DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                    };
                menu.AddSubMenu(drawing);
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q)*2;

            if (DFG.IsReady())
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * 3;

            if (DFG.IsReady())
                damage = damage * 1.2;

            if (Ignite_Ready())
                damage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            return (float)damage;
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo").GetValue<bool>(), menu.Item("UseWCombo").GetValue<bool>(),
                menu.Item("UseECombo").GetValue<bool>(), menu.Item("UseRCombo").GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass").GetValue<bool>(), menu.Item("UseWHarass").GetValue<bool>(),
                menu.Item("UseEHarass").GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            if (source == "Harass" && !HasMana("Harass"))
                return;

            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            int igniteMode = menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;
            bool hasMana = manaCheck();
            float dmg = GetComboDamage(target);

            if (useW)
            {
                if (useQ)
                {
                    var pred = Q.GetPrediction(target, true);
                    if (pred.Hitchance >= GetHitchance(source))
                    {
                        Q.Cast(target, packets());
                        Cast_W(pred.CastPosition);
                    }
                }

                //Ignite
                if (menu.Item("ignite").GetValue<bool>() && Ignite_Ready() && hasMana)
                {
                    if (igniteMode == 0 && dmg > target.Health)
                    {
                        Player.Spellbook.CastSpell(IgniteSlot, target);
                    }
                }

                if (useE)
                {
                    var pred = E.GetPrediction(target, true);
                    if (pred.Hitchance >= GetHitchance(source))
                    {
                        E.Cast(target, packets());
                        Cast_W(pred.CastPosition);
                    }
                }
            }
            else
            {
                if(useQ)
                    CastBasicSkillShot(Q, Q.Range, TargetSelector.DamageType.Magical, GetHitchance(source));

                if(useE)
                    CastBasicSkillShot(E, E.Range, TargetSelector.DamageType.Magical, GetHitchance(source));
            }

            if(useR)
                Cast_R();
        }

        private void Cast_W(Vector3 pos)
        {
            if (!W.IsReady() || Player.Distance(pos) > W.Range || W.Instance.Ammo == 0)
                return;

            if (W.Instance.Ammo == 1)// 1 cast
            {
                Utility.DelayAction.Add(50, () => W.Cast(pos, packets()));
            }
            else if (W.Instance.Ammo == 2)// 2 cast
            {
                Utility.DelayAction.Add(50, () => W.Cast(pos, packets()));
                Utility.DelayAction.Add(75, () => W.Cast(pos, packets()));
            }
        }

        private void Cast_R()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;

            var pred = R.GetPrediction(target, true);

            if (GetComboDamage(target) > target.Health - 150 && pred.Hitchance >= HitChance.High)
            {
                R.Cast(target);
                return;
            }

            int minHit = menu.Item("R_Min").GetValue<Slider>().Value;

            if (minHit == 6) return;

            if (pred.AoeTargetsHitCount >= minHit)
                R.Cast(target);
        }

        private void Farm()
        {
            if (!HasMana("LaneClear"))
                return;

            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm").GetValue<bool>();
            var useW = menu.Item("UseWFarm").GetValue<bool>();
            var useE = menu.Item("UseEFarm").GetValue<bool>();

            if (useQ && allMinionsQ.Count > 0 && Q.IsReady())
            {
                var pred = Q.GetCircularFarmLocation(allMinionsQ);
                Q.Cast(pred.Position);

                if(useW)
                    Cast_W(pred.Position.To3D());
            }
            if (useE && allMinionsE.Count > 0 && E.IsReady())
            {
                var pred = E.GetLineFarmLocation(allMinionsE);
                E.Cast(pred.Position);

                if (useW)
                    Cast_W(pred.Position.To3D());
            }
        }

        private void CheckKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.IsValidTarget(E.Range)).OrderByDescending(GetComboDamage))
            {
                //QEW
                if (Player.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E) > target.Health && Q.IsReady() && E.IsReady())
                {
                    E.Cast(target, packets());
                    Q.Cast(target, packets());
                    W.Cast(Q.GetPrediction(target).CastPosition);
                    return;
                }
                //Q + plants
                if (Player.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q)*2 > target.Health && Q.IsReady() && W.IsReady())
                {
                    Q.Cast(Q.GetPrediction(target).CastPosition, packets());
                    W.Cast(Q.GetPrediction(target).CastPosition);
                    return;
                }
                //Q
                if (Player.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && Q.IsReady())
                {
                    Q.Cast(target, packets());
                    return;
                }

                //E
                if (Player.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.E) > target.Health && E.IsReady())
                {
                    E.Cast(target, packets());
                    return;
                }

                //R
                if (Player.Distance(target.ServerPosition) <= R.Range && Player.GetSpellDamage(target, SpellSlot.R) > target.Health && R.IsReady() && menu.Item("R_KS").GetValue<bool>())
                {
                    R.Cast(target);
                    return;
                }
                //ignite
                if (menu.Item("ignite").GetValue<bool>() && Ignite_Ready())
                {
                    int igniteMode = menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;
                    if (igniteMode == 1 && Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health + 20)
                    {
                        Player.Spellbook.CastSpell(IgniteSlot, target);
                    }
                }
            }
        }

        private void AutoBloom()
        {
            if (!Q.IsReady() || !menu.Item("Auto_Bloom").GetValue<bool>())
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.IsValidTarget(Q.Range)).OrderByDescending(GetComboDamage))
            {
                foreach (Obj_AI_Minion plants in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Name == "Zyra" && x.Distance(Player) < Q.Range))
                {
                    var predQ = Q.GetPrediction(target, true);

                    if (Q.IsReady() && plants.Distance(predQ.UnitPosition) < Q.Width)
                        Q.Cast(plants);
                }
            }
        }

        public override void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Player.IsZombie)
            {
                Game.PrintChat("RAWR");
                var target = TargetSelector.GetTarget(P.Range, TargetSelector.DamageType.True);
                if (target == null)
                    return;
                var pred = P.GetPrediction(target);
                if (pred.Hitchance >= HitChance.High)
                    Q.Cast(target);
            }

            if (menu.Item("smartKS").GetValue<bool>())
                CheckKs();

            AutoBloom();

            if (menu.Item("Escape").GetValue<KeyBind>().Active && E.IsReady())
            {
                foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.IsValidTarget(E.Range)).OrderBy(x => x.Distance(Player)))
                {
                    E.Cast(target);
                    return;
                }
            }

            if (menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        public override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("E_GapCloser").GetValue<bool>()) return;

            if (E.IsReady() && gapcloser.Sender.Distance(Player) < 300)
                E.Cast(gapcloser.Sender);
        }

        /*
        public override void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            GamePacket g = new GamePacket(args.PacketData);
            if (g.Header != 0xFE)
                return;

            if (menu.Item("qAA").GetValue<KeyBind>().Active)
            {
                if (Packet.MultiPacket.OnAttack.Decoded(args.PacketData).Type == Packet.AttackTypePacket.TargetedAA)
                {
                    g.Position = 1;
                    var k = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(g.ReadInteger());
                    if (k is Obj_AI_Hero && k.IsEnemy)
                    {
                        if (Vector3.Distance(k.Position, Player.Position) <= Q.Range)
                        {
                            Q.Cast(k.Position, packets());
                        }
                    }
                }
            }
        }*/

        public override void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (menu.Item("Draw_Q").GetValue<bool>())
                if (Q.Level > 0)
                    Utility.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_W").GetValue<bool>())
                if (W.Level > 0)
                    Utility.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E").GetValue<bool>())
                if (E.Level > 0)
                    Utility.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R").GetValue<bool>())
                if (R.Level > 0)
                    Utility.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }
    }
}
