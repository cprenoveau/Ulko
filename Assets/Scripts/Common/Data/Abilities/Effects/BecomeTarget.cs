using System;
using System.Collections.Generic;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class BecomeTarget : Effect
    {
        public override EffectType Type => EffectType.BecomeTarget;

        public TargetConditionAsset condition;
        public float percentChance = 50;

        public override Effect Clone()
        {
            return new BecomeTarget()
            {
                condition = condition,
                percentChance = percentChance
            };
        }

        public override bool IsEqual(Effect otherEffect)
        {
            if(otherEffect is BecomeTarget other)
            {
                return condition == other.condition
                    && percentChance == other.percentChance;
            }

            return false;
        }

        public override void Apply(CharacterAction action, ActionState state)
        {
            var actor = state.FindCharacter(action.actorId);
            if (actor == null)
                return;

            if (UnityEngine.Random.Range(0, 100) < percentChance)
            {
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
        }

        public override string Description()
        {
            string str = "";
            return str;
        }
    }
}
