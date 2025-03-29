using Ulko.Data.Abilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Ulko.Data.Abilities.AbilityTarget;

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
        public CharacterState Owner { get; private set; }
        public TargetSize TargetSize { get; private set; }

        private void Start()
        {
            Localization.LocaleChanged += Refresh;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
        }

        public void Init(Effect effect, CharacterState owner, TargetSize targetSize)
        {
            Effect = effect;
            Owner = owner;
            TargetSize = targetSize;

            Refresh();
        }

        private void Refresh()
        {
            string extraDesc = "";

            if (Effect is Damage damage)
            {
                image.sprite = attackIcon;
                powerText.text = damage.Description(Owner.stats);

                if(damage.condition.FindCondition(typeof(HasType)) is HasType hasType)
                {
                    if (TargetSize == TargetSize.Group)
                    {
                        if (hasType.invert)
                            extraDesc = "without " + TextFormat.Localize(hasType.stat);
                        else
                            extraDesc = "with " + TextFormat.Localize(hasType.stat);
                    }
                    else
                    {
                        if (hasType.invert)
                            extraDesc = "if has no " + TextFormat.Localize(hasType.stat);
                        else
                            extraDesc = "if has " + TextFormat.Localize(hasType.stat);
                    }
                }
            }
            else if (Effect is Heal heal)
            {
                image.sprite = healIcon;
                powerText.text = heal.Description(Owner.stats);
            }
            else if (Effect is GiveStatus giveStatus)
            {
                image.sprite = giveStatus.status.icon;
                powerText.text = giveStatus.Description(Owner.stats);
            }
            else if (Effect is RemoveStatus removeStatus)
            {
                image.sprite = healIcon;
                powerText.text = removeStatus.Description(Owner.stats);
            }
            else if(Effect is ModifyStat modifyStat)
            {
                image.sprite = modifyStat.IsNegative() ? attackIcon : healIcon;
                powerText.text = modifyStat.Description(Owner.stats);
            }
            else
            {
                gameObject.SetActive(false);
            }

            if(TargetSize == TargetSize.Group)
            {
                powerText.text += " " + Localization.Localize("to_all");
            }

            if(!string.IsNullOrEmpty(extraDesc))
            {
                powerText.text += " " + extraDesc;
            }
        }
    }
}
