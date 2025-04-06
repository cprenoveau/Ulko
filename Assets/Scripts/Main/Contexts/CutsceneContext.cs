using Ulko.Data.Cutscenes;
using Ulko.UI;
using HotChocolate.Utils;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Ulko.Contexts
{
    public class CutsceneContext : Context<IGameState>
    {
        public override ContextType ContextType => ContextType.Cutscene;
        public override Camera Camera => cutscene != null ? cutscene.cam : null;
        public override Camera UICamera => uiCam;

        public Camera uiCam;
        public MenuDefinition dialogueBox;
        public MenuDefinition narrationBox;
        public MenuDefinition labelScreen;

        private Cutscene cutscene;

        protected override async Task _Begin(CancellationToken ct)
        {
            uiRoot.SetInfo(null);
            uiRoot.FadeAmount(1f);

            PlayDialogueStep.OnPlay += ShowDialogue;
            ShowLabelStep.OnPlay += ShowLabel;
            FadeInStep.OnPlay += FadeIn;
            FadeOutStep.OnPlay += FadeOut;

            cutscene = FindFirstObjectByType<Cutscene>();
            if (cutscene != null)
            {
                var cameraData = cutscene.cam.GetUniversalAdditionalCameraData();
                cameraData.cameraStack.Add(uiCam);

                cutscene.Play(() => { StartNextMilestone(ct); });
            }
        }

        protected override async Task _End(CancellationToken ct)
        {
            if (cutscene != null)
                cutscene.Stop(true);

            uiRoot.menuStack.PopAll();
            uiRoot.overlayStack.PopAll();

            while (!ct.IsCancellationRequested &&
                (uiRoot.menuStack.Count > 0 || uiRoot.menuStack.PendingCount > 0 || uiRoot.overlayStack.Count > 0 || uiRoot.overlayStack.PendingCount > 0))
            {
                await Task.Yield();
            }

            _Dispose();
        }

        protected override void _Dispose()
        {
            PlayDialogueStep.OnPlay -= ShowDialogue;
            ShowLabelStep.OnPlay -= ShowLabel;
            FadeInStep.OnPlay -= FadeIn;
            FadeOutStep.OnPlay -= FadeOut;
        }

        private void ShowDialogue(PlayDialogueStep showDialogue)
        {
            showDialogue.IsPlaying = true;

            Data.Dialogue dialogue = cutscene.DefaultDialogue;

            if (showDialogue.dialogueOverride != null)
            {
                dialogue = new Data.Dialogue();
                dialogue.AddNode(JToken.Parse(showDialogue.dialogueOverride.text));
            }

            var page = dialogue.GetCurrentNode().page;
            foreach (var evt in showDialogue.events)
            {
                page.lines[evt.lineIndex].OnPlay += () => { cutscene.StartCoroutine(evt.action.Play()); };
            }

            var menuDef = showDialogue.isNarration ? narrationBox : dialogueBox;
            var menuStack = showDialogue.isNarration ? uiRoot.overlayStack : uiRoot.menuStack;

            menuStack.Push(menuDef.asset, menuDef.id, new DialogueMenuData
            {
                dialogue = dialogue,
                startLineIndex = showDialogue.startLineIndex,
                endLineIndex = showDialogue.endLineIndex,
                onClose = () => { showDialogue.IsPlaying = false; }
            });
        }

        private void ShowLabel(ShowLabelStep showLabel)
        {
            showLabel.IsPlaying = true;

            uiRoot.overlayStack.Push(labelScreen.asset, labelScreen.id, new LabelScreenData
            {
                labelKey = showLabel.labelKey,
                onClose = () => { showLabel.IsPlaying = false; }
            });
        }

        private void FadeIn(FadeInStep fadeIn)
        {
            uiRoot.FadeIn(fadeIn.duration);
        }

        private void FadeOut(FadeOutStep fadeOut)
        {
            uiRoot.FadeOut(fadeOut.duration);
        }

        private void StartNextMilestone(CancellationToken ct)
        {
            if (!ct.IsCancellationRequested)
            {
                StartNextMilestoneAsync(ct).FireAndForgetTask();
            }
        }

        private async Task StartNextMilestoneAsync(CancellationToken ct)
        {
            if (cutscene != null && cutscene.playBattleTransition && !ct.IsCancellationRequested)
            {
                cutscene.Stop(false);
                uiRoot.FadeAmount(0f);
            }
            else
            {
                uiRoot.FadeAmount(1f);
            }

            await Data.StartNextMilestone(ct);
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
