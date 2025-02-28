using Ulko.Data.Abilities;
using UnityEngine;

namespace Ulko.Battle
{
    [CreateAssetMenu(fileName = "BattleConfig", menuName = "Ulko/Battle/Battle Config", order = 1)]
    public class BattleConfig : ScriptableObject
    {
        public int maxHeroCount = 3;
        public AbilityAsset cardThrowAbility;
    }
}
