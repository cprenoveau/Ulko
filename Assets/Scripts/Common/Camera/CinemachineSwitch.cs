using Unity.Cinemachine;
using UnityEngine;

namespace Ulko
{
    [RequireComponent(typeof(CinemachineBrain))]
    public class CinemachineSwitch : MonoBehaviour
    {
        private bool wasSwitched;
        public void Switch(CinemachineCamera vcam)
        {
            vcam.Prioritize();
            wasSwitched = true;
        }

        private void OnPreRender()
        {
            if (wasSwitched)
            {
                OrientToCamera.RefreshAll();
                wasSwitched = false;
            }
        }
    }
}
