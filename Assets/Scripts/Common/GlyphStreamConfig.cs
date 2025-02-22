using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko
{
    [CreateAssetMenu(fileName = "GlyphStream", menuName = "Ulko/Glyph Stream", order = 1)]
    public class GlyphStreamConfig : ScriptableObject
    {
        [Serializable]
        public class ColorMap
        {
            public char tag;
            public Color color;
        }

        public Color defaultColor;
        public List<ColorMap> colorMap = new();

        [Serializable]
        public class CharDuration
        {
            public char character;
            public float duration;
        }

        public float defaultCharDuration = 0.2f;
        public List<CharDuration> specialCharDuration = new();

        public float GetDuration(char c)
        {
            var special = specialCharDuration.Find(s => s.character == c);
            if (special != null)
            {
                return special.duration;
            }
            else
            {
                return defaultCharDuration;
            }
        }

        public Color GetColor(char tag)
        {
            var color = colorMap.Find(c => c.tag == tag);
            if (color != null)
                return color.color;

            return defaultColor;
        }
    }
}
