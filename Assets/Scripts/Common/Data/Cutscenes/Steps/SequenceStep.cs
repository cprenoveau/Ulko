using System.Collections;
using System.Collections.Generic;

namespace Ulko.Data.Cutscenes
{
    public class SequenceStep : StepAction
    {
        public List<StepAction> actions = new List<StepAction>();

        public override IEnumerator Play()
        {
            foreach(var action in actions)
            {
                yield return action.Play();
            }
        }
    }
}
