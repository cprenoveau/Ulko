using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace Ulko.Data.Cutscenes
{
    public class TimelineStep : StepAction
    {
        public PlayableDirector director;

        public override IEnumerator Play()
        {
            director.Play();

            while(director.playableGraph.IsValid() && !director.playableGraph.IsDone())
            {
                director.playableGraph.Evaluate(Time.deltaTime);
                yield return null;
            }
        }
    }
}
