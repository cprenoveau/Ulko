using System;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class CancelEffect : Effect
    {
        public override EffectType Type => EffectType.CancelEffect;

        public TargetConditionAsset condition;
        public EffectType effectType;

        public override Effect Clone()
        {
            return new CancelEffect()
            {
                condition = condition,
                effectType = effectType
            };
        }
        public override bool IsEqual(Effect otherEffect)
        {
            if (otherEffect is CancelEffect other)
            {
                return condition == other.condition
                    && effectType == other.effectType;
            }

            return false;
        }

        public override void Apply(CharacterAction action, ActionState state)
        {
            var actor = state.FindCharacter(action.ActorId);
            if (actor == null)
                return;

            var pendingActionActor = state.FindCharacter(state.pendingAction.ActorId);
            if (pendingActionActor != null && (condition == null || condition.IsTrue(actor, pendingActionActor)))
            {
                state.pendingAction.RemoveEffectOfType(effectType);
            }
        }

        public override string Description(CharacterState actor)
        {
            string str = "";
            return str;
        }
    }
}
