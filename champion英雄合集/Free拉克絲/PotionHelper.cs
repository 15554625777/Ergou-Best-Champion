using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace FreeLux
{
    static class PotionHelper
    {
        private enum PotionType
        {
            HP,
            MP
        };

        private class Potion
        {
            public string Name { get; set; }
            public int Charges { get; set; }
            public ItemId ItemId { get; set; }
            public int Priority { get; set; }
            public List<PotionType> TypeList { get; set; } 
        }

        private static List<Potion> potionsList;

        private static void Initialize()
        {
            potionsList = new List<Potion>();
            potionsList.Add(
                new Potion
                {
                    Name = "ItemCrystalFlask",
                    Charges = 1,
                    ItemId = (ItemId) 2041,
                    Priority = 1,
                    TypeList = new List<PotionType>() { PotionType.HP, PotionType.MP }
                });
            potionsList.Add(
                new Potion
                {
                    Name = "RegenerationPotion",
                    Charges = 0,
                    ItemId = (ItemId) 2003,
                    Priority = 2,
                    TypeList = new List<PotionType>() { PotionType.HP }
                });
            potionsList.Add(
                new Potion
                {
                    Name = "FlaskOfCrystalWater",
                    Charges = 0,
                    ItemId = (ItemId) 2004,
                    Priority = 3,
                    TypeList = new List<PotionType>() { PotionType.MP }
                });
            potionsList.Add(
                new Potion
                {
                    Name = "ItemMiniRegenPotion",
                    Charges = 1,
                    ItemId = (ItemId) 2010,
                    Priority = 4,
                    TypeList = new List<PotionType>() { PotionType.HP, PotionType.MP }
                });
            potionsList = potionsList.OrderBy(p => p.Priority).ToList();
        }

        public static void AddToMenu(Menu potionMenu)
        {
            Initialize();

            potionMenu.AddItem(new MenuItem("potionHpEnabled", "Use HP Potion").SetValue(true));
            potionMenu.AddItem(new MenuItem("potionHpPercentage", "Use HP Potion at %").SetValue(new Slider(40)));

            potionMenu.AddItem(new MenuItem("potionMpEnabled", "Use MP Potion").SetValue(true));
            potionMenu.AddItem(new MenuItem("potionMpPercentage", "Use MP Potion at %").SetValue(new Slider(40)));

            Game.OnGameUpdate += OnGameUpdate;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (FreeLux.Player.IsRecalling() || ObjectManager.Player.InFountain() && ObjectManager.Player.InShop())
                return;

            bool useHP = FreeLux.Menu.Item("potionHpEnabled").GetValue<bool>();
            bool useMP = FreeLux.Menu.Item("potionMpEnabled").GetValue<bool>();
            int useHPPercentage = FreeLux.Menu.Item("potionHpPercentage").GetValue<Slider>().Value;
            int useMPPercentage = FreeLux.Menu.Item("potionMpPercentage").GetValue<Slider>().Value;

            try
            {
                if (useHP && FreeLux.Player.HealthPercentage() <= useHPPercentage)
                {
                    var hpSlot = GetPotionSlot(PotionType.HP);
                    if (!IsBuffActive(PotionType.HP))
                        FreeLux.Player.Spellbook.CastSpell(hpSlot.SpellSlot);
                }
                if (useMP && FreeLux.Player.ManaPercentage() <= useMPPercentage)
                {
                    var mpSlot = GetPotionSlot(PotionType.MP);
                    if (!IsBuffActive(PotionType.MP))
                        FreeLux.Player.Spellbook.CastSpell(mpSlot.SpellSlot);
                }
            }
            catch (Exception)
            {
                
            }
        }

        private static bool IsBuffActive(PotionType potionType)
        {
            return (from potion in potionsList
                where potion.TypeList.Contains(potionType)
                from buff in ObjectManager.Player.Buffs
                where buff.Name == potion.Name && buff.IsActive
                select potion).Any();
        }

        private static InventorySlot GetPotionSlot(PotionType potionType)
        {
            return (from potion in potionsList
                where potion.TypeList.Contains(potionType)
                from item in ObjectManager.Player.InventoryItems
                where item.Id == potion.ItemId && item.Charges >= potion.Charges
                select item).FirstOrDefault();
        }
    }
}
