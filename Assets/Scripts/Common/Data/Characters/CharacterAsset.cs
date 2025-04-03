using System.Collections.Generic;
using Ulko.Data.Abilities;
using UnityEngine;

namespace Ulko.Data.Characters
{
    public abstract class CharacterAsset : ScriptableObject
    {
        public string id;
        public int turnCooldown = 3;

        public abstract IEnumerable<AbilityAsset> Abilities { get; }
        public abstract IEnumerable<StatusAsset> Statuses { get; }
    }
}