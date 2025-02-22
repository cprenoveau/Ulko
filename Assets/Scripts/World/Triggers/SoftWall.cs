using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.World
{
    public class SoftWall : Trigger
    {
        public enum Side
        {
            Left,
            Right,
            Bottom,
            Top
        }

        public Side exitSide;
        public float walkDistance = 1f;
        public float speed = 1f;
        public List<TextAsset> pageAssets = new();

        public Data.Dialogue Dialogue { get; private set; }

        internal static event Action<SoftWall> OnTrigger;

        private void Awake()
        {
            Dialogue = new Data.Dialogue();

            foreach (var page in pageAssets)
            {
                Dialogue.AddNode(JToken.Parse(page.text));
            }
        }

        public void PlayAnimation(Player player, Action callback)
        {
            StartCoroutine(PlayAnimationAsync(player, callback));
        }

        private IEnumerator PlayAnimationAsync(Player player, Action callback)
        {
            var pos = ToPosition(player);

            Vector3 v = pos - player.transform.position;
            Vector3 vNormalized = v.normalized;
            float distance = v.magnitude;
            float duration = distance / speed;

            float elapsed = 0;
            while(elapsed < duration)
            {
                elapsed += Time.deltaTime;
                player.Move(vNormalized, new Vector2(speed,speed), Time.deltaTime);

                yield return null;
            }

            callback?.Invoke();
        }

        public override void OnEnter(Player player)
        {
            OnTrigger?.Invoke(this);
        }

        public override void OnExit(Player player)
        {
        }

        private Vector3 ToPosition(Player player)
        {
            var collider = GetComponent<BoxCollider>();

            switch (exitSide)
            {
                case Side.Left:
                    return new Vector3(collider.bounds.min.x - walkDistance, player.transform.position.y, player.transform.position.z);
                case Side.Right:
                    return new Vector3(collider.bounds.max.x + walkDistance, player.transform.position.y, player.transform.position.z);
                case Side.Bottom:
                    return new Vector3(player.transform.position.x, player.transform.position.y, collider.bounds.min.z - walkDistance);
                case Side.Top:
                    return new Vector3(player.transform.position.x, player.transform.position.y, collider.bounds.max.z + walkDistance);
            }

            return player.transform.position;
        }

        protected override void OnDrawGizmos()
        {
            gizmoColorDown = exitSide == Side.Bottom ? Color.blue : Color.yellow;
            gizmoColorRight = exitSide == Side.Right ? Color.blue : Color.yellow;
            gizmoColorUp = exitSide == Side.Top ? Color.blue : Color.yellow;
            gizmoColorLeft = exitSide == Side.Left ? Color.blue : Color.yellow;

            base.OnDrawGizmos();
        }
    }
}