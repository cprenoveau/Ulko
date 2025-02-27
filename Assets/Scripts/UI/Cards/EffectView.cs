using Ulko.Data.Abilities;
using System;
using TMPro;
using UnityEngine;

namespace Ulko.UI
{
    public class EffectView : MonoBehaviour
    {
        public UiConfig config;
        public Sprite attackIcon;
        public Sprite healIcon;
        public TMP_Text powerText;

        public Effect Effect { get; private set; }

        private void Start()
        {
            Localization.LocaleChanged += Refresh;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
        }

        public void Init(Effect effect)
        {
            Effect = effect;
            Refresh();
        }

        private void Refresh()
        {
            if (Effect is Damage damage)
            {
                if (damage.damageMultiplier != 0)
                {
                    powerText.text = string.Format("{0}% {1}", damage.damageMultiplier * 100, TextFormat.Localize(damage.attackStat));
                }
                else if (damage.percentDamage != 0)
                {
                    powerText.text = string.Format("{0}%", damage.percentDamage);
                }
                else
                {
                    powerText.text = string.Format("{0}<color=#00000000>-</color>", damage.flatDamage);
                }
            }
            else if (Effect is Heal heal)
            {
                powerText.text = "";

                if (heal.revive)
                    powerText.text = string.Format("{0} +", Localization.Localize("revives"));

                if (heal.healMultiplier != 0)
                {
                    powerText.text += " " + string.Format("{0}% {1}", heal.healMultiplier * 100, TextFormat.Localize(heal.healStat));
                }
                else if (heal.percentHeal != 0)
                {
                    powerText.text += " " + Localization.LocalizeFormat("heal_percent_desc", heal.percentHeal);
                }
                else if (heal.flatHeal != 0)
                {
                    powerText.text +=  " " + Localization.LocalizeFormat("heal_flat_desc", heal.flatHeal);
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
