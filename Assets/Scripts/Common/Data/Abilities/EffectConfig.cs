using UnityEngine;

namespace Ulko.Data.Abilities
{
    [CreateAssetMenu(fileName = "EffectConfig", menuName = "Ulko/Abilities/Effect Config", order = 1)]
    public class EffectConfig : ScriptableObject
    {
        public float flatModifier = 100;
    }
}
