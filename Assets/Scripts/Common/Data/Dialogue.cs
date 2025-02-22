using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Ulko.Data
{
    public class Dialogue
    {
        public class Line
        {
            private event Action OnPlay;

            public string speakerKey;
            public Dictionary<string, string> textByLanguage = new();

            public Line(JToken json)
            {
                speakerKey = json["speaker"]?.ToString();

                var locales = Localization.GetLocales();
                foreach (var locale in locales)
                {
                    var text = json[locale.Identifier.Code];
                    if (text != null)
                    {
                        textByLanguage.Add(locale.Identifier.Code, text.ToString());
                    }
                }
            }

            public void RaisePlayEvent()
            {
                OnPlay?.Invoke();
            }

            public string GetSpeakerName()
            {
                return Localization.Localize(speakerKey);
            }

            public string GetText()
            {
                if (textByLanguage.ContainsKey(Localization.CurrentLocaleKey))
                    return textByLanguage[Localization.CurrentLocaleKey];
                else
                    return "$" + textByLanguage["en"];
            }
        }

        public class Page
        {
            public int CurrentLineIndex { get; set; }
            public List<Line> lines = new();

            public Page(JToken json)
            {
                foreach (var entry in json)
                {
                    lines.Add(new Line(entry));
                }
            }

            public Line GetCurrentLine()
            {
                return lines[CurrentLineIndex];
            }

            public bool IsLastLine()
            {
                return CurrentLineIndex == lines.Count - 1;
            }

            public void SetNextLine()
            {
                if (!IsLastLine()) CurrentLineIndex++;
            }
        }

        public class Graph
        {
            public int CurrentPageIndex { get; set; }
            public int CurrentChildIndex { get; set; } = -1;

            public Line choice;
            public Page page;
            public int rewindCount;

            public Graph parent;
            public List<Graph> children = new List<Graph>();

            public Graph(JToken json)
            {
                if(json is JArray)
                {
                    page = new Page(json);
                }
                else
                {
                    page = new Page(json["lines"]);
                    rewindCount = json["rewindCount"]?.ToObject<int>() ?? 0;

                    if (json["choice"] != null)
                        choice = new Line(json["choice"]);

                    if(json["children"] != null)
                    {
                        foreach(var child in json["children"])
                        {
                            var graph = new Graph(child);
                            graph.parent = this;
                            children.Add(graph);
                        }
                    }
                }
            }

            public List<Graph> GetBranches()
            {
                return GetCurrentNode().children;
            }

            public void SetBranch(int index)
            {
                GetCurrentNode().CurrentChildIndex = index;
            }

            public Graph GetCurrentNode()
            {
                if (CurrentChildIndex == -1)
                    return this;
                else
                    return children[CurrentChildIndex].GetCurrentNode();
            }

            public bool IsLastNode()
            {
                if (CurrentChildIndex == -1)
                    return children.Count == 0;
                else
                    return children[CurrentChildIndex].IsLastNode();
            }

            public void Rewind()
            {
                var node = this;

                for(int i = 0; i < rewindCount; ++i)
                {
                    node = node.parent;
                    node.CurrentChildIndex = -1;
                }
            }
        }

        public int CurrentNodeIndex { get; set; }
        public List<Graph> nodes = new();

        public Dialogue(){ }

        public void AddNode(JToken json)
        {
            nodes.Add(new Graph(json));
        }

        public Graph GetCurrentNode()
        {
            if (nodes.Count <= CurrentNodeIndex)
                return null;

            return nodes[CurrentNodeIndex].GetCurrentNode();
        }

        public bool IsLastNode()
        {
            return nodes.Count == 0 || CurrentNodeIndex == nodes.Count - 1;
        }

        public void SetNextNode()
        {
            if (!IsLastNode()) CurrentNodeIndex++;
        }
    }
}
