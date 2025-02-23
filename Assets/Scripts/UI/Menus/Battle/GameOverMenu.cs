using Ulko.Battle;
using HotChocolate.UI;
using System;
using UnityEngine;
using UnityEngine.UI;
using HotChocolate.Utils;

namespace Ulko.UI
{
    public class GameOverMenuData
    {
        public IGameState gameState;
        public UIRoot uiRoot;
        public BattleInstance battleInstance;
        public Action onRetry;
    }

    public class GameOverMenu : Menu
    {
        public MenuDefinition loadMenu;

        public Button retryButton;
        public Button loadButton;
        public Button quitButton;

        private MenuStack stack;
        private GameOverMenuData data;

        private void Start()
        {
            retryButton.onClick.AddListener(Retry);
            loadButton.onClick.AddListener(LoadGame);
            quitButton.onClick.AddListener(Quit);
        }

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as GameOverMenuData;
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu)
        {
            data.uiRoot.SetInfo(null);
        }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void Retry()
        {
            Audio.Player.PlayUISound(Audio.UISoundId.NewGame);

            data.onRetry?.Invoke();
        }

        private void LoadGame()
        {
            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

            stack.Push(loadMenu.asset, loadMenu.id, new MenuData { gameState = data.gameState, uiRoot = data.uiRoot });
        }

        private void Quit()
        {
            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

            stack.Pop();
            data.gameState.GoToStartup(default).FireAndForgetTask();
        }
    }
}
