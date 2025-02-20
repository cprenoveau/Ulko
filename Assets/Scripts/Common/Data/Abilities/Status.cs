using HotChocolate.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [CreateAssetMenu(fileName = "Status", menuName = "Ulko/Abilities/Status", order = 1)]
    public class Status : ScriptableObject
    {
        public enum TargetType
        {
            ActionTarget,
            ActionActor,
            Wielder
        }

        public enum ApplyType
        {
            OnAction,
            OverTime,
            OnTurnStart,
            OnDeath,
            AfterAction
        }

        public string id;
        public string customDescKey;
        public Sprite icon;
        public TargetType targetType;
        public ApplyType applyType;
        public float applyInterval;
        [Tooltip("This status can be applied multiple times")]
        public bool allowStacking;
        [Tooltip("A higher number means higher priority in order of execution")]
        public int priority;
        public VfxAsset vfxOnHead;
        public CompositeStatusCondition condition;
        public AbilitySequence onGainSequence;
        public AbilityNode node;

        public string Description()
        {
            string str = !string.IsNullOrEmpty(customDescKey) ? Localization.Localize(customDescKey) : node.Description();

            if(applyType == ApplyType.OverTime)
            {
                if (applyInterval > 1)
                    str += " " + Localization.LocalizeFormat("every_seconds", applyInterval);
                else
                    str += " " + Localization.Localize("every_second");
            }
            else if(applyType == ApplyType.OnTurnStart)
            {
                if (applyInterval > 1)
                    str += " " + Localization.LocalizeFormat("every_turns", applyInterval);
                else
                    str += " " + Localization.Localize("every_turn");
            }
            else if(applyType == ApplyType.OnDeath)
            {
                str += " " + Localization.Localize("on_death");
            }

            var random = condition.conditions.FirstOrDefault(c => c is RandomRoll);
            if (random != null)
                str += " (" + Localization.LocalizeFormat("random_roll", (random as RandomRoll).percentChance) + ")";

            return str;
        }

        private static List<Instantiator> conditionInstantiators = new List<Instantiator>();
        public static IEnumerable<Instantiator> ConditionInstantiators()
        {
            conditionInstantiators.Clear();

            var conditionTypes = typeof(Ability).Assembly.GetTypes().Where(t => t != typeof(StatusCondition) && typeof(StatusCondition).IsAssignableFrom(t));
            foreach (var conditionType in conditionTypes)
            {
                conditionInstantiators.Add(Ability.Create(conditionType));
            }

            return conditionInstantiators;
        }
    }
}
