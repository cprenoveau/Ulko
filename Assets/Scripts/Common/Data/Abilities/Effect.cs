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

    public interface ICharacterData
    {
        CharacterSide CharacterSide { get; }
        List<CharacterTag> CharacterTags { get; }
        ElementalTag Element { get; }
        string Id { get; }
        string Name { get; }
        int HP { get; }
        float GetStat(Stat stat);
        List<ActiveStatus> Statuses { get; }

        bool HasStatus(string id);
        void ApplyResult(CharacterResult result);
    }

    public interface IActionData
    {
        IActionData Clone();

        ICharacterData Actor { get; set; }
        List<ICharacterData> Targets { get; set; }
        List<IEffect> Effects { get; set; }
        float Multiplier { get; }
    }

    public class StatModifier
    {
        public ICharacterData target;
        public ICharacterData against;
        public Stat stat;
        public float multiply = 1f;
        public int add;

        public StatModifier(ICharacterData target, ICharacterData against, Stat stat, float multiply, int add)
        {
            this.target = target;
            this.against = against;
            this.stat = stat;
            this.multiply = multiply;
            this.add = add;
        }

        public static float GetStat(ICharacterData target, ICharacterData against, Stat stat, List<StatModifier> modifiers)
        {
            var value = target.GetStat(stat);
            var mods = modifiers.FindAll(m => m.target.Id == target.Id && m.against.Id == against.Id && m.stat == stat);
            foreach(var mod in mods)
            {
                if (mod.multiply == 0)
                {
                    value = 0;
                    break;
                }

                value *= mod.multiply;
                value += mod.add;
            }

            return value;
        }
    }

    public class ActiveStatus
    {
        public enum DurationType
        {
            TurnCount,
            Infinite
        }

        public DurationType durationType;
        public float duration;
        public float lastApplyTime;
        public float elapsedTime;
        public Status status;

        public ActiveStatus Clone()
        {
            return new ActiveStatus
            {
                durationType = durationType,
                duration = duration,
                lastApplyTime = lastApplyTime,
                elapsedTime = elapsedTime,
                status = status
            };
        }
    }

    public class CharacterResult
    {
        public string targetId;
        public string actorId;

        public bool revive;
        public int hpDiff;
        public int mpDiff;
        public List<ActiveStatus> addStatus = new List<ActiveStatus>();
        public List<Status> removeStatus = new List<Status>();

        public CharacterResult(string targetId, string actorId)
        {
            this.targetId = targetId;
            this.actorId = actorId;
        }
    }

    public class EffectResults
    {
        public List<EffectResult> results = new List<EffectResult>();
        public IActionData nextAction;

        public bool Success => results.Find(r => r.success) != null;
    }

    public class EffectResult
    {
        public bool success;
        public Type effectType;
        public IActionData nextAction;
        public List<CharacterResult> characters = new List<CharacterResult>();

        public EffectResult(bool success, Type effectType)
        {
            this.success = success;
            this.effectType = effectType;
        }

        public static EffectResults GetResults(IActionData action, List<ICharacterData> all, List<StatModifier> statModifiers, IActionData nextAction)
        {
            IActionData newAction = null;
            var results = new List<EffectResult>();

            foreach (var target in action.Targets)
            {
                foreach (var effect in action.Effects)
                {
                    var result = effect.ComputeResult(action.Actor, target, all, statModifiers, results, nextAction, action.Multiplier);

                    if (result.nextAction != null)
                    {
                        newAction = result.nextAction;
                        nextAction = result.nextAction;
                    }

                    results.Add(result);

                    if (!result.success)
                        break;
                }
            }

            return new EffectResults { results = results, nextAction = newAction };
        }
    }

    public interface IEffect
    {
        IEffect Clone();
        string Description();

        EffectResult ComputeResult(
            ICharacterData actor,
            ICharacterData target,
            List<ICharacterData> all,
            List<StatModifier> statModifiers,
            List<EffectResult> lastResults,
            IActionData nextAction,
            float multiplier);
    }

    [Serializable]
    public class Damage : IEffect
    {
        public EffectConfig config;
        public Stat attackStat = Stat.Wisdom;
        public ElementalTag element;
        [Tooltip("Multiplier applied to stat based damage")]
        public float damageMultiplier = 1f;
        public float percentDamage;
        public float flatDamage;
        public bool noMiss;

        public IEffect Clone()
        {
            return new Damage
            {
                config = config,
                attackStat = attackStat,
                element = element,
                damageMultiplier = damageMultiplier,
                percentDamage = percentDamage,
                flatDamage = flatDamage,
                noMiss = noMiss
            };
        }

        public string Description()
        {
            string str = "";

            if(damageMultiplier != 0)
            {
                if (element != null)
                    str = Localization.LocalizeFormat("damage_wtype_desc", damageMultiplier * 100, TextFormat.Localize(element), TextFormat.Localize(attackStat));
                else
                    str = Localization.LocalizeFormat("damage_desc", damageMultiplier * 100, TextFormat.Localize(attackStat));
            }
            else if(percentDamage != 0)
            {
                str = Localization.LocalizeFormat("damage_percent_desc", percentDamage);
            }
            else if(flatDamage != 0)
            {
                str = Localization.LocalizeFormat("damage_flat_desc", flatDamage);
            }

            if (noMiss)
            {
                str += " ";
                str += Localization.Localize("no_miss");
            }
               
            return str;
        }

        public EffectResult ComputeResult(
            ICharacterData actor,
            ICharacterData target,
            List<ICharacterData> all,
            List<StatModifier> statModifiers,
            List<EffectResult> lastResults,
            IActionData nextAction,
            float multiplier)
        {
            var characterResult = ComputeResult(actor, target, statModifiers, multiplier);
            bool success = characterResult.hpDiff < 0;

            var result = new EffectResult(success, typeof(Damage));
            result.characters.Add(characterResult);

            return result;
        }

        public CharacterResult ComputeResult(ICharacterData actor, ICharacterData target, List<StatModifier> statModifiers, float multiplier)
        {
            float atk = StatModifier.GetStat(actor, target, attackStat, statModifiers);

            float damage = atk * damageMultiplier;

            float randomMod = UnityEngine.Random.Range(config.randomMinMultiplier, config.randomMaxMultiplier);
            damage *= randomMod;

            damage += StatModifier.GetStat(target, actor, Stat.Fortitude, statModifiers) * percentDamage / 100f;
            damage += flatDamage;

            float attackMult = config.GetAttackMultiplier(element, target.Element);

            int finalDamage = Mathf.RoundToInt(damage * attackMult * multiplier);
            return new CharacterResult(target.Id, actor.Id) { hpDiff = -finalDamage };
        }
    }

    [Serializable]
    public class Heal : IEffect
    {
        public EffectConfig config;
        public Stat healStat = Stat.Wisdom;
        public ElementalTag element;
        [Tooltip("Multiplier applied to stat based healing")]
        public float healMultiplier = 1f;
        public float percentHeal;
        public float flatHeal;
        public bool revive;

        public IEffect Clone()
        {
            return new Heal
            {
                config = config,
                healStat = healStat,
                element = element,
                healMultiplier = healMultiplier,
                percentHeal = percentHeal,
                flatHeal = flatHeal,
                revive = revive
            };
        }

        public string Description()
        {
            string str = "";

            if (revive)
                str = string.Format("{0} + ", Localization.LocalizeFormat("revives"));

            if (healMultiplier != 0)
            {
                if (element != null)
                    str += Localization.LocalizeFormat("heal_wtype_desc", healMultiplier * 100, TextFormat.Localize(element), TextFormat.Localize(healStat));
                else
                    str += Localization.LocalizeFormat("heal_desc", healMultiplier * 100, TextFormat.Localize(healStat));
            }
            else if (percentHeal != 0)
            {
                str += Localization.LocalizeFormat("heal_percent_desc", percentHeal);
            }
            else if (flatHeal != 0)
            {
                str += Localization.LocalizeFormat("heal_flat_desc", flatHeal);
            }

            return str;
        }

        public EffectResult ComputeResult(
            ICharacterData actor,
            ICharacterData target,
            List<ICharacterData> all,
            List<StatModifier> statModifiers,
            List<EffectResult> lastResults,
            IActionData nextAction,
            float multiplier)
        {
            var characterResult = ComputeResult(actor, target, statModifiers, multiplier);
            bool success = characterResult.hpDiff > 0;

            if(config.zombieStatus != null && target.HasStatus(config.zombieStatus.id))
            {
                characterResult.hpDiff *= -1;
            }

            var result = new EffectResult(success, typeof(Heal));
            result.characters.Add(characterResult);

            return result;
        }

        public CharacterResult ComputeResult(ICharacterData actor, ICharacterData target, List<StatModifier> statModifiers, float multiplier)
        {
            float stat = StatModifier.GetStat(actor, target, healStat, statModifiers);
            float heal = stat * healMultiplier;

            float randomMod = UnityEngine.Random.Range(config.randomMinMultiplier, config.randomMaxMultiplier);
            heal *= randomMod;

            heal += StatModifier.GetStat(target, actor, Stat.Fortitude, statModifiers) * percentHeal / 100f;
            heal += flatHeal;

            int finalHeal = Mathf.RoundToInt(heal * multiplier);
            return new CharacterResult(target.Id, actor.Id) { hpDiff = finalHeal, revive = revive };
        }
    }

    [Serializable]
    public class Leech : IEffect
    {
        public bool applyToAllAllies;
        public bool revive;

        public IEffect Clone()
        {
            return new Leech
            {
                applyToAllAllies = applyToAllAllies,
                revive = revive
            };
        }

        public string Description()
        {
            return "Leech";
        }

        public EffectResult ComputeResult(
            ICharacterData actor,
            ICharacterData target,
            List<ICharacterData> all,
            List<StatModifier> statModifiers,
            List<EffectResult> lastResults,
            IActionData nextAction,
            float multiplier)
        {
            var damage = lastResults.Find(r => r.effectType == typeof(Damage));
            if(damage != null)
            {
                float total = 0;
                foreach (var character in damage.characters)
                {
                    total -= character.hpDiff;
                }

                var healTargets = new List<ICharacterData>();
                if (applyToAllAllies)
                    healTargets.AddRange(all.Where(c => c.CharacterSide == actor.CharacterSide));
                else
                    healTargets.Add(actor);

                int healAmount = (int)(total / healTargets.Count);

                var characterResult = ComputeResult(actor, target, healAmount);
                bool success = healAmount > 0;

                var result = new EffectResult(success, typeof(Leech));
                result.characters.Add(characterResult);

                return result;
            }

            return new EffectResult(false, typeof(Leech));
        }

        public CharacterResult ComputeResult(ICharacterData actor, ICharacterData target, int healAmount)
        {
            return new CharacterResult(target.Id, actor.Id) { hpDiff = healAmount, revive = revive };
        }
    }

    [Serializable]
    public class ModifyStat : IEffect
    {
        public enum TargetType
        {
            Actor,
            Targets,
            All
        }

        [Serializable]
        public class Target
        {
            public TargetType type;
            public List<CharacterTag> withAllTags = new List<CharacterTag>();
            public List<CharacterTag> withAnyTag = new List<CharacterTag>();

            public Target Clone()
            {
                var clone = new Target
                {
                    type = type
                };

                clone.withAllTags.AddRange(withAllTags);
                clone.withAnyTag.AddRange(withAnyTag);

                return clone;
            }

            public string Description()
            {
                if (withAllTags.Count == 0 && withAnyTag.Count == 0)
                    return Localization.Localize("all");

                string str = "";
                for(int i = 0; i < withAllTags.Count; ++i)
                {
                    if (withAllTags.Count > 1 && i == 0)
                        str += "(";

                    str += TextFormat.Localize(withAllTags[i]);
                    if (i < withAllTags.Count - 1)
                        str += " ";
                    else if (withAllTags.Count > 1 && withAnyTag.Count == 0)
                        str += ")";
                }

                if (withAllTags.Count > 0 && withAnyTag.Count > 0)
                    str += " + ";

                for (int i = 0; i < withAnyTag.Count; ++i)
                {
                    if (withAllTags.Count == 0 && withAnyTag.Count > 1 && i == 0)
                        str += "(";

                    str += TextFormat.Localize(withAnyTag[i]);
                    if (i < withAnyTag.Count - 1)
                        str += " " + Localization.Localize("or") + " ";
                    else if (withAllTags.Count > 1 || withAnyTag.Count > 1)
                        str += ")";
                }

                return str;
            }
        }

        public Target applyTo;
        public Target against;
        public Stat stat;
        public float multiply = 1f;
        public int add;

        public IEffect Clone()
        {
            return new ModifyStat()
            {
                applyTo = applyTo.Clone(),
                against = against.Clone(),
                stat = stat,
                multiply = multiply,
                add = add
            };
        }

        public string Description()
        {
            string valueStr = "";

            if(multiply > 1f)
            {
                float diff = (multiply - 1f) * 100f;
                valueStr += string.Format("+{0:F0}%", diff);
                if (add != 0) valueStr += " ";
            }
            else if(multiply < 1f)
            {
                float diff = (1f - multiply) * 100f;
                valueStr += string.Format("-{0:F0}%", diff);
                if (add != 0) valueStr += " ";
            }

            if(add > 0)
            {
                valueStr += string.Format("+{0}", add);
            }
            else if(add < 0)
            {
                valueStr += string.Format("{0}", add);
            }

            if (against.withAllTags.Count > 0 || against.withAnyTag.Count > 0)
            {
                if (applyTo.type == TargetType.Targets)
                {
                    return Localization.LocalizeFormat("main", "modify_stat_against_neg_desc", valueStr, TextFormat.Localize(stat), against.Description());
                }
                else
                {
                    return Localization.LocalizeFormat("main", "modify_stat_against_desc", valueStr, TextFormat.Localize(stat), against.Description());
                }
            }
            else if (applyTo.type == TargetType.Targets)
            {
                return Localization.LocalizeFormat("main", "modify_stat_neg_desc", valueStr, TextFormat.Localize(stat));
            }
            else
            {
                return Localization.LocalizeFormat("main", "modify_stat_desc", valueStr, TextFormat.Localize(stat));
            }
        }

        public EffectResult ComputeResult(
            ICharacterData actor,
            ICharacterData target,
            List<ICharacterData> all,
            List<StatModifier> statModifiers,
            List<EffectResult> lastResults,
            IActionData nextAction,
            float multiplier)
        {
            var toModify = GetTargets(applyTo, actor, target, all);
            var modifyAgainst = GetTargets(against, actor, target, all);

            bool success = false;
            foreach(var x in toModify)
            {
                foreach(var y in modifyAgainst)
                {
                    success = true;
                    statModifiers.Add(new StatModifier(x, y, stat, multiply, add));
                }
            }

            return new EffectResult(success, typeof(ModifyStat));
        }

        private List<ICharacterData> GetTargets(Target applyTo, ICharacterData actor, ICharacterData target, List<ICharacterData> all)
        {
            var candidates = new List<ICharacterData>();
            switch (applyTo.type)
            {
                case TargetType.Actor: candidates.Add(actor);
                    break;
                case TargetType.Targets: candidates.Add(target);
                    break;
                case TargetType.All: candidates.AddRange(all);
                    break;
            }

            return candidates.Where(c => CharacterTag.HasAllTags(c, applyTo.withAllTags) && CharacterTag.HasAnyTag(c, applyTo.withAnyTag)).ToList();
        }
    }

    [Serializable]
    public class ChangeTarget : IEffect
    {
        public enum NewTarget
        {
            Wielder,
            Random
        }

        public NewTarget newTarget;
        public bool aliveOnly;

        public IEffect Clone()
        {
            return new ChangeTarget()
            {
                newTarget = newTarget,
                aliveOnly = aliveOnly
            };
        }

        public string Description()
        {
            string str = "";
            switch (newTarget)
            {
                case NewTarget.Wielder: str += Localization.Localize("change_target_desc");
                    break;

                case NewTarget.Random: str += Localization.Localize("change_target_random_desc");
                    break;
            }

            return str;
        }

        public EffectResult ComputeResult(
            ICharacterData actor,
            ICharacterData target,
            List<ICharacterData> all,
            List<StatModifier> statModifiers,
            List<EffectResult> lastResults,
            IActionData nextAction,
            float multiplier)
        {
            if(nextAction == null)
                return new EffectResult(false, typeof(ChangeTarget));

            var newAction = nextAction.Clone();
            PickTarget(actor, all, newAction);

            return new EffectResult(true, typeof(ChangeTarget)) { nextAction = newAction };
        }

        private void PickTarget(ICharacterData actor, List<ICharacterData> all, IActionData newAction)
        {
            switch (newTarget)
            {
                case NewTarget.Wielder: newAction.Targets = new List<ICharacterData> { actor };
                    break;
                case NewTarget.Random: PickRandomTarget(actor, all, newAction);
                    break;
            }
        }

        private void PickRandomTarget(ICharacterData actor, List<ICharacterData> all, IActionData newAction)
        {
            var sides = Enum.GetValues(typeof(CharacterSide));
            CharacterSide side = (CharacterSide)sides.GetValue(UnityEngine.Random.Range(0, sides.Length));

            var candidates = new List<ICharacterData>();
            if (aliveOnly)
                candidates = all.Where(c => c.HP > 0 && c.CharacterSide == side).ToList();
            else
                candidates = all.Where(c => c.CharacterSide == side).ToList();

            var newTargets = new List<ICharacterData>();
            newTargets.AddRange(newAction.Targets);

            for (int i = 0; i < newAction.Targets.Count; ++i)
            {
                if (candidates.Count == 0)
                    break;

                int index = UnityEngine.Random.Range(0, candidates.Count);
                newTargets[i] = candidates[index];

                candidates.RemoveAt(index);
            }

            newAction.Targets = newTargets;
        }
    }

    [Serializable]
    public class Cancel : IEffect
    {
        public Status status;

        public IEffect Clone()
        {
            return new Cancel { status = status };
        }
        public string Description()
        {
            return Localization.LocalizeFormat("main", "cancel_desc", Localization.Localize(status.id));
        }

        public EffectResult ComputeResult(
            ICharacterData actor,
            ICharacterData target,
            List<ICharacterData> all,
            List<StatModifier> statModifiers,
            List<EffectResult> lastResults,
            IActionData nextAction,
            float multiplier)
        {
            bool success = false;

            if (nextAction == null)
                return new EffectResult(success, typeof(Cancel));

            var newAction = nextAction.Clone();
            for(int i = 0; i < newAction.Effects.Count;)
            {
                if(status != null && newAction.Effects[i] is GiveStatus giveStatus)
                {
                    if (giveStatus.status == status)
                    {
                        newAction.Effects.RemoveAt(i);
                        success = true;
                    }
                    else ++i;
                }
                else ++i;
            }

            return new EffectResult(success, typeof(Cancel)) { nextAction = newAction };
        }
    }

    [Serializable]
    public class GiveStatus : IEffect
    {
        public EffectConfig config;
        public ElementalTag element;
        public ActiveStatus.DurationType durationType;
        public float duration;
        public Status status;
        public float percentChance = 50;

        public IEffect Clone()
        {
            return new GiveStatus
            {
                config = config,
                element = element,
                durationType = durationType,
                duration = duration,
                status = status,
                percentChance = percentChance,
            };
        }

        public string Description()
        {
            if (durationType == ActiveStatus.DurationType.TurnCount)
                duration = Mathf.RoundToInt(duration);

            if(percentChance < 100)
            {
                switch (durationType)
                {
                    case ActiveStatus.DurationType.TurnCount:
                        return Localization.LocalizeFormat("main", "give_status_turn_chance_desc", percentChance, status.Description(), duration);

                    case ActiveStatus.DurationType.Infinite:
                        return Localization.LocalizeFormat("main", "give_status_infinite_chance_desc", percentChance, status.Description());
                }
            }
            else
            {
                switch (durationType)
                {
                    case ActiveStatus.DurationType.TurnCount:
                        return Localization.LocalizeFormat("main", "give_status_turn_desc", status.Description(), duration);

                    case ActiveStatus.DurationType.Infinite:
                        return Localization.LocalizeFormat("main", "give_status_infinite_desc", status.Description());
                }
            }

            return null;
        }

        public EffectResult ComputeResult(
            ICharacterData actor,
            ICharacterData target,
            List<ICharacterData> all,
            List<StatModifier> statModifiers,
            List<EffectResult> lastResults,
            IActionData nextAction,
            float multiplier)
        {
            float modifiedChance = percentChance * multiplier;

            bool success = UnityEngine.Random.Range(0, 100) < modifiedChance;

            var result = new EffectResult(success, typeof(GiveStatus));
            if (success)
            {
                result.characters.Add(ComputeResult(actor, target, multiplier));
            }

            return result;
        }

        public CharacterResult ComputeResult(ICharacterData actor, ICharacterData target, float multiplier)
        {
            float modifiedDuration = duration * multiplier;

            if(durationType == ActiveStatus.DurationType.TurnCount)
                modifiedDuration = Mathf.RoundToInt(modifiedDuration);

            var result = new CharacterResult(target.Id, actor.Id);
            result.addStatus.Add(new ActiveStatus
            {
                durationType = durationType,
                duration = modifiedDuration,
                status = status
            });
            return result;
        }
    }

    [Serializable]
    public class RemoveStatus : IEffect
    {
        public Status status;

        public IEffect Clone()
        {
            return new RemoveStatus
            {
                status = status
            };
        }

        public string Description()
        {
            return string.Format(Localization.Localize("remove_status_desc"), Localization.Localize(status.id));
        }

        public EffectResult ComputeResult(
            ICharacterData actor,
            ICharacterData target,
            List<ICharacterData> all,
            List<StatModifier> statModifiers,
            List<EffectResult> lastResults,
            IActionData nextAction,
            float multiplier)
        {
            var result = new EffectResult(true, typeof(RemoveStatus));
            result.characters.Add(ComputeResult(actor, target));
            
            return result;
        }

        public CharacterResult ComputeResult(ICharacterData actor, ICharacterData target)
        {
            var result = new CharacterResult(target.Id, actor.Id);
            result.removeStatus.Add(status);
            return result;
        }
    }
}
