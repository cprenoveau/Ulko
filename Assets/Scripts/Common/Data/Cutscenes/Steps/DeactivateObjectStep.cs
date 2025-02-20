using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class DeactivateObjectStep : StepAction
    {
        public GameObject obj;
        public float duration = float.PositiveInfinity;

        public override IEnumerator Play()
        {
            obj.SetActive(false);

            if (duration != float.PositiveInfinity)
            {
                yield return new WaitForSeconds(duration);
                obj.SetActive(true);
            }
        }
    }
}
