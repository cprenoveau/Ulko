using System;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class GiveStatus : Effect
    {
        public override EffectType Type => EffectType.GiveStatus;

        public TargetConditionAsset condition;
        public StatusAsset status;
        public int baseTurns = 1;
        public Stat bonusStat = Stat.Intelligence;
        public float bonusTurnsPerTenPoints = 1; 

        public override Effect Clone()
        {
            return new GiveStatus()
            {
                condition = condition,
                status = status,
                baseTurns = baseTurns,
                bonusStat = bonusStat,
                bonusTurnsPerTenPoints = bonusTurnsPerTenPoints
            };
        }

        public override bool IsEqual(Effect otherEffect)
        {
            if (otherEffect is GiveStatus other)
            {
                return condition == other.condition
                    && status == other.status
                    && baseTurns == other.baseTurns
                    && bonusStat == other.bonusStat
                    && bonusTurnsPerTenPoints == other.bonusTurnsPerTenPoints;
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
                    Apply(actor, target);
                }
            }
        }

        public int NumberOfTurns(Level actorStats)
        {
            int bonus = (int)(actorStats.GetStat(bonusStat) / 10f * bonusTurnsPerTenPoints);
            return baseTurns + bonus;
        }

        public void Apply(CharacterState actor, CharacterState target)
        {
            int turns = NumberOfTurns(actor.CurrentStats);

            int index = target.statuses.FindIndex(s => s.statusAsset.id == status.id);
            if(index == -1)
            {
                target.statuses.Add(new StatusState(status, turns, 0));
            }
            else
            {
                target.statuses[index].maxTurns += turns;
            }
        }

        public override string Description(CharacterState actor)
        {
            string str;

            int turns = NumberOfTurns(actor.CurrentStats);

            if (turns < 100)
                str = Localization.LocalizeFormat("main", "give_status_turn_desc", Localization.Localize(status.id), turns);
            else
                str = Localization.LocalizeFormat("main", "give_status_infinite_desc", Localization.Localize(status.id));

            return str;
        }
    }
}
