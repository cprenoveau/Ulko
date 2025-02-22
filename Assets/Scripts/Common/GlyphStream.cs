using UnityEngine;
using System.Collections.Generic;

namespace Ulko
{
    public class GlyphStream
    {
        private class Glyph
        {
            public string character;
            public float duration;
        }

        private readonly GlyphStreamConfig config;
        private readonly List<Glyph> glyphs = new();

        public int GlyphCount => glyphs.Count;

        public GlyphStream(string text, float totalDuration, GlyphStreamConfig config)
        {
            this.config = config;

            Stack<Color> colors = new();
            colors.Push(config.defaultColor);

            bool isItalic = false;
            bool isBold = false;

            float duration = 0;
            for (int i = 0; i < text.Length;)
            {
                if (text[i] == '<')
                {
                    i = ProcessTag(ref text, ref isItalic, ref isBold, ref colors, i);
                }
                else
                {
                    var newGlyph = CreateGlyph(text[i], isItalic, isBold, colors);
                    duration += newGlyph.duration;

                    glyphs.Add(newGlyph);
                    i++;
                }
            }

            if (totalDuration != 0)
            {
                float multiplier = totalDuration / duration;
                foreach (var glyph in glyphs)
                {
                    glyph.duration *= multiplier;
                }
            }
        }

        public int GetGlyphCount(float time)
        {
            float glyphDuration = 0;
            int glyphLength = 0;

            while (glyphDuration < time && glyphLength < glyphs.Count)
            {
                glyphDuration += glyphs[glyphLength].duration;
                glyphLength++;
            }

            return glyphLength;
        }

        public string GetString(int glyphCount)
        {
            string str = "";
            for (int i = 0; i < glyphCount; ++i)
            {
                str += glyphs[i].character;
            }

            return str;
        }

        private Glyph CreateGlyph(char c, bool isItalic, bool isBold, Stack<Color> colors)
        {
            string character = " ";
            if (c != ' ')
            {
                character = "<color=#" + ColorUtility.ToHtmlStringRGB(colors.Peek()) + ">" + c + "</color>";

                if (isItalic)
                    character = "<i>" + character + "</i>";

                if (isBold)
                    character = "<b>" + character + "</b>";
            }

            return new Glyph { character = character, duration = config.GetDuration(c) };
        }

        private int ProcessTag(ref string text, ref bool isItalic, ref bool isBold, ref Stack<Color> colors, int i)
        {
            if (text[i + 1] == '/')
            {
                if (text[i + 2] == 'i')
                {
                    isItalic = false;
                }
                else if (text[i + 2] == 'b')
                {
                    isBold = false;
                }
                else
                {
                    colors.Pop();
                }
                i += 4;
            }
            else if (text[i + 1] == 'i')
            {
                isItalic = true;
                i += 3;
            }
            else if (text[i + 1] == 'b')
            {
                isBold = true;
                i += 3;
            }
            else
            {
                var color = config.GetColor(text[i + 1]);
                colors.Push(color);
                i += 3;
            }

            return i;
        }
    }
}
