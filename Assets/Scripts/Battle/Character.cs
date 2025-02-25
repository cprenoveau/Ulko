﻿using Ulko.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Ulko.Data.Abilities;
using Ulko.Data.Characters;

namespace Ulko.Battle
{
    public interface ICharacterInternal
    {
        string Id { get; }
        string IdWithoutSuffix { get; }
        string Name { get; }
        CharacterSide CharacterSide { get; }
        int Level { get; }
        int Exp { get; }
        int HP { get; set; }
        Level Stats { get; }
        AbilityAsset Attack { get; }

        Vector2 FacingDirection { get; }
        CharacterAsset Asset { get; }

        public delegate GameObject InstantiateDelegate(Transform parent);
        public InstantiateDelegate Instantiate { get; }

        List<SpriteAnimation> GetAnimation(string id);
    }

    public class Character : MonoBehaviour, ICharacterCosmetics
    {
        public BattleConfig config;

        public float runSpeed = 1.5f;
        public AnimationTag battleStanceTag;
        public AnimationTag hurtTag;
        public AnimationTag dyingTag;
        public AnimationTag deadTag;

        public bool Initialized => characterInternal != null;

        public string Id => characterInternal.Id;
        public string IdWithoutSuffix => characterInternal.IdWithoutSuffix;
        public string Name => characterInternal.Name;
        public CharacterSide CharacterSide => characterInternal.CharacterSide;
        public int Level => characterInternal.Level;
        public int Exp => characterInternal.Exp;
        public int HP => characterInternal.HP;
        public bool IsDead => HP <= 0;
        public Level Stats => characterInternal.Stats;
        public AbilityAsset Attack => characterInternal.Attack;
        public List<StatusState> StatusState { get; private set; } = new List<StatusState>();

        public Vector3 Position { get; private set; }
        public int OrderInParty { get; private set; }
        public Vector2 FacingDirection => characterInternal.FacingDirection;
        public Transform Transform => transform;
        public CharacterAsset Asset => characterInternal.Asset;

        public enum AnimState
        {
            Idle,
            Dead,
            RunInto,
            RunAway
        }

        public AnimState CurrentAnimState { get; private set; }
        public CharacterAnimation CharacterInstance { get; private set; }

        private ICharacterInternal characterInternal;

        private void Start()
        {
            SetAnimationState(CurrentAnimState);
        }

        private void OnDestroy()
        {
            if(CharacterInstance != null)
            {
                Addressables.ReleaseInstance(CharacterInstance.gameObject);
            }
        }

        public void Init(ICharacterInternal characterInternal, Vector3 position, int orderInParty)
        {
            this.characterInternal = characterInternal;

            Position = position;
            OrderInParty = orderInParty;
            transform.position = Position;

            if (CharacterInstance != null)
            {
                Addressables.ReleaseInstance(CharacterInstance.gameObject);
            }

            var instance = this.characterInternal.Instantiate(transform);
            CharacterInstance = instance.GetComponentInChildren<CharacterAnimation>();
            CharacterInstance.transform.localPosition = Vector3.zero;

            CharacterInstance.Stand(FacingDirection);
        }

        public CharacterState CaptureState()
        {
            return new(Id, Name, HP, CharacterSide, Stats, StatusState.Clone());
        }

        public void ApplyState(CharacterState state)
        {
            characterInternal.HP = (int)Mathf.Clamp(state.hp, 0, Stats.MaxHP);
            StatusState = state.statuses.Clone();

            foreach(var status in StatusState)
            {
                Debug.Log(Id + " has status " + status.statusAsset.id + " for " + status.nTurns + "/" + status.maxTurns + " turns ");
            }
        }

        public string Description()
        {
            string str = Name;
            str += " " + Localization.LocalizeFormat("hp_value", HP, Stats.MaxHP);

            return str;
        }

        public void ResetPosition()
        {
            transform.position = Position;
            SetAnimationState(CurrentAnimState);
        }

        public void SetAnimationState(AnimState state)
        {
            if (!gameObject.activeInHierarchy)
                return;

            switch (state)
            {
                case AnimState.Idle:
                    PlayAnimation(battleStanceTag, true);
                    break;

                case AnimState.Dead:
                    PlayAnimation(deadTag, true);
                    break;

                case AnimState.RunInto:
                    CharacterInstance.Walk(new Vector2(-1, 0), runSpeed);
                    break;

                case AnimState.RunAway:
                    CharacterInstance.Walk(new Vector2(1, 0), runSpeed);
                    break;
            }

            CurrentAnimState = state;
        }

        public void PlayAnimation(AnimationTag tag, bool loop, float speed = 1f, float duration = float.PositiveInfinity)
        {
            if (tag != null)
                PlayAnimation(tag.id, loop, speed, duration);
        }

        public void PlayAnimation(string id, bool loop, float speed = 1f, float duration = float.PositiveInfinity)
        {
            var anim = characterInternal.GetAnimation(id);
            CharacterInstance.Play(anim, loop, speed, duration);
        }

        public IEnumerator PlayAnimationAsync(string id, bool loop, bool holdPose, float speed = 1f, float duration = float.PositiveInfinity)
        {
            PlayAnimation(id, loop, speed, duration);

            while (CharacterInstance.IsPlaying)
                yield return null;

            if(!holdPose) SetAnimationState(CurrentAnimState);
        }

        private void PlayHurtAnimation()
        {
            if (hurtAnimCoroutine != null)
                StopCoroutine(hurtAnimCoroutine);

            hurtAnimCoroutine = StartCoroutine(PlayHurtAnimationAsync());
        }

        private Coroutine hurtAnimCoroutine;
        private IEnumerator PlayHurtAnimationAsync()
        {
            yield return PlayAnimationAsync(hurtTag.id, false, false);

            if(CurrentAnimState == AnimState.Dead && dyingTag != null)
                yield return PlayAnimationAsync(dyingTag.id, false, false);

            SetAnimationState(CurrentAnimState);

            hurtAnimCoroutine = null;
        }
    }
}
