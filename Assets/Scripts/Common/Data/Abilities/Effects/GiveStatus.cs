using System;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class GiveStatus : Effect, IEquatable<GiveStatus>
    {
        public override EffectType Type => EffectType.GiveStatus;

        public TargetConditionAsset condition;
        public float percentChance = 50;
        public StatusAsset status;

        public override void Clone(object source)
        {
            Clone(source as GiveStatus);
        }

        private void Clone(GiveStatus source)
        {
            condition = source.condition;
            percentChance = source.percentChance;
            status = source.status;
        }

        public bool Equals(GiveStatus other)
        {
            return condition == other.condition
                && percentChance == other.percentChance
                && status == other.status;
        }

        public override void Apply(CharacterAction action, ActionState state)
        {
            var actor = state.FindCharacter(action.actorId);
            if (actor == null)
                return;

            if (UnityEngine.Random.Range(0, 100) > percentChance)
            {
                foreach (string targetId in action.targetIds)
                {
                    var target = state.FindCharacter(targetId);
                    if (target != null && (condition == null || condition.IsTrue(actor, target)))
                    {
                        Apply(target);
                    }
                }
            }
        }

        public void Apply(CharacterState actor)
        {
            if(!actor.statusIds.Contains(status.id))
            {
                actor.statusIds.Add(status.id);
            }
        }

        public override string Description()
        {
            string str = "";
            return str;
        }
    }
}
