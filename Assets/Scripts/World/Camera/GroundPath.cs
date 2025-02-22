using UnityEngine;
using UnityEngine.Splines;

namespace Ulko.World
{
    public class GroundPath : MonoBehaviour
    {
        public Vector3 PosOnPath { get; private set; }
        public float TimeOnPath => currentTime ?? 0;

        public SplineContainer groundPath;
        public float followSpeed = 5f;

        private Transform playerTransform;
        private float? currentTime;

        public void Init(Transform playerTransform)
        {
            this.playerTransform = playerTransform;
            currentTime = null;

            UpdatePosOnPath();
        }

        private Vector3 ClosestPointOnSpline(Vector3 aP, Spline spline)
        {
            float time = ClosestTimeOnSpline(aP, spline);

            if(currentTime.HasValue)
            {
                currentTime = Mathf.Lerp(currentTime.Value, time, Time.deltaTime * followSpeed);
            }
            else
            {
                currentTime = time;
            }

            return spline.EvaluatePosition(currentTime.Value);
        }

        private float ClosestTimeOnSpline(Vector3 aP, Spline spline)
        {
            SplineUtility.GetNearestPoint(spline, aP, out _, out float nearPosTime);
            return nearPosTime;
        }

        private void UpdatePosOnPath()
        {
            if (playerTransform == null)
                return;

            PosOnPath = ClosestPointOnSpline(playerTransform.position, groundPath.Spline);
        }

        private void LateUpdate()
        {
            UpdatePosOnPath();
        }

        private void OnDrawGizmos()
        {
            Vector3 center = PosOnPath;
            Debug.DrawLine(center - Vector3.right * 0.5f, center + Vector3.right * 0.5f, Color.red);
            Debug.DrawLine(center - Vector3.forward * 0.5f, center + Vector3.forward * 0.5f, Color.red);
        }
    }
}
