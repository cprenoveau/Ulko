using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class DeviceButton : MonoBehaviour
    {
        public bool playerOne = true;
        public DeviceUIConfig config;
        public string actionName;
        public TMP_Text keyText;
        public Image icon;
        public Image bg;

        private void Start()
        {
            if (playerOne)
            {
                Init(PlayerController.PlayerOne);
                PlayerController.OnCurrentDeviceChanged += Init;
            }
        }

        private void OnDestroy()
        {
            if (playerOne)
            {
                PlayerController.OnCurrentDeviceChanged -= Init;
            }
        }

        public void Init(PlayerController controller)
        {
            if (playerOne && controller != PlayerController.PlayerOne)
                return;

            keyText.text = "";
            icon.gameObject.SetActive(false);

            var actionConfig = config.GetActionConfig(actionName);
            if(actionConfig != null)
            {
                bg.color = actionConfig.color;
            }

            if (controller.CurrentDevice.type == Rewired.ControllerType.Keyboard)
            {
                var keyboardMap = controller.CurrentPlayerMap;
                var actionMap = keyboardMap.GetFirstElementMapWithAction(actionName);

                keyText.text = actionMap.elementIdentifierName;
            }
            else
            {
                var button = config.GetButtonConfig(controller, actionName);
                if (button != null)
                {
                    icon.gameObject.SetActive(button.icon != null);
                    icon.sprite = button.icon;
                    bg.color = button.color;
                }
            }
        }
    }
}
