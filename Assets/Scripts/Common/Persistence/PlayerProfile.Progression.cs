using System.Linq;
using Ulko.Data.Timeline;
using Ulko.Persistence;
using UnityEngine;

namespace Ulko
{
    public static partial class PlayerProfile
    {
        public const string MAIN_STORY_ID = "main";
        public static string CurrentStoryId => loadedGame?.currentStoryId;

        public static bool CurrentStoryIsMain()
        {
            return CurrentStoryId == MAIN_STORY_ID;
        }

        public static void SetCurrentStoryToMain()
        {
            loadedGame.currentStoryId = MAIN_STORY_ID;
        }

        public static void SetCurrentStory(string storyId)
        {
            loadedGame.currentStoryId = storyId;
        }

        public static bool CurrentStoryIsCompleted()
        {
            return StoryIsCompleted(CurrentStoryId);
        }

        public static bool StoryIsCompleted(string storyId)
        {
            var story = Database.GetStory(storyId);
            Debug.Assert(story != null, "Story with id " + storyId + " doesn't exist.");

            string chapterId = story.chapters.Last().chapterId;
            return ChapterIsCompleted(chapterId);
        }

        public static bool ChapterIsCompleted(string chapterId)
        {
            var chapterProg = GetChapterProgression(chapterId);
            return chapterProg.ChapterIsCompleted();
        }

        public static ChapterProgression GetChapterProgression(string chapterId)
        {
            return loadedGame.GetChapterProgression(chapterId);
        }

        public static void ResetChapterProgression(string chapterId)
        {
            loadedGame.ResetChapterProgression(chapterId);
        }

        public static void CompleteCurrentMilestone()
        {
            var chapter = GetCurrentChapter();
            CompleteMilestone(chapter, GetCurrentMilestone(chapter.chapterId).Name);
        }

        public static void CompleteMilestone(string chapterId, string milestoneName)
        {
            var chapter = Database.GetChapter(chapterId).chapter;
            Debug.Assert(chapter != null, "Chapter with id " + chapterId + " doesn't exist.");

            CompleteMilestone(chapter, milestoneName);
        }

        public static void CompleteMilestone(Chapter chapter, string milestoneName)
        {
            loadedGame.CompleteMilestone(chapter.chapterId, milestoneName);

            if (Database.IsLastMilestone(chapter, milestoneName))
                loadedGame.CompleteChapter(chapter.chapterId);
        }

        public static Story GetCurrentStory()
        {
            return Database.GetStory(CurrentStoryId);
        }

        public static Chapter GetCurrentChapter()
        {
            return GetCurrentChapter(CurrentStoryId);
        }

        public static Chapter GetCurrentChapter(string storyId)
        {
            var story = Database.GetStory(storyId);
            Debug.Assert(story != null, "Story with id " + storyId + " doesn't exist.");

            for (int i = story.chapters.Count - 2; i >= 0; --i)
            {
                var chapterProg = GetChapterProgression(story.chapters[i].chapterId);

                if (chapterProg.ChapterIsCompleted())
                    return story.chapters[i + 1];
            }

            return story.chapters.First();
        }

        public static int GetCurrentChapterIndex(string storyId)
        {
            var story = Database.GetStory(storyId);
            Debug.Assert(story != null, "Story with id " + storyId + " doesn't exist.");

            var chapter = GetCurrentChapter();

            return story.chapters.FindIndex(c => c.chapterId == chapter.chapterId);
        }

        public static IMilestone GetCurrentMilestone()
        {
            return GetCurrentMilestone(GetCurrentChapter().chapterId);
        }

        public static IMilestone GetCurrentMilestone(string chapterId)
        {
            return GetCurrentMilestone(GetChapterProgression(chapterId));
        }

        public static IMilestone GetCurrentMilestone(ChapterProgression chapterProg)
        {
            Debug.Assert(!chapterProg.ChapterIsCompleted(), "Trying to get current milestone of chapter " + chapterProg.chapterId + " but chapter is already completed.");

            var chapter = Database.GetChapter(chapterProg.chapterId).chapter;
            Debug.Assert(chapter != null, "Chapter with id " + chapterProg.chapterId + " doesn't exist.");

            for (int i = chapter.milestones.Count - 2; i >= 0; --i)
            {
                if (chapterProg.MilestoneIsCompleted(chapter.milestones[i].Name))
                    return chapter.milestones[i + 1];
            }

            return chapter.milestones.First();
        }
    }
}
