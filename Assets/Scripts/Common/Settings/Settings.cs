using UnityEngine;

namespace Ulko
{
    public static class Settings
    {
        public static SettingsConfig Config { get; private set; }

        public static float MusicVolume
        {
            get { return musicVolume; }
            set
            {
                musicVolume = value;
                Config.SetMusicVolume(musicVolume);
            }
        }
        private static float musicVolume;

        public static float SoundVolume
        {
            get { return soundVolume; }
            set
            {
                soundVolume = value;
                Config.SetSoundVolume(soundVolume);
            }
        }
        private static float soundVolume;

        public static float VoiceVolume
        {
            get { return voiceVolume; }
            set
            {
                voiceVolume = value;
                Config.SetVoiceVolume(voiceVolume);
            }
        }
        private static float voiceVolume;

        public delegate void ResolutionChanged();
        public static ResolutionChanged OnResolutionChanged;

        public static string ResolutionKey
        {
            get { return resolutionKey; }
            set
            {
                if (Config.HasResolution(value))
                {
                    resolutionKey = value;
                    Config.SetResolution(resolutionKey);
                    OnResolutionChanged?.Invoke();
                }
            }
        }
        private static string resolutionKey;
        public static Resolution Resolution => Config.Resolution(resolutionKey);

        public static void Init(SettingsConfig config)
        {
            Config = config;
            Load();
        }

        public static void Load()
        {
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("language")))
            {
                Reset();
                Save();
            }

            Localization.SetLocale(PlayerPrefs.GetString("language"));
            ResolutionKey = PlayerPrefs.GetString("resolution");
            MusicVolume = PlayerPrefs.GetFloat("music");
            SoundVolume = PlayerPrefs.GetFloat("sound");
            VoiceVolume = PlayerPrefs.GetFloat("voice");
        }

        public static void Save()
        {
            PlayerPrefs.SetString("language", Localization.CurrentLocaleKey);
            PlayerPrefs.SetString("resolution", resolutionKey);
            PlayerPrefs.SetFloat("music", MusicVolume);
            PlayerPrefs.SetFloat("sound", SoundVolume);
            PlayerPrefs.SetFloat("voice", VoiceVolume);

            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            ResolutionKey = Config.DefaultResolution;
            MusicVolume = Config.musicDefault;
            SoundVolume = Config.soundDefault;
            VoiceVolume = Config.voiceDefault;
        }
    }
}
