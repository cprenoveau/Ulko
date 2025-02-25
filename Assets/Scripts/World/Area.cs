using Ulko.Data;
using System.Linq;
using UnityEngine;
using System.Collections;

namespace Ulko.World
{
    public class Area : MonoBehaviour
    {
        public AreaTag areaTag;
        public Encounters encounters;
        public AmbientAudioConfig ambientAudioConfig;
        public AudioDefinition musicDef;
        public Limits limits;
        public bool isInterior;

        public static Area[] AllAreas => FindObjectsByType<Area>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        public Encounters.Encounter TryPickEncounter(int steps)
        {
            if (encounters == null)
                return null;

            return encounters.TryPickEncounter(steps);
        }

        public void InitArea(Player player, Camera worldCamera)
        {
            StartCoroutine(FindVirtualCamera(player, worldCamera));
        }

        private IEnumerator FindVirtualCamera(Player player, Camera worldCamera)
        {
            yield return new WaitForEndOfFrame();

            var zone = VirtualCameraZone.FindCurrentZone(player);
            if(zone != null)
                zone.Init(worldCamera, player.transform, limits, true);
        }

        public static void EnterArea(string areaId, Player player, Camera worldCamera)
        {
            foreach(var area in AllAreas)
            {
                bool isCurrent = area.areaTag.id == areaId;
                area.gameObject.SetActive(isCurrent);

                if (isCurrent)
                    area.InitArea(player, worldCamera);
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
