using Ulko.Data;
using System;
using UnityEngine;

namespace Ulko
{
    public static class TextFormat
    {
        public static string Time(double seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            return Localization.LocalizeFormat("time_hms", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
        }

        public static string Date(DateTime date)
        {
            return date.ToString("g", Localization.GetCurrentCulture());
        }

        public static string Localize(Stat stat)
        {
            return Localization.Localize("stat_" + stat.ToString().ToLower());
        }

        public static string Localize(CharacterTag tag)
        {
            return Localization.Localize(tag.id);
        }

        public static string Localize(ElementalTag element)
        {
            string color = ColorUtility.ToHtmlStringRGB(element.color);
            return "<#" + color + ">" + Localization.Localize(element.id) + "</color>";
        }
    }
}
