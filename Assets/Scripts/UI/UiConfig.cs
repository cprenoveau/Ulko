using System;
using Ulko.Data;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Ulko.UI
{
    [CreateAssetMenu(fileName = "UiConfig", menuName = "Ulko/UI/UI Config", order = 1)]
    public class UiConfig : ScriptableObject
    {
        public float battleTextInterval = 1f;
        public float battleTextDuration = 2f;
        public float battleTextSpeed = 5f;
        public Color damageTextColor = Color.white;
        public Color healTextColor = Color.green;
        public Color defaultTextColor = Color.white;

        [Serializable]
        public class StatProgress
        {
            public Stat stat;
            public int minValue;
            public int maxValue;
        }

        public int defaultMaxStat = 100;
        public List<StatProgress> statProgress = new List<StatProgress>();

        public (int min, int max) FindMinMaxStat(Stat stat)
        {
            var statProg = statProgress.FirstOrDefault(s => s.stat == stat);
            if (statProg == null)
                return (0, defaultMaxStat);

            return (statProg.minValue, statProg.maxValue);
        }

        public Color disabledItemColor = Color.gray;
        public Color positiveDiffColor = Color.green;
        public Color negativeDiffColor = Color.red;
    }
}
