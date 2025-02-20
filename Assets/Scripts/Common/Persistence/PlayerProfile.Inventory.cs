using Ulko.Data;
using Ulko.Data.Inventory;
using Ulko.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ulko
{
    public static partial class PlayerProfile
    {
        public static IEnumerable<Item> GetInventory(bool includeEquipped)
        {
            if (!includeEquipped)
            {
                return loadedGame.inventory.Where(i => i.count > 0);
            }
            else
            {
                var inventory = new List<Item>();

                foreach (var item in loadedGame.inventory)
                {
                    if (item.count > 0)
                        inventory.Add(item.Clone());
                }

                foreach (var hero in loadedGame.party)
                {
                    var equipment = GetEquipment(hero.id);
                    foreach (var item in equipment)
                    {
                        var index = inventory.FindIndex(i => i.id == item);
                        if (index != -1)
                            inventory[index].count++;
                        else
                            inventory.Add(new Item(item, 1));
                    }
                }

                return inventory;
            }
        }

        public static Item GetInventoryItem(string id)
        {
            return loadedGame.inventory.Find(i => i.id == id);
        }

        public static IEnumerable<Item> GetConsumables()
        {
            return loadedGame.inventory.FindAll(i => Database.Items[i.id].Type == ItemType.Consumable && i.count > 0);
        }

        public static IEnumerable<Item> GetMods()
        {
            return loadedGame.inventory.FindAll(i => Database.Items[i.id].Type == ItemType.Mod && i.count > 0);
        }

        public static IEnumerable<Item> GetMods(string elementId)
        {
            return loadedGame.inventory.FindAll(
                i => Database.Items[i.id].Type == ItemType.Mod
                && i.count > 0
                && (Database.Items[i.id] as Mod).elements.Find(e => e.id == elementId) != null);
        }

        public static void BuyItem(string id, int amount)
        {
            var itemData = Database.Items[id];
            int cost = itemData.value * amount;
            AddMoney(-cost);
            AddInventoryItem(id, amount);
        }

        public static int ItemValue(int value, float multiplier)
        {
            return (int)(value * multiplier);
        }

        public static void SellItem(string id, int amount, float valueMultiplier)
        {
            var itemData = Database.Items[id];
            int value = ItemValue(itemData.value * amount, valueMultiplier);
            AddMoney(value);
            RemoveInventoryItem(id, amount);
        }

        public static void AddInventoryItem(string id, int amount)
        {
            var inventoryItem = loadedGame.inventory.FirstOrDefault(i => i.id == id);
            if (inventoryItem != null)
            {
                inventoryItem.count += amount;
            }
            else
            {
                loadedGame.inventory.Add(new Item(id, amount));
            }
        }

        public static bool RemoveInventoryItem(string id, int amount)
        {
            var item = loadedGame.inventory.FirstOrDefault(i => i.id == id);
            if (item != null && item.count >= amount)
            {
                item.count -= amount;
                return true;
            }

            return false;
        }

        public static int GetAvailableItemCount(string id)
        {
            var item = loadedGame.inventory.Find(i => i.id == id);
            if (item == null) return 0;
            else return item.count;
        }

        public static int GetTotalItemCount(string id)
        {
            var inventory = GetInventory(true);
            var item = inventory.FirstOrDefault(i => i.id == id);
            if (item == null) return 0;
            else return item.count;
        }

        public static string GetWeapon(string heroId)
        {
            var hero = GetPartyMember(heroId);
            if (hero != null)
            {
                return hero.equippedWeapon;
            }
            else
            {
                return null;
            }
        }

        public static Weapon GetWeaponData(string heroId)
        {
            var weaponId = GetWeapon(heroId);
            if (weaponId != null && Database.Items.ContainsKey(weaponId))
                return Database.Items[weaponId] as Weapon;

            return null;
        }

        public static string GetArmor(string heroId, string armorType)
        {
            return GetArmor(GetPartyMember(heroId), armorType);
        }

        public static string GetArmor(Hero hero, string armorType)
        {
            if (hero == null)
                return null;

            foreach (var armorId in hero.equippedArmor)
            {
                var armor = Database.Items[armorId];
                if (armor.tag.id == armorType)
                    return armorId;
            }

            return null;
        }

        public static Armor GetArmorData(string heroId, string armorType)
        {
            var armorId = GetArmor(heroId, armorType);

            if (!string.IsNullOrEmpty(armorId))
                return Database.Items[armorId] as Armor;

            return null;
        }

        public static IEnumerable<string> GetArmor(string heroId)
        {
            return GetArmor(GetPartyMember(heroId));
        }

        public static IEnumerable<string> GetArmor(Hero hero)
        {
            var equipped = new List<string>();
            if (hero == null)
                return equipped;

            foreach (var armor in hero.equippedArmor)
            {
                equipped.Add(armor);
            }

            return equipped;
        }

        public static List<Armor> GetArmorData(string heroId)
        {
            var armorData = new List<Armor>();
            var armorItem = GetArmor(heroId);

            foreach (var armor in armorItem)
            {
                if (Database.Items.ContainsKey(armor))
                    armorData.Add(Database.Items[armor] as Armor);
            }

            return armorData;
        }

        public static string GetMod(string heroId, int slotIndex)
        {
            return GetMod(GetPartyMember(heroId), slotIndex);
        }

        public static string GetMod(Hero hero, int slotIndex)
        {
            if (hero == null)
                return null;

            if(slotIndex >= 0 && slotIndex < hero.modSlots.Count)
            {
                return hero.modSlots[slotIndex].modId;
            }

            return null;
        }

        public static Mod GetModData(string heroId, int slotIndex)
        {
            var modId = GetMod(heroId, slotIndex);

            if (!string.IsNullOrEmpty(modId))
                return Database.Items[modId] as Mod;

            return null;
        }

        public static IEnumerable<string> GetMod(string heroId)
        {
            return GetMod(GetPartyMember(heroId));
        }

        public static IEnumerable<string> GetMod(Hero hero)
        {
            var equipped = new List<string>();
            if (hero == null)
                return equipped;

            foreach (var slot in hero.modSlots)
            {
                if(!string.IsNullOrEmpty(slot.modId))
                    equipped.Add(slot.modId);
            }

            return equipped;
        }

        public static List<Mod> GetModData(string heroId)
        {
            var modData = new List<Mod>();
            var modItems = GetMod(heroId);

            foreach (var mod in modItems)
            {
                if (Database.Items.ContainsKey(mod))
                    modData.Add(Database.Items[mod] as Mod);
            }

            return modData;
        }

        public static bool IsEquipped(string heroId, string itemId)
        {
            var equipment = GetEquipment(heroId);
            return equipment.FirstOrDefault(e => e == itemId) != null;
        }

        public static IEnumerable<string> GetEquipment(string heroId)
        {
            var equipped = new List<string>();

            var weapon = GetWeapon(heroId);
            if (!string.IsNullOrEmpty(weapon))
                equipped.Add(weapon);

            var hero = GetPartyMember(heroId);
            if (hero != null)
            {
                foreach (var armor in hero.equippedArmor)
                {
                    equipped.Add(armor);
                }

                equipped.AddRange(GetMod(hero));
            }

            return equipped;
        }

        public static IEnumerable<Item> GetAvailableWeapons(string weaponType)
        {
            var inventory = GetInventory(false);
            return inventory.Where(i => Database.Items[i.id].Type == ItemType.Weapon && Database.Items[i.id].tag.id == weaponType);
        }

        public static IEnumerable<Item> GetAvailableArmor(string armorType)
        {
            var inventory = GetInventory(false);
            return inventory.Where(i => Database.Items[i.id].Type == ItemType.Armor && Database.Items[i.id].tag.id == armorType);
        }

        public static void EquipWeapon(string heroId, string weaponId)
        {
            if (Database.Heroes[heroId].isGuest)
            {
                Debug.LogWarning("Cannot equip weapon on guest " + heroId);
                return;
            }

            var currentlyEquipped = GetWeapon(heroId);
            if (!string.IsNullOrEmpty(currentlyEquipped))
            {
                AddInventoryItem(currentlyEquipped, 1);
            }

            var hero = CreateOrGetHero(heroId);
            hero.equippedWeapon = weaponId;
            RemoveInventoryItem(weaponId, 1);
        }

        public static void UnequipWeapon(string heroId)
        {
            if (Database.Heroes[heroId].isGuest)
            {
                Debug.LogWarning("Cannot unequip weapon on guest " + heroId);
                return;
            }

            var currentlyEquipped = GetWeapon(heroId);
            if (!string.IsNullOrEmpty(currentlyEquipped))
            {
                AddInventoryItem(currentlyEquipped, 1);
            }

            var hero = CreateOrGetHero(heroId);
            hero.equippedWeapon = "";
        }

        public static void EquipArmor(string heroId, string armorId)
        {
            if (Database.Heroes[heroId].isGuest)
            {
                Debug.LogWarning("Cannot equip armor on guest " + heroId);
                return;
            }

            var hero = GetPartyMember(heroId);

            var armor = Database.Items[armorId];
            var currentlyEquipped = GetArmor(hero, armor.tag.id);

            if (!string.IsNullOrEmpty(currentlyEquipped))
            {
                hero.equippedArmor.Remove(currentlyEquipped);
                AddInventoryItem(currentlyEquipped, 1);
            }

            hero.equippedArmor.Add(armorId);
            RemoveInventoryItem(armorId, 1);
        }

        public static void UnequipArmor(string heroId, string armorType)
        {
            if (Database.Heroes[heroId].isGuest)
            {
                Debug.LogWarning("Cannot unequip armor on guest " + heroId);
                return;
            }

            var hero = GetPartyMember(heroId);

            var currentlyEquipped = GetArmor(hero, armorType);
            if (!string.IsNullOrEmpty(currentlyEquipped))
            {
                hero.equippedArmor.Remove(currentlyEquipped);
                AddInventoryItem(currentlyEquipped, 1);
            }
        }

        public static void EquipMod(string heroId, int slotIndex, string modId)
        {
            if (Database.Heroes[heroId].isGuest)
            {
                Debug.LogWarning("Cannot equip mod on guest " + heroId);
                return;
            }

            var hero = GetPartyMember(heroId);
            var mod = Database.Items[modId] as Mod;

            if(slotIndex >= 0 && slotIndex < hero.modSlots.Count)
            {
                string elementId = hero.modSlots[slotIndex].elementId;
                if (mod.elements.Find(e => e.id == elementId) != null)
                {
                    var currentlyEquipped = GetMod(hero, slotIndex);
                    if (!string.IsNullOrEmpty(currentlyEquipped))
                    {
                        AddInventoryItem(currentlyEquipped, 1);
                    }

                    hero.modSlots[slotIndex] = new ModSlot { elementId = elementId, modId = modId };
                    RemoveInventoryItem(modId, 1);
                }
            }
        }

        public static void UnequipMod(string heroId, int slotIndex)
        {
            if (Database.Heroes[heroId].isGuest)
            {
                Debug.LogWarning("Cannot unequip mod on guest " + heroId);
                return;
            }

            var hero = GetPartyMember(heroId);

            var currentlyEquipped = GetMod(hero, slotIndex);
            if (!string.IsNullOrEmpty(currentlyEquipped))
            {
                hero.modSlots[slotIndex].modId = "";
                AddInventoryItem(currentlyEquipped, 1);
            }
        }

        public static int GetEquipCount(string itemId)
        {
            int count = 0;
            foreach (var hero in loadedGame.party)
            {
                var equipped = GetEquipment(hero.id);
                foreach (var equip in equipped)
                {
                    if (equip == itemId) count++;
                }
            }
            return count;
        }

        public static int Compare(Weapon a, Weapon b)
        {
            int diff = 0;
            foreach(Stat stat in Enum.GetValues(typeof(Stat)))
            {
                int statA = a != null ? a.GetStat(stat) : 0;
                int statB = b != null ? b.GetStat(stat) : 0;

                diff += statB - statA;
            }
            return diff;
        }

        public static int Compare(Armor a, Armor b)
        {
            int diff = 0;
            foreach (Stat stat in Enum.GetValues(typeof(Stat)))
            {
                int statA = a != null ? a.GetStat(stat) : 0;
                int statB = b != null ? b.GetStat(stat) : 0;

                diff += statB - statA;
            }
            return diff;
        }

        public static (int current, int updated) CompareStats(string heroId, Stat stat, Weapon newWeapon)
        {
            var currentStat = GetHeroStat(heroId, stat);
            var newStat = GetHeroStat(heroId, stat, newWeapon, GetArmorData(heroId), GetModData(heroId));

            return (currentStat, newStat);
        }

        public static (int current, int updated) CompareStats(string heroId, Stat stat, List<Armor> newArmor)
        {
            var currentStat = GetHeroStat(heroId, stat);
            var newStat = GetHeroStat(heroId, stat, GetWeaponData(heroId), newArmor, GetModData(heroId));

            return (currentStat, newStat);
        }

        public static (int current, int updated) CompareStats(string heroId, Stat stat, List<Mod> newMod)
        {
            var currentStat = GetHeroStat(heroId, stat);
            var newStat = GetHeroStat(heroId, stat, GetWeaponData(heroId), GetArmorData(heroId), newMod);

            return (currentStat, newStat);
        }

        public static List<Armor> TestEquipArmor(string heroId, Armor newArmor)
        {
            var currentArmor = GetArmorData(heroId);
            int index = currentArmor.FindIndex(a => a.tag == newArmor.tag);
            if (index != -1)
                currentArmor[index] = newArmor;
            else
                currentArmor.Add(newArmor);

            return currentArmor;
        }

        public static List<Armor> TestUnequipArmor(string heroId, string armorType)
        {
            var currentArmor = GetArmorData(heroId);
            int index = currentArmor.FindIndex(a => a.tag.id == armorType);
            if (index != -1)
                currentArmor.RemoveAt(index);

            return currentArmor;
        }

        public static List<Mod> TestEquipMod(string heroId, int slotIndex, Mod mod)
        {
            var mods = new List<Mod>();

            var hero = GetPartyMember(heroId);
            for (int i = 0; i < hero.modSlots.Count; ++i)
            {
                if (i == slotIndex)
                {
                    mods.Add(mod);
                }
                else if (!string.IsNullOrEmpty(hero.modSlots[i].modId))
                {
                    mods.Add(Database.Items[hero.modSlots[i].modId] as Mod);
                }
            }

            return mods;
        }

        public static List<Mod> TestUnequipMod(string heroId, int slotIndex)
        {
            var mods = new List<Mod>();

            var hero = GetPartyMember(heroId);
            for (int i = 0; i < hero.modSlots.Count; ++i)
            {
                if (i != slotIndex && !string.IsNullOrEmpty(hero.modSlots[i].modId))
                {
                    mods.Add(Database.Items[hero.modSlots[i].modId] as Mod);
                }
            }

            return mods;
        }

        public static void CollectChest(Data.Inventory.Chest chest, string instanceId)
        {
            List<Item> collectedItems = new List<Item>();

            var remainingContent = RemainingChestContent(chest, instanceId);
            foreach(var item in remainingContent)
            {
                var itemAsset = Database.Items[item.id];

                int currentAmount = GetTotalItemCount(item.id);
                int maxCollectAmount = itemAsset.inventoryMaxAmount - currentAmount;

                int collectAmount = Mathf.Clamp(item.count, 0, maxCollectAmount);

                AddInventoryItem(item.id, collectAmount);
                collectedItems.Add(new Item(item.id, collectAmount));
            }

            var collectedChest = loadedGame.collectedChests.FirstOrDefault(c => c.id == instanceId);
            if (collectedChest == null)
            {
                loadedGame.collectedChests.Add(new Persistence.Chest(instanceId, collectedItems));
            }
            else
            {
                foreach(var item in collectedItems)
                {
                    var collectedItem = collectedChest.collectedItems.FirstOrDefault(i => i.id == item.id);
                    if (collectedItem != null)
                        collectedItem.count += item.count;
                    else
                        collectedChest.collectedItems.Add(item);
                }
            }
        }

        public static List<Item> RemainingChestContent(Data.Inventory.Chest chest, string instanceId)
        {
            var content = new List<Item>();

            var collectedChest = loadedGame.collectedChests.FirstOrDefault(c => c.id == instanceId);
            if(collectedChest == null)
            {
                foreach(var item in chest.content)
                {
                    content.Add(new Item(item.item.id, item.count));
                }
            }
            else
            {
                foreach(var item in chest.content)
                {
                    var collectedItem = collectedChest.collectedItems.FirstOrDefault(i => i.id == item.item.id);
                    int collectedCount = collectedItem == null ? 0 : collectedItem.count;

                    int remainingCount = item.count - collectedCount;
                    if (remainingCount > 0)
                        content.Add(new Item(item.item.id, remainingCount));
                }
            }

            return content;
        }
    }
}
