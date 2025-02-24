﻿using Ulko.Data.Abilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ulko.Battle
{
    public class BattleAction : IClonable
    {
        public ActionState state;
        public AbilitySequence sequence;

        public BattleAction(ActionState state, AbilitySequence sequence)
        {
            this.state = state;
            this.sequence = sequence;
        }

        public void Clone(object source)
        {
            Clone(source as BattleAction);
        }

        public void Clone(BattleAction source)
        {
            state = source.state.Clone();
            sequence = source.sequence;
        }

        public IEnumerator PlaySequence(BattleInstance instance)
        {
            yield return PlaySequence(
                instance.Battlefield,
                sequence,
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
                ActionState.Apply(action.state.pendingAction, actionStack.Peek().state);
            }
        }
    }
}