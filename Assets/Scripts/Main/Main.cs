using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using HotChocolate.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ulko
{
    public class Main : MonoBehaviour
    {
        public GameInstance gameInstancePrefab;

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

        private void OnDestroy()
        {
            if (ctSource != null)
            {
                ctSource.Cancel();
                ctSource.Dispose();
            }
        }

        private static async Task StartupSequence(Main main, CancellationToken ct)
        {
            permanentContainer = new GameObject("Universe");
            DontDestroyOnLoad(permanentContainer);

            var gameInstance = Instantiate(main.gameInstancePrefab, permanentContainer.transform);
            await gameInstance.InitAsync(ctSource.Token);

            await gameInstance.GoToStartupAsync(ctSource.Token);
        }
    }
}
