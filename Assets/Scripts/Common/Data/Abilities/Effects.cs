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

    public class StatusState : IClonable, IEquatable<StatusState>
    {
        public StatusAsset statusAsset;
        public int maxTurns;
        public int nTurns;

        public StatusState() { }

        public StatusState(StatusAsset statusAsset, int maxTurns, int nTurns)
        {
            this.statusAsset = statusAsset;
            this.maxTurns = maxTurns;
            this.nTurns = nTurns;
        }

        public void Clone(object source)
        {
            Clone(source as StatusState);
        }

        public void Clone(StatusState source)
        {
            statusAsset = source.statusAsset;
            maxTurns = source.maxTurns;
            nTurns = source.nTurns;
        }

        public bool Equals(StatusState other)
        {
            return statusAsset == other.statusAsset
                && maxTurns == other.maxTurns
                && nTurns == other.nTurns;
        }
    }

    public class CharacterState : IClonable, IEquatable<CharacterState>
    {
        public string id;
        public string nameKey;
        public int hp;
        public CharacterSide characterSide;
        public Level stats;
        public Level permanentBuff;
        public List<StatusState> statuses;

        public CharacterState(){}

        public CharacterState(string id, string nameKey, int hp, CharacterSide characterSide, Level stats, List<StatusState> statuses)
        {
            this.id = id;
            this.nameKey = nameKey;
            this.hp = hp;
            this.characterSide = characterSide;
            this.stats = stats;
            this.statuses = statuses;

            permanentBuff = new Level();
        }

        public List<Stat> GetCharacterType()
        {
            var characterType = new List<Stat>();
            foreach (Stat stat in Enum.GetValues(typeof(Stat)))
            {
                if (stats.GetStat(stat) > 0)
                    characterType.Add(stat);
            }

            return characterType;
        }

        public void Clone(object source)
        {
            Clone(source as CharacterState);
        }

        public void Clone(CharacterState source)
        {
            id = source.id;
            nameKey = source.nameKey;
            hp = source.hp;
            characterSide = source.characterSide;
            stats = source.stats.Clone();
            permanentBuff = source.permanentBuff.Clone();
            statuses = source.statuses.Clone();
        }

        public bool Equals(CharacterState other)
        {
            return id.Equals(other.id)
                && nameKey.Equals(other.nameKey)
                && hp.Equals(other.hp)
                && characterSide.Equals(other.characterSide)
                && stats.Equals(other.stats)
                && permanentBuff.Equals(other.permanentBuff)
                && statuses.SequenceEqual(other.statuses);
        }
    }

    public class CharacterAction : IClonable, IEquatable<CharacterAction>
    {
        public string actorId;
        public List<string> targetIds = new();
        public List<Effect> effects = new();

        public CharacterAction() { }

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

            effects = new();
            foreach(var effect in source.effects)
            {
                effects.Add(effect.Clone());
            }
        }

        public bool Equals(CharacterAction other)
        {
            return actorId.Equals(other.actorId)
                && targetIds.SequenceEqual(other.targetIds)
                && EffectsAreEqual(other);
        }

        public bool EffectsAreEqual(CharacterAction other)
        {
            if(other.effects.Count != effects.Count)
                return false;

            for(int i = 0; i < effects.Count; ++i)
            {
                if (!effects[i].IsEqual(other.effects[i]))
                    return false;
            }

            return true;
        }
    }

    public class ActionState : IClonable, IEquatable<ActionState>
    {
        public CharacterAction pendingAction;
        public List<CharacterState> characters = new();

        public CharacterState FindCharacter(string id) => characters.FirstOrDefault(c => c.id == id);

        public ActionState() { }

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
    public abstract class Effect
    {
        public enum EffectType
        {
            Damage,
            Heal,
            BecomeTarget,
            CancelEffect,
            ModifyStat,
            GiveStatus,
            RemoveStatus
        }

        public abstract EffectType Type { get; }

        public abstract Effect Clone();
        public abstract bool IsEqual(Effect other);
        public abstract string Description(Level actorStats);
        public abstract void Apply(CharacterAction action, ActionState state);
    }
}