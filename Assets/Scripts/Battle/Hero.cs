using Ulko.Data;
using Ulko.Data.Characters;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Battle
{
    public class Hero : ICharacterType
    {
        public string Id => HeroData.id;
        public string IdWithoutSuffix => HeroData.id;
        public string Name => Localization.Localize(Asset.displayName);
        public CharacterSide CharacterSide => CharacterSide.Heroes;
        public int Level => HeroData.GetLevelDataFromExp(SavedData.exp).level;
        public int Exp => SavedData.exp;

        public ICharacterType.InstantiateDelegate Instantiate { get; private set; }
        public Vector2 FacingDirection { get; private set; }

        public int HP
        {
            get { return SavedData.hp; }
            set
            {
                SavedData.hp = value;
                PlayerProfile.UpdatePartyMember(SavedData);
            }
        }

        public float GetStat(Stat stat)
        {
            return PlayerProfile.GetHeroStat(Id, stat);
        }

        public int TurnCount { get; set; }

        public HeroAsset Asset { get; private set; }
        public Persistence.Hero SavedData => PlayerProfile.GetPartyMember(Id);
        public Data.Characters.Hero HeroData { get; private set; }

        public List<SpriteAnimation> GetAnimation(string id)
        {
            return Asset.GetAnimation(id);
        }

        public Hero(
            HeroAsset asset,
            Data.Characters.Hero heroData,
            Vector2 direction)
        {
            Asset = asset;
            HeroData = heroData;
            FacingDirection = direction;
            Instantiate = asset.Instantiate;
        }
    }
}
