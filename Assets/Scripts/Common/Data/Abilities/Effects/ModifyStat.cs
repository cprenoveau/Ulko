using System;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class ModifyStat : Effect
    {
        public override EffectType Type => EffectType.ModifyStat;

        public TargetConditionAsset condition;
        public Stat stat;
        public float multiply = 1f;
        public int add;
        public bool isPermanent;

        public override Effect Clone()
        {
            return new ModifyStat()
            {
                condition = condition,
                stat = stat,
                multiply = multiply,
                add = add,
                isPermanent = isPermanent
            };
        }

        public override bool IsEqual(Effect otherEffect)
        {
            if (otherEffect is ModifyStat other)
            {
                return condition == other.condition
                    && stat == other.stat
                    && multiply == other.multiply
                    && add == other.add
                    && isPermanent == other.isPermanent;
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
                    Apply(target);
                }
            }
        }

        public void Apply(CharacterState target)
        {
            float originalValue;
            float statValue = originalValue = target.BaseStats.GetStat(stat);
            statValue *= multiply;
            statValue += add;

            if (isPermanent)
            {
                target.Buff.AddToStat(stat, statValue - originalValue);
            }
            else
            {
                target.BaseStats.SetStat(stat, statValue);
            }
        }

        public bool IsNegative()
        {
            return add < 0 || multiply < 0;
        }

        public override string Description(CharacterState actor)
        {
            string str = "";

            if (multiply > 1f)
            {
                float diff = (multiply - 1f) * 100f;
                str += string.Format("+{0:F0}%", diff);
                if (add != 0) str += " ";
            }
            else if (multiply < 1f)
            {
                float diff = (1f - multiply) * 100f;
                str += string.Format("-{0:F0}%", diff);
                if (add != 0) str += " ";
            }

            if (add > 0)
            {
                str += string.Format("+{0}", add);
            }
            else if (add < 0)
            {
                str += string.Format("{0}", add);
            }

            if (IsNegative())
            {
                return Localization.LocalizeFormat("main", "modify_stat_neg_desc", str, TextFormat.Localize(stat));
            }
            else
            {
                return Localization.LocalizeFormat("main", "modify_stat_desc", str, TextFormat.Localize(stat));
            }
        }
    }
}
