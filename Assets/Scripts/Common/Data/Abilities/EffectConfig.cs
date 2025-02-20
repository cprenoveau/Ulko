using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [CreateAssetMenu(fileName = "EffectConfig", menuName = "Ulko/Abilities/Effect Config", order = 1)]
    public class EffectConfig : ScriptableObject
    {
        public float criticalMultiplier = 1.5f;
        public float flatModifier = 100;
        public float randomMinMultiplier = 0.95f;
        public float randomMaxMultiplier = 1.05f;
        [Tooltip("A character with this status is dealt damage when healed. Optional")]
        public Status zombieStatus;

        [Serializable]
        public class ElementalMultiplier
        {
            public ElementalTag tag;
            public ElementalTag against;
            public float multiplier;
        }

        public List<ElementalMultiplier> attackMultipliers = new List<ElementalMultiplier>();

        public float GetAttackMultiplier(ElementalTag tag, ElementalTag against)
        {
            if (tag == null || against == null)
                return 1f;

            var mult = attackMultipliers.Find(e => e.tag == tag && e.against == against);
            if (mult != null)
                return mult.multiplier;
            else
                return 1f;
        }
    }
}
