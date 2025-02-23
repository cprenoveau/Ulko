using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ulko.UI
{
    [CreateAssetMenu(fileName = "DeviceUIConfig", menuName = "Ulko/UI/Device UI Config", order = 1)]
    public class DeviceUIConfig : ScriptableObject
    {
        [Serializable]
        public class ActionDefaultConfig
        {
            public string actionName;
            public Color color = Color.white;
        }

        public List<ActionDefaultConfig> actions = new();

        public ActionDefaultConfig GetActionConfig(string actionName)
        {
            return actions.FirstOrDefault(a => a.actionName == actionName);
        }

        [Serializable]
        public class DeviceButtonConfig
        {
            public string buttonName;
            public Color color = Color.white;
            public Sprite icon;
        }

        public List<DeviceButtonConfig> buttons = new();

        public DeviceButtonConfig GetButtonConfig(PlayerController controller, string actionName)
        {
            if (controller == null || controller.CurrentPlayerMap == null)
                return null;

            var action = controller.CurrentPlayerMap.GetFirstElementMapWithAction(actionName);
            if (action == null)
                return null;

            return GetButtonConfig(action.elementIdentifierName);
        }

        public DeviceButtonConfig GetButtonConfig(string buttonName)
        {
            return buttons.Find(b => b.buttonName == buttonName);
        }
    }
}
