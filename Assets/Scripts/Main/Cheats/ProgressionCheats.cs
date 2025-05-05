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
                GameInstance.CompleteMilestone(default).FireAndForgetTask();
        }

        private IEnumerable<Story> Stories => Database.Stories.Values;
        public string[] StoryNames => Stories.Select(c => c.id).ToArray();
        private Story SelectedStory => Stories.ElementAtOrDefault(storyIndex);

        private int storyIndex = 0;
        [CheatProperty(StringArrayProperty = nameof(StoryNames))]
        public int Story
        {
            get => storyIndex;
            set
            {
                if (storyIndex != value)
                {
                    storyIndex = value;
                    chapterIndex = 0;
                    milestoneIndex = 0;

                    NeedsRefresh?.Invoke(this);
                }
            }
        }

        public IEnumerable<Chapter> Chapters => SelectedStory.chapters;
        public string[] ChapterNames => Chapters.Select(c => c.chapterName).ToArray();

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
            {
                PlayerProfile.SetCurrentStory(SelectedStory.id);

                //complete previous chapter
                if (chapterIndex > 0)
                {
                    var previousChapter = Chapters.ElementAt(chapterIndex - 1);
                    PlayerProfile.CompleteMilestone(previousChapter, previousChapter.milestones.Last().Name);
                }

                //reset current and all future chapters
                for (int i = chapterIndex; i < SelectedStory.chapters.Count; ++i)
                {
                    PlayerProfile.ResetChapterProgression(SelectedStory.chapters[i].chapterId);
                }

                //complete previous milestone
                if (milestoneIndex > 0)
                {
                    string previousMilestone = MilestoneNames[milestoneIndex - 1];
                    PlayerProfile.CompleteMilestone(SelectedChapter, previousMilestone);
                }

                PlayerProfile.ResetLocation();

                GameInstance.StartMilestone(SelectedMilestone, default).FireAndForgetTask();
            }
        }
    }
}