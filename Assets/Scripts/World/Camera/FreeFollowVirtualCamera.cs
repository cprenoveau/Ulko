using UnityEngine;

namespace Ulko.World
{
    public class FreeFollowVirtualCamera : VirtualCameraZone
    {
        public BoxCollider cameraBoundaries;
        public Transform followTransform;
        public Transform lookAtTransform;

        public float followMultY = -0.5f;
        public float followMultZ = -0.25f;
        public float lookAtMultY = 1;
        public float lookAtMultZ = 0.5f;

        private Vector3 cameraForward;
        private Vector3 cameraUp;
        private Vector3 cameraRight;
        private Vector3 cameraPosition;
        private Vector3 cameraLookDistance;

        private Camera cam;
        private Transform playerTransform;
        private Limits limits;

        private void Awake()
        {
            cameraForward = vCam.transform.forward;
            cameraUp = vCam.transform.up;
            cameraRight = vCam.transform.right;
            cameraPosition = vCam.transform.position;
            cameraLookDistance = lookAtTransform.position - cameraPosition;
        }

        protected override void _Init(Camera cam, Transform playerTransform, Limits limits)
        {
            this.cam = cam;
            this.playerTransform = playerTransform;
            this.limits = limits;
           
            float hFOV = Camera.VerticalToHorizontalFieldOfView(vCam.m_Lens.FieldOfView, vCam.m_Lens.Aspect);

            var vTopLeft = Quaternion.AngleAxis(hFOV / 2f, -cameraUp) * cameraForward;
            vTopLeft = Quaternion.AngleAxis(cam.fieldOfView / 2f, -cameraRight) * vTopLeft;

            var vTopRight = Quaternion.AngleAxis(hFOV / 2f, cameraUp) * cameraForward;
            vTopRight = Quaternion.AngleAxis(cam.fieldOfView / 2f, -cameraRight) * vTopRight;

            var vBottomLeft = Quaternion.AngleAxis(hFOV / 2f, -cameraUp) * cameraForward;
            vBottomLeft = Quaternion.AngleAxis(cam.fieldOfView / 2f, cameraRight) * vBottomLeft;

            var plane = new Plane(Vector3.up, 0);

            plane.Raycast(new Ray(cameraPosition + Vector3.up * limits.Height, vTopLeft), out float enterTopLeft);
            plane.Raycast(new Ray(cameraPosition + Vector3.up * limits.Height, vTopRight), out float enterTopRight);
            plane.Raycast(new Ray(cameraPosition, vBottomLeft), out float enterBottomLeft);

            var boundaries = limits.Absolute();

            Vector3 farthestPointLeft = new Vector3(boundaries.x, 0, boundaries.y) + new Vector3(0, 0, boundaries.height);
            Vector3 farthestPointRight = farthestPointLeft + new Vector3(boundaries.width, 0, 0);

            Vector3 closestPointLeft = new(boundaries.x, 0, boundaries.y);

            Vector3 pointTopLeft = farthestPointLeft - vTopLeft * enterTopLeft;
            Vector3 pointTopRight = farthestPointRight - vTopRight * enterTopRight;
            Vector3 pointBottomLeft = closestPointLeft - vBottomLeft * enterBottomLeft;

            float sizeX = pointTopRight.x - pointTopLeft.x;
            float posX = pointTopLeft.x;
            if (sizeX < 0)
            {
                sizeX = 0;
                posX = boundaries.x + boundaries.width / 2f;
            }

            float sizeZ = pointTopLeft.z - pointBottomLeft.z;
            if (sizeZ < 0)
            {
                sizeZ = 0;
            }

            cameraBoundaries.size = new Vector3(sizeX, pointTopLeft.y, sizeZ);
            cameraBoundaries.center = cameraBoundaries.size / 2f + new Vector3(0, 0, 0);
            cameraBoundaries.transform.position = new Vector3(posX, 0, pointBottomLeft.z);
        }

        protected override void _UpdateCamera()
        {
            if (playerTransform == null)
                return;

            var boundaries = limits.Absolute();
            float zRatio = (playerTransform.position.z - boundaries.min.y) / (boundaries.max.y - boundaries.min.y);

            followTransform.position = playerTransform.position;
            followTransform.position = new Vector3(followTransform.position.x, followTransform.position.y * followMultY, followTransform.position.z + zRatio * boundaries.height * followMultZ);

            lookAtTransform.position = cam.transform.position + cameraLookDistance;
            lookAtTransform.position = new Vector3(lookAtTransform.position.x, playerTransform.position.y * lookAtMultY, lookAtTransform.position.z + zRatio * boundaries.height * lookAtMultZ);
        }
    }
}
