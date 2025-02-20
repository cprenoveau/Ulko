using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class ActivateObjectStep : StepAction
    {
        public GameObject obj;
        public float duration = float.PositiveInfinity;

        public override IEnumerator Play()
        {
            obj.SetActive(true);

            if (duration != float.PositiveInfinity)
            {
                yield return new WaitForSeconds(duration);
                obj.SetActive(false);
            }
        }
    }
}
