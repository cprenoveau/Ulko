using System;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class Damage : Effect, IEquatable<Damage>
    {
        public override EffectType Type => EffectType.Damage;

        public TargetConditionAsset condition;
        public EffectConfig config;
        public Stat attackStat = Stat.Wisdom;
        public Stat defenseStat = Stat.Intuition;
        [Tooltip("Multiplier applied to stat damage")]
        public float damageMultiplier = 1f;
        public float percentDamage;
        public float flatDamage;

        public override void Clone(object source)
        {
            Clone(source as Damage);
        }

        private void Clone(Damage source)
        {
            condition = source.condition;
            config = source.config;
            attackStat = source.attackStat;
            defenseStat = source.defenseStat;
            damageMultiplier = source.damageMultiplier;
            percentDamage = source.percentDamage;
            flatDamage = source.flatDamage;
        }

        public bool Equals(Damage other)
        {
            return condition == other.condition
                && config == other.config
                && attackStat == other.attackStat
                && defenseStat == other.defenseStat
                && damageMultiplier == other.damageMultiplier
                && percentDamage == other.percentDamage
                && flatDamage == other.flatDamage;
        }

        public override void Apply(CharacterAction action, ActionState state)
        {
            var actor = state.FindCharacter(action.actorId);
            if (actor == null)
                return;

            foreach (string targetId in action.targetIds)
            {
                var target = state.FindCharacter(targetId);
                if (target != null && (condition == null || condition.IsTrue(actor, target)))
                {
                    Apply(actor, target);
                }
            }
        }

        public void Apply(CharacterState actor, CharacterState target)
        {
            float atk = actor.stats.GetStat(attackStat);
            float def = target.stats.GetStat(defenseStat);

            float damage = atk * damageMultiplier;

            if (def != 0)
                damage = damage * config.flatModifier / (config.flatModifier + def);

            damage += target.stats.GetStat(Stat.Fortitude) * percentDamage / 100f;
            damage += flatDamage;

            damage = Mathf.Clamp(damage, 1, target.hp);

            target.hp -= (int)damage;
        }

        public override string Description()
        {
            string str = "";

            if (damageMultiplier != 0)
            {
                str = Localization.LocalizeFormat("damage_desc", damageMultiplier * 100, TextFormat.Localize(attackStat), TextFormat.Localize(defenseStat));
            }
            else if (percentDamage != 0)
            {
                str = Localization.LocalizeFormat("damage_percent_desc", percentDamage);
            }
            else if (flatDamage != 0)
            {
                str = Localization.LocalizeFormat("damage_flat_desc", flatDamage);
            }

            return str;
        }
    }
}
