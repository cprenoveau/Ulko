using Cinemachine;
using Ulko.World;
using Ulko.UI;
using HotChocolate.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static Ulko.Data.Encounters;

namespace Ulko
{
    public class WorldContext : Context<IGameState>
    {
        public override ContextType ContextType => ContextType.World;
        public override Camera Camera => cam;

        public WorldConfig config;

        public MenuDefinition hud;
        public MenuDefinition mainMenu;
        public MenuDefinition dialogueMenu;
        public MenuDefinition saveMenu;

        public Camera cam;
        public RawImage screenshotImage;
        public CinemachineVirtualCamera dummyCamera;
        public Player playerPrefab;
        public PlayerFollower followerPrefab;

        private WorldInstance worldInstance;

        protected override async Task _Begin(CancellationToken ct)
        {
            if (worldInstance == null)
            {
                worldInstance = WorldInstance.Create(
                    playerPrefab, followerPrefab, transform, config, CanInteract, cam, ShowScreenshot, HideScreenshot);
            }

            dummyCamera.Priority = 1000;
            dummyCamera.PreviousStateIsValid = false;
            skybox = null;

            worldInstance.Begin(Data.CurrentMilestone);

            worldInstance.OnAreaEntered += OnAreaEntered;
            worldInstance.OnShowDialogue += ShowDialogue;
            worldInstance.OnNextMilestone += StartNextMilestone;
            worldInstance.OnSaveGame += SaveGame;

            Settings.OnResolutionChanged += OnResolutionChanged;

            uiRoot.SetInfo(null);
            uiRoot.FadeAmount(1f);

            var loc = CurrentLocation();
            var area = CurrentArea();

            worldInstance.Teleport(new Vector3(loc.x, loc.y, loc.z), area);

            uiRoot.menuStack.Push(hud.asset, hud.id, new WorldHudData { gameState = Data, worldInstance = worldInstance });
            uiRoot.FadeIn(2f);

            await Task.Delay(1000, ct);
        }

        protected override async Task _End(CancellationToken ct)
        {
            worldInstance.End();

            worldInstance.OnAreaEntered -= OnAreaEntered;
            worldInstance.OnShowDialogue -= ShowDialogue;
            worldInstance.OnNextMilestone -= StartNextMilestone;
            worldInstance.OnSaveGame -= SaveGame;

            Settings.OnResolutionChanged -= OnResolutionChanged;

            uiRoot.menuStack.PopAll();
            uiRoot.FadeOut(1f);

            await Task.Delay(1000, ct);

            while (!ct.IsCancellationRequested && uiRoot.menuStack.Count > 0)
                await Task.Yield();
        }

        protected override void _Dispose()
        {
            if (worldInstance != null)
            {
                worldInstance.OnAreaEntered -= OnAreaEntered;
                worldInstance.OnShowDialogue -= ShowDialogue;
                worldInstance.OnNextMilestone -= StartNextMilestone;
                worldInstance.OnSaveGame -= SaveGame;

                worldInstance.Dispose();
            }

            Settings.OnResolutionChanged -= OnResolutionChanged;
        }

        private static Texture2D screenshot;
        private void ShowScreenshot()
        {
            HideScreenshot();

            screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            screenshotImage.texture = screenshot;
            screenshotImage.gameObject.SetActive(true);
        }

        private void HideScreenshot()
        {
            if (screenshot != null)
            {
                Destroy(screenshot);
                screenshot = null;
            }

            screenshotImage.gameObject.SetActive(false);
        }

        private Area CurrentArea()
        {
            var location = Data.CurrentLocation;
            if (!string.IsNullOrEmpty(location.area))
            {
                return Area.FindArea(location.area);
            }
            else
            {
                return Area.FindArea(Data.CurrentLevel.area.id);
            }
        }

        private Persistence.Location CurrentLocation()
        {
            var location = Data.CurrentLocation;
            if (!string.IsNullOrEmpty(location.area))
            {
                return location;
            }
            else
            {
                var area = Area.FindArea(Data.CurrentLevel.area.id);
                var spawnPoint = area.FindSpawnPoint(Data.CurrentLevel.spawnPoint.id);

                return new Persistence.Location
                {
                    x = spawnPoint.transform.position.x,
                    y = spawnPoint.transform.position.y,
                    z = spawnPoint.transform.position.z,
                    area = area.areaTag.id
                };
            }
        }

