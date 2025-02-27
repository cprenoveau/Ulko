using System.Collections.Generic;
using Ulko.Data.Abilities;
using UnityEngine;

namespace Ulko.Data.Characters
{
    public abstract class CharacterAsset : ScriptableObject
    {
        public string id;
        public List<AbilityAsset> abilities = new();
        public List<StatusAsset> status = new();
    }
}