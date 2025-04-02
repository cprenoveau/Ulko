using Newtonsoft.Json.Linq;
using Ulko.Data.Abilities;

namespace Ulko.Data
{
    public class AbilityCardData : IJsonObject, IClonable
    {
        public AbilityAsset abilityAsset;
        public string ownerId;

        public AbilityCardData(AbilityAsset abilityAsset, string ownerId)
        {
            this.abilityAsset = abilityAsset;
            this.ownerId = ownerId;
        }

        public void Clone(object source)
        {
            Clone(source as AbilityCardData);
        }

        public void Clone(AbilityCardData source)
        {
            abilityAsset = source.abilityAsset;
            ownerId = source.ownerId;
        }

        public void FromJson(JToken token)
        {
            abilityAsset = Database.Abilities[token["abilityId"].ToString()];
            ownerId = token["ownerId"].ToString();
        }

        public JToken ToJson()
        {
            return new JObject
            {
                { "abilityId", abilityAsset.id },
                { "ownerId", ownerId }
            };
        }
    }
}