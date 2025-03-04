using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko
{
    public class CharacterAnimation : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;

        public DirectionalSpriteAnimation idle;
        public DirectionalSpriteAnimation walk;
        public int footstepFrameInterval = 4;

        public List<CharacterAnimation> children = new();

        public enum State
        {
            None,
            Idle,
            Walk,
            Custom
        }

        public State CurrentState { get; private set; }
        public Vector2 CurrentDirection { get; private set; }
        public Coroutine CurrentRoutine { get; private set; }
        public DirectionalSpriteAnimation CurrentAnimation => CurrentState == State.Idle ? idle : CurrentState == State.Walk ? walk : null;
        public int CurrentFrameIndex => CurrentAnimation != null ? CurrentAnimation.CurrentFrameIndex : -1;
        public bool IsFootstep => CurrentState == State.Walk && CurrentFrameIndex != -1 && CurrentFrameIndex % footstepFrameInterval == 0;
        public bool IsPlaying => CurrentRoutine != null;

        private List<SpriteAnimation> lastPlayed;

        private bool IsCurrentlyPlaying(List<SpriteAnimation> anims)
        {
            if (CurrentState != State.Custom || CurrentRoutine == null || lastPlayed == null)
                return false;

            for(int i = 0; i < anims.Count; ++i)
            {
                if (lastPlayed[i] != anims[i])
                    return false;
            }

            return true;
        }

        public void Play(List<SpriteAnimation> anims, bool loop, float speed = 1f, float duration = float.PositiveInfinity)
        {
            if (IsCurrentlyPlaying(anims))
                return;

            lastPlayed = anims;
            CurrentState = State.Custom;

            if (CurrentRoutine != null)
            {
                StopCoroutine(CurrentRoutine);
                CurrentRoutine = null;
            }

            foreach (var child in children)
                child.spriteRenderer.enabled = false;

            for (int i = 0; i < anims.Count; ++i)
            {
                if (i == 0)
                {
                    spriteRenderer.enabled = true;
                    CurrentRoutine = StartCoroutine(anims[i].Play(this, spriteRenderer, loop, speed, duration, () => { CurrentRoutine = null; }));
                }
                else if (i <= children.Count)
                {
                    children[i - 1].spriteRenderer.enabled = true;
                    children[i - 1].Play(new List<SpriteAnimation> { anims[i] }, loop, speed, duration);
                }
            }
        }

        public void Stand(Vector2 direction, float speed = 1f)
        {
            if (State.Idle != CurrentState || direction != CurrentDirection)
            {
                CurrentDirection = direction;
                CurrentState = State.Idle;

                if (CurrentRoutine != null)
                {
                    StopCoroutine(CurrentRoutine);
                    CurrentRoutine = null;
                }

                foreach (var child in children)
                {
                    if(child.gameObject.activeSelf)
                        child.Stand(direction, speed);
                }

                if (idle != null)
                {
                    spriteRenderer.enabled = true;
                    var anim = idle.GetAnimation(direction);
                    anim.SetFirstFrame(spriteRenderer);
                    CurrentRoutine = StartCoroutine(idle.Play(this, spriteRenderer, direction, true, speed));
                }
                else
                {
                    spriteRenderer.enabled = false;
                }
            }
        }

        public void Walk(Vector2 direction, float speed = 1f)
        {
            State newState = direction != Vector2.zero ? State.Walk : State.Idle;
            if (newState != CurrentState || direction != CurrentDirection)
            {
                CurrentState = newState;

                if (CurrentRoutine != null)
                {
                    StopCoroutine(CurrentRoutine);
                    CurrentRoutine = null;
                }

                foreach (var child in children)
                {
                    if(child.gameObject.activeSelf)
                        child.Walk(direction, speed);
                }

                if (CurrentState == State.Idle)
                {
                    if (idle != null)
                    {
                        spriteRenderer.enabled = true;
                        CurrentRoutine = StartCoroutine(idle.Play(this, spriteRenderer, CurrentDirection, true, speed));
                    }
                    else
                    {
                        spriteRenderer.enabled = false;
                    }
                }
                else
                {
                    CurrentDirection = direction;

                    if (walk != null)
                    {
                        spriteRenderer.enabled = true;
                        CurrentRoutine = StartCoroutine(walk.Play(this, spriteRenderer, direction, true, speed));
                    }
                    else
                    {
                        spriteRenderer.enabled = false;
                    }
                }
            }
        }
    }
}
