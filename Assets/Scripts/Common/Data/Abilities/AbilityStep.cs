using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HotChocolate.Utils;
using UnityEngine.AddressableAssets;

namespace Ulko.Data.Abilities
{
    public interface ICharacter
    {
        Vector2 FacingDirection { get; }
        Transform Transform { get; }
        IEnumerator PlayAnimationAsync(string id, bool loop, bool holdPose, float speed, float duration);
    }

    public interface IAbilityStep
    {
        bool Blocking { get; }

        IEnumerator Play(MonoBehaviour holder, ICharacter actor, List<ICharacter> targets);
    }

    [Serializable]
    public class MoveToTarget : IAbilityStep
    {
        public bool blocking = true;
        public bool Blocking => blocking;

        public float delay;
        public float speed = 50f;
        public float distance = 1f;
        public bool ranged;

        public IEnumerator Play(MonoBehaviour holder, ICharacter actor, List<ICharacter> targets)
        {
            yield return new WaitForSeconds(delay);

            Vector3 position = targets[0].Transform.position;

            if (actor.FacingDirection.x < 0)
            {
                if (ranged)
                {
                    position.x = actor.Transform.position.x + distance;
                    if(position.x - targets[0].Transform.position.x < distance)
                    {
                        position.x = targets[0].Transform.position.x + distance;
                    }
                }
                else
                {
                    position.x += distance;
                }
            }
            else
            {
                if (ranged)
                {
                    position.x = actor.Transform.position.x - distance;
                    if (targets[0].Transform.position.x - position.x < distance)
                    {
                        position.x = targets[0].Transform.position.x - distance;
                    }
                }
                else
                {
                    position.x -= distance;
                }
            }

            if(!ranged)
                position.z -= 0.1f;

            float maxDistance = (actor.Transform.position - position).magnitude;
            float d = 0;

            while (true)
            {
                Vector3 v = (position - actor.Transform.position);
                Vector3 dir = v.normalized;
                actor.Transform.position += dir * speed * Time.deltaTime;

                d += speed * Time.deltaTime;
                if (d >= maxDistance)
                {
                    actor.Transform.transform.position = position;
                    break;
                }

                yield return null;
            }
        }
    }

    [Serializable]
    public class PlayAnimation : IAbilityStep
    {
        public bool blocking = true;
        public bool Blocking => blocking;

        public float delay;
        public AnimationTag animTag;
        public bool loop;
        public float speed = 1f;
        public float duration = float.PositiveInfinity;
        public bool holdPose;

        public IEnumerator Play(MonoBehaviour holder, ICharacter actor, List<ICharacter> targets)
        {
            yield return new WaitForSeconds(delay);
            yield return actor.PlayAnimationAsync(animTag.id, loop, holdPose, speed, duration);
        }
    }

    [Serializable]
    public class PlayVFX : IAbilityStep
    {
        public bool blocking = true;
        public bool Blocking => blocking;

        public float delay;
        public bool onTargets;
        public Vector3 position;
        public VfxAsset vfx;
        public bool loop;
        public float speed = 1f;
        public float duration = 2f;

        public IEnumerator Play(MonoBehaviour holder, ICharacter actor, List<ICharacter> targets)
        {
            yield return new WaitForSeconds(delay);

            if(vfx.sound != null)
            {
                Audio.Player.Play(vfx.sound);
            }

            if(onTargets)
            {
                for(int i = 0; i < targets.Count; ++i)
                {
                    if (i == targets.Count - 1)
                        yield return PlayVfx(targets[i].Transform.position, vfx);
                    else
                        holder.StartCoroutine(PlayVfx(targets[i].Transform.position, vfx));
                }
            }
            else
            {
                yield return PlayVfx(position, vfx);
            }

            if(vfx.sound != null && vfx.sound.loop)
            {
                Audio.Player.StopAll(vfx.sound);
            }
        }

        private IEnumerator PlayVfx(Vector3 position, VfxAsset vfx)
        {
            var op = Addressables.InstantiateAsync(vfx.prefab.RuntimeKey);
            while (!op.IsDone)
            {
                yield return null;
            }

            var instance = op.Result;
            instance.transform.position = position;

            yield return new WaitForSeconds(duration);

            Addressables.ReleaseInstance(instance);
        }
    }

    [Serializable]
    public class Wait : IAbilityStep
    {
        public bool Blocking => true;

        public float duration;

        public IEnumerator Play(MonoBehaviour holder, ICharacter actor, List<ICharacter> targets)
        {
            yield return new WaitForSeconds(duration);
        }
    }
}
