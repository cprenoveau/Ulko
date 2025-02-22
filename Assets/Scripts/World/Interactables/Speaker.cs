﻿using Ulko.Data.Timeline;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.World
{
    public class Speaker : Interactable, INpcBehaviour, IMilestoneSpecific
    {
        public bool fixedPose;
        public bool useCurrentDirection;
        public Vector2 standDirection = Vector2.zero;
        public List<TextAsset> defaultDialogue = new();

        [Serializable]
        public class DialoguePerMilestone
        {
            public string milestone;
            public List<TextAsset> pageAssets = new();
        }

        public List<DialoguePerMilestone> dialoguePerMilestone = new();

        internal static event Action<Speaker> OnInteract;

        public Data.Dialogue Dialogue { get; private set; }

        private void Awake()
        {
            var character = GetComponentInChildren<CharacterAnimation>();
            if (character != null && !fixedPose)
                character.Stand(standDirection);
        }

        public void Init(IMilestone milestone)
        {
            Dialogue = new Data.Dialogue();

            var pages = defaultDialogue;
            var dialogue = dialoguePerMilestone.Find(d => d.milestone == milestone.Name);
            if(dialogue != null)
            {
                pages = dialogue.pageAssets;
            }

            foreach (var page in pages)
            {
                Dialogue.AddNode(JToken.Parse(page.text));
            }
        }

        public override bool CanInteract => Dialogue.nodes.Count > 0;
        public override void Interact(Player player)
        {
            var character = GetComponentInChildren<CharacterAnimation>();
            if (character != null && !fixedPose)
            {
                if (useCurrentDirection)
                    standDirection = character.CurrentDirection;

                var v = player.transform.position - transform.position;
                character.Stand(new Vector2(v.x, v.z));
            }

            OnInteract?.Invoke(this);
        }

        public void Pause()
        {
            enabled = false;
        }

        public void Resume()
        {
            var character = GetComponentInChildren<CharacterAnimation>();
            if (character != null && !fixedPose)
                character.Stand(standDirection);

            enabled = true;
        }
    }
}
