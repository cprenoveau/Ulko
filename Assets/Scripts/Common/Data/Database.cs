using Ulko.Data.Characters;
using Ulko.Data.Timeline;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Ulko.Data.Abilities;
using System.Linq;

namespace Ulko
{
    public static class Database
    {
        public static Dictionary<string, Story> Stories { get; private set; } = new Dictionary<string, Story>();
        public static Dictionary<string, Hero> Heroes { get; private set; } = new Dictionary<string, Hero>();
        public static Dictionary<string, Enemy> Enemies { get; private set; } = new Dictionary<string, Enemy>();
        public static Dictionary<string, AbilityAsset> Abilities { get; private set; } = new Dictionary<string, AbilityAsset>();
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

            Abilities.Clear();

            var abilities = await Addressables.LoadAssetsAsync<AbilityAsset>("ability", null).Task;
            foreach (var ability in abilities)
            {
                Abilities.Add(ability.id, ability);
            }

            Statuses.Clear();

            var statuses = await Addressables.LoadAssetsAsync<StatusAsset>("status", null).Task;
            foreach (var status in statuses)
            {
                Statuses.Add(status.id, status);
            }
        }

        public static Story GetStory(string storyId)
        {
            if (Stories.ContainsKey(storyId))
            {
                return Stories[storyId];
            }
            else
            {
                Debug.LogError("Story with id " + storyId + " doesn't exist.");
                return null;
            }
        }

        public static (Story story, Chapter chapter) GetChapter(string chapterId)
        {
            foreach(var story in Stories.Values)
            {
                var chapter = story.chapters.FirstOrDefault(c => c.chapterId == chapterId);
                if (chapter != null)
                    return (story, chapter);
            }

            Debug.LogError("Chapter with id " + chapterId + " doesn't exist.");
            return (null,null);
        }

        public static (Story story, Chapter chapter, IMilestone milestone) GetMilestone(string milestoneName)
        {
            foreach (var story in Stories.Values)
            {
                foreach(var chapter in story.chapters)
                {
                    var milestone = chapter.milestones.FirstOrDefault(m => m.Name == milestoneName);
                    if (milestone != null)
                        return (story, chapter, milestone);
                }
            }

            Debug.LogError("Milestone with name " + milestoneName + " doesn't exist.");
            return (null,null,null);
        }

        public static bool IsLastMilestone(Chapter chapter, string milestoneName)
        {
            return chapter.milestones.Last().Name == milestoneName;
        }
    }
}
