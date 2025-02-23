using HotChocolate.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ulko.Battle
{
    public static partial class BattleFunctions
    {
        public enum BattleResult
        {
            None,
            Victory,
            Defeat
        }

        public delegate void PlayerTurn(PlayerAction action);
        public static async Task<BattleResult> DoBattle(
            MonoBehaviour holder,
            BattleInstance instance,
            Camera cam,
            PlayerTurn onPlayerTurn,
            CancellationToken ct)
        {
            var battleResult = BattleResult.None;

            await instance.Battlefield.PlayIntroAnim(instance.Heroes, cam, ct);

            while (!ct.IsCancellationRequested)
            {
                battleResult = await DoHeroesTurn(holder, instance, onPlayerTurn, ct);

                if (battleResult != BattleResult.None)
                    break;

                battleResult = await DoEnemiesTurn(holder, instance, ct);

                if (battleResult != BattleResult.None)
                    break;

                await Task.Yield();

                if (ct.IsCancellationRequested)
                    break;

                battleResult = GetResult(instance);
            }

            return battleResult;
        }

        private static async Task<BattleResult> DoHeroesTurn(
            MonoBehaviour holder,
            BattleInstance instance,
            PlayerTurn onPlayerTurn,
            CancellationToken ct)
        {
            var playerAction = new PlayerAction();
            onPlayerTurn?.Invoke(playerAction);

            var result = await playerAction;

            return BattleResult.None;
        }

        private static async Task<BattleResult> DoEnemiesTurn(MonoBehaviour holder, BattleInstance instance, CancellationToken ct)
        {
            return BattleResult.None;
        }

        public static BattleResult GetResult(BattleInstance instance)
        {
            if (instance.GetHeroes(BattleInstance.FetchCondition.AliveOnly).Count() == 0)
            {
                return BattleResult.Defeat;
            }
            else if(instance.GetEnemies(BattleInstance.FetchCondition.AliveOnly).Count() == 0)
            {
                return BattleResult.Victory;
            }
            else
            {
                return BattleResult.None;
            }
        }

        public static void ResetAllPositions(BattleInstance instance)
        {
            foreach (var hero in instance.Heroes)
            {
                hero.ResetPosition();
            }

            foreach (var enemy in instance.Enemies)
            {
                enemy.ResetPosition();
            }
        }
    }
}
