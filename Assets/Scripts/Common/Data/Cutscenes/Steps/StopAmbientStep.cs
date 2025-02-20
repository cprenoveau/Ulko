using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class StopAmbientStep : StepAction
    {
        public float fadeOutDuration = 2f;

        public override IEnumerator Play()
        {
            Audio.Player.StopAll(AudioType.Ambient, fadeOutDuration);

            yield return new WaitForSeconds(fadeOutDuration);
        }
    }
}
