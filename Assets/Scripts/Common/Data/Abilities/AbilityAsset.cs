using HotChocolate.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class AbilityEffects
    {
        [SerializeReference]
        public List<Effect> effects = new();

        public string Description()
        {
            string str = "";
            for(int i = 0; i < effects.Count; ++i)
            {
                str += effects[i].Description();
                if (i < effects.Count - 1)
                    str += ",";
            }
            return str;
        }
    }

    [Serializable]
    public class AbilityNode
    {
        public bool forceValidTarget;
        public AbilitySequence applySequence;
        public AbilityEffects effects;

        public bool HasEffectOfType(Effect.EffectType effectType)
        {
            return effects.effects.Find(e => e.Type == effectType) != null;
        }

        public string Description()
        {
            return effects.Description();
        }
    }

    [Serializable]
    public class AbilityTarget
    {
        public enum TargetType
        {
            Enemies,
            Allies
        }

        public enum TargetSize
        {
            One,
            Group
        }

        public enum UsabilityScope
        {
            Everywhere,
            BattleOnly,
            WorldOnly
        }

        public UsabilityScope usabilityScope;
        public TargetType targetType;
        public TargetSize targetSize;
        public TargetConditionAsset targetCondition;

        public bool IsValidTarget(CharacterState actor, CharacterState target)
        {
            if (targetCondition == null)
                return true;

            return targetCondition.IsTrue(actor, target);
        }

        public bool HasCondition(Type conditionType)
        {
            if (targetCondition == null)
                return false;

            return targetCondition.HasCondition(conditionType);
        }
    }

    [CreateAssetMenu(fileName = "AbilityAsset", menuName = "Ulko/Abilities/Ability Asset", order = 1)]
    public class AbilityAsset : ScriptableObject
    {
        public string id;
        public AbilityTarget target;
        public Stat mainStat;

        [SerializeReference]
        public List<AbilityNode> nodes = new();

        public string Description()
        {
            if (nodes.Count == 0)
            {
                return Localization.Localize("nothing_desc");
            }

            string str = "";

            for(int i = 0; i < nodes.Count; ++i)
            {
                string desc = nodes[i].Description();

                str += desc;
                if (i < nodes.Count - 1)
                {
                    int descCount = 1;
                    int nextNode = i + 1;

                    while (nextNode < nodes.Count && nodes[nextNode].Description() == desc)
                    {
                        nextNode++;
                        descCount++;
                        i++;
                    }

                    if(descCount > 1)
                    {
                        str += string.Format(" x{0}", descCount);
                    }

                    if (i < nodes.Count - 1)
                    {
                        str += ", ";
                    }
                }
            }

            str += " " + Localization.Localize("size_"+target.targetSize.ToString().ToLower());

            return str;
        }

        private static readonly List<Instantiator> conditionInstantiators = new();
        public static IEnumerable<Instantiator> ConditionInstantiators()
        {
            conditionInstantiators.Clear();

            var conditionTypes = typeof(AbilityAsset).Assembly.GetTypes().Where(t => t != typeof(TargetCondition) && typeof(TargetCondition).IsAssignableFrom(t));
            foreach (var conditionType in conditionTypes)
            {
                conditionInstantiators.Add(Create(conditionType));
            }

            return conditionInstantiators;
        }

        private static readonly List<Instantiator> stepInstantiators = new();
        public static IEnumerable<Instantiator> StepInstantiators()
        {
            stepInstantiators.Clear();

            var stepTypes = typeof(AbilityAsset).Assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IAbilityStep)));
            foreach (var stepType in stepTypes)
            {
                stepInstantiators.Add(Create(stepType));
            }

            return stepInstantiators;
        }

        private static readonly List<Instantiator> effectInstantiators = new();
        public static IEnumerable<Instantiator> EffectInstantiators()
        {
            effectInstantiators.Clear();

            var effectTypes = typeof(AbilityAsset).Assembly.GetTypes().Where(t => t != typeof(Effect) && typeof(Effect).IsAssignableFrom(t));
            foreach (var effectType in effectTypes)
            {
                effectInstantiators.Add(Create(effectType));
            }

            return effectInstantiators;
        }

        public static Instantiator Create(Type type)
        {
            return new Instantiator()
            {
                displayName = type.Name,
                type = type,
                Instantiate = () => { return Activator.CreateInstance(type); }
            };
        }
    }
}
