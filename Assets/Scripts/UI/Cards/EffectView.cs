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

                if (damage.damageMultiplier != 0)
                {
                    powerText.text = string.Format("{0} HP", (int)damage.RawValue(Owner.CaptureState()));
                }
                else if (damage.percentDamage != 0)
                {
                    powerText.text = string.Format("{0}% HP", damage.percentDamage);
                }
                else
                {
                    powerText.text = string.Format("{0} HP", damage.flatDamage);
                }
            }
            else if (Effect is Heal heal)
            {
                image.sprite = healIcon;

                powerText.text = "";

                if (heal.revive)
                    powerText.text = string.Format("{0} +", Localization.Localize("revives"));

                if (heal.healMultiplier != 0)
                {
                    powerText.text += string.Format("<color=#00FF00>{0}</color> HP", (int)heal.RawValue(Owner.CaptureState()));
                }
                else if (heal.percentHeal != 0)
                {
                    powerText.text += Localization.LocalizeFormat("heal_percent_desc", heal.percentHeal);
                }
                else if (heal.flatHeal != 0)
                {
                    powerText.text += string.Format("<color=#00FF00>{0}</color> HP", heal.flatHeal);
                }
            }
            else if (Effect is GiveStatus giveStatus)
            {
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
