using Ulko.Data.Abilities;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data
{
    [CreateAssetMenu(fileName = "CharacterTag", menuName = "Ulko/Character Tag", order = 1)]
    public class CharacterTag : ScriptableObject
    {
        public string id;

        public static bool HasAllTags(ICharacterData character, List<CharacterTag> tags)
        {
            foreach(var tag in tags)
            {
                if (!character.CharacterTags.Contains(tag))
                    return false;
            }

            return true;
        }

        public static bool HasAnyTag(ICharacterData character, List<CharacterTag> tags)
        {
            foreach (var tag in tags)
            {
                if (character.CharacterTags.Contains(tag))
                    return true;
            }

            return tags.Count == 0;
        }
    }
}
