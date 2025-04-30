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
        public StatusAsset StatusAsset { get; private set; }
        public int MaxTurns { get; private set; }
        public int CurrentTurn { get; private set; }

        public void IncrementCurrentTurn()
        {
            CurrentTurn++;
        }

        public void AddTotalTurns(int turns)
        {
            MaxTurns += turns;
        }

        public StatusState() { }

        public StatusState(StatusAsset statusAsset, int maxTurns, int currentTurn)
        {
            StatusAsset = statusAsset;
            MaxTurns = maxTurns;
            CurrentTurn = currentTurn;
        }

        public void Clone(object source)
        {
            Clone(source as StatusState);
        }

        public void Clone(StatusState source)
        {
            StatusAsset = source.StatusAsset;
            MaxTurns = source.MaxTurns;
            CurrentTurn = source.CurrentTurn;
        }

        public bool Equals(StatusState other)
        {
            return StatusAsset == other.StatusAsset
                && MaxTurns == other.MaxTurns
                && CurrentTurn == other.CurrentTurn;
        }
    }

    public class CharacterState : IClonable, IEquatable<CharacterState>
    {
        public string Id { get; private set; }
        public string NameKey { get; private set; }
        public int HP { get; private set; }

        public void AddHP(float hp)
        {
            HP += (int)hp;

            if (HP <= 0)
                statuses.Clear();
        }

        public CharacterSide CharacterSide { get; private set; }
        public Level BaseStats { get; private set; }
        public Level Buff { get; private set; }

        public IEnumerable<StatusState> Statuses => statuses;
        private List<StatusState> statuses = new();

        public void RemoveStatus(string statusId)
        {
            statuses.RemoveAll(s => s.StatusAsset.id == statusId);
        }

        public void AddStatus(StatusAsset status, int turns)
        {
            int index = statuses.FindIndex(s => s.StatusAsset.id == status.id);
            if (index == -1)
            {
                statuses.Add(new StatusState(status, turns, 0));
            }
            else
            {
                statuses[index].AddTotalTurns(turns);
            }
        }

        public List<StatusState> CaptureStatus()
        {
            return statuses.Clone();
        }

        public Level CurrentStats => BaseStats + Buff;
        public Level OriginalStats { get; private set; }

        public CharacterState(){}

        public CharacterState(string id, string nameKey, int hp, CharacterSide characterSide, Level baseStats, Level buff, List<StatusState> statuses)
        {
            Id = id;
            NameKey = nameKey;
            HP = hp;
            CharacterSide = characterSide;
            BaseStats = baseStats;
            Buff = buff;
            this.statuses = statuses;

            OriginalStats = CurrentStats;
        }

        public List<Stat> GetCharacterType()
        {
            var characterType = new List<Stat>();
            foreach (Stat stat in Enum.GetValues(typeof(Stat)))
            {
                if (CurrentStats.GetStat(stat) > 0)
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
            Id = source.Id;
            NameKey = source.NameKey;
            HP = source.HP;
            CharacterSide = source.CharacterSide;
            BaseStats = source.BaseStats.Clone();
            Buff = source.Buff.Clone();
            statuses = source.statuses.Clone();
        }

        public bool Equals(CharacterState other)
        {
            return Id.Equals(other.Id)
                && NameKey.Equals(other.NameKey)
                && HP.Equals(other.HP)
                && CharacterSide.Equals(other.CharacterSide)
                && BaseStats.Equals(other.BaseStats)
                && Buff.Equals(other.Buff)
                && statuses.SequenceEqual(other.statuses);
        }
    }

    public class CharacterAction : IClonable, IEquatable<CharacterAction>
    {
        public string ActorId { get; private set; }

        public IEnumerable<string> TargetIds => targetIds;
        private List<string> targetIds = new();

        public void ReplaceTarget(string oldTargetId, string newTargetId)
        {
            int index = targetIds.IndexOf(oldTargetId);
            if(index != -1)
            {
                targetIds[index] = newTargetId;
            }
        }

        public IEnumerable<Effect> Effects => effects;
        private List<Effect> effects = new();

        public void RemoveEffectOfType(Effect.EffectType effectType)
        {
            effects.RemoveAll(e => e.Type == effectType);
        }

        public CharacterAction() { }

        public CharacterAction(string actorId, List<string> targetIds, List<Effect> effects)
        {
            ActorId = actorId;
            this.targetIds = targetIds;
            this.effects = effects;
        }

        public void Clone(object source)
        {
            Clone(source as CharacterAction);
        }

        public void Clone(CharacterAction source)
        {
            ActorId = source.ActorId;
            targetIds = source.targetIds.Clone();

            effects = new();
            foreach(var effect in source.effects)
            {
                effects.Add(effect.Clone());
            }
        }

        public bool Equals(CharacterAction other)
        {
            return ActorId.Equals(other.ActorId)
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

        public CharacterState FindCharacter(string id) => characters.FirstOrDefault(c => c.Id == id);

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
            foreach (var effect in action.Effects)
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
        public abstract string Description(CharacterState actor);
        public abstract void Apply(CharacterAction action, ActionState state);
    }
}