using HotChocolate.UI;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class LabelScreenData
    {
        public string labelKey;
        public Action onClose;
    }

    public class LabelScreen : Menu
    {
        public GlyphStreamConfig glyphStreamConfig;
        public TMP_Text label;
        public Button nextButton;

        private MenuStack stack;
        private LabelScreenData data;

        private GlyphStream glyphStream;
        private int currentGlyphCount;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as LabelScreenData;

            nextButton.onClick.AddListener(Next);
            StartCoroutine(PlayAsync());
        }

        public override bool CanClose()
        {
            return false;
        }

        private void Next()
        {
            if (currentGlyphCount >= glyphStream.GlyphCount)
            {
                stack.Pop();
            }
        }

        private IEnumerator PlayAsync()
        {
            string text = Localization.Localize(data.labelKey);

            currentGlyphCount = 0;
            glyphStream = new GlyphStream(text, 0, glyphStreamConfig);

            label.text = glyphStream.GetString(glyphStream.GlyphCount);
            label.maxVisibleCharacters = 0;

            float time = 0;
            while (currentGlyphCount < glyphStream.GlyphCount)
            {
                time += Time.deltaTime;

                currentGlyphCount = glyphStream.GetGlyphCount(time);
                label.maxVisibleCharacters = currentGlyphCount;

                yield return null;
            }
        }

        protected override void _OnPop()
        {
            data.onClose?.Invoke();
        }

        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }
    }
}
