using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class PauseStep : StepAction
    {
        public float duration;

        public override IEnumerator Play()
        {
            yield return new WaitForSeconds(duration);
        }
    }
}
