﻿using Newtonsoft.Json.Linq;
using System;
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

        public Camera cam;
        public TextAsset defaultDialogue;

        public bool stopAllAmbient;
        public AmbientConfig ambient;
        public float ambientFadeInDuration;

        public bool playBattleTransition;
        public List<CutsceneSequence> sequences = new();

        public Dialogue DefaultDialogue { get; private set; } = new Dialogue();

        private readonly Dictionary<int, Coroutine> playingCoroutines = new();

        private void Awake()
        {
            if(defaultDialogue != null)
                DefaultDialogue.AddNode(JToken.Parse(defaultDialogue.text));
        }

        public void Play(Action onComplete)
        {
            Debug.Log("Cutscene start");

            cam.gameObject.SetActive(true);

            if(stopAllAmbient)
                Audio.Player.StopAll(AudioType.Ambient);

            if (ambient != null)
                ambient.Play(ambientFadeInDuration);

            StopAllCoroutines();
            StartCoroutine(PlayAsync(onComplete));
        }

        public void Stop(bool turnCameraOff)
        {
            Debug.Log("Cutscene stop");

            if(turnCameraOff)
                cam.gameObject.SetActive(false);

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
