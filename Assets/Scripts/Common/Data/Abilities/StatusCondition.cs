using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public abstract class StatusCondition
    {
        public bool invert;

        public bool IsTrue(CharacterState actor, ActionState state)
        {
            bool isTrue = _IsTrue(actor, state);
            if (invert) return !isTrue;
            return isTrue;
        }

        protected abstract bool _IsTrue(CharacterState actor, ActionState state);
    }

    [Serializable]
    public class CompositeStatusCondition : StatusCondition
    {
        public enum Union
        {
            And,
            Or
        }

        public Union union;

        [SerializeReference]
        public List<StatusCondition> conditions = new();

        protected override bool _IsTrue(CharacterState actor, ActionState state)
        {
            switch (union)
            {
                case Union.And:

                    foreach (var condition in conditions)
                    {
                        if (!condition.IsTrue(actor, state))
                            return false;
                    }

                    return true;

                case Union.Or:

                    foreach (var condition in conditions)
                    {
                        if (condition.IsTrue(actor, state))
                            return true;
                    }

                    return false;
            }

            return false;
        }
    }

    [Serializable]
    public class ActorIsCondition : StatusCondition
    {
        public enum ActorType
        {
            Me,
            Ally,
            Enemy
        }

        public ActorType actorType;

        protected override bool _IsTrue(CharacterState actor, ActionState state)
        {
            switch (actorType)
            {
                case ActorType.Me: return state.pendingAction.actorId == actor.id;
                case ActorType.Ally: return state.FindCharacter(state.pendingAction.actorId).characterSide == actor.characterSide;
                case ActorType.Enemy: return state.FindCharacter(state.pendingAction.actorId).characterSide != actor.characterSide;
            }

            return false;
        }
    }

    [Serializable]
    public class TargetIsCondition : StatusCondition
    {
        public enum TargetType
        {
            Me,
            Ally,
            Enemy,
            One
        }

        public TargetType targetType;

        protected override bool _IsTrue(CharacterState actor, ActionState state)
        {
            if (state.pendingAction.targetIds.Count == 0)
                return false;

            switch (targetType)
            {
                case TargetType.Me: return state.pendingAction.targetIds.FirstOrDefault(t => t == actor.id) != null;
                case TargetType.Ally: return state.FindCharacter(state.pendingAction.targetIds[0]).characterSide == actor.characterSide;
                case TargetType.Enemy: return state.FindCharacter(state.pendingAction.targetIds[0]).characterSide != actor.characterSide;
                case TargetType.One: return state.pendingAction.targetIds.Count == 1;
            }

            return false;
        }
    }

    [Serializable]
    public class EffectIsCondition : StatusCondition
    {
        public Effect.EffectType effectType;

        protected override bool _IsTrue(CharacterState actor, ActionState state)
        {
            return state.pendingAction.effects.FirstOrDefault(e => e.Type == effectType) != null;
        }
    }
}
