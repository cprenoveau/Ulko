using Ulko.Data;
using Ulko.Data.Characters;
using System.Collections.Generic;
using UnityEngine;
using Ulko.Data.Abilities;

namespace Ulko.Battle
{
    public class Enemy : ICharacterData
    {
        public string Id => EnemyData.id + Suffix;
        public string IdWithoutSuffix => EnemyData.id;
        public string Name => Localization.Localize(EnemyData.name) + (!string.IsNullOrEmpty(Suffix) ? " " + Suffix : "");
        public CharacterSide CharacterSide => CharacterSide.Enemies;
        public int Level => Asset.level;
        public int Exp => EnemyData.GetLevelData(CurrentLevel).exp;

        public ICharacterData.InstantiateDelegate Instantiate { get; private set; }
        public Vector2 FacingDirection { get; private set; }

        public string Suffix { get; private set; }
        public int HP { get; set; }
        public int MP { get; set; }

        public float GetStat(Stat stat)
        {
            return EnemyData.GetLevelData(CurrentLevel).GetStat(stat);
        }

        public int TurnCount { get; set; }

        public EnemyAsset Asset { get; private set; }
        public Data.Characters.Enemy EnemyData { get; private set; }
        public int CurrentLevel { get; private set; }

        public AbilityAsset Attack => Asset.attack;

        public List<SpriteAnimation> GetAnimation(string id)
        {
            return Asset.GetAnimation(id);
        }

        public Enemy(
            EnemyAsset asset,
            Data.Characters.Enemy enemyData, 
            int level,
            Vector2 direction,
            string suffix)
        {
            Asset = asset;
            EnemyData = enemyData;
            CurrentLevel = level;
            Suffix = suffix;
            FacingDirection = direction;
            Instantiate = asset.Instantiate;

            HP = (int)GetStat(Stat.Fortitude);
        }
    }
}
