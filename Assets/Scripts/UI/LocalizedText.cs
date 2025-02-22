using TMPro;
using UnityEngine;

namespace Ulko.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        public string key;

        private void Start()
        {
            Refresh();
            Localization.LocaleChanged += Refresh;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
        }

        private void Refresh()
        {
            GetComponent<TMP_Text>().text = Localization.Localize(key);
        }
    }
}
