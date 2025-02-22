using Cinemachine;
using UnityEngine;

namespace Ulko.World
{
    public class DollyTrackVirtualCamera : VirtualCameraZone
    {
        public GroundPath groundPath;
        public Transform lookAtTransform;

        private Camera cam;
        private Transform playerTransform;

        private void Awake()
        {
            lookAtTransform.position = new Vector3(vCam.transform.position.x, lookAtTransform.position.y, lookAtTransform.position.z);
        }

        protected override void _Init(Camera cam, Transform playerTransform, Limits limits)
        {
            this.cam = cam;
            this.playerTransform = playerTransform;

            groundPath.Init(playerTransform);
        }

        protected override void _UpdateCamera()
        {
            if (playerTransform == null)
                return;

            vCam.GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition = groundPath.TimeOnPath;

            lookAtTransform.position = new Vector3(cam.transform.position.x, playerTransform.position.y, playerTransform.position.z);
        }
    }
}
