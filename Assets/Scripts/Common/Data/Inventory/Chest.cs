using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Inventory
{
    [CreateAssetMenu(fileName = "Chest", menuName = "Ulko/Inventory/Chest", order = 1)]
    public class Chest : ScriptableObject
    {
        [Serializable]
        public class ChestItem
        {
            public ItemAsset item;
            public int count;
        }

        public List<ChestItem> content = new List<ChestItem>();
    }
}
