using System;
using UnityEngine;

namespace Ulko
{
    public static class RaycastUtil
    {
        public static Material GetMeshMaterial(Ray ray, float distance = 2)
        {
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit, distance, ~LayerMask.GetMask("Player")))
            {
                return null;
            }

            Renderer r = hit.collider.GetComponent<Renderer>();
            MeshCollider mc = hit.collider as MeshCollider;

            if (r == null || r.sharedMaterial == null)
            {
                return null;
            }
            else if (!mc || mc.convex)
            {
                return r.material;
            }

            int materialIndex = -1;
            Mesh m = mc.sharedMesh;
            int triangleIdx = hit.triangleIndex;
            int lookupIdx1 = m.triangles[triangleIdx * 3];
            int lookupIdx2 = m.triangles[triangleIdx * 3 + 1];
            int lookupIdx3 = m.triangles[triangleIdx * 3 + 2];
            int subMeshesNr = m.subMeshCount;

            for (int i = 0; i < subMeshesNr; i++)
            {
                int[] tr = m.GetTriangles(i);

                for (int j = 0; j < tr.Length; j += 3)
                {
                    if (tr[j] == lookupIdx1 && tr[j + 1] == lookupIdx2 && tr[j + 2] == lookupIdx3)
                    {
                        materialIndex = i;
                        break;
                    }
                }

                if (materialIndex != -1) break;
            }

            return r.materials[materialIndex];
        }
    }
}
