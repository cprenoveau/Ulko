using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ulko.World
{
    [RequireComponent(typeof(SphereCollider))]
    public class Player : MonoBehaviour
    {
        public Rigidbody rigidBody;
        public Transform visualAnchor;

        public int Steps => (int)distanceWalked;
        public CharacterAnimation CharacterInstance { get; private set; }
        public bool InTrigger => triggerCount > 0;
        public List<Vector3> Trace { get; private set; } = new List<Vector3>();

        public event Action OnMoved;

        private float distanceWalked = 0;
        private Vector3 lastPosition;
        private int triggerCount = 0;
        private int triggerLockFrames = 0;

        private void OnDestroy()
        {
            if (CharacterInstance != null)
            {
                Addressables.ReleaseInstance(CharacterInstance.gameObject);
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent<Trigger>(out var trigger))
            {
                if(triggerLockFrames == 0 || trigger.CosmeticOnly)
                    trigger.OnEnter(this);

                if(!trigger.CosmeticOnly) triggerLockFrames++;
                triggerCount++;
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.TryGetComponent<Trigger>(out var trigger))
            {
                if(triggerLockFrames == 0 || trigger.CosmeticOnly)
                    trigger.OnExit(this);

                if(!trigger.CosmeticOnly) triggerLockFrames++;
                triggerCount--;
            }
        }

        public void SetCharacter(GameObject instance)
        {
            var dir = Vector2.zero;
            if (CharacterInstance != null)
            {
                dir = CharacterInstance.CurrentDirection;
                Addressables.ReleaseInstance(CharacterInstance.gameObject);
            }

            CharacterInstance = instance.GetComponent<CharacterAnimation>();
            CharacterInstance.transform.localPosition = Vector3.zero;

            CharacterInstance.Stand(dir);
        }

        public void Teleport(Vector3 position)
        {
            triggerLockFrames++;
            transform.position = position;

            PlayerProfile.SetPosition(transform.position, CharacterInstance.CurrentDirection);
            ResetSteps();
            Trace.Clear();
        }

        public void ResetSteps()
        {
            distanceWalked = 0;
            lastPosition = transform.position;
        }

        public void Move(Vector3 direction, Vector2 speed, float deltaTime)
        {
            if (CharacterInstance != null)
                CharacterInstance.Walk(new Vector2(direction.x, direction.z));

            Vector3 scaledDir = new(direction.x * speed.x, 0, direction.z * speed.y);
            rigidBody.MovePosition(rigidBody.position + (scaledDir * deltaTime));

            if (triggerLockFrames > 0)
                triggerLockFrames--;

            distanceWalked += (lastPosition - transform.position).magnitude;
            lastPosition = transform.position;

            PlayerProfile.SetPosition(transform.position, CharacterInstance.CurrentDirection);

            if (Trace.Count == 0 || Trace.Last() != transform.position)
            {
                Trace.Add(new Vector3(transform.position.x, GroundPosition(), transform.position.z));
            }

            OnMoved?.Invoke();
        }

        private float GroundPosition()
        {
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, -Vector3.up * 100f, Color.blue);

            if (Physics.Raycast(
                transform.position + Vector3.up * 0.5f,
                -Vector3.up,
                out RaycastHit hitInfo,
                100f,
                ~LayerMask.GetMask("Player", "NPC", "Ghost"),
                QueryTriggerInteraction.Ignore))
            {
                return hitInfo.point.y;
            }

            return transform.position.y;
        }

        public void Interact()
        {
            var interactable = RaycastInteractable();
            if (interactable != null)
            {
                interactable.Interact(this);
            }
        }

        public Interactable RaycastInteractable()
        {
            return RaycastInteractable(CharacterInstance);
        }

        public static Interactable RaycastInteractable(CharacterAnimation character)
        {
            RaycastHit[] hits = RaycastInFront(character);
            foreach (var hit in hits)
            {
                if (hit.collider != null)
                {
                    var activatable = hit.collider.GetComponent<Interactable>();
                    if (activatable != null && activatable.CanInteract)
                    {
                        return activatable;
                    }
                }
            }

            return null;
        }

        private static RaycastHit[] RaycastInFront(CharacterAnimation character)
        {
            var collider = character.GetComponentInParent<SphereCollider>();
            Vector3 direction = new(character.CurrentDirection.x, 0, character.CurrentDirection.y);

            var raycastOrigin = collider.bounds.center;
            Debug.DrawRay(raycastOrigin, 2f * collider.radius * direction, Color.red);

            return Physics.RaycastAll(
                raycastOrigin,
                direction,
                collider.radius * 2f,
                ~LayerMask.GetMask("Player", "Ghost"));
        }
    }
}
