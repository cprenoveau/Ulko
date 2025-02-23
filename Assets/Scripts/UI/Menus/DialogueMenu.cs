using HotChocolate.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class DialogueMenuData
    {
        public Data.Dialogue dialogue;
        public int startLineIndex = 0;
        public int endLineIndex = -1;
        public Action onClose;
    }

    public class DialogueMenu : Menu
    {
        public GlyphStreamConfig glyphStreamConfig;
        public MenuDefinition choiceMenu;

        public TMP_Text text;
        public TMP_Text speakerName;
        public Button nextButton;
        public GameObject continueWidget;
        public GameObject endWidget;
        public LayoutGroup layout;

        private MenuStack stack;
        private DialogueMenuData data;

        private GlyphStream glyphStream;
        private int currentGlyphCount;

        private SpeechBubble[] speechBubbles;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as DialogueMenuData;

            speechBubbles = FindObjectsByType<SpeechBubble>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            nextButton.onClick.AddListener(Next);
            StartCoroutine(PlayAsync());
        }

        public override bool CanClose()
        {
            return false;
        }

        private void Next()
        {
            StopAllCoroutines();

            var node = data.dialogue.GetCurrentNode();
            if (node == null)
            {
                stack.Pop();
                return;
            }

            if (currentGlyphCount < glyphStream.GlyphCount)
            {
                currentGlyphCount = glyphStream.GlyphCount;
                text.maxVisibleCharacters = currentGlyphCount;

                ShowChoices();
                ShowContinueIcon();
            }
            else if (node.rewindCount > 0)
            {
                node.Rewind();
                StartCoroutine(PlayAsync());
            }
            else if (data.endLineIndex == -1 && !node.page.IsLastLine() || node.page.CurrentLineIndex < data.endLineIndex)
            {
                node.page.SetNextLine();
                StartCoroutine(PlayAsync());
            }
            else if (node.GetBranches().Count > 0)
            {
                ShowChoices();
            }
            else if (!data.dialogue.IsLastNode())
            {
                data.dialogue.SetNextNode();
                StartCoroutine(PlayAsync());
            }
            else
            {
                stack.Pop();
            }
        }

        private void ShowChoices()
        {
            var node = data.dialogue.GetCurrentNode();
            if (node.GetBranches().Count > 0)
            {
                var choices = new List<string>();
                var branches = node.GetBranches();

                foreach (var branch in branches)
                {
                    choices.Add(branch.choice.GetText());
                }

                stack.Push(choiceMenu.asset, choiceMenu.id, new ChoiceMenuData() { choices = choices, onChosen = Choose });
            }
        }

        private void Choose(int index)
        {
            var node = data.dialogue.GetCurrentNode();
            node.SetBranch(index);

            StopAllCoroutines();
            StartCoroutine(PlayAsync());
        }

        private void ShowContinueIcon()
        {
            var node = data.dialogue.GetCurrentNode();

            if (node.GetBranches().Count > 0)
            {
                continueWidget.SetActive(false);
                endWidget.SetActive(false);
            }
            else
            {
                continueWidget.SetActive(node.rewindCount > 0 || !node.IsLastNode() || !node.page.IsLastLine() || !data.dialogue.IsLastNode());
                endWidget.SetActive(!continueWidget.activeSelf);
            }
        }

        private IEnumerator PlayAsync()
        {
            continueWidget.SetActive(false);
            endWidget.SetActive(false);

            var node = data.dialogue.GetCurrentNode();

            if (node == null)
            {
                stack.Pop();
                yield break;
            }

            while (node.page.CurrentLineIndex < data.startLineIndex)
            {
                node.page.SetNextLine();
            }

            var line = node.page.GetCurrentLine();

            foreach (SpeechBubble bubble in speechBubbles)
            {
                bubble.Hide();
            }

            if (speakerName != null)
            {
                speakerName.text = line.GetSpeakerName();

                foreach (var bubble in speechBubbles.Where(b => b.HasSpeakerId(line.speakerKey)))
                {
                    bubble.Show();
                }
            }

            currentGlyphCount = 0;
            glyphStream = new GlyphStream(line.GetText(), 0, glyphStreamConfig);

            text.text = glyphStream.GetString(glyphStream.GlyphCount);
            text.maxVisibleCharacters = 0;

            if (layout != null)
            {
                Canvas.ForceUpdateCanvases();
                layout.enabled = false;
                layout.enabled = true;
            }

            line.RaisePlayEvent();

            float time = 0;
            while (currentGlyphCount < glyphStream.GlyphCount)
            {
                time += Time.deltaTime;

                currentGlyphCount = glyphStream.GetGlyphCount(time);
                text.maxVisibleCharacters = currentGlyphCount;

                yield return null;
            }

            ShowChoices();
            ShowContinueIcon();
        }

        protected override void _OnPop()
        {
            foreach (SpeechBubble bubble in speechBubbles)
            {
                bubble.Hide();
            }

            data.onClose?.Invoke();
        }

        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }
    }
}
