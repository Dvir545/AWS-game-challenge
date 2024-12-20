using UnityEngine;
using System;

namespace Utils.Data
{
    [Serializable]
    public class GameStatistics
    {
        public string username;
        public int totalGamesPlayed;
        public int consecutiveGamesPlayed;
        public string killedLastGameBy;
        public int daysSurvivedLastGame;
        public int daysSurvivedHighScore;
        
        public static GameStatistics Instance { get; private set; }
        
        public static void Initialize(string username)
        {
            if (Instance != null) return;
            
            LoadStatistics(username);
        }
        
        public static void LoadStatistics(string username = null)
        {
            string json = PlayerPrefs.GetString("GameStatistics", "");
            if (string.IsNullOrEmpty(json))
            {
                Instance = new GameStatistics
                {
                    username = username ?? "Player",
                    totalGamesPlayed = 0,
                    consecutiveGamesPlayed = 0,
                    killedLastGameBy = "none",
                    daysSurvivedLastGame = 0,
                    daysSurvivedHighScore = 0
                };
            }
            else
            {
                Instance = JsonUtility.FromJson<GameStatistics>(json);
            }
        }
        
        public static void SaveStatistics()
        {
            string json = JsonUtility.ToJson(Instance);  // DVIR - we want this also in AWS, to support cross-platform statistics
            PlayerPrefs.SetString("GameStatistics", json);
            PlayerPrefs.Save();  // So not in PlayerPrefs
        }
        
        public static void OnGameOver(string killedBy)
        {
            Instance.totalGamesPlayed++;
            Instance.killedLastGameBy = killedBy;
            Instance.daysSurvivedLastGame = GameData.Instance.day;
            
            if (GameData.Instance.day > Instance.daysSurvivedHighScore)
            {
                Instance.daysSurvivedHighScore = GameData.Instance.day;
            }
            
            if (killedBy == "none")
            {
                Instance.consecutiveGamesPlayed++;
            }
            else
            {
                Instance.consecutiveGamesPlayed = 0;
            }
            
            SaveStatistics();
        }
    }
}