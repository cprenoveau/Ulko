using System;
using UnityEngine;

namespace Ulko.Data.Inventory
{
    [Serializable]
    public class ItemStat
    {
        public Stat stat;
        public float value;
    }

    public enum ItemType
    {
        Consumable,
        Weapon,
        Armor,
        Mod,
        Die
    }

    public abstract class ItemAsset : ScriptableObject
    {
        public string id;
        public int value;
        public int buyMaxAmount = 20;
        public int inventoryMaxAmount = 99;
        public ItemTag tag;
        public Sprite icon;

        public abstract ItemType Type { get; }
        public abstract string Description();
    }
}
