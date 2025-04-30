using Ulko.Data.Abilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using HotChocolate.Utils;

namespace Ulko.Battle
{
    public class BattleActions
    {
        public int cardIndex;
        public AbilityAsset ability;
        public bool isCardThrow;
        public string actorId;
        public List<string> targetIds;
        public List<BattleAction> actions = new();

        public BattleActions(int cardIndex, AbilityAsset ability, bool isCardThrow, string actorId, List<string> targetIds, List<CharacterState> characters)
        {
            this.cardIndex = cardIndex;
            this.ability = ability;
            this.isCardThrow = isCardThrow;
            this.actorId = actorId;
            this.targetIds = targetIds;

            foreach (var node in ability.nodes)
            {
                var characterAction = new CharacterAction(actorId, targetIds, node.effects.effects);
                actions.Add(new BattleAction(node, new ActionState(characterAction, characters)));
            }
        }
    }

    public class BattleAction : IClonable
    {
        public AbilityNode node;
        public ActionState state;

        public BattleAction() { }

        public BattleAction(AbilityNode node, ActionState state)
        {
            this.node = node;
            this.state = state;
        }

        public void Clone(object source)
        {
            Clone(source as BattleAction);
        }

        public void Clone(BattleAction source)
        {
            node = source.node;
            state = source.state.Clone();
        }

        public static async Task PlaySequenceAsTask(BattleInstance instance, BattleAction action, CancellationToken ct)
        {
            await PlaySequenceAsTask(instance, action.node.applySequence, action.state.pendingAction.ActorId, action.state.pendingAction.TargetIds, ct);
        }

        public static async Task PlaySequenceAsTask(BattleInstance instance, AbilitySequence sequence, string actorId, IEnumerable<string> targetIds, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;

            await instance.Battlefield.StartCoroutineAsync(PlaySequence(instance, sequence, actorId, targetIds), ct);
        }

        public static IEnumerator PlaySequence(BattleInstance instance, AbilitySequence sequence, string actorId, IEnumerable<string> targetIds)
        {
            yield return PlaySequence(
                instance.Battlefield,
                sequence,
                instance.FindCharacter(actorId),
                instance.FindCharacters(targetIds));
        }

        public static IEnumerator PlaySequence(MonoBehaviour holder, AbilitySequence sequence, Character actor, List<Character> targets)
        {
            yield return sequence.Play(holder, actor, targets.Cast<ICharacterCosmetics>().ToList());
        }
    }

    public class BattleStack
    {
        public BattleAction CurrentAction => actionStack.Peek();

        private readonly Stack<BattleAction> actionStack = new();

        public BattleStack(BattleAction action)
        {
            actionStack.Push(action.Clone());
        }

        public void Push(BattleAction action)
        {
            actionStack.Push(action.Clone());
        }

        public async Task ApplyOutcome(BattleInstance instance, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;

            await CollapseStack(instance, ct);

            var originalState = CurrentAction.state.Clone();
            ActionState.Apply(CurrentAction.state.pendingAction, CurrentAction.state);

            if (!originalState.Equals(CurrentAction.state))
            {
                await BattleAction.PlaySequenceAsTask(instance, CurrentAction, ct);

                if (ct.IsCancellationRequested)
                    return;

                instance.ApplyState(CurrentAction.state);
            }
        }

        public async Task CollapseStack(BattleInstance instance, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;

            while (actionStack.Count > 1)
            {
                var action = CurrentAction;
                actionStack.Pop();

                //propagate state down
                CurrentAction.state.characters = action.state.characters;

                var originalState = CurrentAction.state.Clone();
                ActionState.Apply(action.state.pendingAction, CurrentAction.state);

                if (!originalState.Equals(CurrentAction.state))
                {
                    await BattleAction.PlaySequenceAsTask(instance, action, ct);

                    if (ct.IsCancellationRequested)
                        return;
                }
            }
        }
    }
}