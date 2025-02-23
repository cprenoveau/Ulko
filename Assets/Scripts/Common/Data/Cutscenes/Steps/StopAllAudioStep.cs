using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class StopAllAudioStep : StepAction
    {
        public AudioType audioType = AudioType.Ambient;
        public float fadeOutDuration = 2f;

        public override IEnumerator Play()
        {
            Audio.Player.StopAll(audioType, fadeOutDuration);

            yield return new WaitForSeconds(fadeOutDuration);
        }
    }
}
