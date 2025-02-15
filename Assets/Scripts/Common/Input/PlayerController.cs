using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko
{
    public partial class PlayerController : MonoBehaviour
    {
        public static PlayerController PlayerOne => players.Count > 0 ? players[0] : null;
        public static PlayerController Player(int index) => index < players.Count ? players[index] : null;
        public static IEnumerable<PlayerController> Players => players;
        public static int PlayerCount => players.Count;

        private static readonly List<PlayerController> players = new List<PlayerController>();

        private void Awake()
        {
            Rewired.ReInput.ControllerConnectedEvent += OnControllerConnected;
            Rewired.ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
        }

        private void OnDestroy()
        {
            Rewired.ReInput.ControllerConnectedEvent -= OnControllerConnected;
            Rewired.ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
        }

        private void OnControllerConnected(Rewired.ControllerStatusChangedEventArgs obj)
        {
            Debug.Log("Controller connected: " + obj.name);
        }

        private void OnControllerDisconnected(Rewired.ControllerStatusChangedEventArgs obj)
        {
            Debug.Log("Controller disconnected: " + obj.name);
        }

        public static void DisableAllPlayersControllerMaps(bool disable, string category)
        {
            foreach (var player in players)
            {
                player.DisableControllerMaps(disable, category);
            }
        }

        public static PlayerController AddPlayer(PlayerController prefab, Transform parent, Rewired.Controller pairedDevice)
        {
            return Create(prefab, parent, pairedDevice);
        }

        public static void RemovePlayer(int playerIndex)
        {
            var player = players[playerIndex];
            players.Remove(player);

            player.Dispose();
            GameObject.Destroy(player.gameObject);
        }

        public static void RemoveAllPlayers()
        {
            while (players.Count > 0)
            {
                RemovePlayer(0);
            }
        }

        public Rewired.ControllerMap CurrentMap(string categoryId) => RewiredPlayer.controllers.maps.GetFirstMapInCategory(CurrentDevice.type, 0, categoryId);
        public Rewired.ControllerMap CurrentPlayerMap => CurrentMap("Player");

        public Rewired.Controller CurrentDevice
        {
            get
            {
                if (currentDevice == null)
                    return GetDefaultDevice();

                return currentDevice;
            }
            private set
            {
                if (value != currentDevice)
                {
                    currentDevice = value;
                    OnCurrentDeviceChanged?.Invoke(this);
                }
            }
        }
        private Rewired.Controller currentDevice;
        public static event Action<PlayerController> OnCurrentDeviceChanged;

        public Rewired.Player RewiredPlayer => Rewired.ReInput.players.GetPlayer(PlayerIndex);
        public int PlayerIndex { get; private set; }

        public static PlayerController Create(PlayerController prefab, Transform parent, Rewired.Controller pairedDevice)
        {
            var controller = Instantiate(prefab, parent);
            controller.Init(pairedDevice);

            players.Add(controller);
            return controller;
        }

        private void Init(Rewired.Controller device)
        {
            PlayerIndex = players.Count;

            if (PlayerIndex > 0)
                SwitchDevice(device);
            else
                SwitchDevice(null);

            RewiredPlayer.AddInputEventDelegate(InputEvent, Rewired.UpdateLoopType.Update);
        }

        private void Dispose()
        {
            RewiredPlayer.ClearInputEventDelegates();
        }

        public void DisableControllerMaps(bool disable, string category)
        {
            RewiredPlayer.controllers.maps.SetMapsEnabled(disable, category);
        }

        public Vector2 Move => KeyPad.Value;
        public RewiredKeyPadComposite KeyPad { get; private set; } = new RewiredKeyPadComposite();
        public bool IsFastForwarding => RewiredPlayer.GetButton("FastForward");

        public event Action OnInteract;
        public event Action OnCancel;
        public event Action OnBack;
        public event Action OnMainMenu;
        public event Action OnPause;
        public event Action OnSkip;
        public event Action OnShowNext;
        public event Action OnShowPrevious;
        public event Action OnCheats;

        private void InputEvent(Rewired.InputActionEventData data)
        {
            if (data.actionName.Contains("Move"))
            {
                KeyPad.ReadInput(data);
            }

            if (data.player.GetButtonDown(data.actionName))
            {
                switch (data.actionName)
                {
                    case "Interact": OnInteract?.Invoke(); break;
                    case "Cancel": OnCancel?.Invoke(); break;
                    case "Back": OnBack?.Invoke(); break;
                    case "MainMenu": OnMainMenu?.Invoke(); break;
                    case "Pause": OnPause?.Invoke(); break;
                    case "Skip": OnSkip?.Invoke(); break;
                    case "Next": OnShowNext?.Invoke(); break;
                    case "Previous": OnShowPrevious?.Invoke(); break;
                    case "Cheats": OnCheats?.Invoke(); break;
                }
            }

            if (data.GetCurrentInputSources().Count > 0)
            {
                var inputSource = data.GetCurrentInputSources()[0];
                if (inputSource.controller.type != Rewired.ControllerType.Mouse)
                {
                    CurrentDevice = inputSource.controller;
                }
            }
        }

        private void SwitchDevice(Rewired.Controller device)
        {
            CurrentDevice = device;

            //single player. give back all controllers to player one
            if (device == null)
            {
                foreach (var player in players)
                    player.RewiredPlayer.controllers.ClearAllControllers();

                RewiredPlayer.controllers.AddController(RewiredPlayer.controllers.Keyboard, true);

                foreach (var joystick in Rewired.ReInput.controllers.Joysticks)
                {
                    RewiredPlayer.controllers.AddController(joystick, true);
                }
            }
            else //assign controller to unique player
            {
                RewiredPlayer.controllers.ClearAllControllers();
                RewiredPlayer.controllers.AddController(device, true);
            }
        }

        private static Rewired.Controller GetDefaultDevice()
        {
            var joysticks = PlayerOne.RewiredPlayer.controllers.Joysticks;
            if (joysticks.Count > 0)
                return joysticks[0];

            return PlayerOne.RewiredPlayer.controllers.Keyboard;
        }
    }
}
