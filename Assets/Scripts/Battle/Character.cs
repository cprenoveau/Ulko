using Ulko.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Ulko.Data.Abilities;
using Ulko.Data.Characters;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Ulko.Battle
{
    public interface ICharacterInternal
    {
        string Id { get; }
        string IdWithoutSuffix { get; }
        string NameKey { get; }
        string Name { get; }
        CharacterSide CharacterSide { get; }
        int Level { get; }
        int Exp { get; }
        int HP { get; set; }
        Level Stats { get; }

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
        public string NameKey => characterInternal.NameKey;
        public string Name => characterInternal.Name;
        public CharacterSide CharacterSide => characterInternal.CharacterSide;
        public int Level => characterInternal.Level;
        public int Exp => characterInternal.Exp;
        public int HP => characterInternal.HP;
        public bool IsDead => HP <= 0;
        public Level CurrentStats => BaseStats + Buff;
        public Level BaseStats => characterInternal.Stats;
        public Level Buff { get; private set; } = new Level();
        public IEnumerable<AbilityAsset> Abilities => Asset.Abilities;
        public List<StatusState> StatusState { get; private set; } = new List<StatusState>();

        public Vector3 Position { get; private set; }
        public int OrderInParty { get; private set; }
        public Vector2 FacingDirection => characterInternal.FacingDirection;
        public Transform Transform => transform;
        public CharacterAsset Asset => characterInternal.Asset;

        public int CurrentTurn { get; private set; }

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

            foreach(var status in Asset.Statuses)
            {
                StatusState.Add(new StatusState(status, 10000, 0));
            }
        }

        public string Description()
        {
            string str = Name;
            str += " " + Localization.LocalizeFormat("hp_value", HP, CurrentStats.maxHP);

            return str;
        }

        public CharacterState CaptureState()
        {
            var characterState = new CharacterState(Id, NameKey, HP, CharacterSide, BaseStats.Clone(), Buff.Clone(), StatusState.Clone());

            foreach (var statusState in StatusState)
            {
                if(statusState.statusAsset.targetType == StatusAsset.TargetType.Wielder
                    && statusState.statusAsset.applyType == StatusAsset.ApplyType.WhileActive)
                {
                    foreach(var effect in statusState.statusAsset.node.effects.effects)
                    {
                        if(effect is ModifyStat modifyStat)
                        {
                            modifyStat.Apply(characterState);
                        }
                    }
                }
            }

            return characterState;
        }

        public void ApplyState(CharacterState state)
        {
            Buff = state.buff.Clone();

            int originalHP = characterInternal.HP;
            characterInternal.HP = (int)Mathf.Clamp(state.hp, 0, CurrentStats.maxHP);

            StatusState = state.statuses.Clone();
            UpdateStatusCosmetics();

            if (IsDead)
            {
                CurrentAnimState = AnimState.Dead;
            }
            else if (CurrentAnimState == AnimState.Dead)
            {
                SetAnimationState(AnimState.Idle);
            }

            if (originalHP > characterInternal.HP)
            {
                PlayHurtAnimation();
            }
        }

        public int TurnCooldown()
        {
            return (Asset.turnCooldown - 1) - (CurrentTurn % Asset.turnCooldown);
        }

        public void IncrementTurnCount()
        {
            for(int i = 0; i < StatusState.Count;)
            {
                StatusState[i].nTurns++;

                if(StatusState[i].nTurns >= StatusState[i].maxTurns)
                {
                    RemoveStatusCosmetics(StatusState[i].statusAsset.id);
                    StatusState.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            CurrentTurn++;
        }

        private readonly Dictionary<string, CancellationTokenSource> statusCosmeticsTasks = new();
        private void UpdateStatusCosmetics()
        {
            List<string> toRemove = new();

            foreach (string statusId in statusCosmeticsTasks.Keys)
            {
                if (StatusState.FirstOrDefault(s => s.statusAsset.id == statusId) == null)
                    toRemove.Add(statusId);
            }

            foreach(string statusId in toRemove)
            {
                RemoveStatusCosmetics(statusId);
            }

            foreach (var status in StatusState)
            {
                AddStatusCosmetics(status);
            }
        }

        private void AddStatusCosmetics(StatusState status)
        {
            string statusId = status.statusAsset.id;

            if (!statusCosmeticsTasks.ContainsKey(statusId))
            {
                var ctSource = new CancellationTokenSource();
                var task = PlayStatusCosmeticsAsync(status, ctSource.Token);

                statusCosmeticsTasks.Add(statusId, ctSource);
            }
        }

        private void RemoveStatusCosmetics(string statusId)
        {
            if (statusCosmeticsTasks.ContainsKey(statusId))
            {
                statusCosmeticsTasks[statusId].Cancel();
                statusCosmeticsTasks[statusId].Dispose();

                statusCosmeticsTasks.Remove(statusId);
            }
        }

        private async Task PlayStatusCosmeticsAsync(StatusState status, CancellationToken ct)
        {
            await Task.Yield();

            if (ct.IsCancellationRequested)
                return;

            var anchor = GetComponentInChildren<HeadAnchor>();
            if (anchor != null && status.statusAsset.vfxOnHead != null)
            {
                var vfx = status.statusAsset.vfxOnHead;
                var result = await Addressables.InstantiateAsync(vfx.prefab.RuntimeKey, anchor.transform.position, Quaternion.identity, anchor.transform).Task;
                result.name = vfx.name;

                while (!ct.IsCancellationRequested)
                {
                    await Task.Yield();
                }

                Addressables.ReleaseInstance(result);
            }
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