        private Material skybox;
        private void OnAreaEntered(Area area)
        {
            StopAllCoroutines();
            StartCoroutine(uiRoot.SetInfoAsync(Localization.Localize(area.areaTag.id), 2f));

            if (area.isInterior)
            {
                if (RenderSettings.skybox != null)
                {
                    skybox = RenderSettings.skybox;
                    RenderSettings.skybox = null;
                }
            }
            else if (skybox != null)
            {
                RenderSettings.skybox = skybox;
            }
        }

        private void OnResolutionChanged()
        {
            if (worldInstance.CurrentArea != null && VirtualCameraZone.CurrentZone != null)
                VirtualCameraZone.CurrentZone.Init(cam, worldInstance.Player.transform, worldInstance.CurrentArea.limits, false);
        }

        private void ShowDialogue(Data.Dialogue dialogue, Action callback)
        {
            uiRoot.menuStack.Push(dialogueMenu.asset, dialogueMenu.id, new DialogueMenuData { dialogue = dialogue, onClose = callback });
        }

        private void StartNextMilestone()
        {
            Data.StartNextMilestone(default).FireAndForgetTask();
        }

        private void SaveGame()
        {
            uiRoot.menuStack.Push(saveMenu.asset, saveMenu.id, new MenuData { gameState = Data, uiRoot = uiRoot });
        }

        protected override void _Move(Vector2 direction, float deltaTime)
        {
            if (CanInteract())
            {
                var encounter = worldInstance.Walk(new Vector3(direction.x, 0, direction.y), deltaTime);
                if (encounter != null)
                {
                    StartBattleAsync(encounter, default).FireAndForgetTask();
                }
            }
            else
            {
                worldInstance.Stand();
            }
        }

        private bool battleStarting;
        private async Task StartBattleAsync(Encounter encounter, CancellationToken ct)
        {
            battleStarting = true;

            uiRoot.menuStack.PopAll();

            await Data.StartBattle(encounter.battle, default);

            battleStarting = false;
        }

        private bool CanInteract()
        {
            return uiRoot.menuStack.Count == 1 && uiRoot.menuStack.PendingCount == 0 && !battleStarting;
        }

        protected override void _Interact()
        {
            if (CanInteract())
            {
                worldInstance.Interact();
            }
        }

        protected override void _Cancel()
        {
            if (battleStarting)
                return;

            if (uiRoot.menuStack.Count > 1 && uiRoot.menuStack.PendingCount == 0)
            {
                var topMenu = uiRoot.menuStack.Top as Menu;
                if (!topMenu.Cancel() && topMenu.CanClose())
                {
                    Audio.Player.PlayUISound(Audio.UISoundId.MenuCancel);
                    uiRoot.menuStack.Pop();
                }
            }
        }

        protected override bool _Back()
        {
            if (battleStarting)
                return false;

            if (uiRoot.menuStack.Count > 1)
            {
                var topMenu = uiRoot.menuStack.Top as Menu;
                if (topMenu.Cancel())
                {
                    return true;
                }
                else if (topMenu.CanClose() && uiRoot.menuStack.PendingCount == 0)
                {
                    Audio.Player.PlayUISound(Audio.UISoundId.MenuCancel);
                    uiRoot.menuStack.Pop();
                    return true;
                }
            }

            return false;
        }

        protected override void _OpenMenu()
        {
            if (CanInteract())
            {
                Audio.Player.PlayUISound(Audio.UISoundId.Swish);

                int menuIndex = uiRoot.menuStack.FindIndex(mainMenu.id);
                if (menuIndex != -1)
                {
                    uiRoot.menuStack.PopAllAbove(uiRoot.menuStack.Menus[menuIndex - 1].Id);
                }
                else
                {
                    uiRoot.menuStack.Push(mainMenu.asset, mainMenu.id, new MenuData() { gameState = Data, uiRoot = uiRoot });
                }
            }
        }
    }
}
