using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class PlayDialogueStep : StepAction
    {
        public bool isNarration;
        public DialogueAsset dialogueOverride;
        public int startLineIndex = 0;
        public int endLineIndex = -1;

        [Serializable]
        public class DialogueEvent
        {
            public int lineIndex;

            [SerializeReference]
            public StepAction action;
        }

        public List<DialogueEvent> events = new();

        public static event Action<PlayDialogueStep> OnPlay;
        public bool IsPlaying { get; set; }

        public override IEnumerator Play()
        {
            OnPlay?.Invoke(this);

            while (IsPlaying)
            {
                yield return null;
            }
        }
    }
}
