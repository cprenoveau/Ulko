using HotChocolate.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ulko
{
    public partial class UIRoot : MonoBehaviour
    {
        public bool allowMouseInputs = false;

        public MenuStack menuStack;
        public AssetReference messagePrompt;
        public AssetReference choicePrompt;

        public CanvasGroup fade;
        public CanvasGroup infoView;
        public TMP_Text infoText;
        public GameObject fastForward;

        public GameObject inputBlocker;
        private int inputBlockCount;

        public void Init()
        {
            Menu.UseAddressables(menuStack);
            SetInfo(null);
            fastForward.SetActive(false);

            inputBlocker.SetActive(inputBlockCount > 0 || !allowMouseInputs);

            menuStack.OnBlockInput -= DisableUISubmit;
            menuStack.OnBlockInput += DisableUISubmit;
        }

        private static void DisableUISubmit(bool disable)
        {
            PlayerController.DisableUISubmit(disable, "MenuStack");
        }

        public readonly struct BlockInputScope : IDisposable
        {
            private readonly UIRoot uiRoot;

            public BlockInputScope(UIRoot uiRoot)
            {
                this.uiRoot = uiRoot;
                uiRoot.BlockInput(false);
            }

            public void Dispose()
            {
                uiRoot.UnblockInput(false);
            }
        }

        private GameObject lastSelectedObject;
        private readonly List<Selectable> blockedSelectables = new();

        public void BlockInput(bool releaseAllOtherBlockers)
        {
            inputBlocker.SetActive(true);

            if (inputBlockCount == 0)
            {
                lastSelectedObject = EventSystem.current.currentSelectedGameObject;

                blockedSelectables.Clear();

                var selectables = transform.GetComponentsInChildren<Selectable>();
                foreach (var selectable in selectables)
                {
                    if (selectable.interactable)
                    {
                        selectable.interactable = false;
                        blockedSelectables.Add(selectable);
                    }
                }
            }

            inputBlockCount++;
            if (releaseAllOtherBlockers) inputBlockCount = 1;
        }

        public void UnblockInput(bool releaseAllBlockers)
        {
            inputBlockCount--;
            if (releaseAllBlockers) inputBlockCount = 0;

            if (inputBlockCount <= 0)
            {
                inputBlocker.SetActive(!allowMouseInputs);
                inputBlockCount = 0;

                foreach (var selectable in blockedSelectables)
                {
                    if (selectable != null)
                        selectable.interactable = true;
                }

                EventSystem.current.SetSelectedGameObject(lastSelectedObject);
            }
        }

        public IEnumerator SetInfoAsync(string text, float duration)
        {
            SetInfo(text, infoView.alpha);

            var anim = new HotChocolate.Motion.ClipSequence();

            var fadeIn = new HotChocolate.Motion.Tween<float>(0.2f, infoView.alpha, 1f, Mathf.Lerp, HotChocolate.Motion.Easing.Linear);
            fadeIn.OnUpdate += UpdateInfoFade;

            var silence = new HotChocolate.Motion.Silence(duration - 0.4f);

            var fadeOut = new HotChocolate.Motion.Tween<float>(0.2f, 1f, 0f, Mathf.Lerp, HotChocolate.Motion.Easing.Linear);
            fadeOut.OnUpdate += UpdateInfoFade;

            anim.Append(fadeIn);
            anim.Append(silence);
            anim.Append(fadeOut);

            while (anim.Play(Time.unscaledDeltaTime))
            {
                yield return null;
            }
        }

        private void UpdateInfoFade(float alpha, float progress)
        {
            infoView.alpha = alpha;
        }

        public void SetInfo(string text, float alpha = 1f)
        {
            if (string.IsNullOrEmpty(text))
            {
                infoView.alpha = 0;
            }
            else
            {
                infoView.alpha = alpha;
                infoText.text = text;
            }
        }

        public void FadeAmount(float fade)
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }

            this.fade.alpha = fade;
        }

        private Coroutine fadeRoutine;
        public void FadeIn(float duration)
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(Fade(duration, fade.alpha, 0f));
        }

        public void FadeOut(float duration)
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(Fade(duration, fade.alpha, 1f));
        }

        private IEnumerator Fade(float duration, float from, float to)
        {
            var anim = new HotChocolate.Motion.Tween<float>(duration, from, to, Mathf.Lerp, HotChocolate.Motion.Easing.Linear);
            anim.OnUpdate += UpdateFade;

            while (anim.Play(Time.unscaledDeltaTime))
            {
                yield return null;
            }

            UpdateFade(to, 1f);
        }

        private void UpdateFade(float alpha, float progress)
        {
            fade.alpha = alpha;
        }
    }
}
