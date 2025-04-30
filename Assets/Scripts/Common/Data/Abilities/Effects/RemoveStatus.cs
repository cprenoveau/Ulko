using System;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class RemoveStatus : Effect
    {
        public override EffectType Type => EffectType.RemoveStatus;

        public TargetConditionAsset condition;
        public StatusAsset status;

        public override Effect Clone()
        {
            return new RemoveStatus()
            {
                condition = condition,
                status = status
            };
        }

        public override bool IsEqual(Effect otherEffect)
        {
            if (otherEffect is RemoveStatus other)
            {
                return condition == other.condition
                    && status == other.status;
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
            target.RemoveStatus(status.id);
        }

        public override string Description(CharacterState actor)
        {
            return Localization.LocalizeFormat("main", "remove_status_turn_desc", Localization.Localize(status.id));
        }
    }
}
