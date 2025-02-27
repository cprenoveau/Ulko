using Ulko.Battle;
using HotChocolate.UI;
using System.Collections.Generic;
using UnityEngine;
using Ulko.Data.Abilities;

namespace Ulko.UI
{
    public class BattleHudData
    {
        public IGameState gameState;
        public UIRoot uiRoot;
        public BattleInstance battleInstance;
    }

    public class BattleHud : Menu
    {
        public UiConfig config;

        public BattleHeroView heroPrefab;
        public RectTransform heroParent;

        public TextQueue textQueuePrefab;
        public RectTransform textQueueParent;

        private MenuStack stack;
        private BattleHudData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as BattleHudData;

            PlayerProfile.OnPartyChanged += RefreshHeroes;

            this.data.battleInstance.OnCharacterStateChanged += ShowCharacterStateChanged;
        }

        protected override void _OnPop() 
        {
            PlayerProfile.OnPartyChanged -= RefreshHeroes;

            if(data != null)
                data.battleInstance.OnCharacterStateChanged -= ShowCharacterStateChanged;
        }

        protected override void _OnFocusIn(bool fromPush, string previousMenu)
        {
            RefreshHeroes();
            data.uiRoot.SetInfo(null);
        }

        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void RefreshHeroes()
        {
            foreach (Transform child in heroParent)
            {
                HotChocolate.UI.Menu.DisposeAllAnimations(child.gameObject);
                Destroy(child.gameObject);
            }

            foreach(var hero in data.battleInstance.Heroes)
            {
                var instance = Instantiate(heroPrefab, heroParent);
                instance.Init(hero, false);
            }
        }

        private void ShowCharacterStateChanged(CharacterState oldState, CharacterState newState)
        {
            var character = data.battleInstance.FindCharacter(oldState.id);
            if (character != null)
            {
                int hpDiff = newState.hp - oldState.hp;

                if (hpDiff < 0)
                {
                    ShowText(character, (-hpDiff).ToString(), config.damageTextColor);
                }
                else if (hpDiff > 0)
                {
                    ShowText(character, hpDiff.ToString(), config.healTextColor);
                }
            }
        }

        private readonly Dictionary<string, TextQueue> textQueues = new();
        private void ShowText(Character character, string text, Color color)
        {
            if (!textQueues.ContainsKey(character.Id))
            {
                var instance = Instantiate(textQueuePrefab, textQueueParent);
                instance.Init(data.gameState.Camera);
                instance.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                instance.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                textQueues.Add(character.Id, instance);
            }

            textQueues[character.Id].Enqueue(
                text,
                color,
                config.battleTextSpeed,
                config.battleTextInterval,
                config.battleTextDuration,
                character.GetComponentInChildren<ArrowAnchor>().transform);
        }
    }
}
