using System.Collections.Generic;
using UnityEngine;

namespace Ulko
{
    public class PaletteSwap : MonoBehaviour
    {
        public TextAsset defaultPalette;
        public TextAsset newPalette;

        private List<Color> defaultColors = new List<Color>();
        private List<Color> newColors = new List<Color>();

        private void Awake()
        {
            if (newPalette == null)
                return;

            defaultColors = ParsePalette(defaultPalette);
            newColors = ParsePalette(newPalette);

            SwapColors();
        }

        private List<Color> ParsePalette(TextAsset textAsset)
        {
            var colors = new List<Color>();

            string text = textAsset.text;
            string[] hexColors = text.Split('\n');

            foreach (var hexColor in hexColors)
            {
                var fixedHexColor = hexColor.Split('\r');

                ColorUtility.TryParseHtmlString(fixedHexColor[0], out Color color);
                colors.Add(color);
            }

            return colors;
        }

        private void SwapColors()
        {
            Texture2D colorSwapTex = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
            colorSwapTex.filterMode = FilterMode.Point;

            for (int i = 0; i < newColors.Count; ++i)
            {
                colorSwapTex.SetPixel((int)(defaultColors[i].r * 255), 0, newColors[i]);
            }

            colorSwapTex.Apply();

            var renderer = GetComponent<SpriteRenderer>();
            renderer.material.SetTexture("_SwapTex", colorSwapTex);
        }
    }
}