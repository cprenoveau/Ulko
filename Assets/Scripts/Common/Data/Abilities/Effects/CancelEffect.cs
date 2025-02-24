using System;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class CancelEffect : Effect, IEquatable<CancelEffect>
    {
        public override EffectType Type => EffectType.CancelEffect;

        public TargetConditionAsset condition;
        public float percentChance = 50;
        public EffectType effectType;

        public override void Clone(object source)
        {
            Clone(source as CancelEffect);
        }

        private void Clone(CancelEffect source)
        {
            condition = source.condition;
            percentChance = source.percentChance;
            effectType = source.effectType;
        }

        public bool Equals(CancelEffect other)
        {
            return condition == other.condition
                && percentChance == other.percentChance
                && effectType == other.effectType;
        }

        public override void Apply(CharacterAction action, ActionState state)
        {
            var actor = state.FindCharacter(action.actorId);
            if (actor == null)
                return;

            if (UnityEngine.Random.Range(0, 100) > percentChance)
            {
                var pendingActionActor = state.FindCharacter(state.pendingAction.actorId);
                if (pendingActionActor != null && (condition == null || condition.IsTrue(actor, pendingActionActor)))
                {
                    state.pendingAction.effects.RemoveAll(e => e.Type == effectType);
                }
            }
        }

        public override string Description()
        {
            string str = "";
            return str;
        }
    }
}
