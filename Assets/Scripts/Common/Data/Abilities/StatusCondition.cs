using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public abstract class StatusCondition
    {
        public bool invert;

        public bool IsTrue(ICharacterData actor, IActionData action)
        {
            bool isTrue = _IsTrue(actor, action);
            if (invert) return !isTrue;
            return isTrue;
        }

        protected abstract bool _IsTrue(ICharacterData actor, IActionData action);
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
        public List<StatusCondition> conditions = new List<StatusCondition>();

        protected override bool _IsTrue(ICharacterData actor, IActionData action)
        {
            switch (union)
            {
                case Union.And:

                    foreach (var condition in conditions)
                    {
                        if (!condition.IsTrue(actor, action))
                            return false;
                    }

                    return true;

                case Union.Or:

                    foreach (var condition in conditions)
                    {
                        if (condition.IsTrue(actor, action))
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

        protected override bool _IsTrue(ICharacterData actor, IActionData action)
        {
            switch (actorType)
            {
                case ActorType.Me: return action.Actor.Id == actor.Id;
                case ActorType.Ally: return action.Actor.CharacterSide == actor.CharacterSide;
                case ActorType.Enemy: return action.Actor.CharacterSide != actor.CharacterSide;
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

        protected override bool _IsTrue(ICharacterData actor, IActionData action)
        {
            if (action.Targets.Count == 0)
                return false;

            switch (targetType)
            {
                case TargetType.Me: return action.Targets[0].Id == actor.Id;
                case TargetType.Ally: return action.Targets[0].CharacterSide == actor.CharacterSide;
                case TargetType.Enemy: return action.Targets[0].CharacterSide != actor.CharacterSide;
                case TargetType.One: return action.Targets.Count == 1;
            }

            return false;
        }
    }

    [Serializable]
    public class EffectIs : StatusCondition
    {
        public enum EffectType
        {
            Damage,
            PhysicalDamage,
            MagicDamage,
            Heal,
            GiveStatus,
            RemoveStatus
        }

        public EffectType effectType;

        protected override bool _IsTrue(ICharacterData actor, IActionData action)
        {
            switch (effectType)
            {
                case EffectType.Damage: return action.Effects.Find(e => e.GetType() == typeof(Damage)) != null;
                case EffectType.PhysicalDamage: return action.Effects.Find(e => e.GetType() == typeof(Damage) && (e as Damage).attackStat == Stat.Attack) != null;
                case EffectType.MagicDamage: return action.Effects.Find(e => e.GetType() == typeof(Damage) && (e as Damage).attackStat == Stat.Magic) != null;
                case EffectType.Heal: return action.Effects.Find(e => e.GetType() == typeof(Heal)) != null;
                case EffectType.GiveStatus: return action.Effects.Find(e => e.GetType() == typeof(GiveStatus)) != null;
                case EffectType.RemoveStatus: return action.Effects.Find(e => e.GetType() == typeof(RemoveStatus)) != null;
            }

            return false;
        }
    }

    [Serializable]
    public class RandomRoll : StatusCondition
    {
        public float percentChance = 50;

        protected override bool _IsTrue(ICharacterData actor, IActionData action)
        {
            return UnityEngine.Random.Range(0, 100) < percentChance;
        }
    }
}
