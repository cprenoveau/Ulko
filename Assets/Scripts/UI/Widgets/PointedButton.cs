using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class PointedButton : Button
    {
        public MoveTween pointer;
        public FlashTween flash;
        public Image background;
        public Color bgDefaultColor = Color.black;
        public Color bgSelectedColor = Color.white;

        public bool Selected { get; private set; }
        public bool SuperSelected { get; private set; }

        public static bool SuppressSound { get; set; }

        public event Action OnSelected;
        public event Action OnDeselected;

        protected override void Start()
        {
            base.Start();

            if (Application.isPlaying)
            {
                if (!Selected)
                {
                    if(pointer != null)
                    {
                        pointer.Stop();
                        pointer.gameObject.SetActive(false);
                    }

                    if(background != null)
                        background.color = bgDefaultColor;
                }

                if (!SuperSelected)
                {
                    if (flash != null)
                        flash.Stop();
                }
            }
        }

        public void SuperSelect(bool superSelected, bool selected = true)
        {
            if (!Selected)
                return;

            SuperSelected = superSelected;

            if (SuperSelected)
            {
                if (flash != null)
                {
                    flash.Play();

                    if (pointer != null)
                        pointer.Stop();
                }
            }
            else if(selected)
            {
                SuppressSound = true;
                Select();
                SuppressSound = false;

                if(pointer != null)
                {
                    pointer.gameObject.SetActive(true);
                    pointer.Play();
                }

                if (flash != null)
                    flash.Stop();
            }
            else
            {
                Selected = false;

                if (background != null)
                    background.color = bgDefaultColor;

                if(pointer != null)
                {
                    pointer.Stop();
                    pointer.gameObject.SetActive(false);
                }

                if (flash != null)
                    flash.Stop();
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (SuperSelected)
                return;

            base.DoStateTransition(state, instant);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            if (Application.isPlaying)
            {
                if (!SuperSelected)
                {
                    Selected = true;
                    if (!SuppressSound) Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

                    if(background != null)
                        background.color = bgSelectedColor;

                    if(pointer != null)
                    {
                        pointer.gameObject.SetActive(true);
                        pointer.Play();
                    }

                    if (flash != null)
                        flash.Stop();

                    OnSelected?.Invoke();
                }
            }
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            if (Application.isPlaying)
            {
                if (SuperSelected)
                    return;

                Selected = false;

                if(background != null)
                    background.color = bgDefaultColor;

                if(pointer != null)
                {
                    pointer.Stop();
                    pointer.gameObject.SetActive(false);
                }

                if (flash != null)
                    flash.Stop();

                OnDeselected?.Invoke();
            }
        }
    }
}
