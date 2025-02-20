using Cinemachine;
using UnityEngine;

namespace Ulko
{
    [RequireComponent(typeof(CinemachineBrain))]
    public class CinemachineSwitch : MonoBehaviour
    {
        private bool wasSwitched;
        public void Switch(CinemachineVirtualCamera vcam)
        {
            GetComponent<CinemachineBrain>().ActiveVirtualCamera.Priority = 0;
            vcam.Priority = 1000;

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
