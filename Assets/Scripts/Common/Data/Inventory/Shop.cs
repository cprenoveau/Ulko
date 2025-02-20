using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Inventory
{
    [CreateAssetMenu(fileName = "Shop", menuName = "Ulko/Inventory/Shop", order = 1)]
    public class Shop : ScriptableObject
    {
        public string id;
        public List<ItemAsset> items = new List<ItemAsset>();
    }
}
