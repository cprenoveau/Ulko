using Ulko.UI;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Ulko.Data.Timeline;
using UnityEngine.Rendering.Universal;

namespace Ulko.Contexts
{
    public class PuzzleContext : Context<IGameState>
    {
        public override ContextType ContextType => ContextType.Puzzle;
        public override Camera Camera => Camera.main;
        public override Camera UICamera => uiCam;

        public Camera uiCam;

        protected override async Task _Begin(CancellationToken ct)
        {
            uiRoot.SetInfo(null);

            uiRoot.FadeAmount(1f);
            uiRoot.FadeIn(1f);

            var cameraData = Camera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(uiCam);

            var puzzle = Data.CurrentMilestone as Puzzle;

            uiRoot.menuStack.Push(puzzle.startingMenu.asset, puzzle.startingMenu.id, new MenuData() { gameState = Data, uiRoot = uiRoot });
        }

        protected override async Task _End(CancellationToken ct)
        {
            uiRoot.menuStack.PopAll();

            while (!ct.IsCancellationRequested && (uiRoot.menuStack.Count > 0 || uiRoot.menuStack.PendingCount > 0))
                await Task.Yield();

            _Dispose();
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
        }

        protected override bool _Back()
        {
            return false;
        }

        protected override void _OpenMenu()
        {
        }
    }
}
