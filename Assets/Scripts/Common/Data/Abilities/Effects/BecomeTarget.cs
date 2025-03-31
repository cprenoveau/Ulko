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
            var actor = state.FindCharacter(action.actorId);
            if (actor == null)
                return;

            List<string> newTargets = new();

            foreach (var targetId in state.pendingAction.targetIds)
            {
                var target = state.FindCharacter(targetId);
                if (target != null && (condition == null || condition.IsTrue(actor, target)))
                {
                    newTargets.Add(action.actorId);
                }
                else
                {
                    newTargets.Add(targetId);
                }
            }

            state.pendingAction.targetIds = newTargets;
        }

        public override string Description(CharacterState actor)
        {
            string str = "";
            return str;
        }
    }
}
