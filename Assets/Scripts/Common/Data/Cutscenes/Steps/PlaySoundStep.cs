using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class PlaySoundStep : StepAction
    {
        public AudioDefinition sound;
        public bool solo;
        public float volume = 1f;
        public float fadeInDuration = 2f;

        public override IEnumerator Play()
        {
            if(solo)
                Audio.Player.PlaySolo(sound, volume, fadeInDuration);
            else
                Audio.Player.Play(sound, volume, fadeInDuration);

            yield return new WaitForSeconds(fadeInDuration);
        }
    }
}
