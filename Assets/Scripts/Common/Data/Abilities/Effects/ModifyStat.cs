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

        public override Effect Clone()
        {
            return new ModifyStat()
            {
                condition = condition,
                stat = stat,
                multiply = multiply,
                add = add
            };
        }

        public override bool IsEqual(Effect otherEffect)
        {
            if (otherEffect is ModifyStat other)
            {
                return condition == other.condition
                    && stat == other.stat
                    && multiply == other.multiply
                    && add == other.add;
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

        public void Apply(CharacterState actor)
        {
            float statValue = actor.stats.GetStat(stat);
            statValue *= multiply;
            statValue += add;

            actor.stats.SetStat(stat, statValue);
        }

        public override string Description(Level actorStats)
        {
            string str = "";
            return str;
        }
    }
}
