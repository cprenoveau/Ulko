using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class StopAudioStep : StepAction
    {
        public AudioDefinition sound;
        public float fadeOutDuration = 2f;

        public override IEnumerator Play()
        {
            Audio.Player.StopAll(sound, fadeOutDuration);

            yield return new WaitForSeconds(fadeOutDuration);
        }
    }
}
