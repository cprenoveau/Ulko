﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data
{
    [CreateAssetMenu(fileName = "Encounters", menuName = "Ulko/Battle/Encounters", order = 1)]
    public class Encounters : ScriptableObject
    {
        [Serializable]
        public class Encounter
        {
            public int frequency = 1;
            public BattleAsset battle;
        }

        public int minSteps = 10;
        public int checkInterval = 5;
        public float probability = 1;
        public List<Encounter> encounters = new();

        public int TryFindEncounterIndex(Encounter encounter)
        {
            return encounters.IndexOf(encounter);
        }

        public Encounter TryFindEncounter(int index)
        {
            if(index >= 0 && index < encounters.Count)
                return encounters[index];
            else return null;
        }

        public Encounter TryPickEncounter(int stepCount)
        {
            if (stepCount < minSteps)
                return null;

            int steps = stepCount - minSteps;
            if (steps % checkInterval == 0)
            {
                float prob = probability * steps;
                Debug.Log("Check encounter: " + prob + "%");

                if (UnityEngine.Random.Range(0, 100f) < prob)
                {
                    return PickEncounter();
                }
            }

            return null;
        }

        private readonly List<Encounter> remaining = new();
        private Encounter PickEncounter()
        {
            if(remaining.Count == 0)
            {
                foreach (var encounter in encounters)
                {
                    for(int i = 0; i < encounter.frequency; ++i)
                        remaining.Add(encounter);
                }
            }

            int index = UnityEngine.Random.Range(0, remaining.Count);
            var picked = remaining[index];
            remaining.RemoveAt(index);

            return picked;
        }
    }
}
