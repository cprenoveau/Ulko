using Ulko.World;
using HotChocolate.UI;
using UnityEngine;

namespace Ulko.UI
{
    public class WorldHudData
    {
        public IGameState gameState;
        public WorldInstance worldInstance;
    }

    public class WorldHud : Menu
    {
        public InteractButtonView interactButtonPrefab;
        public RectTransform interactButtonParent;

        private MenuStack stack;
        private WorldHudData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as WorldHudData;
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void FixedUpdate()
        {
            if (data != null && data.worldInstance.CanInteract())
            {
                if (data.worldInstance.Player.RaycastInteractable() != null)
                {
                    ShowInteractButton(GetInteractButton(0), PlayerController.PlayerOne, data.worldInstance.Player.transform.position);
                }
                else
                {
                    HideInteractButton(GetInteractButton(0));
                }
            }
            else
            {
                foreach (Transform t in interactButtonParent)
                {
                    HideInteractButton(t.GetComponent<InteractButtonView>());
                }
            }
        }

        private InteractButtonView GetInteractButton(int index)
        {
            if (interactButtonParent.childCount <= index)
                Instantiate(interactButtonPrefab, interactButtonParent);

            return interactButtonParent.GetChild(index).GetComponent<InteractButtonView>();
        }

        private void ShowInteractButton(InteractButtonView button, PlayerController controller, Vector3 playerPos)
        {
            button.Show(controller);

            Vector2 viewportPoint = data.gameState.Camera.WorldToViewportPoint(playerPos + new Vector3(0, 1.2f, 0));
            button.GetComponent<RectTransform>().anchorMin = viewportPoint;
            button.GetComponent<RectTransform>().anchorMax = viewportPoint;
        }

        private void HideInteractButton(InteractButtonView button)
        {
            button.Hide();
        }
    }
}
