using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ulko.Data;
using UnityEngine;

namespace Ulko.UI
{
    public class CardView : MonoBehaviour
    {
        public PointedButton button;
        public List<PointedButton> extraButtons = new();

        public event Action<CardView> OnSelected;
        public event Action<CardView> OnDeselected;
        public event Action<CardView> OnClick;

        public ICard Card { get; private set; }

        public PointedButton SelectedExtraButton => extraButtons.FirstOrDefault(b => b.Selected);

        private void Start()
        {
            button.OnSelected += OnSelect;
            button.OnDeselected += OnDeselect;

            foreach (var extraButton in extraButtons)
            {
                extraButton.OnSelected += OnSelect;
                extraButton.OnDeselected += OnDeselect;
            }

            button.onClick.AddListener(() => { button.SuperSelect(true); OnClick?.Invoke(this); });

            if (button.Selected)
                OnSelect();
        }

        private void OnDestroy()
        {
            button.OnSelected -= OnSelect;
            button.OnDeselected -= OnDeselect;

            foreach (var extraButton in extraButtons)
            {
                extraButton.OnSelected -= OnSelect;
                extraButton.OnDeselected -= OnDeselect;
            }
        }

        public void Init(ICard card)
        {
            Card = card;
        }

        private void OnSelect()
        {
            StopAllCoroutines();

            foreach (var extraButton in extraButtons)
                extraButton.gameObject.SetActive(true);

            OnSelected?.Invoke(this);
        }

        private void OnDeselect()
        {
            StartCoroutine(OnDeselectDelayed());
        }

        private IEnumerator OnDeselectDelayed()
        {
            yield return null;

            foreach (var extraButton in extraButtons)
                extraButton.gameObject.SetActive(false);

            OnDeselected?.Invoke(this);
        }
    }
}
