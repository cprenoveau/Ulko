using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data
{
    [CreateAssetMenu(fileName = "AmbientConfig", menuName = "Ulko/Audio/Ambient Config", order = 1)]
    public class AmbientConfig : ScriptableObject
    {
        [Serializable]
        public class Ambient
        {
            public AudioDefinition sound;
            public float volume = 1f;
        }

        public List<Ambient> sounds = new List<Ambient>();

        public void Play(float fadeInDuration)
        {
            foreach(var sound in sounds)
            {
                Audio.Player.Play(sound.sound, sound.volume, fadeInDuration);
            }
        }
    }
}
