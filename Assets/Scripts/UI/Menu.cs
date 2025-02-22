using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class MenuData
    {
        public IGameState gameState;
        public UIRoot uiRoot;
    }

    public abstract class Menu : MonoBehaviour, HotChocolate.UI.IMenu
    {
        [SerializeField]
        private GameObject firstSelected;

        public void Select(GameObject go)
        {
            firstSelected = lastSelectedObject = go;

            PointedButton.SuppressSound = true;
            EventSystem.current.SetSelectedGameObject(go);
            PointedButton.SuppressSound = false;
        }

        public string Id { get; set; }
        public bool HasFocus { get; set; }

        public virtual bool SoloDisplay => false;

        public virtual HotChocolate.UI.Menu.BeforeFocusIn BeforeFocusIn => HotChocolate.UI.Menu.ActivateInstance;
        public virtual HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.DeactivateInstance;

        public virtual bool CanClose() { return true; }
        public bool Cancel()
        {
            if(_Cancel())
            {
                Audio.Player.PlayUISound(Audio.UISoundId.MenuCancel);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual bool _Cancel() { return false; }

        public void OnPush(HotChocolate.UI.MenuStack stack, object data)
        {
            _OnPush(stack, data);
            lastSelectedObject = firstSelected;
        }

        public void OnPop()
        {
            _OnPop();
        }

        private GameObject lastSelectedObject;
        private readonly List<Selectable> blockedSelectables = new();

        public void OnFocusIn(bool fromPush, string previousMenu)
        {
            foreach (var selectable in blockedSelectables)
            {
                if(selectable != null)
                    selectable.interactable = true;
            }

            if (lastSelectedObject != null && lastSelectedObject.GetComponent<PointedButton>() != null)
                lastSelectedObject.GetComponent<PointedButton>().SuperSelect(false);

            PointedButton.SuppressSound = true;
            EventSystem.current.SetSelectedGameObject(lastSelectedObject);
            PointedButton.SuppressSound = false;

            _OnFocusIn(fromPush, previousMenu);
        }

        public void OnFocusOut(bool fromPop, string nextMenu)
        {
            _OnFocusOut(fromPop, nextMenu);

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

        protected abstract void _OnPush(HotChocolate.UI.MenuStack stack, object data);
        protected abstract void _OnPop();
        protected abstract void _OnFocusIn(bool fromPush, string previousMenu);
        protected abstract void _OnFocusOut(bool fromPop, string nextMenu);
    }
}
