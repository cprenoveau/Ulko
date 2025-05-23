﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class Cutscene : MonoBehaviour
    {
        [Serializable]
        public class CutsceneSequence
        {
            public bool blocking = true;
            public float delay;
            public Sequence sequence;
        }

        public DialogueAsset dialogue;

        public bool stopAllAmbientAudio;
        public AmbientAudioConfig ambientAudioConfig;
        public float ambientFadeInDuration;

        public bool playBattleTransition;
        public List<CutsceneSequence> sequences = new();

        public Camera Camera { get; private set; }
        public Dialogue DefaultDialogue { get; private set; } = new Dialogue();

        private readonly Dictionary<int, Coroutine> playingCoroutines = new();

        private void Awake()
        {
            if (dialogue != null)
            {
                DefaultDialogue = dialogue.CreateDialogue();
            }
        }

        public void Play(Camera camera, Action onComplete)
        {
            Debug.Log("Cutscene start");

            Camera = camera;
            Camera.gameObject.SetActive(true);

            if (stopAllAmbientAudio)
                Audio.Player.StopAll(AudioType.Ambient);

            if (ambientAudioConfig != null)
                ambientAudioConfig.Play(ambientFadeInDuration);

            StopAllCoroutines();
            StartCoroutine(PlayAsync(onComplete));
        }

        public void Stop(bool turnCameraOff)
        {
            Debug.Log("Cutscene stop");

            if(turnCameraOff)
                Camera.gameObject.SetActive(false);

            StopAllCoroutines();
            playingCoroutines.Clear();
        }

        private int index;
        private IEnumerator PlayAsync(Action onComplete)
        {
            playingCoroutines.Clear();

            foreach (var sequence in sequences)
            {
                if (sequence.blocking)
                {
                    yield return PlaySequence(sequence, null);
                }
                else
                {
                    int i = index;
                    playingCoroutines.Add(i, StartCoroutine(PlaySequence(sequence, () => { RemoveCoroutine(i); })));
                    index++;
                }
            }

            while (playingCoroutines.Count > 0)
            {
                yield return null;
            }

            Debug.Log("Cutscene complete");
            onComplete?.Invoke();
        }

        private IEnumerator PlaySequence(CutsceneSequence sequence, Action callback)
        {
            yield return new WaitForSeconds(sequence.delay);
            yield return sequence.sequence.Play(this, callback);
        }

        private void RemoveCoroutine(int index)
        {
            playingCoroutines.Remove(index);
        }
    }
}
