using System;
using System.Collections.Generic;
using System.Linq;
using Ulko.UI;
using UnityEngine;

namespace Ulko
{
    public partial class GameInstance
    {
        private void BindAllInputs(PlayerController controller)
        {
            UnbindAllInputs(controller);

            controller.OnInteract += Interact;
            controller.OnCancel += Cancel;
            controller.OnBack += Back;
            controller.OnMainMenu += OpenMainMenu;
            controller.OnPause += TogglePause;
            controller.OnShowNext += ShowNext;
            controller.OnShowPrevious += ShowPrevious;
            controller.OnCheats += ToggleCheats;
        }

        private void UnbindAllInputs(PlayerController controller)
        {
            controller.OnInteract -= Interact;
            controller.OnCancel -= Cancel;
            controller.OnBack -= Back;
            controller.OnMainMenu -= OpenMainMenu;
            controller.OnPause -= TogglePause;
            controller.OnShowNext -= ShowNext;
            controller.OnShowPrevious -= ShowPrevious;
            controller.OnCheats -= ToggleCheats;
        }

        private void BindMenuInput(PlayerController controller)
        {
            UnbindMenuInput(controller);

            controller.OnMainMenu += OpenMainMenu;
        }

        private void UnbindMenuInput(PlayerController controller)
        {
            controller.OnMainMenu -= OpenMainMenu;
        }

        private readonly List<Rewired.Controller> backseatPlayers = new();

        public int BackseatPlayerCount()
        {
            return backseatPlayers.Count;
        }

        public AddBackseatPlayerResult AddBackseatPlayer()
        {
            var device = PlayerController.PlayerOne.CurrentDevice;

            if (!backseatPlayers.Contains(device))
            {
                backseatPlayers.Add(device);

                //no more devices left for player 1: this is not allowed
                if (FirstUnusedDevice() == null)
                {
                    backseatPlayers.Remove(device);
                    return AddBackseatPlayerResult.NoMoreController;
                }

                return AddBackseatPlayerResult.Success;
            }

            return AddBackseatPlayerResult.PlayerAlreadyJoined;
        }

        public bool RemoveBackseatPlayer(int playerIndex)
        {
            if(backseatPlayers.Count > playerIndex)
            {
                backseatPlayers.RemoveAt(playerIndex);
                return true;
            }

            return false;
        }

        public event Action OnPlayerControllersChanged;

        public void MakeSinglePlayer()
        {
            if (PlayerController.PlayerCount == 0)
            {
                PlayerController.AddPlayer(playerControllerPrefab, transform, null);
                BindAllInputs(PlayerController.PlayerOne);
            }
            else if (PlayerController.PlayerCount > 1)
            {
                UnbindAllInputs(PlayerController.PlayerOne);

                var currentDevice = PlayerController.PlayerOne.CurrentDevice;

                PlayerController.RemoveAllPlayers();

                PlayerController.AddPlayer(playerControllerPrefab, transform, currentDevice);
                BindAllInputs(PlayerController.PlayerOne);
            }

            OnPlayerControllersChanged?.Invoke();
        }

        public void MakeMultiPlayer()
        {
            if (backseatPlayers.Count > 0)
            {
                List<Rewired.Controller> devices = new List<Rewired.Controller>();

                var playerOneDevice = FirstUnusedDevice();
                if (playerOneDevice != null)
                    devices.Add(playerOneDevice);

                foreach (var device in backseatPlayers)
                {
                    if (!devices.Contains(device))
                        devices.Add(device);
                }

                UnbindAllInputs(PlayerController.PlayerOne);
                PlayerController.RemoveAllPlayers();

                foreach (var device in devices)
                {
                    var controller = PlayerController.AddPlayer(playerControllerPrefab, transform, device);
                    BindMenuInput(controller);
                }

                BindAllInputs(PlayerController.PlayerOne);
            }
            else
            {
                MakeSinglePlayer();
            }

            OnPlayerControllersChanged?.Invoke();
        }

        private Rewired.Controller FirstUnusedDevice()
        {
            var devices = new List<Rewired.Controller>
            {
                Rewired.ReInput.controllers.Keyboard
            };

            devices.AddRange(Rewired.ReInput.controllers.Joysticks);

            foreach (var device in devices)
            {
                if (backseatPlayers.FirstOrDefault(d => d == device) == null)
                    return device;
            }

            return null;
        }

        private void ToggleCheats()
        {
            OnToggleCheats?.Invoke();
        }

        public void OnCheatOpen()
        {
            if (CurrentContext != ContextType.None)
                contexts[CurrentContext].Pause();
        }

        public void OnCheatClosed()
        {
            if (CurrentContext != ContextType.None)
                contexts[CurrentContext].Resume();
        }

        private void Interact()
        {
            if (CurrentContext != ContextType.None)
                contexts[CurrentContext].Interact();
        }

        private void Cancel()
        {
            if (uiRoot.menuStack.Find(pauseMenu.id) != null)
            {
                TogglePause();
            }
            else if (uiRoot.menuStack.Find(saveMenu.id) != null)
            {
                var topMenu = uiRoot.menuStack.Top as Menu;
                if (!topMenu.Cancel() && topMenu.CanClose())
                {
                    Audio.Player.PlayUISound(Audio.UISoundId.MenuCancel);
                    uiRoot.menuStack.Pop();
                }
            }
            else
            {
                if (CurrentContext != ContextType.None)
                    contexts[CurrentContext].Cancel();
            }
        }

        private void Back()
        {
            if (uiRoot.menuStack.Find(pauseMenu.id) != null)
            {
                TogglePause();
            }
            else
            {
                if (CurrentContext != ContextType.None)
                {
                    bool success = contexts[CurrentContext].Back();
                    if (!success) TogglePause();
                }
            }
        }

        private void OpenMainMenu()
        {
            if (CurrentContext != ContextType.None)
                contexts[CurrentContext].OpenMenu();
        }

        private void ShowNext()
        {
            OnShowNext?.Invoke();
        }

        private void ShowPrevious()
        {
            OnShowPrevious?.Invoke();
        }

        private void TogglePause()
        {
            if (CurrentContext == ContextType.None)
                return;

            if (uiRoot.menuStack.Find(pauseMenu.id) != null)
            {
                Time.timeScale = 1f;
                uiRoot.menuStack.PopAll();

                if (CurrentContext != ContextType.None)
                    contexts[CurrentContext].Resume();
            }
            else
            {
                uiRoot.menuStack.Push(pauseMenu.asset, pauseMenu.id, new MenuData() { gameState = this, uiRoot = uiRoot });

                if (CurrentContext != ContextType.None)
                    contexts[CurrentContext].Pause();

                Time.timeScale = 0f;
            }
        }
    }
}
