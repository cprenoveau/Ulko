using UnityEngine;

namespace Ulko.World
{
    public class TeleportTrigger : Trigger
    {
        public enum Side
        {
            Left,
            Right,
            Bottom,
            Top,
            Any
        }

        public string id;
        public Side exitSide;
        public Data.AreaTag toArea;
        public string toTeleportId = "entrance";

        public delegate void TeleportDelegate(Vector3 pos, Area area);
        public static event TeleportDelegate OnTrigger;

        public override void OnEnter(Player player)
        {
            if(exitSide == Side.Any)
            {
                var area = Area.FindArea(toArea.id);
                var teleport = area.FindTeleport(toTeleportId);

                OnTrigger?.Invoke(teleport.transform.position, area);
            }
        }

        public override void OnExit(Player player)
        {
            if (exitSide != Side.Any && OnExitSide(player))
            {
                var area = Area.FindArea(toArea.id);
                OnTrigger?.Invoke(TeleportPosition(player), area);
            }
        }

        private Vector3 TeleportPosition(Player player)
        {
            var area = Area.FindArea(toArea.id);
            var teleport = area.FindTeleport(toTeleportId);

            Collider fromCollider = GetComponent<Collider>();
            Collider toCollider = teleport.GetComponent<Collider>();
            Vector3 localPlayerPos = player.transform.position - transform.position;

            switch (exitSide)
            {
                case Side.Left:
                    return new Vector3(
                        toCollider.bounds.min.x,
                        toCollider.transform.position.y,
                        (localPlayerPos.z / fromCollider.bounds.size.z) * toCollider.bounds.size.z + teleport.transform.position.z);

                case Side.Right:
                    return new Vector3(
                        toCollider.bounds.max.x,
                        toCollider.transform.position.y,
                        (localPlayerPos.z / fromCollider.bounds.size.z) * toCollider.bounds.size.z + teleport.transform.position.z);

                case Side.Bottom:
                    return new Vector3(
                        (localPlayerPos.x / fromCollider.bounds.size.x) * toCollider.bounds.size.x + teleport.transform.position.x,
                        toCollider.transform.position.y,
                        toCollider.bounds.min.z);

                case Side.Top:
                    return new Vector3(
                        (localPlayerPos.x / fromCollider.bounds.size.x) * toCollider.bounds.size.x + teleport.transform.position.x,
                        toCollider.transform.position.y,
                        toCollider.bounds.max.z);
            }

            return player.transform.position;
        }

        private bool OnExitSide(Player player)
        {
            var collider = GetComponent<BoxCollider>();

            switch (exitSide)
            {
                case Side.Left:
                    return player.transform.position.x <= collider.bounds.min.x;

                case Side.Right:
                    return player.transform.position.x >= collider.bounds.max.x;

                case Side.Bottom:
                    return player.transform.position.z <= collider.bounds.min.z;

                case Side.Top:
                    return player.transform.position.z >= collider.bounds.max.z;
            }

            return false;
        }

        private TeleportTrigger toTeleport;
        protected override void OnDrawGizmos()
        {
            gizmoColorDown = exitSide == Side.Bottom ? Color.blue : Color.yellow;
            gizmoColorRight = exitSide == Side.Right ? Color.blue : Color.yellow;
            gizmoColorUp = exitSide == Side.Top ? Color.blue : Color.yellow;
            gizmoColorLeft = exitSide == Side.Left ? Color.blue : Color.yellow;

            base.OnDrawGizmos();

            if (!string.IsNullOrEmpty(toTeleportId))
            {
                if(toTeleport == null || toTeleport.id != toTeleportId)
                {
                    var area = Area.FindArea(toArea.id);
                    toTeleport = area?.FindTeleport(toTeleportId);
                }

                if (toTeleport != null)
                {
                    var collider = GetComponent<BoxCollider>();
                    var toCollider = toTeleport.GetComponent<Collider>();

                    Vector3 startPoint = new Vector3(collider.bounds.center.x, collider.transform.position.y, collider.bounds.center.z);
                    Vector3 endPoint = new Vector3(toCollider.bounds.center.x, toCollider.transform.position.y, toCollider.bounds.center.z);

                    DrawArrow(startPoint, endPoint);
                }
            }
        }

        private void DrawArrow(Vector3 startPoint, Vector3 endPoint)
        {
            Debug.DrawLine(startPoint, endPoint, Color.red);

            Vector3 v = (startPoint - endPoint).normalized;
            var cross = Vector3.Cross(v, -Vector3.up).normalized;

            Debug.DrawLine(endPoint, endPoint + (v * 0.25f + cross * 0.25f), Color.red);
            Debug.DrawLine(endPoint, endPoint + (v * 0.25f - cross * 0.25f), Color.red);
        }
    }
}