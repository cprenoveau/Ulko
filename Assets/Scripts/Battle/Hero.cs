using Ulko.Data;
using Ulko.Data.Characters;
using System.Collections.Generic;
using UnityEngine;
using Ulko.Data.Abilities;

namespace Ulko.Battle
{
    public class Hero : ICharacterInternal
    {
        public string Id => HeroData.id;
        public string IdWithoutSuffix => HeroData.id;
        public string Name => Localization.Localize(HeroAsset.displayName);
        public CharacterSide CharacterSide => CharacterSide.Heroes;
        public int Level => HeroData.GetLevelDataFromExp(SavedData.exp).level;
        public int Exp => SavedData.exp;

        public int HP
        {
            get { return SavedData.hp; }
            set
            {
                SavedData.hp = value;
                PlayerProfile.UpdatePartyMember(SavedData);
            }
        }
        public Level Stats => PlayerProfile.GetHeroStats(Id, Level);
        public AbilityAsset Attack => HeroAsset.attack;

        public ICharacterInternal.InstantiateDelegate Instantiate { get; private set; }
        public Vector2 FacingDirection { get; private set; }

        public CharacterAsset Asset => HeroAsset;
        public HeroAsset HeroAsset { get; private set; }
        public Persistence.Hero SavedData => PlayerProfile.GetPartyMember(Id);
        public Data.Characters.Hero HeroData { get; private set; }

        public List<SpriteAnimation> GetAnimation(string id)
        {
            return HeroAsset.GetAnimation(id);
        }

        public Hero(
            HeroAsset asset,
            Data.Characters.Hero heroData,
            Vector2 direction)
        {
            HeroAsset = asset;
            HeroData = heroData;
            FacingDirection = direction;
            Instantiate = asset.Instantiate;
        }
    }
}
