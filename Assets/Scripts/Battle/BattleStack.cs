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
                actions.Add(new BattleAction(new ActionState(characterAction, characters), node));
            }
        }
    }

    public class BattleAction : IClonable
    {
        public ActionState state;
        public AbilityNode node;

        public BattleAction(ActionState state, AbilityNode node)
        {
            this.state = state;
            this.node = node;
        }

        public void Clone(object source)
        {
            Clone(source as BattleAction);
        }

        public void Clone(BattleAction source)
        {
            state = source.state.Clone();
            node = source.node;
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

        public void Evaluate()
        {
            while (actionStack.Count > 1)
            {
                var action = CurrentAction;
                actionStack.Pop();
                ActionState.EvaluateOutcome(action.state.pendingAction, actionStack.Peek().state);
            }
        }
    }
}