using System;
using System.Collections.Generic;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class BecomeTarget : Effect
    {
        public override EffectType Type => EffectType.BecomeTarget;

        public TargetConditionAsset condition;

        public override Effect Clone()
        {
            return new BecomeTarget()
            {
                condition = condition
            };
        }

        public override bool IsEqual(Effect otherEffect)
        {
            if(otherEffect is BecomeTarget other)
            {
                return condition == other.condition;
            }

            return false;
        }

        public override void Apply(CharacterAction action, ActionState state)
        {
            var actor = state.FindCharacter(action.ActorId);
            if (actor == null)
                return;

            foreach (var targetId in state.pendingAction.TargetIds)
            {
                var target = state.FindCharacter(targetId);
                if (target != null && (condition == null || condition.IsTrue(actor, target)))
                {
                    state.pendingAction.ReplaceTarget(targetId, action.ActorId);
                }
            }
        }

        public override string Description(CharacterState actor)
        {
            string str = "";
            return str;
        }
    }
}
