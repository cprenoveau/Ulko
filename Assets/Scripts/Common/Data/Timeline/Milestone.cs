using System;
using System.Collections.Generic;

namespace Ulko.Data.Timeline
{
    public interface IMilestone
    {
        ContextType Context { get; }
        string Name { get; }
        string SceneAddress { get; }
        bool SetPartyOrder { get; }
        List<Characters.HeroAsset> Party { get; }
        bool ShowSavePrompt { get; }
        LightingAndWeatherConfig LightingAndWeather { get; }
        bool IsInterior { get; }
    } 

    [Serializable]
    public class Level : IMilestone
    {
        public ContextType Context => ContextType.World;

        public string Name => name;
        public string name;

        public string SceneAddress => sceneAddress;
        public string sceneAddress;

        public LightingAndWeatherConfig LightingAndWeather => lightingAndWeather;
        public LightingAndWeatherConfig lightingAndWeather;

        public bool IsInterior => false;

        public bool setPartyOrder;
        public bool SetPartyOrder => setPartyOrder;

        public List<Characters.HeroAsset> party = new();
        public List<Characters.HeroAsset> Party => party;

        public bool ShowSavePrompt => showSavePrompt;
        public bool showSavePrompt;

        public AreaTag area;
        public SpawnPointTag spawnPoint;
    }

    [Serializable]
    public class Cutscene : IMilestone
    {
        public ContextType Context => ContextType.Cutscene;

        public string Name => name;
        public string name;

        public string SceneAddress => sceneAddress;
        public string sceneAddress;

        public LightingAndWeatherConfig LightingAndWeather => lightingAndWeather;
        public LightingAndWeatherConfig lightingAndWeather;

        public bool IsInterior => isInterior;
        public bool isInterior;

        public bool setPartyOrder;
        public bool SetPartyOrder => setPartyOrder;

        public List<Characters.HeroAsset> party = new();
        public List<Characters.HeroAsset> Party => party;

        public bool ShowSavePrompt => showSavePrompt;
        public bool showSavePrompt;
    }

    [Serializable]
    public class BossBattle : IMilestone
    {
        public ContextType Context => ContextType.Battle;

        public string Name => name;
        public string name;

        public string SceneAddress => battleAsset.sceneAddress;

        public LightingAndWeatherConfig LightingAndWeather => lightingAndWeather;
        public LightingAndWeatherConfig lightingAndWeather;

        public bool IsInterior => isInterior;
        public bool isInterior;

        public bool setPartyOrder;
        public bool SetPartyOrder => setPartyOrder;

        public List<Characters.HeroAsset> party = new();
        public List<Characters.HeroAsset> Party => party;

        public bool ShowSavePrompt => showSavePrompt;
        public bool showSavePrompt;

        public BattleAsset battleAsset;
    }

    [Serializable]
    public class Puzzle : IMilestone
    {
        public ContextType Context => ContextType.Puzzle;

        public string Name => name;
        public string name;

        public string SceneAddress => sceneAddress;
        public string sceneAddress;

        public LightingAndWeatherConfig LightingAndWeather => lightingAndWeather;
        public LightingAndWeatherConfig lightingAndWeather;

        public bool IsInterior => isInterior;
        public bool isInterior;

        public bool setPartyOrder;
        public bool SetPartyOrder => setPartyOrder;

        public List<Characters.HeroAsset> party = new();
        public List<Characters.HeroAsset> Party => party;

        public bool ShowSavePrompt => showSavePrompt;
        public bool showSavePrompt;

        public MenuDefinition startingMenu;
    }
}
