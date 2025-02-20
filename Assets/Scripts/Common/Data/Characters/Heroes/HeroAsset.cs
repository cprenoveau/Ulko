using Ulko.Data.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Ulko.Data.Characters.HeroProgression;

namespace Ulko.Data.Characters
{
    [CreateAssetMenu(fileName = "HeroAsset", menuName = "Ulko/Characters/Hero Asset", order = 1)]
    public class HeroAsset : CharacterAsset
    {
        [Serializable]
        public class AnimationData
        {
            public AnimationTag tag;
            public SpriteAnimation animation;
        }

        [Serializable]
        public class WeaponAnimationData
        {
            public Weapon weapon;
            public List<AnimationData> animations = new List<AnimationData>();
        }

        public string displayName;

        [Tooltip("Sets this as the current weapon. Will destroy any weapon that was already equipped.")]
        public Weapon overrideWeapon;
        [Tooltip("Sets this as the current armor. Will destroy any armor that was already equipped.")]
        public List<Armor> overrideArmor = new List<Armor>();

        public AssetReferenceT<GameObject> prefab;
        public Sprite portrait;
        public Sprite fullDrawing;
        public List<CharacterTag> tags = new List<CharacterTag>();
        public ElementalTag element;
        public List<ItemTag> weaponTags = new List<ItemTag>();
        public Abilities.Ability attackAbility;
        public HeroProgression progression;
        public List<WeaponAnimationData> weapons = new List<WeaponAnimationData>();
        public List<AnimationData> animations = new List<AnimationData>();

        public GameObject Instantiate(Transform parent)
        {
            try
            {
                var op = Addressables.InstantiateAsync(prefab.RuntimeKey, parent);
                op.WaitForCompletion();
                return op.Result;
            }
            catch (Exception e)
            {
                Debug.LogError("Could not instantiate " + id + " Exception: " + e.Message);
            }

            return null;
        }

        public List<SpriteAnimation> GetAnimation(string id, string weaponId)
        {
            var anims = new List<SpriteAnimation>();
            var anim = animations.Find(a => a.tag.id == id);
            if (anim != null) anims.Add(anim.animation);

            var weaponAnim = GetWeaponAnimation(weaponId)?.animations.Find(a => a.tag.id == id);
            if (weaponAnim != null) anims.Add(weaponAnim.animation);

            return anims;
        }

        public WeaponAnimationData GetWeaponAnimation(string id)
        {
            return weapons.Find(w => w.weapon.id == id);
        }

        public IEnumerable<AbilityProgression> GetAbilities(int level)
        {
            return progression.GetAbilities(level);
        }

        public IEnumerable<AbilityProgression> GetNextAbilities(int level)
        {
            return progression.GetNextAbilities(level);
        }

        public IEnumerable<StatusProgression> GetStatuses(int level)
        {
            return progression.GetStatuses(level);
        }

        public IEnumerable<StatusProgression> GetNextStatuses(int level)
        {
            return progression.GetNextStatuses(level);
        }
    }
}
