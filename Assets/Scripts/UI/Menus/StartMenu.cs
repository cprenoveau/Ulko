using HotChocolate.UI;
using HotChocolate.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class StartMenu : Menu
    {
        public MenuDefinition loadMenu;
        public MenuDefinition settingsMenu;

        public TMP_Text newGameLabel;
        public Button newGameButton;
        public Button loadButton;
        public Button settingsButton;
        public Button quitButton;

        private MenuStack stack;
        private MenuData data;

        private void Start()
        {
            newGameButton.onClick.AddListener(StartGame);
            loadButton.onClick.AddListener(LoadGame);
            settingsButton.onClick.AddListener(ShowSettings);
            quitButton.onClick.AddListener(Quit);

            Localization.LocaleChanged += Refresh;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
        }

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as MenuData;

            Refresh();
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void Refresh()
        {
            newGameLabel.text = PlayerProfile.HasSuspendedGame() ? Localization.Localize("resume") : Localization.Localize("new_game");
        }

        private void StartGame()
        {
            Audio.Player.PlayUISound(Audio.UISoundId.NewGame);

            if (PlayerProfile.HasSuspendedGame())
            {
                PlayerProfile.ResumeGame();
            }
            else
            {
                PlayerProfile.NewGame();
            }

            data.gameState.StartMilestone(data.gameState.CurrentMilestone, default).FireAndForgetTask();
            stack.Pop();
        }

        private void LoadGame()
        {
            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

            stack.Push(loadMenu.asset, loadMenu.id, data);
        }

        private void ShowSettings()
        {
            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

            stack.Push(settingsMenu.asset, settingsMenu.id, data);
        }

        private void Quit()
        {
            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

            Application.Quit();
        }
    }
}
