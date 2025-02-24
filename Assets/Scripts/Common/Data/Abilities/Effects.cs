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

    public class Character : IClonable
    {
        public string id;
        public string name;
        public int hp;
        public CharacterSide characterSide;
        public Level stats;

        public Character(){}

        public Character(string id, string name, int hp, CharacterSide characterSide, Level stats)
        {
            this.id = id;
            this.name = name;
            this.hp = hp;
            this.characterSide = characterSide;
            this.stats = stats;
        }

        public void Clone(object source)
        {
            Clone(source as Character);
        }

        public void Clone(Character source)
        {
            id = source.id;
            name = source.name;
            hp = source.hp;
            characterSide = source.characterSide;
            stats = source.stats.Clone();
        }
    }

    public class Action : IClonable
    {
        public Character actor;
        public List<Character> targets = new();
        public List<Effect> effects = new();

        public Action(Character actor, List<Character> targets, List<Effect> effects)
        {
            this.actor = actor;
            this.targets = targets;
            this.effects = effects;
        }

        public void Clone(object source)
        {
            Clone(source as Action);
        }

        public void Clone(Action source)
        {
            actor = source.actor.Clone();
            targets = source.targets.Clone();
            effects = source.effects.Clone();
        }
    }

    public class State : IClonable
    {
        public Action currentAction;
        public List<Character> characters = new();

        public State(Action currentAction, List<Character> characters)
        {
            this.currentAction = currentAction;
            this.characters = characters;
        }

        public void Clone(object source)
        {
            Clone(source as State);
        }

        public void Clone(State source)
        {
            currentAction = source.currentAction.Clone();
            characters = source.characters.Clone();
        }

        public static State EvaluateOutcome(State state)
        {
            var outcome = state.Clone();

            foreach(var effect in outcome.currentAction.effects)
            {
                outcome = effect.Apply(outcome);
            }

            return outcome;
        }
    }

    public abstract class Effect : IClonable
    {
        public abstract void Clone(object source);
        public abstract string Description();
        public abstract State Apply(State state);
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

        public override State Apply(State state)
        {
            var outcome = state.Clone();

            foreach(var target in outcome.currentAction.targets)
            {
                Apply(outcome, target);
            }
            
            return outcome;
        }

        private void Apply(State state, Character target)
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
