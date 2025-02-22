using Ulko.Data.Timeline;
using UnityEngine;

namespace Ulko.World
{
    public interface IMilestoneSpecific
    {
        void Init(IMilestone milestone);
    }

    public class MilestoneSpecific : MonoBehaviour, IMilestoneSpecific
    {
        public string milestoneName;

        public void Init(IMilestone milestone)
        {
            gameObject.SetActive(milestone.Name == milestoneName);
        }
    }
}