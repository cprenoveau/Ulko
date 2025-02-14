using HotChocolate.Utils;
using HotChocolate.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ulko
{
    public class GameInstance : MonoBehaviour
    {
        public string startupScene;
        public SceneStack sceneStack;

        private CancellationTokenSource ctSource = new CancellationTokenSource();

        private void OnDestroy()
        {
            ctSource.Cancel();
            ctSource.Dispose();
        }

        public async Task InitAsync(CancellationToken ct)
        {
            Scene.UseAddressables(sceneStack);
        }

        public void GoToStartup()
        {
            ctSource.Cancel();
            ctSource.Dispose();
            ctSource = new CancellationTokenSource();

            GoToStartupAsync(ctSource.Token).FireAndForgetTask();
        }

        public async Task GoToStartupAsync(CancellationToken ct)
        {
            await sceneStack.Jump(startupScene, null);
        }
    }
}
