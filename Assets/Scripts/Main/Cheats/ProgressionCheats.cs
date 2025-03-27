using Codice.Client.BaseCommands.Merge.Xml;
using HotChocolate.Cheats;
using HotChocolate.UI.Cheats;
using HotChocolate.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Ulko.Data.Timeline;
using UnityEngine;

namespace Ulko.Cheats
{
    [CreateAssetMenu(fileName = "ProgressionCheats", menuName = "Ulko/Cheats/Progression", order = 1)]
    public class ProgressionCheats : ScriptableObject, ICheatUiRefresher
    {
        public event Action<ScriptableObject> NeedsRefresh;

        private GameInstance GameInstance => FindFirstObjectByType<GameInstance>(FindObjectsInactive.Include);

        [CheatMethod(ShortcutKeys = new KeyCode[] { KeyCode.F12 })]
        public void NextMilestone()
        {
            if (GameInstance != null)
                GameInstance.StartNextMilestone(default).FireAndForgetTask();
        }

        private Story MainStory => Database.Stories["main"];
        public IEnumerable<Chapter> Chapters => MainStory.acts.SelectMany(a => a.chapters);
        public string[] ChapterNames => Chapters.Select(c => c.chapterName + ": " + c.description).ToArray();

        private int chapterIndex = 0;
        [CheatProperty(StringArrayProperty = nameof(ChapterNames))]
        public int Chapter
        {
            get => chapterIndex;
            set
            {
                if (chapterIndex != value)
                {
                    chapterIndex = value;
                    milestoneIndex = 0;

                    NeedsRefresh?.Invoke(this);
                }
            }
        }

        private Chapter SelectedChapter => Chapters.ElementAtOrDefault(chapterIndex);
        public IMilestone SelectedMilestone => SelectedChapter != null && milestoneIndex < SelectedChapter.milestones.Count ? SelectedChapter.milestones[milestoneIndex] : null;
        public string[] MilestoneNames => SelectedChapter != null ? SelectedChapter.milestones.Select(m => m.Name).ToArray() : new string[] { "None" };

        private int milestoneIndex = 0;

        [CheatProperty(StringArrayProperty = nameof(MilestoneNames))]
        public int Milestone
        {
            get => milestoneIndex;
            set
            {
                milestoneIndex = value;
            }
        }

        [CheatMethod]
        public void StartMilestone()
        {
            if (GameInstance != null && SelectedMilestone != null)
                GameInstance.StartMilestone(SelectedMilestone, default).FireAndForgetTask();
        }
    }
}