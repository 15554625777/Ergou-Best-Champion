#region
using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using System.Linq;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
#endregion

namespace LightningRyze
{
	internal class Program
	{
		private static Menu Config;
		private static Orbwalking.Orbwalker Orbwalker;
		private static Obj_AI_Hero Target;
		private static Obj_AI_Hero Player;
		private static Spell Q;
		private static Spell W;
		private static Spell E;
		private static Spell R;
		private static SpellSlot IgniteSlot;
		private static string LastCast;
		private static float LastFlashTime;
		
		private static void Main(string[] args)
		{
			CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
		}
		
		private static void Game_OnGameLoad(EventArgs args)
		{
			Player = ObjectManager.Player;
			
			if (Player.ChampionName != "Ryze") return;
			
			Q = new Spell(SpellSlot.Q, 625);
			W = new Spell(SpellSlot.W, 600);
			E = new Spell(SpellSlot.E, 600);
			R = new Spell(SpellSlot.R);
			
			IgniteSlot = Player.GetSpellSlot("SummonerDot");
			
			Config = new Menu("Lightning 瑞兹", "Lightning Ryze", true);
			
			var targetSelectorMenu = new Menu("目标 选择", "Target Selector");
			TargetSelector.AddToMenu(targetSelectorMenu);
			Config.AddSubMenu(targetSelectorMenu);
			
			Config.AddSubMenu(new Menu("走 砍", "Orbwalker"));
			Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
			
			Config.AddSubMenu(new Menu("连 招", "Combo"));
			Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "开启!").SetValue(new KeyBind(32, KeyBindType.Press)));
			Config.SubMenu("Combo").AddItem(new MenuItem("TypeCombo", "模式").SetValue(new StringList(new[] {"混合 模式","爆发 模式","长 模式"},0)));
			Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "使用 R").SetValue(true));
			Config.SubMenu("Combo").AddItem(new MenuItem("UseIgnite", "使用 点燃").SetValue(true));
			Config.SubMenu("Combo").AddItem(new MenuItem("UseAA", "自动 攻击").SetValue(true));
			
			Config.AddSubMenu(new Menu("骚 扰", "Harass"));
			Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "开启!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
			Config.SubMenu("Harass").AddItem(new MenuItem("HQ", "使用 Q").SetValue(true));
			Config.SubMenu("Harass").AddItem(new MenuItem("HW", "使用 W").SetValue(true));
			Config.SubMenu("Harass").AddItem(new MenuItem("HE", "使用 E").SetValue(true));
			Config.SubMenu("Harass").AddItem(new MenuItem("AutoPoke", "自动 Q 骚扰").SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Toggle)));
			Config.SubMenu("Harass").AddItem(new MenuItem("ManaH", "当 MP 大于多少时骚扰").SetValue(new Slider(30, 1, 100)));
			
			Config.AddSubMenu(new Menu("打 钱", "Farm"));
			Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "控 线!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
			Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "清 线!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
			Config.SubMenu("Farm").AddItem(new MenuItem("FQ", "使用 Q").SetValue(true));
			Config.SubMenu("Farm").AddItem(new MenuItem("FW", "使用 W").SetValue(true));
			Config.SubMenu("Farm").AddItem(new MenuItem("FE", "使用 E").SetValue(true));
			Config.SubMenu("Farm").AddItem(new MenuItem("FR", "使用 R").SetValue(true));
			
			Config.AddSubMenu(new Menu("击 杀", "KillSteal"));
			Config.SubMenu("KillSteal").AddItem(new MenuItem("KQ", "使用 Q").SetValue(true));
			Config.SubMenu("KillSteal").AddItem(new MenuItem("KW", "使用 W").SetValue(true));
			Config.SubMenu("KillSteal").AddItem(new MenuItem("KE", "使用 E").SetValue(true));
			
			Config.AddSubMenu(new Menu("额 外", "Extra"));
			Config.SubMenu("Extra").AddItem(new MenuItem("tearStack", "Q+W 双层效果").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Toggle)));
			Config.SubMenu("Extra").AddItem(new MenuItem("UseSeraphs", "使用 炽天使之拥").SetValue(true));
			Config.SubMenu("Extra").AddItem(new MenuItem("HP", "当 HP 小于或等于").SetValue(new Slider(20, 100, 0)));
			Config.SubMenu("Extra").AddItem(new MenuItem("WInterrupt", "W 打断法术").SetValue(true));
			Config.SubMenu("Extra").AddItem(new MenuItem("WGap", "被突进时 使用w").SetValue(true));
			Config.SubMenu("Extra").AddItem(new MenuItem("UsePacket", "使用 封包").SetValue(true));
			
			Config.AddSubMenu(new Menu("范 围", "Drawings"));
			Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q 范 围").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
			Config.SubMenu("Drawings").AddItem(new MenuItem("WERange", "W+E 范 围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
			Config.AddToMainMenu();
			
			Game.PrintChat("Lightning 鐟炲吂涓ㄥ姞杓夋垚鍔燂紒婕㈠寲by 鑺辫竟锛丵Q缇361630847!");

			Game.OnGameUpdate += Game_OnGameUpdate;
			Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
			Drawing.OnDraw += Drawing_OnDraw;
			AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
			Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
			Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
		}
		
		private static bool GetBool(string s)
		{
			return Config.Item(s).GetValue<bool>();
		}
		
		private static bool GetActive(string s)
		{
			return Config.Item(s).GetValue<KeyBind>().Active;
		}
		
		private static LeagueSharp.Common.Circle GetCircle(string s)
		{
			return Config.Item(s).GetValue<Circle>();
		}
		
		private static int GetSlider(string s)
		{
			return Config.Item(s).GetValue<Slider>().Value;
		}
		
		private static int GetSelected(string s)
		{
			return Config.Item(s).GetValue<StringList>().SelectedIndex;
		}
		
		private static void Game_OnGameUpdate(EventArgs args)
		{
			KillSteal();
			
			Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
			
			if (GetActive("ComboActive"))
			{
				if (!GetBool("UseAA")) Orbwalker.SetAttack(false);
				else Orbwalker.SetAttack(true);
				if (GetSelected("TypeCombo") == 0) ComboMixed();
				else if (GetSelected("TypeCombo") == 1) ComboBurst();
				else if (GetSelected("TypeCombo") == 2) ComboLong();
			}
			else if (GetActive("HarassActive")) Harass();
			
//			if (GetActive("tearStack")) TearExploit();
			if (GetActive("LaneClearActive") || GetActive("FreezeActive")) Farm();
			
			if (GetActive("AutoPoke")) AutoPoke(Target);
		}
		
		private static bool IsOnTheLine(Vector3 point, Vector3 start, Vector3 end)
		{
			var obj = Geometry.ProjectOn(point.To2D(),start.To2D(),end.To2D());
			if (obj.IsOnSegment) return true;
			return false;
		}
		
//		private static void TearExploit()
//		{
//			var allMinions = MinionManager.GetMinions(Player.Position, W.Range, MinionTypes.All ,MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
//			if (allMinions.Count < 1) return;
//			if (Q.IsReady() && W.IsReady())
//			{
//				foreach (var minion in allMinions)
//				{
//					if (Player.GetSpellDamage(minion,SpellSlot.Q) > minion.Health)
//					{
//						var delay = (GetDistance(minion)*60)/Q.Range;
//						Q.CastOnUnit(minion,GetBool("UsePacket"));
//						Utility.DelayAction.Add((int)delay, () => W.CastOnUnit(minion, GetBool("UsePacket")));
//						return;
//					}
//				}
//			}
//		}
		
		private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
		{
			if (sender.IsMe)
			{
				if (args.SData.Name.ToLower() == "overload") LastCast = "Q";
				else if (args.SData.Name.ToLower() == "runeprison") LastCast = "W";
				else if (args.SData.Name.ToLower() == "spellflux") LastCast = "E";
				else if (args.SData.Name.ToLower() == "desperatepower") LastCast = "R";
				else if (args.SData.Name.ToLower() == "summonerflash") LastFlashTime = Game.Time;
				if (GetActive("tearStack"))
				{
					var spellSlot = Player.GetSpellSlot(args.SData.Name, false);
					var target = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(args.Target.NetworkId);
					var distance = Player.ServerPosition.Distance(target.ServerPosition);
					var delay = 1000 * (distance / args.SData.MissileSpeed);
					delay -= Game.Ping / 2;
					if (spellSlot == SpellSlot.Q && W.IsReady() && target.IsMinion && target.Health < Q.GetDamage(target))
						Utility.DelayAction.Add((int)delay, () => W.CastOnUnit(target, true));
				}
			}
			if (GetBool("UseSeraphs") && sender.IsEnemy && (sender.Type == GameObjectType.obj_AI_Hero || sender.Type == GameObjectType.obj_AI_Turret))
			{
				if ( (args.SData.Name != null && IsOnTheLine(Player.Position,args.Start,args.End)) || (args.Target == Player && GetDistance(sender) <= 700))
				{
					if (Player.Health/Player.MaxHealth*100 <= GetSlider("HP") && Items.HasItem(3040) && Items.CanUseItem(3040)) Items.UseItem(3040);
				}
			}
		}
		
		private static void Drawing_OnDraw(EventArgs args)
		{
			if (GetCircle("QRange").Active && !Player.IsDead) Utility.DrawCircle(Player.Position, Q.Range, GetCircle("QRange").Color);
			if (GetCircle("WERange").Active && !Player.IsDead) Utility.DrawCircle(Player.Position, W.Range, GetCircle("WERange").Color);
		}
		
		private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
		{
			if (Config.Item("ComboActive").GetValue<KeyBind>().Active || Config.Item("HarassActive").GetValue<KeyBind>().Active)
				args.Process = !(Q.IsReady() || W.IsReady() || E.IsReady() || GetDistance(args.Target) >= 600);
		}
		
		private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
		{
			if (Player.HasBuff("Recall") || Player.IsWindingUp) return;
			if (GetBool("WGap") && W.IsReady() && GetDistance(gapcloser.Sender) <= W.Range && gapcloser.Sender.IsTargetable)
				W.CastOnUnit(gapcloser.Sender,GetBool("UsePacket"));
		}
		
		private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
		{
			if (GetBool("WInterrupt") && W.IsReady() && GetDistance(unit) <= W.Range && unit.IsTargetable) W.CastOnUnit(unit,GetBool("UsePacket"));
		}
		
		private static float GetDistance(AttackableUnit Target)
		{
			return Vector3.Distance(Player.Position, Target.Position);
		}
		
		private static bool CanIgnite()
		{
			return (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready);
		}
		
		private static double GetComboDamage(Obj_AI_Base target)
		{
			double dmg = 0;
			if (Q.IsReady() && GetDistance(target) <= Q.Range)
				dmg += Player.GetSpellDamage(target, SpellSlot.Q)*2;
			if (W.IsReady() && GetDistance(target) <= W.Range)
				dmg += Player.GetSpellDamage(target, SpellSlot.W);
			if (E.IsReady() && GetDistance(target) <= E.Range)
				dmg += Player.GetSpellDamage(target, SpellSlot.E);
			if (CanIgnite() && GetDistance(target) <= 600)
				dmg += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
			return dmg;
		}
		
		private static void ComboMixed()
		{
			if (Target == null) return;
			if (GetBool("UseIgnite") && CanIgnite() && GetDistance(Target) <= 600 && GetComboDamage(Target) >= (double)Target.Health)
				Player.Spellbook.CastSpell(IgniteSlot, Target);
			if (Game.Time - LastFlashTime < 1 && W.IsReady()) W.CastOnUnit(Target,GetBool("UsePacket"));
			else
			{
				if (Q.IsKillable(Target) && Q.IsReady()) Q.CastOnUnit(Target,GetBool("UsePacket"));
				else if (E.IsKillable(Target) && E.IsReady()) E.CastOnUnit(Target,GetBool("UsePacket"));
				else if (W.IsKillable(Target) && W.IsReady()) W.CastOnUnit(Target,GetBool("UsePacket"));
				else if (GetDistance(Target) >= 575 && !Utility.IsBothFacing(Player,Target,575) && W.IsReady()) W.CastOnUnit(Target,GetBool("UsePacket"));
				else
				{
					if (Q.IsReady() && W.IsReady() && E.IsReady() && GetComboDamage(Target) >= (double)Target.Health)
					{
						if (Q.IsReady()) Q.CastOnUnit(Target,GetBool("UsePacket"));
						else if (R.IsReady() && GetBool("UseR")) R.Cast(GetBool("UsePacket"));
						else if (W.IsReady()) W.CastOnUnit(Target,GetBool("UsePacket"));
						else if (E.IsReady()) E.CastOnUnit(Target,GetBool("UsePacket"));
					}
					else if (Math.Abs(Player.PercentCooldownMod) >= 0.2)
					{
						if (Target.CountEnemysInRange(300) > 1)
						{
							if (LastCast == "Q")
							{
								if (Q.IsReady()) Q.CastOnUnit(Target ,GetBool("UsePacket"));
								if (R.IsReady() && GetBool("UseR")) R.Cast(GetBool("UsePacket"));
								if (!R.IsReady()) W.CastOnUnit(Target ,GetBool("UsePacket"));
								if (!R.IsReady() && !W.IsReady()) E.CastOnUnit(Target ,GetBool("UsePacket"));
							}
							else Q.CastOnUnit(Target,GetBool("UsePacket"));
						}
						else
						{
							if (LastCast == "Q")
							{
								if (Q.IsReady()) Q.CastOnUnit(Target ,GetBool("UsePacket"));
								if (W.IsReady()) W.CastOnUnit(Target ,GetBool("UsePacket"));
								if (!W.IsReady()) E.CastOnUnit(Target ,GetBool("UsePacket"));
								if (!W.IsReady() && !E.IsReady() && GetBool("UseR")) R.Cast(GetBool("UsePacket"));
							}
							else
								if (Q.IsReady()) Q.CastOnUnit(Target ,GetBool("UsePacket"));
						}
					}
					else
					{
						if (Q.IsReady()) Q.CastOnUnit(Target ,GetBool("UsePacket"));
						else if (R.IsReady() && GetBool("UseR")) R.Cast(GetBool("UsePacket"));
						else if (E.IsReady()) E.CastOnUnit(Target ,GetBool("UsePacket"));
						else if (W.IsReady()) W.CastOnUnit(Target ,GetBool("UsePacket"));
					}
				}
			}
		}
		
		private static void ComboBurst()
		{
			if (Target == null) return;
			if (GetBool("UseIgnite") && CanIgnite() && GetDistance(Target) <= 600 && GetComboDamage(Target) >= (double)Target.Health)
				Player.Spellbook.CastSpell(IgniteSlot, Target);
			if (Game.Time - LastFlashTime < 1 && W.IsReady()) W.CastOnUnit(Target ,GetBool("UsePacket"));
			else
			{
				if (Q.IsKillable(Target) && Q.IsReady()) Q.CastOnUnit(Target,GetBool("UsePacket"));
				else if (E.IsKillable(Target) && E.IsReady()) E.CastOnUnit(Target,GetBool("UsePacket"));
				else if (W.IsKillable(Target) && W.IsReady()) W.CastOnUnit(Target,GetBool("UsePacket"));
				else if (GetDistance(Target) >= 575 && !Utility.IsBothFacing(Player,Target,575) && W.IsReady()) W.CastOnUnit(Target,GetBool("UsePacket"));
				else
				{
					if (Q.IsReady()) Q.CastOnUnit(Target ,GetBool("UsePacket"));
					else if (R.IsReady() && GetBool("UseR")) R.Cast(GetBool("UsePacket"));
					else if (E.IsReady()) E.CastOnUnit(Target ,GetBool("UsePacket"));
					else if (W.IsReady()) W.CastOnUnit(Target ,GetBool("UsePacket"));
				}
			}
		}
		
		private static void ComboLong()
		{
			if (Target == null) return;
			if (GetBool("UseIgnite") && CanIgnite() && GetDistance(Target) <= 600 && GetComboDamage(Target) >= (double)Target.Health)
				Player.Spellbook.CastSpell(IgniteSlot, Target);
			if (Game.Time - LastFlashTime < 1 && W.IsReady()) W.CastOnUnit(Target ,GetBool("UsePacket"));
			else
			{
				if (Q.IsKillable(Target) && Q.IsReady()) Q.CastOnUnit(Target,GetBool("UsePacket"));
				else if (E.IsKillable(Target) && E.IsReady()) E.CastOnUnit(Target,GetBool("UsePacket"));
				else if (W.IsKillable(Target) && W.IsReady()) W.CastOnUnit(Target,GetBool("UsePacket"));
				else if (GetDistance(Target) >= 575 && !Utility.IsBothFacing(Player,Target,575) && W.IsReady()) W.CastOnUnit(Target,GetBool("UsePacket"));
				else
				{
					if (Target.CountEnemysInRange(300) > 1)
					{
						if (LastCast == "Q")
						{
							if (Q.IsReady()) Q.CastOnUnit(Target ,GetBool("UsePacket"));
							if (R.IsReady() && GetBool("UseR")) R.Cast(GetBool("UsePacket"));
							if (!R.IsReady()) W.CastOnUnit(Target ,GetBool("UsePacket"));
							if (!R.IsReady() && !W.IsReady()) E.CastOnUnit(Target ,GetBool("UsePacket"));
						}
						else Q.CastOnUnit(Target,GetBool("UsePacket"));
					}
					else
					{
						if (LastCast == "Q")
						{
							if (Q.IsReady()) Q.CastOnUnit(Target ,GetBool("UsePacket"));
							if (W.IsReady()) W.CastOnUnit(Target ,GetBool("UsePacket"));
							if (!W.IsReady()) E.CastOnUnit(Target ,GetBool("UsePacket"));
							if (!W.IsReady() && !E.IsReady() && R.IsReady() && GetBool("UseR")) R.Cast(GetBool("UsePacket"));
						}
						else
							if (Q.IsReady()) Q.CastOnUnit(Target ,GetBool("UsePacket"));
					}
				}
			}
		}
		
		private static void Harass()
		{
			if (Target == null) return;
			if (GetDistance(Target) <= 625 )
			{
				if (GetBool("HQ") && Q.IsReady()) Q.CastOnUnit(Target,GetBool("UsePacket"));
				if (GetBool("HW") && W.IsReady()) W.CastOnUnit(Target,GetBool("UsePacket"));
				if (GetBool("HQ") && Q.IsReady()) Q.CastOnUnit(Target,GetBool("UsePacket"));
				if (GetBool("HE") && E.IsReady()) E.CastOnUnit(Target,GetBool("UsePacket"));
				if (GetBool("HQ") && Q.IsReady()) Q.CastOnUnit(Target,GetBool("UsePacket"));
			}
		}
		
		private static void Farm()
		{
			var allMinions = MinionManager.GetMinions(Player.Position, Q.Range, MinionTypes.All ,MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
			if (allMinions.Count < 1) return;
			if (GetActive("FreezeActive"))
			{
				foreach (var minion in allMinions)
				{
					if (GetBool("FQ") && Q.IsReady() && Player.GetSpellDamage(minion, SpellSlot.Q) > minion.Health)
						Q.CastOnUnit(minion,GetBool("UsePacket"));
					else if (GetBool("FW") && W.IsReady() && Player.GetSpellDamage(minion, SpellSlot.W) > minion.Health)
						W.CastOnUnit(minion,GetBool("UsePacket"));
					else if (GetBool("FE") && E.IsReady() && Player.GetSpellDamage(minion, SpellSlot.E) > minion.Health)
						E.CastOnUnit(minion,GetBool("UsePacket"));
				}
			}
			else if (GetActive("LaneClearActive"))
			{
				foreach (var minion in allMinions)
				{
					if ((allMinions.Count >= 5 || allMinions.Count >= 2 && Player.Health/Player.MaxHealth*100 <= 5) && GetBool("FR") && R.IsReady()) R.Cast(GetBool("UsePacket"));
					if (GetBool("FQ") && Q.IsReady()) Q.CastOnUnit(minion,GetBool("UsePacket"));
					if (GetBool("FW") && W.IsReady()) W.CastOnUnit(minion,GetBool("UsePacket"));
					if (GetBool("FQ") && Q.IsReady()) Q.CastOnUnit(minion,GetBool("UsePacket"));
					if (GetBool("FE") && E.IsReady()) E.CastOnUnit(minion,GetBool("UsePacket"));
					if (GetBool("FQ") && Q.IsReady()) Q.CastOnUnit(minion,GetBool("UsePacket"));
				}
			}
		}
		
		private static void AutoPoke(Obj_AI_Hero enemy)
		{
			if (enemy == null) return;
			if (Q.IsReady() && GetDistance(enemy) <= Q.Range && enemy.IsTargetable && (Player.Mana/Player.MaxMana)*100 > GetSlider("ManaH"))
				Q.CastOnUnit(enemy, GetBool("UsePacket"));
		}
		
		private static void KillSteal()
		{
			if (!GetBool("KQ") && !GetBool("KW") && !GetBool("KE")) return;
			foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => GetDistance(enemy) <= Q.Range && enemy.IsEnemy && enemy.IsVisible && !enemy.IsDead && enemy.IsTargetable))
			{
				if (enemy == null) return;
				if (GetBool("KQ") && Q.IsReady() && Player.GetSpellDamage(enemy, SpellSlot.Q) > enemy.Health) Q.CastOnUnit(enemy,GetBool("UsePacket"));
				if (GetBool("KW") && W.IsReady() && Player.GetSpellDamage(enemy, SpellSlot.W) > enemy.Health) W.CastOnUnit(enemy,GetBool("UsePacket"));
				if (GetBool("KE") && E.IsReady() && Player.GetSpellDamage(enemy, SpellSlot.E) > enemy.Health) E.CastOnUnit(enemy,GetBool("UsePacket"));
				if (GetBool("KQ") && GetBool("KW") && Q.IsReady() && W.IsReady() &&
				    Player.GetSpellDamage(enemy, SpellSlot.Q)+Player.GetSpellDamage(enemy, SpellSlot.W) > enemy.Health)
				{
					W.CastOnUnit(enemy,GetBool("UsePacket"));
					Utility.DelayAction.Add(50, () => Q.CastOnUnit(enemy,GetBool("UsePacket")));
				}
				if (GetBool("KQ") && GetBool("KE") && Q.IsReady() && W.IsReady() &&
				    Player.GetSpellDamage(enemy, SpellSlot.Q)+Player.GetSpellDamage(enemy, SpellSlot.E) > enemy.Health)
				{
					E.CastOnUnit(enemy,GetBool("UsePacket"));
					Utility.DelayAction.Add(50, () => Q.CastOnUnit(enemy,GetBool("UsePacket")));
				}
				if (GetBool("KW") && GetBool("KE") && Q.IsReady() && W.IsReady() &&
				    Player.GetSpellDamage(enemy, SpellSlot.W)+Player.GetSpellDamage(enemy, SpellSlot.E) > enemy.Health)
				{
					E.CastOnUnit(enemy,GetBool("UsePacket"));
					Utility.DelayAction.Add(50, () => W.CastOnUnit(enemy,GetBool("UsePacket")));
				}
				if (GetBool("KQ") && GetBool("KW") && GetBool("KE") && Q.IsReady() && W.IsReady() && E.IsReady() &&
				    Player.GetSpellDamage(enemy, SpellSlot.Q)+Player.GetSpellDamage(enemy, SpellSlot.W)+Player.GetSpellDamage(enemy, SpellSlot.E) > enemy.Health)
				{
					E.CastOnUnit(enemy,GetBool("UsePacket"));
					Utility.DelayAction.Add(50, () => W.CastOnUnit(enemy,GetBool("UsePacket")));
					Utility.DelayAction.Add(50, () => Q.CastOnUnit(enemy,GetBool("UsePacket")));
				}
			}
		}
	}
}