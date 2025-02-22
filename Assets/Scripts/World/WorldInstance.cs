using Ulko.Data.Timeline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ulko.World
{
    public class WorldInstance : IDisposable
    {
        public event Action<Area> OnAreaEntered;
        public event Action<Area> OnAreaExited;

        public delegate void ShowDialogueDelegate(Data.Dialogue dialogue, Action callback);
        public event ShowDialogueDelegate OnShowDialogue;

        public event Action OnNextMilestone;
        public event Action OnSaveGame;

        public delegate bool CanInteractDelegate();
        public CanInteractDelegate CanInteract { get; private set; }
        public Area CurrentArea { get; private set; }
        public MissionObjective CurrentObjective => CurrentArea != null ? CurrentArea.objective : null;
        public Player Player { get; private set; }
        public List<PlayerFollower> PlayerFollowers { get; private set; } = new List<PlayerFollower>();

        public Camera WorldCamera { get; private set; }

        private IMilestone milestone;
        private readonly Player playerPrefab;
        private readonly PlayerFollower followerPrefab;
        private readonly Transform parent;
        private readonly WorldConfig config;

        private readonly Action showScreenshot;
        private readonly Action hideScreenshot;

        private int playerSteps;

        public static WorldInstance Create(
            Player playerPrefab,
            PlayerFollower followerPrefab,
            Transform parent,
            WorldConfig config,
            CanInteractDelegate canInteract,
            Camera worldCamera,
            Action showScreenshot,
            Action hideScreenshot)
        {
            return new WorldInstance(playerPrefab, followerPrefab, parent, config, canInteract, worldCamera, showScreenshot, hideScreenshot);
        }

        private WorldInstance(
            Player playerPrefab,
            PlayerFollower followerPrefab,
            Transform parent,
            WorldConfig config,
            CanInteractDelegate canInteract,
            Camera worldCamera,
            Action showScreenshot,
            Action hideScreenshot)
        {
            this.playerPrefab = playerPrefab;
            this.followerPrefab = followerPrefab;
            this.parent = parent;
            this.config = config;
            this.showScreenshot = showScreenshot;
            this.hideScreenshot = hideScreenshot;

            CanInteract = canInteract;
            WorldCamera = worldCamera;
        }

        public void Begin(IMilestone milestone)
        {
            this.milestone = milestone;

            playerSteps = 0;

            if(Player != null)
                Player.ResetSteps();

            RefreshParty(milestone);
            RegisterEvents();

            FreezePlayer = false;
            Player.rigidBody.useGravity = true;

            var milestoneSpecifics = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IMilestoneSpecific>();
            foreach(var milestoneSpecific in milestoneSpecifics)
            {
                milestoneSpecific.Init(milestone);
            }
        }

        public void End()
        {
            foreach (var follower in PlayerFollowers)
            {
                follower.Move();
            }

            Player.CharacterInstance.Walk(Vector2.zero);

            foreach (var follower in PlayerFollowers)
            {
                follower.CharacterInstance.Walk(Vector2.zero);
            }

            ExitArea();

            UnregisterEvents();
        }

        public void Dispose()
        {
            if(Player != null)
                GameObject.Destroy(Player.gameObject);

            Player = null;

            foreach(var follower in PlayerFollowers)
            {
                GameObject.Destroy(follower.gameObject);
            }

            PlayerFollowers.Clear();

            UnregisterEvents();
        }

        private void RegisterEvents()
        {
            UnregisterEvents();

            Player.OnMoved += MoveFollowers;

            PlayerProfile.OnPartyChanged += RefreshParty;

            TeleportTrigger.OnTrigger += Teleport;
            SoftWall.OnTrigger += HitSoftWall;
            MilestoneTrigger.OnTrigger += StartNextMilestone;
            MilestoneInteractable.OnInteract += StartNextMilestone;
            VirtualCameraZone.OnTrigger += OnVirtualCameraEntered;

            Speaker.OnInteract += TalkTo;
            SavePoint.OnInteract += SaveGame;
        }

        private void UnregisterEvents()
        {
            if(Player != null)
                Player.OnMoved -= MoveFollowers;

            PlayerProfile.OnPartyChanged -= RefreshParty;

            TeleportTrigger.OnTrigger -= Teleport;
            SoftWall.OnTrigger -= HitSoftWall;
            MilestoneTrigger.OnTrigger -= StartNextMilestone;
            MilestoneInteractable.OnInteract -= StartNextMilestone;
            VirtualCameraZone.OnTrigger -= OnVirtualCameraEntered;

            Speaker.OnInteract -= TalkTo;
            SavePoint.OnInteract -= SaveGame;
        }

        private void RefreshParty()
        {
            RefreshParty(milestone);
        }

        private void RefreshParty(IMilestone milestone)
        {
            if(Player == null)
                Player = GameObject.Instantiate(playerPrefab, parent);

            InitFollowers(followerPrefab, parent);

            var party = PlayerProfile.ActiveParty;
            int heroIndex = 0;

            for (int i = 0; i < party.Count(); ++i)
            {
                var hero = party.ElementAt(i);
                var heroAsset = milestone.Party.Find(x => x.id == hero.id);

                if (heroAsset == null)
                    continue;

                if (heroIndex == 0)
                {
                    Player.SetCharacter(heroAsset.Instantiate(Player.visualAnchor));
                }
                else
                {
                    PlayerFollowers[heroIndex - 1].SetCharacter(heroAsset.Instantiate(PlayerFollowers[heroIndex - 1].visualAnchor));
                }

                heroIndex++;
            }
        }

        private void InitFollowers(PlayerFollower followerPrefab, Transform parent)
        {
            var party = PlayerProfile.ActiveParty;

            if (PlayerFollowers.Count == party.Count() - 1)
                return;

            while (PlayerFollowers.Count > party.Count() - 1)
            {
                if (PlayerFollowers.Last() != null)
                    GameObject.Destroy(PlayerFollowers.Last().gameObject);

                PlayerFollowers.RemoveAt(PlayerFollowers.Count - 1);
            }

            while (PlayerFollowers.Count < party.Count() - 1)
            {
                PlayerFollowers.Add(GameObject.Instantiate(followerPrefab, parent));
            }

            for (int i = 0; i < PlayerFollowers.Count; ++i)
            {
                PlayerFollowers[i].Init(Player, i + 1);
            }
        }

        private Vector2? lastDirection;
        private void SetPlayerPosition(Vector3 position)
        {
            if (Player.transform.position == position)
                return;

            Player.Teleport(position);
            Player.CharacterInstance.Stand(lastDirection ?? Vector2.zero);

            foreach (var follower in PlayerFollowers)
            {
                follower.transform.position = position;
                follower.CharacterInstance.Stand(lastDirection ?? Vector2.zero);
            }
        }

        private void EnterArea()
        {
            Player.ResetSteps();

            if (CurrentArea.musicDef == null)
                Audio.Player.StopAll(AudioType.Music);
            else
                Audio.Player.PlaySolo(CurrentArea.musicDef);

            if (CurrentArea.ambientConfig != null)
                CurrentArea.ambientConfig.Play(0.1f);

            Area.SetCurrentArea(CurrentArea.areaTag.id);

            OnAreaEntered?.Invoke(CurrentArea);
        }

        private void OnVirtualCameraEntered(VirtualCameraZone currentZone, VirtualCameraZone newZone)
        {
            newZone.Init(WorldCamera, Player.transform, CurrentArea.limits, false);
        }

        private void ExitArea()
        {
            OnAreaExited?.Invoke(CurrentArea);
            CurrentArea = null;
        }

        public void Teleport(Vector3 pos, Area area)
        {
            Player.StartCoroutine(TeleportAsync(pos, area));
        }

        private IEnumerator TeleportAsync(Vector3 pos, Area area)
        {
            yield return new WaitForEndOfFrame();

            showScreenshot?.Invoke();

            if (CurrentArea != null && CurrentArea.areaTag.id != area.areaTag.id)
            {
                ExitArea();
            }

            SetPlayerPosition(pos);

            if (CurrentArea == null || CurrentArea.areaTag.id != area.areaTag.id)
            {
                CurrentArea = area;
                EnterArea();
            }

            CurrentArea = area;
            PlayerProfile.SetArea(CurrentArea.areaTag.id);

            var currentZone = VirtualCameraZone.FindCurrentZone(Player);
            if (currentZone != null) yield return Player.StartCoroutine(currentZone.InitAsync(WorldCamera, Player.transform, CurrentArea.limits, true));

            hideScreenshot?.Invoke();
        }

        private void StartNextMilestone()
        {
            OnNextMilestone?.Invoke();
        }

        private void SaveGame()
        {
            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);
            OnSaveGame?.Invoke();
        }

        public bool FreezePlayer { get; private set; }
        private void HitSoftWall(SoftWall wall)
        {
            Stand();
            FreezePlayer = true;
            OnShowDialogue?.Invoke(wall.Dialogue, () => { wall.PlayAnimation(Player, () => { FreezePlayer = false; }); });
        }

        private void TalkTo(Speaker speaker)
        {
            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);
            StandTowards(speaker.transform.position);
            PauseNpc(speaker.gameObject);

            OnShowDialogue?.Invoke(speaker.Dialogue, () =>
            {
                ResumeNpc(speaker.gameObject);
            });
        }

        public void Interact()
        {
            Player.Interact();
        }

        public void StandTowards(Vector3 position)
        {
            var v = position - Player.transform.position;
            Player.CharacterInstance.Stand(new Vector2(v.x, v.z));

            foreach (var follower in PlayerFollowers)
            {
                var v2 = position - follower.transform.position;
                follower.CharacterInstance.Stand(new Vector2(v2.x, v2.z));
            }
        }

        public void Stand()
        {
            if (FreezePlayer)
                return;

            Player.CharacterInstance.Walk(Vector2.zero);

            foreach (var follower in PlayerFollowers)
            {
                if (follower != null)
                    follower.CharacterInstance.Walk(Vector2.zero);
            }
        }

        public Data.Encounters.Encounter Walk(Vector3 direction, float deltaTime)
        {
            if (FreezePlayer)
                return null;

            Player.Move(direction, config.playerSpeed, deltaTime);

            if (Player.Steps != playerSteps)
            {
                playerSteps = Player.Steps;

                var encounter = CurrentArea.TryPickEncounter(Player.Steps);
                if(encounter != null)
                {
                    lastDirection = Player.CharacterInstance.CurrentDirection;
                    Player.rigidBody.useGravity = false;

                    return encounter;
                }

            }

            return null;
        }

        private void MoveFollowers()
        {
            foreach (var follower in PlayerFollowers)
            {
                if (follower != null)
                    follower.Move();
            }
        }

        public void PauseNpc(GameObject source)
        {
            var npcBehaviours = source.GetComponentsInChildren<INpcBehaviour>();
            foreach (var behaviour in npcBehaviours)
                behaviour.Pause();
        }

        public void ResumeNpc(GameObject source)
        {
            var npcBehaviours = source.GetComponentsInChildren<INpcBehaviour>();
            foreach (var behaviour in npcBehaviours)
                behaviour.Resume();
        }
    }
}
