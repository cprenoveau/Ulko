using Ulko.Data.Abilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ulko.Battle;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

namespace Ulko.UI
{
    public class EffectView : MonoBehaviour
    {
        public UiConfig config;
        public Sprite attackIcon;
        public Sprite healIcon;
        public Image image;
        public TMP_Text powerText;

        public Effect Effect { get; private set; }
        public Character Owner { get; private set; }

        private void Start()
        {
            Localization.LocaleChanged += Refresh;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
        }

        public void Init(Effect effect, Character owner)
        {
            Effect = effect;
            Owner = owner;

            Refresh();
        }

        private void Refresh()
        {
            if (Effect is Damage damage)
            {
                image.sprite = attackIcon;
                powerText.text = damage.Description(Owner.CaptureState());
            }
            else if (Effect is Heal heal)
            {
                image.sprite = healIcon;
                powerText.text = heal.Description(Owner.CaptureState());
            }
            else if (Effect is GiveStatus giveStatus)
            {
                image.sprite = giveStatus.status.icon;
                powerText.text = giveStatus.Description();
            }
            //else if(effect is RemoveStatus removeStatus)
            //{
            //    image.sprite = healIcon;
            //    powerText.text = string.Format(Localization.Localize("heals_status"), Localization.Localize(removeStatus.status.id));
            //}
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
