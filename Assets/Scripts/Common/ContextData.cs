using System;
using UnityEngine;

namespace Ulko
{
    public enum AddBackseatPlayerResult
    {
        Success,
        PlayerAlreadyJoined,
        NoMoreController
    }

    public interface IContextData
    {
        Persistence.Location CurrentLocation { get; }
        Data.Timeline.Level CurrentLevel { get; }
        Data.Battle CurrentBattle { get; }
        Camera Camera { get; }

        void StartGame();
        void GoToStartup();
        void StartNextMilestone();
        Data.Timeline.IMilestone GetCurrentMilestone();

        void StartBattle(Data.Battle battle);
        void EndBattle();

        void StartMinigame();
        void EndMinigame();

        void MakeSinglePlayer();
        void MakeMultiPlayer();

        int BackseatPlayerCount();
        AddBackseatPlayerResult AddBackseatPlayer();
        bool RemoveBackseatPlayer(int playerIndex);

        event Action OnPlayerControllersChanged;

        event Action OnShowNext;
        event Action OnShowPrevious;
    }
}