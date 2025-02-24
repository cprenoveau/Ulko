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

    public class BattlefieldState : IClonable
    {
        public CharacterAction declaredAction;
        public List<CharacterState> characters = new();

        public CharacterState FindCharacter(string id) => characters.FirstOrDefault(c => c.id == id);

        public BattlefieldState(CharacterAction declaredAction, List<CharacterState> characters)
        {
            this.declaredAction = declaredAction;
            this.characters = characters;
        }

        public void Clone(object source)
        {
            Clone(source as BattlefieldState);
        }

        public void Clone(BattlefieldState source)
        {
            declaredAction = source.declaredAction.Clone();
            characters = source.characters.Clone();
        }

        public static BattlefieldState Apply(CharacterAction action, BattlefieldState state)
        {
            var outcome = state;

            foreach (var effect in action.effects)
            {
                outcome = effect.Apply(action, outcome);
            }

            return outcome;
        }
    }

    [Serializable]
    public abstract class Effect : IClonable
    {
        public abstract void Clone(object source);
        public abstract string Description();
        public abstract BattlefieldState Apply(CharacterAction action, BattlefieldState state);
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

        public override BattlefieldState Apply(CharacterAction action, BattlefieldState state)
        {
            var outcome = state;

            var actor = state.FindCharacter(action.actorId);
            if (actor == null)
                return outcome;

            foreach (string targetId in action.targetIds)
            {
                var target = state.FindCharacter(targetId);
                if(target != null && condition.IsTrue(actor, target))
                {
                    outcome = state.Clone();
                    Apply(actor, target);
                }
            }

            return outcome;
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

        public override BattlefieldState Apply(CharacterAction action, BattlefieldState state)
        {
            var outcome = state;

            var actor = state.FindCharacter(action.actorId);
            if (actor == null)
                return outcome;

            if(UnityEngine.Random.Range(0, 100) > percentChance)
            {
                List<string> newTargets = new();

                foreach(var targetId in state.declaredAction.targetIds)
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

                outcome = state.Clone();
                outcome.declaredAction.targetIds = newTargets;
            }

            return outcome;
        }

        public override string Description()
        {
            string str = "";
            return str;
        }
    }
}
