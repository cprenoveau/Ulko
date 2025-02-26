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
        public AbilityAsset ability;
        public string actorId;
        public List<string> targetIds;
        public List<BattleAction> actions = new();

        public BattleActions(AbilityAsset ability, string actorId, List<string> targetIds, List<CharacterState> characters)
        {
            this.ability = ability;
            this.actorId = actorId;
            this.targetIds = targetIds;

            foreach (var node in ability.AbilityNodes)
            {
                var characterAction = new CharacterAction(actorId, targetIds, node.effects.effects);
                actions.Add(new BattleAction(ability, node, new ActionState(characterAction, characters)));
            }
        }
    }

    public class BattleAction : IClonable
    {
        public AbilityAsset ability;
        public AbilityNode node;
        public ActionState state;

        public BattleAction() { }

        public BattleAction(AbilityAsset ability, AbilityNode node, ActionState state)
        {
            this.ability = ability;
            this.node = node;
            this.state = state;
        }

        public void Clone(object source)
        {
            Clone(source as BattleAction);
        }

        public void Clone(BattleAction source)
        {
            ability = source.ability;
            node = source.node;
            state = source.state.Clone();
        }

        public async Task PlaySequenceAsTask(BattleInstance instance, CancellationToken ct)
        {
            await instance.Battlefield.StartCoroutineAsync(PlaySequence(instance), ct);
        }

        public IEnumerator PlaySequence(BattleInstance instance)
        {
            yield return PlaySequence(
                instance.Battlefield,
                node.applySequence,
                instance.FindCharacter(state.pendingAction.actorId),
                instance.FindCharacters(state.pendingAction.targetIds));
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
            CollapseStack();

            var originalState = CurrentAction.state.Clone();
            ActionState.EvaluateOutcome(CurrentAction.state.pendingAction, CurrentAction.state);

            if (!originalState.Equals(CurrentAction.state))
            {
                await CurrentAction.PlaySequenceAsTask(instance, ct);

                if (ct.IsCancellationRequested)
                    return;

                instance.ApplyState(CurrentAction.state);
            }
        }

        public void CollapseStack()
        {
            while (actionStack.Count > 1)
            {
                var action = CurrentAction;
                actionStack.Pop();

                //propagate state down
                actionStack.Peek().state.characters = action.state.characters;

                ActionState.EvaluateOutcome(action.state.pendingAction, actionStack.Peek().state);
            }
        }
    }
}