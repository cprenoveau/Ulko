using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class Sequence : MonoBehaviour
    {
        public List<Step> steps = new List<Step>();

        private Dictionary<int, Coroutine> playingCoroutines = new Dictionary<int, Coroutine>();

        private int index;
        public IEnumerator Play(MonoBehaviour holder, Action onComplete)
        {
            playingCoroutines.Clear();

            foreach(var step in steps)
            {
                if (step.blocking)
                {
                    yield return holder.StartCoroutine(step.Play(null));
                }
                else
                {
                    int i = index;
                    playingCoroutines.Add(i, holder.StartCoroutine(step.Play(() => { RemoveCoroutine(i); })));
                    index++;
                }
            }

            while (playingCoroutines.Count > 0)
            {
                yield return null;
            }

            onComplete?.Invoke();
        }

        private void RemoveCoroutine(int index)
        {
            playingCoroutines.Remove(index);
        }
    }
}
