using Ulko.Data.Characters;
using Ulko.Data.Timeline;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Ulko.Data.Abilities;

namespace Ulko
{
    public static class Database
    {
        public static Dictionary<string, Story> Stories { get; private set; } = new Dictionary<string, Story>();
        public static Dictionary<string, Hero> Heroes { get; private set; } = new Dictionary<string, Hero>();
        public static Dictionary<string, Enemy> Enemies { get; private set; } = new Dictionary<string, Enemy>();
        public static Dictionary<string, StatusAsset> Statuses { get; private set; } = new Dictionary<string, StatusAsset>();

        public static async Task Init(List<Story> stories, TextAsset heroesAsset, TextAsset enemiesAsset)
        {
            Stories.Clear();
            foreach(var story in stories)
            {
                Stories.Add(story.id, story);
            }

            Heroes.Clear();
            var heroes = JArray.Parse(heroesAsset.text).ParseList<Hero>();
            foreach (var hero in heroes)
            {
                Heroes.Add(hero.id, hero);
            }

            Enemies.Clear();
            var enemies = JArray.Parse(enemiesAsset.text).ParseList<Enemy>();
            foreach (var enemy in enemies)
            {
                Enemies.Add(enemy.id, enemy);
            }

            Statuses.Clear();

            var statuses = await Addressables.LoadAssetsAsync<StatusAsset>("status", null).Task;
            foreach (var status in statuses)
            {
                Statuses.Add(status.id, status);
            }
        }

        public static (string story, Persistence.Progression prog) GetProgression(string milestoneName)
        {
            foreach(var story in Stories)
            {
                string storyName = story.Key;
                for(int i = 0; i < story.Value.acts.Count; ++i)
                {
                    for(int j = 0; j < story.Value.acts[i].chapters.Count; ++j)
                    {
                        int milestoneIndex = story.Value.acts[i].chapters[j].milestones.FindIndex(m => m.Name == milestoneName);
                        if(milestoneIndex != -1)
                        {
                            return (storyName, new Persistence.Progression { act = i, chapter = j, milestone = milestoneIndex });
                        }
                    }
                }
            }

            return (null, null);
        }

        public static IMilestone GetMilestone(string story, Persistence.Progression progression)
        {
            return GetMilestone(story, progression.act, progression.chapter, progression.milestone);
        }

        public static IMilestone GetMilestone(string story, int actIndex, int chapterIndex, int milestoneIndex)
        {
            var chapter = GetChapter(story, actIndex, chapterIndex);
            if(chapter != null)
            {
                return GetMilestone(chapter, milestoneIndex);
            }
            else
            {
                return null;
            }
        }

        public static IMilestone GetMilestone(Chapter chapter, int milestoneIndex)
        {
            if (milestoneIndex >= 0 && milestoneIndex < chapter.milestones.Count)
            {
                return chapter.milestones[milestoneIndex];
            }
            else
            {
                Debug.LogError("Milestone with index " + milestoneIndex + " doesn't exist in chapter " + chapter.name);
                return null;
            }
        }

        public static Chapter GetChapter(string story, int actIndex, int chapterIndex)
        {
            var act = GetAct(story, actIndex);
            if(act != null)
            {
                return GetChapter(act, chapterIndex);
            }
            else
            {
                return null;
            }
        }

        public static Chapter GetChapter(Act act, int chapterIndex)
        {
            if(chapterIndex >= 0 && chapterIndex < act.chapters.Count)
            {
                return act.chapters[chapterIndex];
            }
            else
            {
                Debug.LogError("Chapter with index "+chapterIndex+" doesn't exist in act "+act.name);
                return null;
            }
        }

        public static Act GetAct(string story, int actIndex)
        {
            if(Stories.ContainsKey(story))
            {
                return GetAct(Stories[story], actIndex);
            }
            else
            {
                Debug.LogError("Story with id " + story + " doesn't exist");
                return null;
            }
        }

        public static Act GetAct(Story story, int actIndex)
        {
            if(actIndex >= 0 && actIndex < story.acts.Count)
            {
                return story.acts[actIndex];
            }
            else
            {
                Debug.LogError("Act with index " + actIndex + " doesn't exist in story "+story.id);
                return null;
            }
        }
    }
}
