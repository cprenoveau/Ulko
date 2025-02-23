using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using HotChocolate.Utils;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ulko
{
    public class Main : MonoBehaviour
    {
        [Tooltip("Define the milestone to launch the game into. Leaving this blank will use the name of the scene.")]
        public string startingMilestone;

        public SettingsConfig settingsConfig;

        public TextAsset newGame;
        public TextAsset heroes;
        public TextAsset enemies;

        public LocalizationProvider localizationPrefab;
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

            Settings.Init(main.settingsConfig);
            PlayerProfile.Init(main.newGame);
            await Database.Init(main.stories, main.heroes, main.enemies);

            var locInstance = Instantiate(main.localizationPrefab, permanentContainer.transform);
            Localization.Init(locInstance);

            var gameInstance = Instantiate(main.gameInstancePrefab, permanentContainer.transform);
            await gameInstance.Init(main.playerControllerPrefab, main.contextPrefabs, ct);

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
            }
            else
#endif
            {
                await gameInstance.GoToStartup(ct);
            }
        }
    }
}
