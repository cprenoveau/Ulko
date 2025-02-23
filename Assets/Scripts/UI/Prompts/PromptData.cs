using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.UI
{
    public enum PromptButtonStyle
    {
        Default,
        Ok,
        Cancel,
        Confirm
    }

    public class PromptButtonData
    {
        public string label;
        public PromptButtonStyle style;
        public GameObject customPrefab;
        public Action onSelect;
        public Action onClick;
        public bool closeOnClick = true;
        public bool superSelectOnClick = false;

        public delegate void InitButton(GameObject instance, PromptButtonData data);
        public InitButton initButton;

        public PromptButtonData()
        {
            initButton = DefaultInitButton;
        }

        private void DefaultInitButton(GameObject instance, PromptButtonData data)
        {
            var promptButton = instance.GetComponent<PromptButton>();
            promptButton.label.text = data.label;
        }
    }

    public class PromptData
    {
        public string title;
        public string subtitle;
        public string message;
        public Vector2 panelScreenPosition;
        public bool allowClose;
        public Action onClose;
        public List<PromptButtonData> buttons = new();
    }
}
