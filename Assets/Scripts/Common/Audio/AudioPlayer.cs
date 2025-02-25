using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Ulko
{
    public class AudioPlayer : MonoBehaviour, IAudioPlayer
    {
        [Serializable]
        public class MixerByType
        {
            public AudioType type;
            public AudioMixerGroup mixer;
        }

        [Serializable]
        public class UISound
        {
            public Audio.UISoundId id;
            public AudioDefinition def;
        }

        public List<MixerByType> mixers = new();
        public AudioSourceHandle audioSource;

        public List<UISound> uiSounds = new();

        private readonly Dictionary<AudioDefinition, List<AudioSourceHandle>> playingSounds = new();
        private readonly Stack<AudioSourceHandle> soundPool = new();

        public void PlayUISound(Audio.UISoundId id)
        {
            Play(uiSounds.Find(s => s.id == id).def, 1f, 0f);
        }

        public void PlaySolo(AudioDefinition audioDef, float volume = 1f, float fadeInDuration = 2f)
        {
            StopAll(audioDef.audioType);
            Play(audioDef, volume, fadeInDuration);
        }

        public void Play(AudioDefinition audioDef, float volume = 1f, float fadeInDuration = 2f)
        {
            if(audioDef.loop && playingSounds.ContainsKey(audioDef) && playingSounds[audioDef].Count > 0)
            {
                playingSounds[audioDef][0].Play(audioDef.volumeMultiplier * volume, fadeInDuration);
                return;
            }

            var sound = GetFromPool();
            sound.audioSource.outputAudioMixerGroup = mixers.Find(m => m.type == audioDef.audioType).mixer;
            sound.audioSource.loop = audioDef.loop;
            sound.audioSource.clip = audioDef.clip;
            sound.audioSource.volume = 0;

            if(!playingSounds.ContainsKey(audioDef))
                playingSounds.Add(audioDef, new List<AudioSourceHandle>());

            playingSounds[audioDef].Add(sound);
            sound.Play(audioDef.volumeMultiplier * volume, fadeInDuration);
        }

        public void StopAll(AudioDefinition audioDef, float fadeOutDuration = 2F)
        {
            foreach (var sound in playingSounds[audioDef])
            {
                sound.Stop(fadeOutDuration);
            }
        }

        public void StopAll(AudioType type, float fadeOutDuration = 2F)
        {
            foreach (var sounds in playingSounds)
            {
                if (sounds.Key.audioType == type)
                {
                    StopAll(sounds.Key, fadeOutDuration);
                }
            }
        }

        private void Update()
        {
            foreach(var playingSound in playingSounds)
            {
                for(int i = 0; i < playingSound.Value.Count;)
                {
                    if (Application.isFocused && !playingSound.Value[i].audioSource.isPlaying)
                        ReturnToPool(playingSound.Key, i);
                    else
                        ++i;
                }
            }
        }

        private void ReturnToPool(AudioDefinition audioDef, int index)
        {
            var sounds = playingSounds[audioDef];
            var sound = sounds[index];
            sounds.RemoveAt(index);

            soundPool.Push(sound);
        }

        private AudioSourceHandle GetFromPool()
        {
            if(soundPool.Count == 0)
            {
                return Instantiate(audioSource, transform);
            }
            else
            {
                return soundPool.Pop();
            }
        }
    }
}
