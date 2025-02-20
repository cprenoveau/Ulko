using HotChocolate.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ulko.Data.Timeline
{
    [CreateAssetMenu(fileName = "Act", menuName = "Ulko/Timeline/Act", order = 1)]
    public class Act : ScriptableObject
    {
        public List<Chapter> chapters = new List<Chapter>();
    }
}
