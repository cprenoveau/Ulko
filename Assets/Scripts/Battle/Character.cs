using Ulko.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Ulko.Data.Abilities;

namespace Ulko.Battle
{
    public interface ICharacterType
    {
        string Id { get; }
        string IdWithoutSuffix { get; }
        string Name { get; }
        CharacterSide CharacterSide { get; }
        int Level { get; }
        int Exp { get; }

        Vector2 FacingDirection { get; }

        public delegate GameObject InstantiateDelegate(Transform parent);
        public InstantiateDelegate Instantiate { get; }

        int HP { get; set; }
        Level Stats { get; }
        AbilityAsset Attack { get; }

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

        public enum AnimState
        {
            Idle,
            Dead,
            RunInto,
            RunAway
        }

        public AnimState CurrentAnimState { get; private set; }
        public ICharacterType CharacterType { get; private set; }
        public CharacterState CharacterState => new(Id, Name, HP, CharacterSide, Stats);

        public string Id => CharacterType.Id;
        public string IdWithoutSuffix => CharacterType.IdWithoutSuffix;
        public string Name => CharacterType.Name;
        public CharacterSide CharacterSide => CharacterType.CharacterSide;
        public int Level => CharacterType.Level;
        public int Exp => CharacterType.Exp;

        public Vector3 Position { get; private set; }
        public int OrderInParty { get; private set; }
        public Vector2 FacingDirection => CharacterType.FacingDirection;
        public Transform Transform => transform;

        public int HP => CharacterType.HP;
        public Level Stats => CharacterType.Stats;
        public bool IsDead => HP <= 0;

        public AbilityAsset Attack => CharacterType.Attack;

        public CharacterAnimation CharacterInstance { get; private set; }

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

        public void Init(ICharacterType characterInfo, Vector3 position, int orderInParty)
        {
            CharacterType = characterInfo;

            Position = position;
            OrderInParty = orderInParty;
            transform.position = Position;

            if (CharacterInstance != null)
            {
                Addressables.ReleaseInstance(CharacterInstance.gameObject);
            }

            var instance = CharacterType.Instantiate(transform);
            CharacterInstance = instance.GetComponentInChildren<CharacterAnimation>();
            CharacterInstance.transform.localPosition = Vector3.zero;

            CharacterInstance.Stand(FacingDirection);
        }

        public string Description()
        {
            string str = Name;
            str += " " + Localization.LocalizeFormat("hp_value", HP, Stats.GetStat(Stat.Fortitude));

            return str;
        }

        public void ResetPosition()
        {
            transform.position = Position;
            SetAnimationState(CurrentAnimState);
        }

        public void ApplyState(CharacterState state)
        {
            CharacterType.HP = state.hp;
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
            var anim = CharacterType.GetAnimation(id);
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
