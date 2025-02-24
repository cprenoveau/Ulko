using System;
using System.Collections.Generic;
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
        public CharacterState actor;
        public List<CharacterState> targets = new();
        public List<Effect> effects = new();

        public CharacterAction(CharacterState actor, List<CharacterState> targets, List<Effect> effects)
        {
            this.actor = actor;
            this.targets = targets;
            this.effects = effects;
        }

        public void Clone(object source)
        {
            Clone(source as CharacterAction);
        }

        public void Clone(CharacterAction source)
        {
            actor = source.actor.Clone();
            targets = source.targets.Clone();
            effects = source.effects.Clone();
        }
    }

    public class BattlefieldState : IClonable
    {
        public CharacterAction currentAction;
        public List<CharacterState> characters = new();

        public BattlefieldState(CharacterAction currentAction, List<CharacterState> characters)
        {
            this.currentAction = currentAction;
            this.characters = characters;
        }

        public void Clone(object source)
        {
            Clone(source as BattlefieldState);
        }

        public void Clone(BattlefieldState source)
        {
            currentAction = source.currentAction.Clone();
            characters = source.characters.Clone();
        }

        public static BattlefieldState EvaluateOutcome(BattlefieldState state)
        {
            var outcome = state.Clone();

            foreach(var effect in outcome.currentAction.effects)
            {
                outcome = effect.Apply(outcome);
            }

            return outcome;
        }
    }

    [Serializable]
    public abstract class Effect : IClonable
    {
        public abstract void Clone(object source);
        public abstract string Description();
        public abstract BattlefieldState Apply(BattlefieldState state);
    }

    [Serializable]
    public class Damage : Effect
    {
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
            config = source.config;
            attackStat = source.attackStat;
            defenseStat = source.defenseStat;
            damageMultiplier = source.damageMultiplier;
            percentDamage = source.percentDamage;
            flatDamage = source.flatDamage;
        }

        public override BattlefieldState Apply(BattlefieldState state)
        {
            var outcome = state.Clone();

            foreach(var target in outcome.currentAction.targets)
            {
                Apply(outcome, target);
            }
            
            return outcome;
        }

        private void Apply(BattlefieldState state, CharacterState target)
        {
            float atk = state.currentAction.actor.stats.GetStat(attackStat);
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
}
