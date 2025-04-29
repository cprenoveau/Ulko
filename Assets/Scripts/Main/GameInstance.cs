using Ulko.Persistence;
using Ulko.UI;
using HotChocolate.World;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using Ulko.Data.Timeline;

namespace Ulko
{
    public partial class GameInstance : MonoBehaviour, IGameState
    {
        public string startupScene;

        public SceneStack sceneStack;
        public UIRoot uiRoot;
        public MenuDefinition pauseMenu;
        public MenuDefinition saveMenu;
        public AudioListener audioListener;
        public TMP_Text debugMilestoneText;

        public Volume defaultPostProcess;
        public Volume vhsGlitchEffect;

        public Location CurrentLocation => PlayerProfile.CurrentLocation;
        public IMilestone CurrentMilestone => GetCurrentMilestone();
        public Level CurrentLevel => GetCurrentMilestone() as Level;
        public Cutscene CurrentCutscene => GetCurrentMilestone() as Cutscene;
        public Data.BattleAsset CurrentBattle { get; private set; }
        public Context Context(ContextType type) => contexts[type];
        public Camera Camera => CurrentContext != ContextType.None ? contexts[CurrentContext].Camera : null;
        public Camera UICamera => CurrentContext != ContextType.None ? contexts[CurrentContext].UICamera : null;
        public UIRoot UIRoot => CurrentContext != ContextType.None ? contexts[CurrentContext].UIRoot : uiRoot;

        public event Action OnShowNext;
        public event Action OnShowPrevious;
        public event Action OnToggleCheats;

        public ContextType CurrentContext { get; private set; }
        private readonly Dictionary<ContextType, Context> contexts = new();

        private PlayerController playerControllerPrefab;

        private IMilestone GetCurrentMilestone()
        {
            var progression = PlayerProfile.GetProgression();
            return Database.GetMilestone(PlayerProfile.CurrentStory, progression.act, progression.chapter, progression.milestone);
        }

        public async Task Init(PlayerController playerControllerPrefab, List<Context> contextPrefabs, CancellationToken ct)
        {
            uiRoot.Init();

            this.playerControllerPrefab = playerControllerPrefab;
            MakeSinglePlayer();

            foreach (var prefab in contextPrefabs)
            {
                var instance = Instantiate(prefab, transform);

                instance.Init(this);
                instance.gameObject.SetActive(false);

                contexts.Add(instance.ContextType, instance);
            }

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            debugMilestoneText.gameObject.SetActive(false);
#endif
            Scene.UseAddressables(sceneStack);
            await SetContext(ContextType.None, ct);
        }

        public async Task GoToStartup(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            debugMilestoneText.text = "";
#endif
            Time.timeScale = 1f;

            audioListener.transform.SetParent(transform);

            await sceneStack.Jump(startupScene, null);

            if (ct.IsCancellationRequested)
                return;

            await SetContext(ContextType.Startup, ct);
        }

        public async Task StartMilestone(IMilestone milestone, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;

            CurrentBattle = null;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            debugMilestoneText.text = milestone.Name;
#endif
            await SetContext(ContextType.None, ct);

            PlayerProfile.SetParty(milestone.Party, milestone.SetPartyOrder);

            if (ct.IsCancellationRequested)
                return;

            await sceneStack.Jump(milestone.SceneAddress, null);
            LightingAndWeatherConfig.SetCurrent(milestone.LightingAndWeather, milestone.IsInterior);

            if (milestone is BossBattle battle)
            {
                CurrentBattle = battle.battleAsset;
            }

            await SetContext(milestone.Context, ct);
        }

        public async Task StartNextMilestone(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;

            var milestone = GetCurrentMilestone();
            if (milestone != null && milestone.ShowSavePrompt && !ct.IsCancellationRequested)
            {
                uiRoot.menuStack.Jump(saveMenu.asset, saveMenu.id, null, new MenuData() { gameState = this, uiRoot = uiRoot });

                while (!ct.IsCancellationRequested && (uiRoot.menuStack.PendingCount > 0 || uiRoot.menuStack.Count > 0))
                    await Task.Yield();
            }

            PlayerProfile.SetNextMilestone();

            if (PlayerProfile.IsFirst(PlayerProfile.GetProgression()))
            {
                await GoToStartup(ct);
            }
            else
            {
                await StartMilestone(GetCurrentMilestone(), ct);
            }
        }

        public async Task StartBattle(Data.BattleAsset battle, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;

            CurrentBattle = battle;

            await sceneStack.Jump(battle.sceneAddress, null);

            if (ct.IsCancellationRequested)
                return;

            await SetContext(ContextType.Battle, ct);
        }

        public async Task EndBattle(CancellationToken ct)
        {
            CurrentBattle = null;

            PlayerProfile.DeleteTempState();
            PlayerProfile.SetEncounterIndex(-1);
            PlayerProfile.ReviveParty();

            if (GetCurrentMilestone() is BossBattle)
            {
                await StartNextMilestone(ct);
            }
            else
            {
                await StartMilestone(GetCurrentMilestone(), ct);
            }
        }

        private async Task SetContext(ContextType contextType, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;

            audioListener.transform.SetParent(transform);

            if (CurrentContext != ContextType.None)
                await contexts[CurrentContext].End(ct);

            if (ct.IsCancellationRequested)
                return;

            uiRoot.FadeAmount(1f);

            CurrentContext = contextType;

            if (CurrentContext == ContextType.World)
            {
                MakeMultiPlayer();
            }
            else
            {
                MakeSinglePlayer();
            }

            if (CurrentContext != ContextType.None)
            {
                uiRoot.FadeAmount(0f);

                await contexts[CurrentContext].Begin(ct);

                if (ct.IsCancellationRequested)
                    return;

                audioListener.transform.SetParent(contexts[CurrentContext].Camera.transform);
            }

            audioListener.transform.localPosition = Vector3.zero;
        }

        private void Update()
        {
            if (Time.timeScale != 0)
            {
                Time.timeScale = PlayerController.PlayerOne.IsFastForwarding ? 2f : 1;
                ShowFastForwardEffect(PlayerController.PlayerOne.IsFastForwarding);
            }
            else
            {
                ShowFastForwardEffect(false);
            }

            if (CurrentContext != ContextType.None && CurrentContext != ContextType.Startup)
            {
                PlayerProfile.AddTime(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (CurrentContext != ContextType.None)
                contexts[CurrentContext].Move(PlayerController.PlayerOne.Move, Time.fixedDeltaTime);

#if UNITY_EDITOR
            LightingAndWeatherConfig.RefreshCurrent();
#endif
        }

        private void ShowFastForwardEffect(bool enabled)
        {
            uiRoot.fastForward.SetActive(enabled);

            defaultPostProcess.enabled = !enabled;
            vhsGlitchEffect.enabled = enabled;
        }

        private void OnApplicationQuit()
        {
            PlayerProfile.SuspendGame();
        }
    }
}
