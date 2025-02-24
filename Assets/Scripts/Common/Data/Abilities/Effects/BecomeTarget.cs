using System;
using System.Collections.Generic;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class BecomeTarget : Effect, IEquatable<BecomeTarget>
    {
        public override EffectType Type => EffectType.BecomeTarget;

        public TargetConditionAsset condition;
        public float percentChance = 50;

        public override void Clone(object source)
        {
            Clone(source as BecomeTarget);
        }

        private void Clone(BecomeTarget source)
        {
            condition = source.condition;
            percentChance = source.percentChance;
        }

        public bool Equals(BecomeTarget other)
        {
            return condition == other.condition
                && percentChance == other.percentChance;
        }

        public override void Apply(CharacterAction action, ActionState state)
        {
            var actor = state.FindCharacter(action.actorId);
            if (actor == null)
                return;

            if (UnityEngine.Random.Range(0, 100) > percentChance)
            {
                List<string> newTargets = new();

                foreach (var targetId in state.pendingAction.targetIds)
                {
                    var target = state.FindCharacter(targetId);
                    if (target != null && condition != null && condition.IsTrue(actor, target))
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
