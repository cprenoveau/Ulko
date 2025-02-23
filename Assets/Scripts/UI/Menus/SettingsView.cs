using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class SettingsView : MonoBehaviour
    {
        public SettingsConfig config;

        public ComboBox languageBox;
        public TMP_Text languageText;
        public ComboBox resolutionBox;
        public TMP_Text resolutionText;
        public Slider musicSlider;
        public TMP_Text musicLabel;
        public Slider soundSlider;
        public TMP_Text soundLabel;
        public Slider voiceSlider;
        public TMP_Text voiceLabel;

        private void Start()
        {
            Localization.LocaleChanged += Refresh;
            languageBox.OnValueChanged += SetLanguage;
            resolutionBox.OnValueChanged += SetResolution;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
            languageBox.OnValueChanged -= SetLanguage;
            resolutionBox.OnValueChanged -= SetResolution;
        }

        public void Init()
        {
            var locales = Localization.GetLocales();
            languageBox.itemCount = locales.Count;
            languageBox.Index = locales.FindIndex(l => l.Identifier.Code == Localization.CurrentLocaleKey);

            resolutionBox.itemCount = config.resolutions.Count;
            resolutionBox.Index = config.resolutions.FindIndex(r => r.key == Settings.ResolutionKey);

            musicSlider.value = Settings.MusicVolume;
            soundSlider.value = Settings.SoundVolume;
            voiceSlider.value = Settings.VoiceVolume;

            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            soundSlider.onValueChanged.AddListener(SetSoundVolume);
            voiceSlider.onValueChanged.AddListener(SetVoiceVolume);

            Refresh();
        }

        public void Refresh()
        {
            languageText.text = Localization.Localize("locale_" + Localization.CurrentLocaleKey);

            var res = config.Resolution(Settings.ResolutionKey);
            if (res.fullscreen)
            {
                resolutionText.text = Localization.Localize("fullscreen");
            }
            else
            {
                resolutionText.text = res.size.x + "x" + res.size.y;
            }

            musicLabel.text = Localization.LocalizeFormat("music", musicSlider.value);
            soundLabel.text = Localization.LocalizeFormat("sound", soundSlider.value);
            voiceLabel.text = Localization.LocalizeFormat("voice", voiceSlider.value);
        }

        private void SetMusicVolume(float volume)
        {
            Settings.MusicVolume = volume;
            Refresh();

            Settings.Save();
        }

        private void SetSoundVolume(float volume)
        {
            Settings.SoundVolume = volume;
            Refresh();

            Settings.Save();
        }

        private void SetVoiceVolume(float volume)
        {
            Settings.VoiceVolume = volume;
            Refresh();

            Settings.Save();
        }

        private void SetLanguage(int index)
        {
            var locales = Localization.GetLocales();
            Localization.SetLocale(locales[index]);

            Settings.Save();
        }

        private void SetResolution(int index)
        {
            Settings.ResolutionKey = config.resolutions[index].key;
            Refresh();

            Settings.Save();
        }
    }
}
