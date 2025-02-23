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

        public List<SpriteAnimation> GetAnimation(string id)
        {
            var anims = new List<SpriteAnimation>();
            var anim = animations.Find(a => a.tag.id == id);
            if (anim != null) anims.Add(anim.animation);

            return anims;
        }
    }
}
