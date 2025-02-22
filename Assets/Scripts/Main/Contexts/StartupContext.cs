using Ulko.UI;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ulko
{
    public class StartupContext : Context<IGameState>
    {
        public override ContextType ContextType => ContextType.Startup;
        public override Camera Camera => cam;

        public Camera cam;
        public MenuDefinition startMenu;
        public AudioDefinition music;

        protected override async Task _Begin(CancellationToken ct)
        {
            Audio.Player.StopAll(AudioType.Ambient);

            uiRoot.SetInfo(null);
            Audio.Player.PlaySolo(music);

            uiRoot.menuStack.Push(startMenu.asset, startMenu.id, new MenuData { gameState = Data, uiRoot = uiRoot });
            uiRoot.FadeIn(2f);

            await Task.Delay(2000, ct);
        }

        protected override async Task _End(CancellationToken ct)
        {
            StopAllCoroutines();

            Audio.Player.StopAll(AudioType.Music);

            uiRoot.menuStack.PopAll();
            uiRoot.FadeOut(2f);

            await Task.Delay(2000);

            while (!ct.IsCancellationRequested && uiRoot.menuStack.Count > 0)
                await Task.Yield();
        }

        protected override void _Dispose()
        {
        }

        protected override void _Move(Vector2 direction, float deltaTime)
        {
        }

        protected override void _Interact()
        {
        }

        protected override void _Cancel()
        {
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
        }
    }
}
