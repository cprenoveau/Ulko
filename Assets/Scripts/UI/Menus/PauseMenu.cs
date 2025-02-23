using HotChocolate.UI;
using HotChocolate.Utils;

namespace Ulko.UI
{
    public class PauseMenu : Menu
    {
        public SettingsView settings;
        public PointedButton quitButton;

        private MenuStack stack;
        private MenuData data;

        private void Start()
        {
            quitButton.onClick.AddListener(Quit);
        }

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as MenuData;

            settings.Init();
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void Quit()
        {
            stack.Pop();
            data.gameState.GoToStartup(default).FireAndForgetTask();
        }
    }
}
