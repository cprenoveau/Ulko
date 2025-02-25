using System;
using System.Collections.Generic;
using System.Linq;

namespace Ulko.Data.Abilities
{
    public enum CharacterSide
    {
        Heroes,
        Enemies
    }

    public class CharacterState : IClonable, IEquatable<CharacterState>
    {
        public string id;
        public string name;
        public int hp;
        public CharacterSide characterSide;
        public Level stats;
        public List<string> statusIds;

        public CharacterState(){}

        public CharacterState(string id, string name, int hp, CharacterSide characterSide, Level stats, List<string> statusIds)
        {
            this.id = id;
            this.name = name;
            this.hp = hp;
            this.characterSide = characterSide;
            this.stats = stats;
            this.statusIds = statusIds;
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
            statusIds = source.statusIds.Clone();
        }

        public bool Equals(CharacterState other)
        {
            return id.Equals(other.id)
                && name.Equals(other.name)
                && hp.Equals(other.hp)
                && characterSide.Equals(other.characterSide)
                && stats.Equals(other.stats)
                && statusIds.SequenceEqual(other.statusIds);
        }
    }

    public class CharacterAction : IClonable, IEquatable<CharacterAction>
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

        public bool Equals(CharacterAction other)
        {
            return actorId.Equals(other.actorId)
                && targetIds.SequenceEqual(other.targetIds)
                && effects.SequenceEqual(other.effects);
        }
    }

    public class ActionState : IClonable, IEquatable<ActionState>
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

        public bool Equals(ActionState other)
        {
            return pendingAction.Equals(other.pendingAction)
                && characters.SequenceEqual(other.characters);
        }

        public static void Apply(CharacterAction action, ActionState state)
        {
            foreach (var effect in action.effects)
            {
                effect.Apply(action, state);
            }
        }
    }

    [Serializable]
    public abstract class Effect : IClonable
    {
        public enum EffectType
        {
            Damage,
            BecomeTarget,
            CancelEffect,
            ModifyStat,
            GiveStatus
        }

        public abstract EffectType Type { get; }

        public abstract void Clone(object source);
        public abstract string Description();
        public abstract void Apply(CharacterAction action, ActionState state);
    }
}