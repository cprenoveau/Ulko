using UnityEngine;

namespace Ulko.UI
{
    public class InteractButtonView : MonoBehaviour
    {
        public CanvasGroup canvasGroup;
        public DeviceButton deviceButton;

        public void Show(PlayerController controller)
        {
            canvasGroup.alpha = 1f;

            deviceButton.playerOne = controller == PlayerController.PlayerOne;
            deviceButton.Init(controller);
        }

        public void Hide()
        {
            canvasGroup.alpha = 0f;
        }
    }
}
