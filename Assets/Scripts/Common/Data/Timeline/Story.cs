using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Timeline
{
    [CreateAssetMenu(fileName = "Story", menuName = "Ulko/Timeline/Story", order = 1)]
    public class Story : ScriptableObject
    {
        public string id;
        public List<Act> acts = new();
    }
}
