using System;
using System.Collections;

namespace Ulko.Data.Cutscenes
{
    public class FadeOutStep : StepAction
    {
        public float duration;

        public static event Action<FadeOutStep> OnPlay;
        public bool IsPlaying { get; set; }

        public override IEnumerator Play()
        {
            OnPlay?.Invoke(this);

            while (IsPlaying)
            {
                yield return null;
            }
        }
    }
}
