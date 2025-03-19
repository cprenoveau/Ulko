using HotChocolate.UI;
using System;
using System.IO;
using UnityEngine;

namespace Ulko.UI
{
    public class SaveMenu : Menu
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
            newGameInstance.onClick.AddListener(SaveToNewFile);

            var orderedFiles = PlayerProfile.AllFiles;

            bool selected = false;
            foreach (var (file, game) in orderedFiles)
            {
                var gameFileView = Instantiate(gameFilePrefab, gamesParent);
                gameFileView.Init(game, file.LastWriteTime);
                gameFileView.GetComponent<PointedButton>().onClick.AddListener(() =>
                {
                    gameFileView.GetComponent<PointedButton>().SuperSelect(true);
                    SaveToFile(file);
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

        private void SaveToNewFile()
        {
            string filename = Persistence.GameFile.GameFileName(Guid.NewGuid().ToString());
            Save(filename);
        }

        private void SaveToFile(FileInfo file)
        {
            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

            data.uiRoot.ShowMessagePrompt(UIRoot.OkCancelPrompt(
                "save_confirm",
                Localization.Localize("save_override_message"),
                () => { Save(file.Name); }));
        }

        private void Save(string filename)
        {
            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

            bool success = PlayerProfile.SaveGame(filename);
            if (!success)
            {
                data.uiRoot.ShowMessagePrompt(UIRoot.OkPrompt(
                    "save_error",
                    Localization.Localize("save_error_message"),
                    null));
            }
            else
            {
                data.uiRoot.ShowMessagePrompt(UIRoot.OkPrompt(
                    "save_successful",
                    Localization.Localize("save_successful_message"),
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
