using System.Collections.Generic;
using Ulko.Data.Abilities;
using UnityEngine;

namespace Ulko.Data.Characters
{
    public abstract class CharacterAsset : ScriptableObject
    {
        public string id;
        public int turnCooldown = 3;
        public List<AbilityAsset> abilities = new();
        public List<StatusAsset> status = new();
    }
}