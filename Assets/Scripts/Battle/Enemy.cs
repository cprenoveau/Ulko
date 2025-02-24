using Ulko.Data;
using Ulko.Data.Characters;
using System.Collections.Generic;
using UnityEngine;
using Ulko.Data.Abilities;

namespace Ulko.Battle
{
    public class Enemy : ICharacterType
    {
        public string Id => EnemyData.id + Suffix;
        public string IdWithoutSuffix => EnemyData.id;
        public string Name => Localization.Localize(EnemyData.name) + (!string.IsNullOrEmpty(Suffix) ? " " + Suffix : "");
        public CharacterSide CharacterSide => CharacterSide.Enemies;
        public int Level => Asset.level;
        public int Exp => EnemyData.GetLevelData(Level).exp;

        public ICharacterType.InstantiateDelegate Instantiate { get; private set; }
        public Vector2 FacingDirection { get; private set; }

        public string Suffix { get; private set; }
        public int HP { get; set; }
        public Level Stats => EnemyData.GetLevelData(Level);

        public EnemyAsset Asset { get; private set; }
        public Data.Characters.Enemy EnemyData { get; private set; }

        public AbilityAsset Attack => Asset.attack;

        public List<SpriteAnimation> GetAnimation(string id)
        {
            return Asset.GetAnimation(id);
        }

        public Enemy(
            EnemyAsset asset,
            Data.Characters.Enemy enemyData,
            Vector2 direction,
            string suffix)
        {
            Asset = asset;
            EnemyData = enemyData;
            Suffix = suffix;
            FacingDirection = direction;
            Instantiate = asset.Instantiate;

            HP = (int)Stats.GetStat(Stat.Fortitude);
        }
    }
}
