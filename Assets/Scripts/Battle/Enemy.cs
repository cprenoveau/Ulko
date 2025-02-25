using Ulko.Data;
using Ulko.Data.Characters;
using System.Collections.Generic;
using UnityEngine;
using Ulko.Data.Abilities;

namespace Ulko.Battle
{
    public class Enemy : ICharacterInternal
    {
        public string Id => EnemyData.id + Suffix;
        public string IdWithoutSuffix => EnemyData.id;
        public string Suffix { get; private set; }
        public string Name => Localization.Localize(EnemyData.name) + (!string.IsNullOrEmpty(Suffix) ? " " + Suffix : "");
        public CharacterSide CharacterSide => CharacterSide.Enemies;
        public int Level => EnemyAsset.level;
        public int Exp => EnemyData.GetLevelData(Level).exp;
        public int HP { get; set; }
        public Level Stats => EnemyData.GetLevelData(Level);
        public AbilityAsset Attack => EnemyAsset.attack;

        public ICharacterInternal.InstantiateDelegate Instantiate { get; private set; }
        public Vector2 FacingDirection { get; private set; }

        public CharacterAsset Asset => EnemyAsset;
        public EnemyAsset EnemyAsset { get; private set; }
        public Data.Characters.Enemy EnemyData { get; private set; }

        public List<SpriteAnimation> GetAnimation(string id)
        {
            return EnemyAsset.GetAnimation(id);
        }

        public Enemy(
            EnemyAsset asset,
            Data.Characters.Enemy enemyData,
            Vector2 direction,
            string suffix)
        {
            EnemyAsset = asset;
            EnemyData = enemyData;
            Suffix = suffix;
            FacingDirection = direction;
            Instantiate = asset.Instantiate;

            HP = Stats.MaxHP;
        }
    }
}
