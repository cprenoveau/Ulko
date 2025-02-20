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

        public async Task InitAsync(CancellationToken ct)
        {
            Scene.UseAddressables(sceneStack);
        }

        public async Task GoToStartupAsync(CancellationToken ct)
        {
            await sceneStack.Jump(startupScene, null);
        }
    }
}
