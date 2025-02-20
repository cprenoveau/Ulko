using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [CreateAssetMenu(fileName = "TargetCondition", menuName = "Ulko/Abilities/Target Condition", order = 1)]
    public class TargetConditionAsset : ScriptableObject
    {
        public CompositeTargetCondition condition;

        public bool IsTrue(ICharacterData actor, ICharacterData target, AbilityTarget abilityTarget)
        {
            return condition.IsTrue(actor, target, abilityTarget);
        }

        public bool HasCondition(Type conditionType)
        {
            return condition.HasCondition(conditionType);
        }
    }

    [Serializable]
    public abstract class TargetCondition
    {
        public bool invert;

        public bool IsTrue(ICharacterData actor, ICharacterData target, AbilityTarget abilityTarget)
        {
            bool isTrue = _IsTrue(actor, target, abilityTarget);
            if (invert) return !isTrue;
            return isTrue;
        }

        protected abstract bool _IsTrue(ICharacterData actor, ICharacterData target, AbilityTarget abilityTarget);
    }

    [Serializable]
    public class CompositeTargetCondition : TargetCondition
    {
        public enum Union
        {
            And,
            Or
        }

        public Union union;

        [SerializeReference]
        public List<TargetCondition> conditions = new List<TargetCondition>();

        public bool HasCondition(Type conditionType)
        {
            return conditions.FirstOrDefault(c => c.GetType() == conditionType) != null;
        }

        protected override bool _IsTrue(ICharacterData actor, ICharacterData target, AbilityTarget abilityTarget)
        {
            switch (union)
            {
                case Union.And:

                    foreach (var condition in conditions)
                    {
                        if (!condition.IsTrue(actor, target, abilityTarget))
                            return false;
                    }

                    return true;

                case Union.Or:

                    foreach (var condition in conditions)
                    {
                        if (condition.IsTrue(actor, target, abilityTarget))
                            return true;
                    }

                    return false;
            }

            return false;
        }
    }

    [Serializable]
    public class IsAliveCondition : TargetCondition
    {
        protected override bool _IsTrue(ICharacterData actor, ICharacterData target, AbilityTarget abilityTarget)
        {
            return target.HP > 0;
        }
    }

    [Serializable]
    public class IsSelfCondition : TargetCondition
    {
        protected override bool _IsTrue(ICharacterData actor, ICharacterData target, AbilityTarget abilityTarget)
        {
            return actor != null && actor.Id == target.Id;
        }
    }

    [Serializable]
    public class IsTargetTypeCondition : TargetCondition
    {
        protected override bool _IsTrue(ICharacterData actor, ICharacterData target, AbilityTarget abilityTarget)
        {
            if (actor == null)
                return false;

            if (abilityTarget.targetType == AbilityTarget.TargetType.Allies)
                return actor.CharacterSide == target.CharacterSide;
            else
                return actor.CharacterSide != target.CharacterSide;
        }
    }

    [Serializable]
    public class HasStatusCondition : TargetCondition
    {
        public Status status;

        protected override bool _IsTrue(ICharacterData actor, ICharacterData target, AbilityTarget abilityTarget)
        {
            return target.HasStatus(status.id);
        }
    }

    [Serializable]
    public class IsFullLifeCondition : TargetCondition
    {
        protected override bool _IsTrue(ICharacterData actor, ICharacterData target, AbilityTarget abilityTarget)
        {
            return target.HP >= target.GetStat(Stat.MaxHP);
        }
    }

    [Serializable]
    public class IsFullMPCondition : TargetCondition
    {
        protected override bool _IsTrue(ICharacterData actor, ICharacterData target, AbilityTarget abilityTarget)
        {
            return target.MP >= target.GetStat(Stat.MaxMP);
        }
    }
}
