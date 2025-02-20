using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class AbilitySequence
    {
        [SerializeReference]
        public List<IAbilityStep> steps = new List<IAbilityStep>();

        public IEnumerator Play(MonoBehaviour holder, ICharacter actor, List<ICharacter> targets)
        {
            foreach (var step in steps)
            {
                if (step.Blocking)
                {
                    yield return step.Play(holder, actor, targets);
                }
                else
                {
                    holder.StartCoroutine(step.Play(holder, actor, targets));
                }
            }
        }
    }
}
