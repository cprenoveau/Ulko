using HotChocolate.UI;
using System;
using UnityEngine;
using UnityEngine.UI;
using Ulko.Data.Abilities;

namespace Ulko.UI
{
    public class NewAbilityPopupData
    {
        public AbilityAsset ability;
        public CharacterState character;
    }

    public class NewAbilityPopup : Menu
    {
        public AbilityView abilityView;
        public Button closeButton;

        private MenuStack stack;
        private NewAbilityPopupData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        private void Start()
        {
            closeButton.onClick.AddListener(Close);
        }

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as NewAbilityPopupData;

            abilityView.Init(this.data.ability, this.data.character);
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void Close()
        {
            stack.Pop();
        }
    }
}
