using UnityEngine;

namespace Ulko
{
    public class OrientToCamera : MonoBehaviour
    {
        public float offset;

        public static void RefreshAll()
        {
            var orients = FindObjectsOfType<OrientToCamera>();
            foreach (var orient in orients)
                orient.Refresh();
        }

        public void Refresh()
        {
            if (Camera.main != null)
            {
                float flip = Mathf.Abs(transform.eulerAngles.y) > 45 ? -1 : 1;
                transform.eulerAngles = new Vector3((Camera.main.transform.eulerAngles.x + offset) * flip, transform.eulerAngles.y, transform.eulerAngles.z);
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void LateUpdate()
        {
            Refresh();
        }
    }
}