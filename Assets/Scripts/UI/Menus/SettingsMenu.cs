using HotChocolate.UI;

namespace Ulko.UI
{
    public class SettingsMenu : Menu
    {
        public SettingsView settings;

        private MenuStack stack;
        private MenuData data;

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as MenuData;

            settings.Init();
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }
    }
}
