using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ulko
{
    public enum AddBackseatPlayerResult
    {
        Success,
        PlayerAlreadyJoined,
        NoMoreController
    }

    public interface IGameState
    {
        Persistence.Location CurrentLocation { get; }
        Data.Timeline.IMilestone CurrentMilestone { get; }
        Data.Timeline.Level CurrentLevel { get; }
        Data.BattleAsset CurrentBattle { get; }
        Camera Camera { get; }
        Camera UICamera { get; }

        Task GoToStartup(CancellationToken ct);
        Task StartMilestone(Data.Timeline.IMilestone milestone, CancellationToken ct);
        Task StartNextMilestone(CancellationToken ct);

        Task StartBattle(Data.BattleAsset battle, CancellationToken ct);
        Task EndBattle(CancellationToken ct);

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