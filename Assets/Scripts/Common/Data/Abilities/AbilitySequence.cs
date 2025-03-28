﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    [Serializable]
    public class AbilitySequence
    {
        [SerializeReference]
        public List<IAbilityStep> steps = new();

        public IEnumerator Play(MonoBehaviour holder, ICharacterCosmetics actor, List<ICharacterCosmetics> targets)
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
