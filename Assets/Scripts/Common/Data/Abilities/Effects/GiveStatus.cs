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
        public int turns = 3;

        public override void Clone(object source)
        {
            Clone(source as GiveStatus);
        }

        private void Clone(GiveStatus source)
        {
            condition = source.condition;
            percentChance = source.percentChance;
            status = source.status;
            turns = source.turns;
        }

        public bool Equals(GiveStatus other)
        {
            return condition == other.condition
                && percentChance == other.percentChance
                && status == other.status
                && turns == other.turns;
        }

        public override void Apply(CharacterAction action, ActionState state)
        {
            var actor = state.FindCharacter(action.actorId);
            if (actor == null)
                return;

            if (UnityEngine.Random.Range(0, 100) < percentChance)
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
            int index = actor.statuses.FindIndex(s => s.statusAsset.id == status.id);
            if(index == -1)
            {
                actor.statuses.Add(new StatusState(status, turns, 0));
            }
            else
            {
                actor.statuses[index].maxTurns += turns;
            }
        }

        public override string Description()
        {
            string str = "";
            return str;
        }
    }
}
