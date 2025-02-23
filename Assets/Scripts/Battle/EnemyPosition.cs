using UnityEngine;

namespace Ulko.Battle
{
    public class EnemyPosition : MonoBehaviour
    {
        private void Start()
        {
            foreach(Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
