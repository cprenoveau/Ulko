using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ulko.Data.Abilities;
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
            BattleInstance instance,
            Camera cam,
            PlayerTurn onPlayerTurn,
            CancellationToken ct)
        {
            var battleResult = BattleResult.None;

            await instance.Battlefield.PlayIntroAnim(instance.Heroes, cam, ct);

            while (!ct.IsCancellationRequested)
            {
                battleResult = await DoHeroesTurn(instance, onPlayerTurn, ct);

                if (battleResult != BattleResult.None)
                    break;

                battleResult = await DoEnemiesTurn(instance, ct);

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
            BattleInstance instance,
            PlayerTurn onPlayerTurn,
            CancellationToken ct)
        {
            var playerAction = new PlayerAction(PlayerAction.GetPossibleActions(instance));
            onPlayerTurn?.Invoke(playerAction);

            var action = await playerAction;

            Debug.Log(action.SelectedAction.actorId + " uses " + action.SelectedAction.ability.id);

            foreach(var battleAction in action.SelectedAction.actions)
            {
                battleAction.state.characters = instance.CaptureCharacterStates();

                if (battleAction.node.forceValidTarget)
                    ForceValidTarget(instance, battleAction.ability.target, battleAction.state);

                var battleStack = new BattleStack(battleAction);
                await battleStack.ApplyOutcome(instance, ct);

                await Task.Delay(1000, ct);

                if (ct.IsCancellationRequested)
                    return BattleResult.None;

                ResetAllPositions(instance);

                var result = GetResult(instance);
                if (result != BattleResult.None)
                    return result;
            }

            await Task.Delay(1000, ct);

            return BattleResult.None;
        }

        private static async Task<BattleResult> DoEnemiesTurn(BattleInstance instance, CancellationToken ct)
        {
            foreach(var enemy in instance.GetEnemies(BattleInstance.FetchCondition.AliveOnly))
            {
                var result = await DoEnemyTurn(enemy, instance, ct);
                if (result != BattleResult.None)
                    return result;
            }

            return BattleResult.None;
        }

        private static async Task<BattleResult> DoEnemyTurn(Character enemy, BattleInstance instance, CancellationToken ct)
        {
            var possibleActions = instance.GetPossibleActions(new List<Character> { enemy });
            var action = possibleActions.FirstOrDefault();

            Debug.Log(enemy.Id + " uses " + action.ability.id);

            foreach (var battleAction in action.actions)
            {
                battleAction.state.characters = instance.CaptureCharacterStates();

                if (battleAction.node.forceValidTarget)
                    ForceValidTarget(instance, battleAction.ability.target, battleAction.state);

                var battleStack = new BattleStack(battleAction);
                await battleStack.ApplyOutcome(instance, ct);

                await Task.Delay(1000, ct);

                if (ct.IsCancellationRequested)
                    return BattleResult.None;

                ResetAllPositions(instance);
            }

            await Task.Delay(1000, ct);

            return GetResult(instance);
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

        private static void ForceValidTarget(BattleInstance instance, AbilityTarget abilityTarget, ActionState state)
        {
            var actor = state.FindCharacter(state.pendingAction.actorId);
            var candidates = instance.GetTargetCandidates(abilityTarget, actor);

            for (int i = 0; i < state.pendingAction.targetIds.Count; ++i)
            {
                var target = state.FindCharacter(state.pendingAction.targetIds[i]);

                if (!abilityTarget.IsValidTarget(actor, target))
                {
                    var newTarget = instance.GetRandomSingleTarget(candidates);
                    if (newTarget != null)
                    {
                        state.pendingAction.targetIds[i] = newTarget.Id;
                    }
                }
            }
        }
    }
}
