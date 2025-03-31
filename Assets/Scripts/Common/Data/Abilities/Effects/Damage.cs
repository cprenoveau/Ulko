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
            float damage = RawValue(actor.CurrentStats);
            float shield = target.CurrentStats.GetStat(Stat.Shield);

            damage += target.CurrentStats.maxHP * percentDamage / 100f;
            damage += flatDamage;

            damage = Mathf.RoundToInt(damage * GetAttackMultiplier(target));
            damage = Mathf.Clamp(damage, 1, damage);

            float soak = Mathf.Min(shield, damage);
            damage -= soak;

            target.hp -= (int)damage;
            target.buff.AddToStat(Stat.Shield, -soak);

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

            if (attackStat != Stat.Strength)
                atk += actorStats.GetStat(Stat.Strength);

            float damage = atk * damageMultiplier;

            return (int)damage;
        }

        public override string Description(CharacterState actor)
        {
            string str = "";

            if (damageMultiplier != 0)
            {
                int value = RawValue(actor.CurrentStats);
                int originalValue = RawValue(actor.OriginalStats);

                string valueStr = value < originalValue ? "<color=#FF0000>" + value + "</color>" : value.ToString();
                str = Localization.LocalizeFormat("main", "damage_flat_desc", valueStr);
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
