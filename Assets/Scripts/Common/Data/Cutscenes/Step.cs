using System;
using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    [Serializable]
    public class Step
    {
        public bool blocking = true;
        public float delay;
        public StepAction action;

        public IEnumerator Play(Action onComplete)
        {
            yield return new WaitForSeconds(delay);
            yield return action.Play();

            onComplete?.Invoke();
        }
    }

    public abstract class StepAction : MonoBehaviour
    {
        public abstract IEnumerator Play();
    }
}
