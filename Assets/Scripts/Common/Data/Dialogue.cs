using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ulko.Data
{
    public class Dialogue
    {
        public class Line
        {
            public event Action OnPlay;

            public string speakerKey;
            public Dictionary<string, string> textByLanguage = new();

            public Line(JToken json)
            {
                speakerKey = json["speaker"]?.ToString();
                AddLanguage(json);
            }

            public void AddLanguage(JToken json)
            {
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
                return !string.IsNullOrEmpty(speakerKey) ? Localization.Localize(speakerKey) : null;
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

            public Page(params JToken[] jsonTokens)
            {
                for(int i = 0; i < jsonTokens[0].Count(); ++i)
                {
                    var line = new Line(jsonTokens[0][i]);
                    lines.Add(line);

                    for(int j = 1; j < jsonTokens.Count(); ++j)
                    {
                        line.AddLanguage(jsonTokens[j][i]);
                    }
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
            public List<Graph> children = new();

            public Graph(params JToken[] jsonTokens)
            {
                if (jsonTokens[0] is JArray)
                {
                    page = new Page(jsonTokens);
                }
                else
                {
                    page = new Page(jsonTokens[0]["lines"]);
                    rewindCount = jsonTokens[0]["rewindCount"]?.ToObject<int>() ?? 0;

                    if (jsonTokens[0]["choice"] != null)
                        choice = new Line(jsonTokens[0]["choice"]);

                    if (jsonTokens[0]["children"] != null)
                    {
                        foreach (var child in jsonTokens[0]["children"])
                        {
                            var graph = new Graph(child)
                            {
                                parent = this
                            };
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

        public void AddNode(params JToken[] jsonTokens)
        {
            nodes.Add(new Graph(jsonTokens));
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
