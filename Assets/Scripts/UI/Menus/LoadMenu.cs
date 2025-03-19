using HotChocolate.UI;
using HotChocolate.Utils;
using System;
using System.IO;
using UnityEngine;

namespace Ulko.UI
{
    public class LoadMenu : Menu
    {
        public GameFileView gameFilePrefab;
        public PointedButton newGamePrefab;
        public RectTransform gamesParent;

        private MenuStack stack;
        private MenuData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as MenuData;

            Refresh();
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu)
        {
            var superSelected = GetSuperSelected();
            if (superSelected != null)
                superSelected.SuperSelect(false);
        }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void Refresh()
        {
            foreach (Transform child in gamesParent)
            {
                HotChocolate.UI.Menu.DisposeAllAnimations(child.gameObject);
                Destroy(child.gameObject);
            }

            var newGameInstance = Instantiate(newGamePrefab, gamesParent);
            newGameInstance.onClick.AddListener(StartNewFile);

            var orderedFiles = PlayerProfile.AllFiles;

            bool selected = false;
            foreach (var (file, game) in orderedFiles)
            {
                var gameFileView = Instantiate(gameFilePrefab, gamesParent);
                gameFileView.Init(game, file.LastWriteTime);
                gameFileView.GetComponent<PointedButton>().onClick.AddListener(() =>
                {
                    gameFileView.GetComponent<PointedButton>().SuperSelect(true);
                    LoadFile(file);
                });

                if (file.Name == PlayerProfile.LoadedFile)
                {
                    selected = true;
                    Select(gameFileView.gameObject);
                }
            }

            if (!selected)
            {
                Select(newGameInstance.gameObject);
            }
        }

        private void StartNewFile()
        {
            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

            data.uiRoot.ShowMessagePrompt(UIRoot.OkCancelPrompt(
                "new_file_confirm",
                Localization.Localize("new_file_message"),
                StartNewGame));
        }

        private void StartNewGame()
        {
            Audio.Player.PlayUISound(Audio.UISoundId.NewGame);

            PlayerProfile.NewGame();
            data.gameState.StartMilestone(data.gameState.CurrentMilestone, default).FireAndForgetTask();
        }

        private void LoadFile(FileInfo file)
        {
            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

            var promptData = new PromptData()
            {
                allowClose = true
            };

            var loadButton = new PromptButtonData()
            {
                label = Localization.Localize("load"),
                style = PromptButtonStyle.Ok,
                onClick = () => { Load(file.Name); }
            };

            var deleteButton = new PromptButtonData()
            {
                label = Localization.Localize("delete"),
                style = PromptButtonStyle.Cancel,
                closeOnClick = false,
                onClick = () => { ConfirmDelete(file.Name); }
            };

            promptData.buttons.Add(loadButton);
            promptData.buttons.Add(deleteButton);

            data.uiRoot.ShowChoicePrompt("load_menu", promptData);
        }

        private void Load(string filename)
        {
            bool success = PlayerProfile.LoadGame(filename);
            if (!success)
            {
                Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

                data.uiRoot.ShowMessagePrompt(UIRoot.OkPrompt(
                    "load_error",
                    Localization.Localize("load_error_message"),
                    null));
            }
            else
            {
                Audio.Player.PlayUISound(Audio.UISoundId.LoadGame);

                data.gameState.StartMilestone(data.gameState.CurrentMilestone, default).FireAndForgetTask();
            }
        }

        private void ConfirmDelete(string filename)
        {
            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

            data.uiRoot.ShowMessagePrompt(UIRoot.OkCancelPrompt(
                "delete_confirm",
                Localization.Localize("delete_message"),
                () => { data.uiRoot.menuStack.Pop(); Delete(filename); }));
        }

        private void Delete(string filename)
        {
            bool success = PlayerProfile.DeleteGame(filename);
            if (!success)
            {
                Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

                data.uiRoot.ShowMessagePrompt(UIRoot.OkPrompt(
                    "delete_error",
                    Localization.Localize("delete_error_message"),
                    null));
            }
            else
            {
                Audio.Player.PlayUISound(Audio.UISoundId.MenuOk);

                data.uiRoot.ShowMessagePrompt(UIRoot.OkPrompt(
                    "delete_successful",
                    Localization.Localize("delete_successful_message"),
                    Refresh));
            }
        }

        private PointedButton GetSuperSelected()
        {
            foreach (Transform child in gamesParent)
            {
                var button = child.GetComponent<PointedButton>();
                if (button.SuperSelected) return button;
            }
            return null;
        }
    }
}
