using HotChocolate.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [CreateAssetMenu(fileName = "StatusAsset", menuName = "Ulko/Abilities/Status Asset", order = 1)]
    public class StatusAsset : ScriptableObject
    {
        public enum TargetType
        {
            ActionTarget,
            ActionActor,
            Wielder
        }

        public enum ApplyType
        {
            Permanent,
            OnTurnStart,
            OnAction,
            AfterAction,
            OnDeath
        }

        public string id;
        public string customDescKey;
        public Sprite icon;
        public TargetType targetType;
        public ApplyType applyType;
        public int applyInterval;
        [Tooltip("A higher number means higher priority in order of execution")]
        public int priority;
        public VfxAsset vfxOnHead;
        public CompositeStatusCondition condition;
        public AbilitySequence onGainSequence;
        public AbilityNode node;

        public string Description()
        {
            string str = !string.IsNullOrEmpty(customDescKey) ? Localization.Localize(customDescKey) : node.Description();

            if (applyType == ApplyType.OnTurnStart)
            {
                if (applyInterval > 1)
                    str += " " + Localization.LocalizeFormat("every_turns", applyInterval);
                else
                    str += " " + Localization.Localize("every_turn");
            }
            else if (applyType == ApplyType.OnDeath)
            {
                str += " " + Localization.Localize("on_death");
            }

            return str;
        }

        private static readonly List<Instantiator> conditionInstantiators = new();
        public static IEnumerable<Instantiator> ConditionInstantiators()
        {
            conditionInstantiators.Clear();

            var conditionTypes = typeof(StatusAsset).Assembly.GetTypes().Where(t => t != typeof(StatusCondition) && typeof(StatusCondition).IsAssignableFrom(t));
            foreach (var conditionType in conditionTypes)
            {
                conditionInstantiators.Add(AbilityAsset.Create(conditionType));
            }

            return conditionInstantiators;
        }
    }
}
