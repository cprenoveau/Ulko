using System.Collections.Generic;
using System;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [CreateAssetMenu(fileName = "EffectConfig", menuName = "Ulko/Abilities/Effect Config", order = 1)]
    public class EffectConfig : ScriptableObject
    {
        [Serializable]
        public class StatMultiplier
        {
            public Stat stat;
            public Stat against;
            public float multiplier;
        }

        public List<StatMultiplier> attackMultipliers = new();

        public float GetAttackMultiplier(Stat stat, Stat against)
        {
            var mult = attackMultipliers.Find(e => e.stat == stat && e.against == against);
            if (mult != null)
                return mult.multiplier;
            else
                return 1f;
        }
    }
}
