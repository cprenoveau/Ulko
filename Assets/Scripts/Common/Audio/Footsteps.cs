using UnityEngine;

namespace Ulko
{
    public class Footsteps : MonoBehaviour
    {
        public AudioSource audioSource;
        public FootstepsDefinition footstepsDef;

        private int lastFrameIndex;
        private int stepCount;

        public CharacterAnimation Character { get; set; }

        private void Start()
        {
            Character = GetComponentInParent<CharacterAnimation>();
        }

        private void Update()
        {
            if(Character != null && Character.CurrentState == CharacterAnimation.State.Walk)
            {
                if(Character.CurrentFrameIndex != lastFrameIndex && Character.IsFootstep)
                {
                    PlayFootstep();
                }

                lastFrameIndex = Character.CurrentFrameIndex;
            }
            else
            {
                lastFrameIndex = 0;
            }
        }

        private void PlayFootstep()
        {
            var mat = GetMeshMaterialAtFeet();
            if (mat == null)
                return;

            var (clip, volume) = footstepsDef.GetFootstep(mat, stepCount);
            if (clip == null)
                return;

            audioSource.PlayOneShot(clip, volume);
            stepCount++;
        }

        private Material GetMeshMaterialAtFeet()
        {
            var dir = Character != null ? Character.CurrentDirection : Vector2.zero;

            var ray = new Ray(transform.position + Vector3.up * 0.5f + new Vector3(dir.x, 0, dir.y) * 0.5f, Vector3.down);

            return RaycastUtil.GetMeshMaterial(ray);
        }
    }
}
