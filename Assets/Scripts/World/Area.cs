using Ulko.Data;
using System.Linq;
using UnityEngine;

namespace Ulko.World
{
    public class Area : MonoBehaviour
    {
        public AreaTag areaTag;
        public Encounters encounters;
        public AmbientConfig ambientConfig;
        public AudioDefinition musicDef;
        public Limits limits;
        public MissionObjective objective;
        public bool isInterior;

        public static Area[] AllAreas => FindObjectsByType<Area>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        public Encounters.Encounter TryPickEncounter(int steps)
        {
            if (encounters == null)
                return null;

            return encounters.TryPickEncounter(steps);
        }

        public static void SetCurrentArea(string areaId)
        {
            foreach(var area in AllAreas)
            {
                area.gameObject.SetActive(area.areaTag.id == areaId);
            }
        }

        public static Area FindArea(string areaId)
        {
            return AllAreas.FirstOrDefault(a => a.areaTag.id == areaId);
        }

        public SpawnPoint FindSpawnPoint(string spawnPointId)
        {
            var spawnPoints = GetComponentsInChildren<SpawnPoint>();
            return spawnPoints.FirstOrDefault(s => s.spawnPointTag.id == spawnPointId);
        }

        public TeleportTrigger FindTeleport(string teleportId)
        {
            var teleports = GetComponentsInChildren<TeleportTrigger>();
            return teleports.FirstOrDefault(s => s.id == teleportId);
        }
    }
}
