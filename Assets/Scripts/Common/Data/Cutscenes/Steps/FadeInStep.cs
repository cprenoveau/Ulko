using System;
using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class FadeInStep : StepAction
    {
        public float duration;

        public static event Action<FadeInStep> OnPlay;

        public override IEnumerator Play()
        {
            OnPlay?.Invoke(this);
            yield return new WaitForSeconds(duration);
        }
    }
}
