using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Timeline
{
    [CreateAssetMenu(fileName = "Act", menuName = "Ulko/Timeline/Act", order = 1)]
    public class Act : ScriptableObject
    {
        public List<Chapter> chapters = new();
    }
}
