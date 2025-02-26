using UnityEngine;

namespace Ulko.World
{
    public class SpawnPoint : MonoBehaviour
    {
        public Data.SpawnPointTag spawnPointTag;
        public Vector2 standDirection = Vector2.zero;

        private void OnDrawGizmos()
        {
            Vector3 center = transform.position;
            Debug.DrawLine(center - Vector3.right * 0.5f, center + Vector3.right * 0.5f, Color.red);
            Debug.DrawLine(center - Vector3.forward * 0.5f, center + Vector3.forward * 0.5f, Color.red);
        }
    }
}