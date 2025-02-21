using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ulko.Data.Characters
{
    [CreateAssetMenu(fileName = "EnemyAsset", menuName = "Ulko/Characters/Enemy Asset", order = 1)]
    public class EnemyAsset : CharacterAsset
    {
        [Serializable]
        public class AnimationData
        {
            public AnimationTag tag;
            public SpriteAnimation animation;
        }

        public int level = 1;
        public AssetReferenceT<GameObject> prefab;
        public List<CharacterTag> tags = new();
        public ElementalTag element;
        public Abilities.Ability attackAbility;
        public List<Abilities.Ability> abilities = new();
        public List<Abilities.Status> statuses = new();
        public List<AnimationData> animations = new();

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

        public SpriteAnimation GetAnimation(string id)
        {
            var anim = animations.Find(a => a.tag.id == id);
            if (anim != null) return anim.animation;

            return null;
        }
    }
}
