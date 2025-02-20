using Ulko.Data.Abilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Inventory
{
    [CreateAssetMenu(fileName = "Consumable", menuName = "Ulko/Inventory/Consumable", order = 1)]
    public class Consumable : ItemAsset, IActionAsset
    {
        public AbilityTarget target;
        public AbilityNode abilityNode;

        public override ItemType Type => ItemType.Consumable;

        public string Id => id;
        public AbilityTarget AbilityTarget => target;
        public IEnumerable<AbilityNode> AbilityNodes => new List<AbilityNode>() { abilityNode };

        public override string Description()
        {
            return abilityNode.Description();
        }
    }
}
