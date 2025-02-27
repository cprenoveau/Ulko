using System;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class Damage : Effect
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

        public override Effect Clone()
        {
            return new Damage()
            {
                condition = condition,
                config = config,
                attackStat = attackStat,
                defenseStat = defenseStat,
                damageMultiplier = damageMultiplier,
                percentDamage = percentDamage,
                flatDamage = flatDamage
            };
        }

        public override bool IsEqual(Effect otherEffect)
        {
            if (otherEffect is Damage other)
            {
                return condition == other.condition
                    && config == other.config
                    && attackStat == other.attackStat
                    && defenseStat == other.defenseStat
                    && damageMultiplier == other.damageMultiplier
                    && percentDamage == other.percentDamage
                    && flatDamage == other.flatDamage;
            }

            return false;
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
            float damage = RawValue(actor);

            float def = target.stats.GetStat(defenseStat);
            if (def != 0)
                damage = damage * config.flatModifier / (config.flatModifier + def);

            damage += target.stats.MaxHP * percentDamage / 100f;
            damage += flatDamage;

            damage = Mathf.Clamp(damage, 1, damage);

            target.hp -= (int)damage;
        }

        public float RawValue(CharacterState actor)
        {
            float atk = actor.stats.GetStat(attackStat);
            float damage = atk * damageMultiplier;

            return damage;
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

        public string Description(CharacterState actor)
        {
            int value = (int)RawValue(actor);
            string str = "";

            if (damageMultiplier != 0)
            {
                str = Localization.LocalizeFormat("damage_flat_desc", value);
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
