using TMPro;
using Ulko.Data.Abilities;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class HudStatusView : MonoBehaviour
    {
        public Image icon;
        public TMP_Text valueText;

        public void Init(StatusState statusState)
        {
            int remainingTurns = statusState.maxTurns - statusState.nTurns;

            valueText.text = TextFormat.NumberOfTurns(remainingTurns);
            icon.sprite = statusState.statusAsset.icon;
        }
    }
}
