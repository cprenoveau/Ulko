using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ulko.Data;
using Ulko.Persistence;
using UnityEngine;

namespace Ulko
{
    public static partial class PlayerProfile
    {
        public static IEnumerable<(FileInfo file, GameFile game)> AllFiles { get; private set; }
        public static string LoadedFile { get; private set; }
        public static bool GameIsLoaded => loadedGame != null;

        private static GameFile loadedGame;
        private static GameFile gameCopy;
        private static TextAsset newGameAsset;

        public static void Init(TextAsset newGameAsset)
        {
            PlayerProfile.newGameAsset = newGameAsset;
            RefreshFiles();
        }

        public static void SaveTempState()
        {
            gameCopy = loadedGame.Clone();
        }

        public static void DeleteTempState()
        {
            gameCopy = null;
        }

        public static void RestoreTempState()
        {
            if (gameCopy != null)
            {
                loadedGame = gameCopy.Clone();
                gameCopy = null;
            }
        }

        public static double Time => loadedGame.playTime;
        public static void AddTime(double amount)
        {
            loadedGame.playTime += amount;
        }

        public static Location CurrentLocation => loadedGame.location.Clone();

        public static void ResetLocation()
        {
            loadedGame.location = new Location();
        }

        public static void SetPosition(Vector3 pos, Vector2 standDirection)
        {
            loadedGame.location.x = pos.x;
            loadedGame.location.y = pos.y;
            loadedGame.location.z = pos.z;
            loadedGame.location.standX = standDirection.x;
            loadedGame.location.standY = standDirection.y;
        }

        public static void SetEncounterIndex(int index)
        {
            loadedGame.location.encounterIndex = index;
        }

        public static void SetArea(string area)
        {
            loadedGame.location.area = area;
        }

        public static void NewGame()
        {
            DeleteGame(SUSPENDED_FILENAME);

            loadedGame = new GameFile(JObject.Parse(newGameAsset.text));
            HealParty();

            LoadedFile = null;
        }

        public static bool SaveGame(string filename)
        {
            if(loadedGame != null && GameFile.Save(loadedGame, GameFile.GameSavePath, filename))
            {
                RefreshFiles();
                LoadedFile = filename;
                return true;
            }
            return false;
        }

        public static bool LoadGame(string filename)
        {
            if(GameFile.Load(GameFile.GameSavePath, filename, out GameFile game))
            {
                loadedGame = game;
                LoadedFile = filename;
                return true;
            }

            return false;
        }

        public static string SUSPENDED_FILENAME => GameFile.GameFileName("suspended");
        public static bool SuspendGame()
        {
            RestoreTempState();
            return SaveGame(SUSPENDED_FILENAME);
        }

        public static bool ResumeGame()
        {
            return LoadGame(SUSPENDED_FILENAME);
        }

        public static bool HasSuspendedGame()
        {
            return File.Exists(Path.Combine(GameFile.GameSavePath, SUSPENDED_FILENAME));
        }

        public static bool DeleteGame(string filename)
        {
            if(GameFile.Delete(GameFile.GameSavePath, filename))
            {
                RefreshFiles();
                if (LoadedFile == filename) LoadedFile = null;
                return true;
            }
            return false;
        }

        private static void RefreshFiles()
        {
            var games = new List<(FileInfo file, GameFile game)>();

            var dir = new DirectoryInfo(GameFile.GameSavePath);
            if (dir.Exists)
            {
                FileInfo[] files = dir.GetFiles("*");

                foreach (FileInfo file in files)
                {
                    if(file.Name != SUSPENDED_FILENAME)
                    {
                        if (GameFile.Load(GameFile.GameSavePath, file.Name, out GameFile game))
                        {
                            games.Add((file, game));
                        }
                    }
                }
            }

            AllFiles = games.OrderByDescending(g => g.file.LastWriteTimeUtc);
        }
    }
}
