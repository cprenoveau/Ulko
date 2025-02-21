using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

        public string displayName;
        public AssetReferenceT<GameObject> prefab;
        public Sprite portrait;
        public Sprite fullDrawing;
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
