using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    public enum CharacterSide
    {
        Heroes,
        Enemies
    }

    public class CharacterState : IClonable
    {
        public string id;
        public string name;
        public int hp;
        public CharacterSide characterSide;
        public Level stats;

        public CharacterState(){}

        public CharacterState(string id, string name, int hp, CharacterSide characterSide, Level stats)
        {
            this.id = id;
            this.name = name;
            this.hp = hp;
            this.characterSide = characterSide;
            this.stats = stats;
        }

        public void Clone(object source)
        {
            Clone(source as CharacterState);
        }

        public void Clone(CharacterState source)
        {
            id = source.id;
            name = source.name;
            hp = source.hp;
            characterSide = source.characterSide;
            stats = source.stats.Clone();
        }
    }

    public class CharacterAction : IClonable
    {
        public string actorId;
        public List<string> targetIds = new();
        public List<Effect> effects = new();

        public CharacterAction(string actorId, List<string> targetIds, List<Effect> effects)
        {
            this.actorId = actorId;
            this.targetIds = targetIds;
            this.effects = effects;
        }

        public void Clone(object source)
        {
            Clone(source as CharacterAction);
        }

        public void Clone(CharacterAction source)
        {
            actorId = source.actorId;
            targetIds = source.targetIds.Clone();
            effects = source.effects.Clone();
        }
    }

    public class ActionState : IClonable
    {
        public CharacterAction pendingAction;
        public List<CharacterState> characters = new();

        public CharacterState FindCharacter(string id) => characters.FirstOrDefault(c => c.id == id);

        public ActionState(CharacterAction pendingAction, List<CharacterState> characters)
        {
            this.pendingAction = pendingAction;
            this.characters = characters;
        }

        public void Clone(object source)
        {
            Clone(source as ActionState);
        }

        public void Clone(ActionState source)
        {
            pendingAction = source.pendingAction.Clone();
            characters = source.characters.Clone();
        }

        public static void Apply(CharacterAction action, ActionState state)
        {
            foreach (var effect in action.effects)
            {
                effect.Apply(action, state);
            }
        }
    }

    public class ActionStack
    {
        public ActionState CurrentState => stateStack.Peek();

        private readonly Stack<ActionState> stateStack = new();

        public ActionStack(ActionState currentState)
        {
            stateStack.Push(currentState);
        }

        public void Push(ActionState state)
        {
            stateStack.Push(state);
        }

        public void Evaluate()
        {
            while (stateStack.Count > 1)
            {
                var state = CurrentState;
                stateStack.Pop();
                ActionState.Apply(state.pendingAction, stateStack.Peek());
            }
        }
    }

    [Serializable]
    public abstract class Effect : IClonable
    {
        public abstract void Clone(object source);
        public abstract string Description();
        public abstract void Apply(CharacterAction action, ActionState state);
    }

    [Serializable]
    public class Damage : Effect
    {
        public TargetConditionAsset condition;
        public EffectConfig config;
        public Stat attackStat = Stat.Wisdom;
        public Stat defenseStat = Stat.Intuition;
        [Tooltip("Multiplier applied to stat damage")]
        public float damageMultiplier = 1f;
        public float percentDamage;
        public float flatDamage;

        public override void Clone(object source)
        {
            Clone(source as Damage);
        }

        private void Clone(Damage source)
        {
            condition = source.condition;
            config = source.config;
            attackStat = source.attackStat;
            defenseStat = source.defenseStat;
            damageMultiplier = source.damageMultiplier;
            percentDamage = source.percentDamage;
            flatDamage = source.flatDamage;
        }

        public override void Apply(CharacterAction action, ActionState state)
        {
            var actor = state.FindCharacter(action.actorId);
            if (actor == null)
                return;

            foreach (string targetId in action.targetIds)
            {
                var target = state.FindCharacter(targetId);
                if(target != null && condition.IsTrue(actor, target))
                {
                    Apply(actor, target);
                }
            }
        }

        public void Apply(CharacterState actor, CharacterState target)
        {
            float atk = actor.stats.GetStat(attackStat);
            float def = target.stats.GetStat(defenseStat);

            float damage = atk * damageMultiplier;

            if (def != 0)
                damage = damage * config.flatModifier / (config.flatModifier + def);

            damage += target.stats.GetStat(Stat.Fortitude) * percentDamage / 100f;
            damage += flatDamage;

            damage = Mathf.Clamp(damage, 1, target.hp);

            target.hp -= (int)damage;
        }

        public override string Description()
        {
            string str = "";

            if (damageMultiplier != 0)
            {
                str = Localization.LocalizeFormat("damage_desc", damageMultiplier * 100, TextFormat.Localize(attackStat), TextFormat.Localize(defenseStat));
            }
            else if (percentDamage != 0)
            {
                str = Localization.LocalizeFormat("damage_percent_desc", percentDamage);
            }
            else if (flatDamage != 0)
            {
                str = Localization.LocalizeFormat("damage_flat_desc", flatDamage);
            }

            return str;
        }
    }

    [Serializable]
    public class BecomeTarget : Effect
    {
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

        public override void Apply(CharacterAction action, ActionState state)
        {
            var actor = state.FindCharacter(action.actorId);
            if (actor == null)
                return;

            if(UnityEngine.Random.Range(0, 100) > percentChance)
            {
                List<string> newTargets = new();

                foreach(var targetId in state.pendingAction.targetIds)
                {
                    var target = state.FindCharacter(targetId);
                    if(target != null && condition.IsTrue(actor, target))
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
