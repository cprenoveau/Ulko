using Ulko.Battle;
using Ulko.UI;
using HotChocolate.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using Ulko;
using UnityEngine;

namespace Ulko.Contexts
{
    public class BattleContext : Context<IGameState>
    {
        public override ContextType ContextType => ContextType.Battle;
        public override Camera Camera => cam;

        public BattleConfig config;

        public MenuDefinition hud;
        public MenuDefinition mainMenu;
        public MenuDefinition victoryScreen;
        public MenuDefinition gameOverMenu;
        public MenuDefinition dialogueMenu;

        public Camera cam;
        public Character heroPrefab;
        public Character enemyPrefab;

        private BattleInstance battleInstance;

        private CancellationTokenSource ctSource = new CancellationTokenSource();

        protected override async Task _Begin(CancellationToken ct)
        {
            battleInstance = BattleInstance.Create(
                Data.CurrentBattle,
                FindFirstObjectByType<Battlefield>(),
            heroPrefab,
                enemyPrefab,
                transform,
                Data.CurrentMilestone,
                config);

            battleInstance.OnShowDialogue += ShowDialogue;
            uiRoot.SetInfo(null);

            uiRoot.menuStack.Push(hud.asset, hud.id, new BattleHudData { gameState = Data, uiRoot = UIRoot, battleInstance = battleInstance });
            uiRoot.FadeIn(1f);

            DoBattle(ctSource.Token).FireAndForgetTask();
        }

        private async Task DoBattle(CancellationToken ct)
        {
            PlayerProfile.SaveState();

            var result = await BattleFunctions.DoBattle(
                battleInstance,
                cam,
                (PlayerAction playerAction) => { WaitForPlayer(playerAction); },
                ct).ConfigureAwait(true);

            if (ct.IsCancellationRequested)
                return;

            if (result == BattleFunctions.BattleResult.Victory)
            {
                uiRoot.menuStack.Push(victoryScreen.asset, victoryScreen.id, new VictoryScreenData()
                {
                    gameState = Data,
                    uiRoot = uiRoot,
                    battleInstance = battleInstance,
                    onClose = () => { Data.EndBattle(default).FireAndForgetTask(); }
                });
            }
            else
            {
                uiRoot.menuStack.Push(gameOverMenu.asset, gameOverMenu.id, new GameOverMenuData()
                {
                    gameState = Data,
                    uiRoot = uiRoot,
                    battleInstance = battleInstance,
                    onRetry = () => { RetryBattle(ct); }
                });
            }
        }

        private void RetryBattle(CancellationToken ct)
        {
            PlayerProfile.RestoreState();

            uiRoot.menuStack.PopAll();

            _Dispose();
            _Begin(ct).FireAndForgetTask();
        }

        public void ForceWinBattle()
        {
            ctSource.Cancel();
            ctSource.Dispose();
            ctSource = new CancellationTokenSource();

            uiRoot.menuStack.Push(victoryScreen.asset, victoryScreen.id, new VictoryScreenData()
            {
                gameState = Data,
                uiRoot = uiRoot,
                battleInstance = battleInstance,
                onClose = () => { Data.EndBattle(ctSource.Token).FireAndForgetTask(); }
            });
        }

        public void ForceLoseBattle()
        {
            ctSource.Cancel();
            ctSource.Dispose();
            ctSource = new CancellationTokenSource();

            Data.EndBattle(ctSource.Token).FireAndForgetTask();
        }

        private void WaitForPlayer(PlayerAction playerAction)
        {
            uiRoot.menuStack.Push(mainMenu.asset, mainMenu.id, new BattleMenuData()
            {
                gameState = Data,
                uiRoot = uiRoot,
                battleInstance = battleInstance,
                playerAction = playerAction
            });

            playerAction.OnActionSelected -= PopAllAboveHud;
            playerAction.OnActionSelected += PopAllAboveHud;
        }

        private void ShowDialogue(Data.Dialogue dialogue, Action callback)
        {
            uiRoot.menuStack.Push(dialogueMenu.asset, dialogueMenu.id, new DialogueMenuData { dialogue = dialogue, onClose = callback });
        }

        private void PopAllAboveHud()
        {
            uiRoot.menuStack.PopAllAbove(hud.id);
        }

        protected override async Task _End(CancellationToken ct)
        {
            battleInstance.OnShowDialogue -= ShowDialogue;

            ctSource.Cancel();
            ctSource.Dispose();
            ctSource = new CancellationTokenSource();

            StopAllCoroutines();

            uiRoot.menuStack.PopAll();
            uiRoot.FadeOut(1f);
            await Task.Delay(1000, ct);

            while (!ct.IsCancellationRequested && uiRoot.menuStack.Count > 0)
                await Task.Yield();

            _Dispose();
        }

        private void OnDestroy()
        {
            ctSource.Cancel();
            ctSource.Dispose();
        }

        protected override void _Dispose()
        {
            if (battleInstance != null)
            {
                battleInstance.OnShowDialogue -= ShowDialogue;
                battleInstance.Dispose();
            }
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
