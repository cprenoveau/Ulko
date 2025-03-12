using HotChocolate.UI;
using HotChocolate.UI.MenuAnimations;
using HotChocolate.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Ulko.UI
{
    public class MainMenu : Menu
    {
        public enum ButtonType
        {
            Stats,
            Cards,
            Swap,
            StatusInfo,
            Settings,
            Load,
            Quit,
            Assist
        }

        [Serializable]
        public class MenuButton
        {
            public ButtonType type;
            public PointedButton button;
            public MenuDefinition menu;
            public bool selectHero;
        }

        public TMP_Text locationText;
        public TMP_Text timeText;
        public GameObject heroPrefab;
        public RectTransform heroParent;
        public List<MenuButton> buttons = new();

        private ButtonType current;

        private MenuStack stack;
        private MenuData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        private void Awake()
        {
            Localization.LocaleChanged += RefreshText;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= RefreshText;
        }

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as MenuData;

            foreach(var button in buttons)
            {
                button.button.onClick.AddListener(() => { SelectMenuButton(button); });
            }

            this.data.gameState.MakeSinglePlayer();
        }

        protected override void _OnPop()
        {
            data.gameState.MakeMultiPlayer();
        }

        protected override void _OnFocusIn(bool fromPush, string previousMenu)
        {
            data.uiRoot.SetInfo(null);

            RefreshHeroes();
            RefreshText();

            var superSelected = GetSuperSelectedMenu();
            if (superSelected != null)
            {
                superSelected.SuperSelect(false);
            }

            superSelected = GetSuperSelectedHero();
            if (superSelected != null)
            {
                superSelected.SuperSelect(false);
            }

            FocusMenu(true);
            FocusHeroes(false);
        }

        protected override void _OnFocusOut(bool fromPop, string nextMenu)
        {
            if (!fromPop)
            {
                var superSelected = GetSuperSelectedMenu();
                if (superSelected != null)
                {
                    superSelected.SuperSelect(false);
                }
            }
        }

        private void RefreshText()
        {
            locationText.text = Localization.Localize("locations", PlayerProfile.CurrentLocation.area);
            timeText.text = TextFormat.Time(PlayerProfile.Time);
        }

        private void Update()
        {
            timeText.text = TextFormat.Time(PlayerProfile.Time);
        }

        private string selectedHero;
        private void RefreshHeroes()
        {
            foreach(Transform child in heroParent)
            {
                HotChocolate.UI.Menu.DisposeAllAnimations(child.gameObject);
                Destroy(child.gameObject);
            }

            int heroIndex = 0;
            var party = PlayerProfile.Party;
            for(int i = 0; i < party.Count(); ++i)
            {
                var hero = party.ElementAt(i);
                var heroRef = PlayerProfile.FindHero(PlayerProfile.CurrentStory, PlayerProfile.GetProgression(), hero.id);

                if (heroRef == null)
                    continue;

                var instance = Instantiate(heroPrefab, heroParent);
                instance.GetComponentInChildren<HeroView>().Init(heroRef);

                instance.GetComponentInChildren<SlidingPanel>().pushDelay = heroIndex * 0.1f;
                instance.GetComponentInChildren<FadingPanel>().pushDelay = heroIndex * 0.1f;
                instance.GetComponent<PointedButton>().onClick.AddListener(() => { SelectHero(instance); });

                heroIndex++;

                if (selectedHero != null && hero.id == selectedHero)
                {
                    Select(instance.GetComponent<PointedButton>().gameObject);
                }
            }
        }

        private PointedButton GetSuperSelectedHero()
        {
            foreach (Transform hero in heroParent)
            {
                var heroButton = hero.GetComponent<PointedButton>();
                if (heroButton.SuperSelected) return heroButton;
            }
            return null;
        }

        private PointedButton GetSuperSelectedMenu()
        {
            var button = buttons.Find(b => b.button.SuperSelected);
            if (button != null) return button.button;
            return null;
        }

        private MenuButton GetMenuButton(ButtonType type)
        {
            return buttons.Find(b => b.type == type);
        }

        public override bool CanClose()
        {
            return true;
        }

        protected override bool _Cancel()
        {
            var superSelected = GetSuperSelectedHero();
            if (superSelected != null)
            {
                superSelected.SuperSelect(false);
                return true;
            }

            superSelected = GetSuperSelectedMenu();
            if (superSelected != null)
            {
                superSelected.SuperSelect(false);
                FocusMenu(true);
                FocusHeroes(false);

                return true;
            }

            return false;
        }

        private void SelectMenuButton(MenuButton menuButton)
        {
            if(menuButton.type == ButtonType.Quit)
            {
                stack.Pop();
                data.gameState.GoToStartup(default).FireAndForgetTask();
            }
            else if (menuButton.selectHero)
            {
                SuperSelect(menuButton.button);
            }
            else if(menuButton.menu != null)
            {
                ShowMenu(menuButton.menu);
            }
            else if (menuButton.type == ButtonType.Assist)
            {
                var promptData = new PromptData()
                {
                    allowClose = true
                };

                var addPlayerButton = new PromptButtonData()
                {
                    label = Localization.Localize("add_player"),
                    style = PromptButtonStyle.Ok,
                    onClick = () =>
                    {
                        data.uiRoot.ShowMessagePrompt(UIRoot.OkCancelPrompt(
                            "assist_confirm",
                            Localization.Localize("assist_message"),
                            () => {
                                var result = data.gameState.AddBackseatPlayer();
                                switch (result)
                                {
                                    case AddBackseatPlayerResult.Success:
                                        Audio.Player.PlayUISound(Audio.UISoundId.MenuOk);
                                        break;
                                    case AddBackseatPlayerResult.PlayerAlreadyJoined:
                                        Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);
                                        data.uiRoot.ShowMessagePrompt(UIRoot.WarningPrompt("player_already_joined", Localization.Localize("player_already_joined"), null));
                                        break;
                                    case AddBackseatPlayerResult.NoMoreController:
                                        Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);
                                        data.uiRoot.ShowMessagePrompt(UIRoot.WarningPrompt("no_more_controllers", Localization.Localize("no_more_controllers"), null));
                                        break;
                                }
                            }));
                    }
                };

                var dropPlayersButton = new PromptButtonData()
                {
                    label = Localization.Localize("drop_players"),
                    style = PromptButtonStyle.Cancel,
                    onClick = () =>
                    {
                        while(data.gameState.BackseatPlayerCount() > 0)
                            data.gameState.RemoveBackseatPlayer(0);
                    }
                };

                promptData.buttons.Add(addPlayerButton);
                promptData.buttons.Add(dropPlayersButton);

                data.uiRoot.ShowChoicePrompt("assist_menu", promptData);
            }

            current = menuButton.type;
        }

        private void ShowMenu(MenuDefinition menu)
        {
            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);
            stack.Push(menu.asset, menu.id, data);
        }

        private void SuperSelect(PointedButton button)
        {
            var superSelected = GetSuperSelectedMenu();
            if (superSelected != null)
                superSelected.SuperSelect(false);

            button.SuperSelect(true);
            FocusMenu(false);
            FocusHeroes(true);
        }

        private void SelectHero(GameObject heroView)
        {
            var hero = heroView.GetComponentInChildren<HeroView>().Data;
            selectedHero = hero.id;

            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

            var button = GetMenuButton(current);
            if (button.menu != null)
            {
                stack.Push(button.menu.asset, button.menu.id, new HeroMenuData { gameState = data.gameState, uiRoot = data.uiRoot, hero = heroView.GetComponentInChildren<HeroView>().Data });
            }
            else if(GetSuperSelectedHero() == null)
            {
                heroView.GetComponent<PointedButton>().SuperSelect(true);
            }
            else if(button.type == ButtonType.Swap)
            {
                PlayerProfile.SwapPartyMember(GetSuperSelectedHero().GetComponentInChildren<HeroView>().Data.id, heroView.GetComponentInChildren<HeroView>().Data.id);

                _Cancel();
                _Cancel();

                RefreshHeroes();
            }
        }

        private void FocusMenu(bool focus)
        {
            foreach (var button in buttons)
            {
                button.button.interactable = focus;
                if (focus && button.type == current)
                {
                    Select(button.button.gameObject);
                }
            }
        }

        private void FocusHeroes(bool focus)
        {
            foreach(Transform hero in heroParent)
            {
                hero.GetComponent<PointedButton>().interactable = focus;
            }

            if (focus)
                heroParent.GetChild(0).GetComponent<PointedButton>().Select();
            else
                selectedHero = null;
        }
    }
}
