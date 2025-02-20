using UnityEngine;

namespace Ulko
{
    public enum AudioType
    {
        Sound,
        Music,
        Voice,
        Ambient
    }

    [CreateAssetMenu(fileName = "AudioDefinition", menuName = "Ulko/Audio Definition", order = 1)]
    public class AudioDefinition : ScriptableObject
    {
        public AudioType audioType;
        public AudioClip clip;
        public bool loop = true;
        public float volumeMultiplier = 1f;
    }
}
