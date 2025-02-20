using System;
using System.Collections.Generic;

namespace Ulko.Data.Timeline
{
    public interface IMilestone
    {
        string Name { get; }
        bool SetPartyOrder { get; }
        List<Characters.HeroAsset> Party { get; }
        bool ShowSavePrompt { get; }
        LightingAndWeatherConfig LightingAndWeather { get; }
    } 

    [Serializable]
    public class Level : IMilestone
    {
        public string name;
        public string Name => name;

        public string SceneAddress => sceneAddress;
        public string sceneAddress;

        public LightingAndWeatherConfig LightingAndWeather => lightingAndWeather;
        public LightingAndWeatherConfig lightingAndWeather;

        public bool setPartyOrder;
        public bool SetPartyOrder => setPartyOrder;

        public List<Characters.HeroAsset> party = new List<Characters.HeroAsset>();
        public List<Characters.HeroAsset> Party => party;

        public bool ShowSavePrompt => showSavePrompt;
        public bool showSavePrompt;

        public AreaTag area;
        public SpawnPointTag spawnPoint;
    }

    [Serializable]
    public class Cutscene : IMilestone
    {
        public string name;
        public string Name => name;
        public string SceneAddress => name;

        public LightingAndWeatherConfig LightingAndWeather => lightingAndWeather;
        public LightingAndWeatherConfig lightingAndWeather;

        public bool isInterior;

        public bool setPartyOrder;
        public bool SetPartyOrder => setPartyOrder;

        public List<Characters.HeroAsset> party = new List<Characters.HeroAsset>();
        public List<Characters.HeroAsset> Party => party;

        public bool ShowSavePrompt => showSavePrompt;
        public bool showSavePrompt;
    }

    [Serializable]
    public class Battle : IMilestone
    {
        public string name;
        public string Name => name;

        public LightingAndWeatherConfig LightingAndWeather => lightingAndWeather;
        public LightingAndWeatherConfig lightingAndWeather;

        public bool isInterior;

        public bool setPartyOrder;
        public bool SetPartyOrder => setPartyOrder;

        public List<Characters.HeroAsset> party = new List<Characters.HeroAsset>();
        public List<Characters.HeroAsset> Party => party;

        public bool ShowSavePrompt => showSavePrompt;
        public bool showSavePrompt;

        public Data.Battle battle;
    }
}
