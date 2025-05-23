﻿using HotChocolate.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ulko.Data.Timeline
{
    [CreateAssetMenu(fileName = "Chapter", menuName = "Ulko/Timeline/Chapter", order = 1)]
    public class Chapter : ScriptableObject
    {
        public string chapterId;
        public string chapterName;

        [SerializeReference]
        public List<IMilestone> milestones = new();

        private readonly List<Instantiator> instantiators = new();
        public IEnumerable<Instantiator> MilestoneInstantiators()
        {
            instantiators.Clear();

            var milestoneTypes = typeof(Chapter).Assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IMilestone)));
            foreach (var milestoneType in milestoneTypes)
            {
                instantiators.Add(Create(milestoneType));
            }

            return instantiators;
        }

        private Instantiator Create(Type type)
        {
            return new Instantiator()
            {
                displayName = type.Name,
                type = type,
                Instantiate = () => { return Activator.CreateInstance(type); }
            };
        }
    }
}
