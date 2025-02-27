using System;
using Unity.IO.Archive;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class GiveStatus : Effect
    {
        public override EffectType Type => EffectType.GiveStatus;

        public TargetConditionAsset condition;
        public StatusAsset status;
        public int turns = 3;

        public override Effect Clone()
        {
            return new GiveStatus()
            {
                condition = condition,
                status = status,
                turns = turns
            };
        }

        public override bool IsEqual(Effect otherEffect)
        {
            if (otherEffect is GiveStatus other)
            {
                return condition == other.condition
                    && status == other.status
                    && turns == other.turns;
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
            string str;

            if(turns < 100)
                str = Localization.LocalizeFormat("main", "give_status_turn_desc", Localization.Localize(status.id), turns);
            else
                str = Localization.LocalizeFormat("main", "give_status_infinite_desc", Localization.Localize(status.id));

            return str;
        }
    }
}
