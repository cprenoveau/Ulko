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
            float damage = RawValue(actor.stats);

            damage += target.stats.maxHP * percentDamage / 100f;
            damage += flatDamage;

            damage = Mathf.RoundToInt(damage * GetAttackMultiplier(target));
            damage = Mathf.Clamp(damage, 1, damage);

            target.hp -= (int)damage;

            if (target.hp <= 0)
                target.statuses.Clear();
        }

        public float GetAttackMultiplier(CharacterState target)
        {
            float attackMult = 1f;

            var characterType = target.GetCharacterType();
            foreach (var stat in characterType)
            {
                attackMult *= config.GetAttackMultiplier(attackStat, stat);
            }

            return attackMult;
        }

        public int RawValue(Level actorStats)
        {
            float atk = actorStats.GetStat(attackStat);
            float damage = atk * damageMultiplier;

            return (int)damage;
        }

        public override string Description(Level actorStats)
        {
            int value = RawValue(actorStats);
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
