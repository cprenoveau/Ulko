using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko
{
    public partial class PlayerController : MonoBehaviour
    {
        private static List<string> disableUISubmitSource = new List<string>();
        private static List<string> disableUINavSource = new List<string>();

        public static void DisableUISubmit(bool disable, string source)
        {
            if (disable)
            {
                if (!disableUISubmitSource.Contains(source))
                    disableUISubmitSource.Add(source);

                DisableAllPlayersControllerMaps(disable, "UISubmit");
            }
            else
            {
                disableUISubmitSource.Remove(source);

                if (disableUISubmitSource.Count == 0)
                {
                    DisableAllPlayersControllerMaps(disable, "UISubmit");
                }
            }
        }

        public static void DisableUINavigation(bool disable, string source)
        {
            if (disable)
            {
                if (!disableUINavSource.Contains(source))
                    disableUINavSource.Add(source);

                DisableAllPlayersControllerMaps(disable, "UINavigation");
            }
            else
            {
                disableUINavSource.Remove(source);

                if (disableUINavSource.Count == 0)
                {
                    DisableAllPlayersControllerMaps(disable, "UINavigation");
                }
            }
        }

        public readonly struct DisableUINavigationScope : IDisposable
        {
            private readonly string source;

            public DisableUINavigationScope(string source)
            {
                this.source = source;
                DisableUINavigation(true, source);
            }

            public void Dispose()
            {
                DisableUINavigation(false, source);
            }
        }
    }
}
