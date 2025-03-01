using Ulko.Data.Abilities;

namespace Ulko.Data
{
    public class AbilityCard
    {
        public AbilityAsset abilityAsset;
        public string ownerId;

        public AbilityCard(AbilityAsset abilityAsset, string ownerId)
        {
            this.abilityAsset = abilityAsset;
            this.ownerId = ownerId;
        }
    }
}