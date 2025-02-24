using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Battle
{
    public enum CharacterSide
    {
        Heroes,
        Enemies
    }

    public class BattleCharacter : IClonable
    {
        public delegate float GetStatDelegate(Stat stat);

        public string id;
        public string name;
        public int hp;
        public CharacterSide characterSide;
        public GetStatDelegate getStat;

        public BattleCharacter(){}

        public BattleCharacter(string id, string name, int hp, CharacterSide characterSide, GetStatDelegate getStat)
        {
            this.id = id;
            this.name = name;
            this.hp = hp;
            this.characterSide = characterSide;
            this.getStat = getStat;
        }

        public void Clone(object source)
        {
            Clone(source as BattleCharacter);
        }

        public void Clone(BattleCharacter source)
        {
            id = source.id;
            name = source.name;
            hp = source.hp;
            characterSide = source.characterSide;
            getStat = source.getStat;
        }
    }

    public class BattleAction : IClonable
    {
        public BattleCharacter actor;
        public List<BattleCharacter> targets = new();
        public List<BattleEffect> effects = new();

        public BattleAction(BattleCharacter actor, List<BattleCharacter> targets, List<BattleEffect> effects)
        {
            this.actor = actor;
            this.targets = targets;
            this.effects = effects;
        }

        public void Clone(object source)
        {
            Clone(source as BattleAction);
        }

        public void Clone(BattleAction source)
        {
            actor = source.actor.Clone();
            targets = source.targets.Clone();
            effects = source.effects.Clone();
        }
    }

    public class BattleState : IClonable
    {
        public BattleAction currentAction;
        public List<BattleCharacter> characters = new();

        public BattleState(BattleAction currentAction, List<BattleCharacter> characters)
        {
            this.currentAction = currentAction;
            this.characters = characters;
        }

        public void Clone(object source)
        {
            Clone(source as BattleState);
        }

        public void Clone(BattleState source)
        {
            currentAction = source.currentAction.Clone();
            characters = source.characters.Clone();
        }

        public static BattleState ApplyAction(BattleState state)
        {
            var outcome = state.Clone();

            foreach(var effect in outcome.currentAction.effects)
            {
                outcome = effect.Apply(outcome);
            }

            return outcome;
        }
    }

    public abstract class BattleEffect : IClonable
    {
        public abstract void Clone(object source);
        public abstract string Description();
        public abstract BattleState Apply(BattleState state);
    }

    [Serializable]
    public class Damage : BattleEffect
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

        public override BattleState Apply(BattleState state)
        {
            var outcome = state.Clone();

            foreach(var target in outcome.currentAction.targets)
            {
                Apply(outcome, target);
            }
            
            return outcome;
        }

        private void Apply(BattleState state, BattleCharacter target)
        {
            float atk = state.currentAction.actor.getStat(attackStat);
            float def = target.getStat(defenseStat);

            float damage = atk * damageMultiplier;

            if (def != 0)
                damage = damage * config.flatModifier / (config.flatModifier + def);

            damage += target.getStat(Stat.Fortitude) * percentDamage / 100f;
            damage += flatDamage;

            damage = Mathf.Clamp(damage, 1, damage);

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
