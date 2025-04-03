using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ulko.Data.Characters
{
    [CreateAssetMenu(fileName = "HeroProgression", menuName = "Ulko/Characters/Hero Progression", order = 1)]
    public class HeroProgression : ScriptableObject
    {
        [Serializable]
        public class AbilityProgression
        {
            public int minLevel;
            public Abilities.AbilityAsset ability;
        }

        public List<AbilityProgression> abilities = new();

        public IEnumerable<AbilityProgression> GetAbilities(int level)
        {
            return abilities.Where(a => level >= a.minLevel);
        }

        public IEnumerable<AbilityProgression> GetNextAbilities(int currentLevel)
        {
            int nextLevel = int.MaxValue;
            foreach (var ability in abilities)
            {
                if (ability.minLevel > currentLevel && ability.minLevel < nextLevel)
                    nextLevel = ability.minLevel;
            }

            return abilities.Where(a => a.minLevel == nextLevel);
        }
    }
}
