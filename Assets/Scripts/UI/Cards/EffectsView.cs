using Ulko.Data.Abilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using Ulko.Battle;
using static Ulko.Data.Abilities.AbilityTarget;

namespace Ulko.UI
{
    public class EffectsView : MonoBehaviour
    {
        public TMP_Text nothingText;
        public EffectView effectPrefab;
        public RectTransform effectsAnchor;

        public IEnumerable<Effect> Effects { get; private set; } = new List<Effect>();
        public CharacterState Owner { get; private set; }

        private void Start()
        {
            Localization.LocaleChanged += Refresh;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
        }

        public void Init(List<Effect> effects, CharacterState owner, TargetSize targetSize)
        {
            Effects = effects;
            Owner = owner;

            foreach(Transform child in effectsAnchor)
            {
                Destroy(child.gameObject);
            }

            foreach(var effect in effects)
            {
                var effectView = Instantiate(effectPrefab, effectsAnchor);
                effectView.Init(effect, owner, targetSize);
            }

            Refresh();
        }

        private void Refresh()
        {
            nothingText.text = Effects.Count() == 0 ? Localization.Localize("nothing_desc") : "";
        }
    }
}
