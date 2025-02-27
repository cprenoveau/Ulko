using System;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class Heal : Effect
    {
        public override EffectType Type => EffectType.Heal;

        public TargetConditionAsset condition;
        public EffectConfig config;
        public Stat healStat = Stat.Wisdom;
        [Tooltip("Multiplier applied to stat healing")]
        public float healMultiplier = 1f;
        public float percentHeal;
        public float flatHeal;
        public bool revive;

        public override Effect Clone()
        {
            return new Heal()
            {
                condition = condition,
                config = config,
                healStat = healStat,
                healMultiplier = healMultiplier,
                percentHeal = percentHeal,
                flatHeal = flatHeal,
                revive = revive
            };
        }

        public override bool IsEqual(Effect otherEffect)
        {
            if (otherEffect is Heal other)
            {
                return condition == other.condition
                    && config == other.config
                    && healStat == other.healStat
                    && healMultiplier == other.healMultiplier
                    && percentHeal == other.percentHeal
                    && flatHeal == other.flatHeal
                    && revive == other.revive;
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
            if(target.hp > 0 || revive)
            {
                float heal = RawValue(actor);

                heal += target.stats.MaxHP * percentHeal / 100f;
                heal += flatHeal;

                target.hp += (int)heal;
            }
        }

        public float RawValue(CharacterState actor)
        {
            float stat = actor.stats.GetStat(healStat);
            float heal = stat * healMultiplier;

            return heal;
        }

        public override string Description()
        {
            string str = "";

            if (revive)
                str = string.Format("{0} + ", Localization.LocalizeFormat("revives"));

            if (healMultiplier != 0)
            {
                str += Localization.LocalizeFormat("heal_desc", healMultiplier * 100, TextFormat.Localize(healStat));
            }
            else if (percentHeal != 0)
            {
                str += Localization.LocalizeFormat("heal_percent_desc", percentHeal);
            }
            else if (flatHeal != 0)
            {
                str += Localization.LocalizeFormat("heal_flat_desc", flatHeal);
            }

            return str;
        }
    }
}
