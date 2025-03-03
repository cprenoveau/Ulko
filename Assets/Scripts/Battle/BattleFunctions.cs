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
                instance.IncrementTurnCount();

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
            var playerAction = new PlayerAction(instance.GetPossibleHeroActions());
            onPlayerTurn?.Invoke(playerAction);

            var action = await playerAction;

            if (action.IsCancelled)
            {
                return BattleResult.None;
            }

            Debug.Log(action.SelectedAction.actorId + " uses " + action.SelectedAction.ability.id);

            instance.CurrentHand.DiscardAt(action.SelectedAction.cardIndex, instance.DiscardPile);

            foreach(var battleAction in action.SelectedAction.actions)
            {
                var actor = instance.FindCharacter(battleAction.state.pendingAction.actorId);
                if (actor.IsDead && !action.SelectedAction.isCardThrow)
                    continue;

                battleAction.state.characters = instance.CaptureCharacterStates();

                if (battleAction.node.forceValidTarget)
                    ForceValidTarget(instance, action.SelectedAction.ability.target, battleAction.state);

                var battleStack = new BattleStack(battleAction);

                if(!action.SelectedAction.isCardThrow)
                    await ApplyStatusOnAction(battleStack, instance, ct);

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
            var possibleActions = instance.GetPossibleEnemyActions(enemy);

            int actionIndex = UnityEngine.Random.Range(0, possibleActions.Count);
            var action = possibleActions[actionIndex];

            Debug.Log(enemy.Id + " uses " + action.ability.id);

            foreach (var battleAction in action.actions)
            {
                var actor = instance.FindCharacter(battleAction.state.pendingAction.actorId);
                if (actor.IsDead)
                    continue;

                battleAction.state.characters = instance.CaptureCharacterStates();

                if (battleAction.node.forceValidTarget)
                    ForceValidTarget(instance, action.ability.target, battleAction.state);

                var battleStack = new BattleStack(battleAction);

                await ApplyStatusOnAction(battleStack, instance, ct);
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

        private static async Task ApplyStatusOnAction(BattleStack battleStack, BattleInstance instance, CancellationToken ct)
        {
            Dictionary<string, HashSet<StatusAsset>> usedStatus = new();

            ActionState originalState;
            do
            {
                originalState = battleStack.CurrentAction.state.Clone();

                foreach (var character in battleStack.CurrentAction.state.characters)
                {
                    if(character.hp > 0)
                        ApplyStatusOnAction(battleStack, character, ref usedStatus);
                }

                await battleStack.CollapseStack(instance, ct);
            }
            while (!originalState.Equals(battleStack.CurrentAction.state));
        }

        private static void ApplyStatusOnAction(BattleStack battleStack, CharacterState actorState, ref Dictionary<string, HashSet<StatusAsset>> usedStatus)
        {
            foreach (var statusState in actorState.statuses)
            {
                if (statusState.statusAsset.applyType == StatusAsset.ApplyType.OnAction
                    && (!usedStatus.ContainsKey(actorState.id) || !usedStatus[actorState.id].Contains(statusState.statusAsset))
                    && (!statusState.statusAsset.node.HasEffectOfType(Effect.EffectType.BecomeTarget) || !ContainsEffectType(Effect.EffectType.BecomeTarget, ref usedStatus))
                    && statusState.statusAsset.condition.IsTrue(actorState, battleStack.CurrentAction.state))
                {
                    Debug.Log(actorState.id + " reacts with " + statusState.statusAsset.id);

                    var characterAction = CreateActionFromStatus(battleStack.CurrentAction, actorState, statusState);

                    var battleAction = new BattleAction(
                        statusState.statusAsset.node,
                        new ActionState(characterAction, battleStack.CurrentAction.state.characters));

                    battleStack.Push(battleAction);

                    if (!usedStatus.ContainsKey(actorState.id))
                        usedStatus.Add(actorState.id, new HashSet<StatusAsset>());

                    usedStatus[actorState.id].Add(statusState.statusAsset);
                }
            }
        }

        private static bool ContainsEffectType(Effect.EffectType effectType, ref Dictionary<string, HashSet<StatusAsset>> usedStatus)
        {
            foreach(var status in usedStatus.Values)
            {
                if (status.FirstOrDefault(s => s.node.HasEffectOfType(effectType)) != null)
                    return true;
            }

            return false;
        }

        private static CharacterAction CreateActionFromStatus(BattleAction battleAction, CharacterState actorState, StatusState statusState)
        {
            List<string> targetIds = new();
            switch (statusState.statusAsset.targetType)
            {
                case StatusAsset.TargetType.ActionTarget:
                    targetIds = battleAction.state.pendingAction.targetIds;
                    break;

                case StatusAsset.TargetType.ActionActor:
                    targetIds = new List<string> { battleAction.state.pendingAction.actorId };
                    break;

                case StatusAsset.TargetType.Wielder:
                    targetIds = new List<string> { actorState.id };
                    break;
            }

            return new CharacterAction(actorState.id, targetIds, statusState.statusAsset.node.effects.effects);
        }
    }
}
