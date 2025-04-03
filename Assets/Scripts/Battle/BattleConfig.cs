using Ulko.Data.Abilities;
using UnityEngine;

namespace Ulko.Battle
{
    [CreateAssetMenu(fileName = "BattleConfig", menuName = "Ulko/Battle/Battle Config", order = 1)]
    public class BattleConfig : ScriptableObject
    {
        public int maxHeroCount = 4;
        public int minCardsInDeck = 5;
        public int maxCardsInDeck = 15;
        public AbilityAsset cardThrowAbility;
    }
}
