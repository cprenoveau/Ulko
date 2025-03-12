using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class HeroMenuData
    {
        public IGameState gameState;
        public UIRoot uiRoot;
        public Data.Characters.HeroAsset hero;
    }

    public class HeroMenu : MonoBehaviour
    {
        public HeroView heroView;
        public Image drawing;

        public HeroMenuData Data { get; private set; }

        private Action onRefresh;

        public void Init(HeroMenuData data, Action onRefresh)
        {
            Data = data;
            this.onRefresh = onRefresh;

            Data.gameState.OnShowNext += ShowNext;
            Data.gameState.OnShowPrevious += ShowPrevious;

            Refresh();
            onRefresh?.Invoke();
        }

        private void OnDestroy()
        {
            if (Data != null)
            {
                Data.gameState.OnShowNext -= ShowNext;
                Data.gameState.OnShowPrevious -= ShowPrevious;
            }
        }

        public void Refresh()
        {
            heroView.Init(Data.hero);
            if (drawing != null) drawing.sprite = Data.hero.fullDrawing;
        }

        public void ShowNext()
        {
            Audio.Player.PlayUISound(Audio.UISoundId.Swish);

            var next = PlayerProfile.GetNextPartyMember(Data.hero.id);
            if (next.id != Data.hero.id)
            {
                Data.hero = next;

                Refresh();
                onRefresh?.Invoke();
            }
        }

        public void ShowPrevious()
        {
            Audio.Player.PlayUISound(Audio.UISoundId.Swish);

            var previous = PlayerProfile.GetPreviousPartyMember(Data.hero.id);
            if (previous.id != Data.hero.id)
            {
                Data.hero = previous;

                Refresh();
                onRefresh?.Invoke();
            }
        }
    }
}
