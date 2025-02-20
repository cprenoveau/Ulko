using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Ulko
{
    public interface ILocalizationProvider
    {
        void Init();
        bool Initialized { get; }

        event Action LocaleChanged;
        string Localize(string entryKey);
        string Localize(string tableId, string entryKey);
    }

    public static class Localization
    {
        public static bool Initialized => localization != null && localization.Initialized;

        public static event Action LocaleChanged;
        public static string CurrentLocaleKey => GetCurrentLocale().Identifier.Code;

        public static string Localize(string entryKey) => localization?.Localize(entryKey);
        public static string Localize(string tableId, string entryKey) => localization?.Localize(tableId, entryKey);
        public static string LocalizeFormat(string entryKey, params object[] args) => string.Format(localization?.Localize(entryKey), args);
        public static string LocalizeFormat(string tableId, string entryKey, params object[] args) => string.Format(localization?.Localize(tableId, entryKey), args);

        public static void SetLocale(string key)
        {
            var locale = LocalizationSettings.AvailableLocales.Locales.Find(l => l.Identifier.Code == key);
            SetLocale(locale);
        }

        public static void SetLocale(Locale locale)
        {
            LocalizationSettings.SelectedLocale = locale;
        }

        public static Locale GetCurrentLocale()
        {
            return LocalizationSettings.SelectedLocale;
        }

        public static CultureInfo GetCurrentCulture()
        {
            return GetCurrentLocale().Identifier.CultureInfo;
        }

        public static List<Locale> GetLocales()
        {
            return LocalizationSettings.AvailableLocales.Locales;
        }

        private static ILocalizationProvider localization;
        private static bool init;

        public static void Init(ILocalizationProvider loc)
        {
            if (!init)
            {
                localization = loc;
                localization.LocaleChanged += () => { LocaleChanged?.Invoke(); };

                localization.Init();

                init = true;
            }
        }
    }
}
