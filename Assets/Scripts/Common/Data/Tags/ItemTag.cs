using UnityEngine;

namespace Ulko.Data
{
    [CreateAssetMenu(fileName = "ItemTag", menuName = "Ulko/Item Tag", order = 1)]
    public class ItemTag : ScriptableObject
    {
        public string id;
        public int inventoryOrder;
    }
}
