using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using HotChocolate.Utils;
using System.Collections.Generic;
using HotChocolate.UI.Cheats;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ulko
{
    public class Main : MonoBehaviour
    {
        [Tooltip("Define the milestone to launch the game into. Leaving this blank will use the name of the scene.")]
        public string startingMilestone;
        [Tooltip("Immediately start this encounter after milestone is started.")]
        public Data.BattleAsset startingEncounter;

        public SettingsConfig settingsConfig;
        public CheatUi cheats;

        public TextAsset newGame;
        public TextAsset heroes;
        public TextAsset enemies;

        public LocalizationProvider localizationPrefab;
        public AudioPlayer audioPlayerPrefab;
        public PlayerController playerControllerPrefab;
        public GameInstance gameInstancePrefab;
        public List<Context> contextPrefabs = new();

        public List<Data.Timeline.Story> stories = new();

        private static bool gameStarted;
        private static CancellationTokenSource ctSource;

        private static GameObject permanentContainer;

        /// <summary>
        /// Forces a full clean restart from the main scene
        /// </summary>
        public static void Restart()
        {
            if (Application.isPlaying)
            {
                if (ctSource != null)
                {
                    ctSource.Cancel();
                    ctSource.Dispose();
                }

                if (permanentContainer != null)
                {
                    Destroy(permanentContainer);
                }

                if (SceneManager.GetActiveScene().name != "Main")
                {
                    gameStarted = false;
                    SceneManager.LoadScene("Main");
                }
                else
                {
                    var main = FindFirstObjectByType<Main>();

                    ctSource = new CancellationTokenSource();
                    StartupSequence(main, ctSource.Token).FireAndForgetTask();
                }
            }
        }

        private void Start()
        {
            if (!gameStarted)
            {
                gameStarted = true;

                ctSource = new CancellationTokenSource();
                StartupSequence(this, ctSource.Token).FireAndForgetTask();
            }
        }

        private static async Task StartupSequence(Main main, CancellationToken ct)
        {
            permanentContainer = new GameObject("Universe");
            DontDestroyOnLoad(permanentContainer);

            await Database.Init(main.stories, main.heroes, main.enemies);

            Settings.Init(main.settingsConfig);
            PlayerProfile.Init(main.newGame);

            var locInstance = Instantiate(main.localizationPrefab, permanentContainer.transform);
            Localization.Init(locInstance);

            var audioInstance = Instantiate(main.audioPlayerPrefab, permanentContainer.transform);
            Audio.Init(audioInstance);

            var gameInstance = Instantiate(main.gameInstancePrefab, permanentContainer.transform);
            await gameInstance.Init(main.playerControllerPrefab, main.contextPrefabs, ct);

#if UNITY_EDITOR || DEVELOPMENT_BUILD || ALLOW_CHEATS
            var cheatsInstance = Instantiate(main.cheats, permanentContainer.transform);
            cheatsInstance.Init();

            cheatsInstance.OnOpen += () => { gameInstance.OnCheatOpen(); };
            cheatsInstance.OnClose += () => { gameInstance.OnCheatClosed(); };

            gameInstance.OnToggleCheats += () => { cheatsInstance.Toggle(); };
#endif

#if UNITY_EDITOR

            string milestoneName = null;

            if (!string.IsNullOrEmpty(main.startingMilestone))
                milestoneName = main.startingMilestone;

            if (string.IsNullOrEmpty(milestoneName))
                milestoneName = SceneManager.GetActiveScene().name;

            var (story, prog) = Database.GetProgression(milestoneName);

            if (prog != null && !EditorPrefs.GetBool("ForcePlayFromStartup"))
            {
                PlayerProfile.NewGame();
                PlayerProfile.SetMilestone(milestoneName);

                await gameInstance.StartMilestone(Database.GetMilestone(PlayerProfile.CurrentStory, PlayerProfile.GetProgression()), ct);

                if (main.startingEncounter != null)
                    await gameInstance.StartBattle(main.startingEncounter, ct);
            }
            else
#endif
            {
                await gameInstance.GoToStartup(ct);
            }
        }
    }
}
