using Unity.Cinemachine;
using System.Collections;

namespace Ulko.Data.Cutscenes
{
    public class SwitchCameraStep : StepAction
    {
        public CinemachineSwitch cinemachineSwitch;
        public CinemachineCamera virtualCamera;

        public override IEnumerator Play()
        {
            cinemachineSwitch.Switch(virtualCamera);

            yield break;
        }
    }
}
