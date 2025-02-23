using HotChocolate.UI;
using HotChocolate.UI.MenuAnimations;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Ulko.UI
{
    public class ChoiceMenuData
    {
        public List<string> choices = new();
        public Action<int> onChosen;
    }

    public class ChoiceMenu : Menu
    {
        public RectTransform choiceParent;
        public PointedButton choicePrefab;

        private MenuStack stack;
        private ChoiceMenuData data;

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as ChoiceMenuData;

            foreach (Transform child in choiceParent)
            {
                HotChocolate.UI.Menu.DisposeAllAnimations(child.gameObject);
                Destroy(child.gameObject);
            }

            for (int i = 0; i < this.data.choices.Count; ++i)
            {
                int index = i;

                var instance = Instantiate(choicePrefab, choiceParent);
                instance.GetComponentInChildren<TMP_Text>().text = this.data.choices[i];
                instance.GetComponentInChildren<BouncyPanel>().pushDelay = (i + 1) * 0.15f;
                instance.onClick.AddListener(() => { Choose(index); });

                if (i == 0) Select(instance.gameObject);
            }
        }

        private void Choose(int index)
        {
            data.onChosen?.Invoke(index);
            stack.Pop();
        }

        public override bool CanClose()
        {
            return false;
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }
    }
}
