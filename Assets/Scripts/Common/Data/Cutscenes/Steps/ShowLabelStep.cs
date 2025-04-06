using System;
using System.Collections;

namespace Ulko.Data.Cutscenes
{
    public class ShowLabelStep : StepAction
    {
        public string labelKey;

        public static event Action<ShowLabelStep> OnPlay;
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
