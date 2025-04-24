using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data
{
    [CreateAssetMenu(fileName = "DialogueAsset", menuName = "Ulko/Dialogue Asset", order = 1)]
    public class DialogueAsset : ScriptableObject
    {
        public List<TextAsset> languages = new();

        public Dialogue CreateDialogue()
        {
            Dialogue dialogue = new();
            return AddNode(dialogue);
        }

        public Dialogue AddNode(Dialogue dialogue)
        {
            JToken[] jTokens = new JToken[languages.Count];
            for (int i = 0; i < jTokens.Length; ++i)
            {
                jTokens[i] = JToken.Parse(languages[i].text);
            }

            dialogue.AddNode(jTokens);
            return dialogue;
        }
    }
}
