using HotChocolate.UI;
using HotChocolate.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class PuzzleTestMenu : Menu
    {
        public Button continueButton;

        private MenuStack stack;
        private MenuData data;

        private void Start()
        {
            Localization.LocaleChanged += Refresh;
            continueButton.onClick.AddListener(Continue);
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
        }

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as MenuData;

            Refresh();
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void Refresh()
        {
        }

        private void Continue()
        {
            data.gameState.StartNextMilestone(default).FireAndForgetTask();
        }
    }
}
