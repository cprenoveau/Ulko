using Ulko.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Ulko.Data.Abilities;

namespace Ulko.Battle
{
    public interface ICharacterData
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
        float GetStat(Stat stat);
        AbilityAsset Attack { get; }

        int TurnCount { get; set; }

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
        public ICharacterData CharacterData { get; private set; }

        public string Id => CharacterData.Id;
        public string IdWithoutSuffix => CharacterData.IdWithoutSuffix;
        public string Name => CharacterData.Name;
        public CharacterSide CharacterSide => CharacterData.CharacterSide;
        public int Level => CharacterData.Level;
        public int Exp => CharacterData.Exp;

        public Vector3 Position { get; private set; }
        public int OrderInParty { get; private set; }
        public Vector2 FacingDirection => CharacterData.FacingDirection;
        public Transform Transform => transform;

        public int HP => CharacterData.HP;
        public float GetStat(Stat stat) => CharacterData.GetStat(stat);
        public bool IsDead => HP <= 0;
        public int TurnCount => CharacterData.TurnCount;

        public AbilityAsset Attack => CharacterData.Attack;

        public CharacterAnimation CharacterInstance { get; private set; }

        private void Start()
        {
            SetState(CurrentAnimState);
        }

        private void OnDestroy()
        {
            if(CharacterInstance != null)
            {
                Addressables.ReleaseInstance(CharacterInstance.gameObject);
            }
        }

        public void Init(ICharacterData characterInfo, Vector3 position, int orderInParty)
        {
            CharacterData = characterInfo;

            Position = position;
            OrderInParty = orderInParty;
            transform.position = Position;

            if (CharacterInstance != null)
            {
                Addressables.ReleaseInstance(CharacterInstance.gameObject);
            }

            var instance = CharacterData.Instantiate(transform);
            CharacterInstance = instance.GetComponentInChildren<CharacterAnimation>();
            CharacterInstance.transform.localPosition = Vector3.zero;

            CharacterInstance.Stand(FacingDirection);
        }

        public string Description()
        {
            string str = Name;
            str += " " + Localization.LocalizeFormat("hp_value", HP, GetStat(Stat.Fortitude));

            return str;
        }

        public void IncrementTurnCount()
        {
            CharacterData.TurnCount++;
        }

        public void ResetPosition()
        {
            transform.position = Position;
            SetState(CurrentAnimState);
        }

        public void SetState(AnimState state)
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
            var anim = CharacterData.GetAnimation(id);
            CharacterInstance.Play(anim, loop, speed, duration);
        }

        public IEnumerator PlayAnimationAsync(string id, bool loop, bool holdPose, float speed = 1f, float duration = float.PositiveInfinity)
        {
            PlayAnimation(id, loop, speed, duration);

            while (CharacterInstance.IsPlaying)
                yield return null;

            if(!holdPose) SetState(CurrentAnimState);
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

            SetState(CurrentAnimState);

            hurtAnimCoroutine = null;
        }
    }
}
