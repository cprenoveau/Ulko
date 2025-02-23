using HotChocolate.UI;
using HotChocolate.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class GenericPrompt : Menu
    {
        [Serializable]
        public class PromptButtonByStyle
        {
            public PromptButtonStyle style;
            public PromptButton prefab;
        }

        public List<PromptButtonByStyle> promptButtons = new();

        public TMP_Text title;
        public TMP_Text subtitle;
        public TMP_Text message;
        public RectTransform panel;
        public GameObject empty;

        public RectTransform buttonsAnchor;
        public List<Button> closeButtons = new();

        [HideInInspector]
        public List<GameObject> buttonInstances = new();

        private MenuStack stack;
        private PromptData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        private void Awake()
        {
            foreach(Transform child in buttonsAnchor)
            {
                HotChocolate.UI.Menu.DisposeAllAnimations(child.gameObject);
                Destroy(child.gameObject);
            }
        }

        private void Start()
        {
            foreach (var button in closeButtons)
            {
                button.onClick.AddListener(OnCloseButton);
            }
        }

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as PromptData;

            if (!this.data.allowClose)
            {
                foreach (var button in closeButtons)
                {
                    button.gameObject.SetActive(false);
                }
            }

            if (title != null) SetText(title, this.data.title);
            if (subtitle != null) SetText(subtitle, this.data.subtitle);
            if (message != null) SetText(message, this.data.message);

            if(this.data.panelScreenPosition != Vector2.zero)
                panel.anchoredPosition = Positions.ToLocalPos(this.data.panelScreenPosition, panel.parent.GetComponent<RectTransform>());

            for (int i = 0; i < this.data.buttons.Count; ++i)
            {
                var button = this.data.buttons[i];
                var instance = Instantiate(FindButtonPrefab(button), buttonsAnchor);
                buttonInstances.Add(instance);

                button.initButton?.Invoke(instance, button);

                instance.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    if(button.superSelectOnClick)
                    {
                        var pointedButton = instance.GetComponentInChildren<PointedButton>();
                        pointedButton.SuperSelect(true);
                    }

                    OnClickButton(button.onClick, button.closeOnClick);
                });
            }

            if(this.data.buttons.Count > 0)
                Select(buttonInstances[0]);

            if(empty != null)
                empty.SetActive(title == null && subtitle == null && message == null && this.data.buttons.Count == 0);
        }

        private GameObject lastSelectedObjectLocal;
        private void Update()
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if(selected != lastSelectedObjectLocal)
            {
                lastSelectedObjectLocal = selected;
                if(selected != null && selected.transform.parent == buttonsAnchor)
                {
                    int index = selected.transform.GetSiblingIndex();
                    data.buttons[index].onSelect?.Invoke();
                }
            }
        }

        protected override void _OnPop()
        {
            data.onClose?.Invoke();
        }

        protected override void _OnFocusIn(bool fromPush, string previousMenu)
        {
            foreach(var pointedButton in GetComponentsInChildren<PointedButton>())
            {
                pointedButton.SuperSelect(false);
            }

            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.transform.parent == buttonsAnchor)
            {
                int index = selected.transform.GetSiblingIndex();
                data.buttons[index].onSelect?.Invoke();
            }
        }

        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        public override bool CanClose()
        {
            return data.allowClose;
        }

        private GameObject FindButtonPrefab(PromptButtonData data)
        {
            if (data.customPrefab != null)
                return data.customPrefab;
            else
                return FindButtonPrefab(data.style);
        }

        private GameObject FindButtonPrefab(PromptButtonStyle style)
        {
            var button = promptButtons.Find(p => p.style == style);

            if (button == null)
                return promptButtons.Find(p => p.style == PromptButtonStyle.Default).prefab.gameObject;
            else
                return button.prefab.gameObject;
        }

        private void OnClickButton(Action callback, bool close)
        {
            if(close) stack.Pop();
            callback?.Invoke();
        }

        private void OnCloseButton()
        {
            stack.Pop();
        }

        public void SetText(TMP_Text text, string message)
        {
            if (!string.IsNullOrEmpty(message))
                text.text = message;
            else
                text.gameObject.SetActive(false);
        }
    }
}
