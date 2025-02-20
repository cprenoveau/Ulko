using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Ulko
{
    [Serializable]
    public class Resolution
    {
        public string key;
        public Vector2 size;
        public bool fullscreen;
    }

    [CreateAssetMenu(fileName = "Settings", menuName = "Ulko/Settings", order = 1)]
    public class SettingsConfig : ScriptableObject
    {
        public List<Resolution> resolutions = new List<Resolution>();

        public AudioMixer masterMixer;

        [Tooltip("Exposed volume float parameter from music group.")]
        public string musicVolumeParam = "musicVolume";
        [Tooltip("Exposed volume float parameter from sound group.")]
        public string soundVolumeParam = "soundVolume";
        [Tooltip("Exposed volume float parameter from voice group.")]
        public string voiceVolumeParam = "voiceVolume";

        [Tooltip("Lowest volume in dB before music is muted.")]
        public float musicMinVolume = -60f;
        [Tooltip("Lowest volume in dB before sound is muted.")]
        public float soundMinVolume = -60f;
        [Tooltip("Lowest volume in dB before voice is muted.")]
        public float voiceMinVolume = -60f;

        [Tooltip("Highest music volume in db.")]
        public float musicMaxVolume = 0f;
        [Tooltip("Highest sound volume in db.")]
        public float soundMaxVolume = 0f;
        [Tooltip("Highest voice volume in db.")]
        public float voiceMaxVolume = 0f;

        [Range(0,100),Tooltip("Default music volume in percent.")]
        public float musicDefault = 80;
        [Range(0,100),Tooltip("Default sound volume in percent.")]
        public float soundDefault = 80;
        [Range(0, 100),Tooltip("Default voice volume in percent.")]
        public float voiceDefault = 80;

        public string DefaultResolution => resolutions[0].key;

        public Resolution Resolution(string resolutionKey) => resolutions.FirstOrDefault(r => r.key == resolutionKey);
        public bool HasResolution(string resolutionKey) => Resolution(resolutionKey) != null;

        private Vector2 nativeRes = Vector2.zero;
        public void SetResolution(string resolutionKey)
        {
            if (nativeRes == Vector2.zero)
                nativeRes = new Vector2(Screen.width, Screen.height);

            if (HasResolution(resolutionKey))
            {
                var res = Resolution(resolutionKey);
                if(res.fullscreen)
                    Screen.SetResolution((int)nativeRes.x, (int)nativeRes.y, true);
                else
                    Screen.SetResolution((int)res.size.x, (int)res.size.y, false);
            }
        }

        public void SetMusicVolume(float volume)
        {
            masterMixer.SetFloat(musicVolumeParam, volume == 0 ? -80 : musicMinVolume + volume / 100f * (musicMaxVolume - musicMinVolume));
        }

        public void SetSoundVolume(float volume)
        {
            masterMixer.SetFloat(soundVolumeParam, volume == 0 ? -80 : soundMinVolume + volume / 100f * (soundMaxVolume - soundMinVolume));
        }

        public void SetVoiceVolume(float volume)
        {
            masterMixer.SetFloat(voiceVolumeParam, volume == 0 ? -80 : voiceMinVolume + volume / 100f * (voiceMaxVolume - voiceMinVolume));
        }
    }
}
