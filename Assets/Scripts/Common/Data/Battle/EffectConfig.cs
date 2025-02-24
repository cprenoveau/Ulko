using UnityEngine;

namespace Ulko.Data.Battle
{
    [CreateAssetMenu(fileName = "EffectConfig", menuName = "Ulko/Battle/Effect Config", order = 1)]
    public class EffectConfig : ScriptableObject
    {
        public float flatModifier = 100;
    }
}
