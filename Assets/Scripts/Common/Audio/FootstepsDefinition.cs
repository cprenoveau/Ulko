using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko
{
    [CreateAssetMenu(fileName = "FootstepsDefinition", menuName = "Ulko/Footsteps Definition", order = 1)]
    public class FootstepsDefinition : ScriptableObject
    {
        [Serializable]
        public class FootstepSurface
        {
            public Material material;
            public float minVolume = 0.5f;
            public float maxVolume = 1f;
            public List<AudioClip> sounds = new List<AudioClip>();
        }

        public FootstepSurface defaultFootstep;
        public List<FootstepSurface> footsteps = new List<FootstepSurface>();

        public (AudioClip clip, float volume) GetFootstep(Material mat, int stepCount)
        {
            var surface = footsteps.Find(s => s.material != null && s.material.color == mat.color && s.material.mainTexture == mat.mainTexture);
            if (surface == null)
            {
                surface = defaultFootstep;
            }

            int stepIndex = stepCount % surface.sounds.Count;

            var clip = surface.sounds[stepIndex];
            var volume = UnityEngine.Random.Range(surface.minVolume, surface.maxVolume);

            return (clip, volume);
        }
    }
}
