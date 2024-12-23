using System;
using Unity.Serialization.Json;

namespace Utils.Data
{
    
    [Serializable]
    public class ScoreInfo
    {
        public int daysSurvived;
        public float secondsSurvived;
        
        public ScoreInfo(int daysSurvived, float secondsSurvived)
        {
            this.daysSurvived = daysSurvived;
            this.secondsSurvived = secondsSurvived;
        }
    }
    
    class SerializableGameStatistics
    {
        public string username;
        public int TotalGamesPlayed;
        public int ConsecutiveGamesPlayed;
        public int KilledLastGameBy;
        public ScoreInfo LastGameScore;
        public ScoreInfo HighScore;
        public bool LeftHanded;
    }
    
    public class GameStatistics: Singleton<GameStatistics>
    {
        public string username;
        public int totalGamesPlayed;
        public int consecutiveGamesPlayed;
        public int killedLastGameBy;
        public ScoreInfo lastGameScore;
        public ScoreInfo highScore;
        // settings
        public bool leftHanded;

        public void Init(string username)
        {
            this.username = username;
            totalGamesPlayed = 0;
            consecutiveGamesPlayed = 0;
            killedLastGameBy = 0;
            lastGameScore = new ScoreInfo(0, 0);
            highScore = new ScoreInfo(0, 0);
            leftHanded = true;
        }
        
        public void LoadFromJson(string json)
        {
            // DVIR - use this function after login to load user data from aws
            var data = JsonSerialization.FromJson<SerializableGameStatistics>(json);
            username = data.username;
            totalGamesPlayed = data.TotalGamesPlayed;
            consecutiveGamesPlayed = 0;  // this field is reset after each entry to the app so not saved in cloud
            killedLastGameBy = data.KilledLastGameBy;
            lastGameScore = data.LastGameScore;
            highScore = data.HighScore;
            leftHanded = data.LeftHanded;
        }
        
        public void SaveToJson()
        {
            var serializableData = new SerializableGameStatistics
            {
                TotalGamesPlayed = totalGamesPlayed,
                ConsecutiveGamesPlayed = consecutiveGamesPlayed,
                KilledLastGameBy = killedLastGameBy,
                LastGameScore = lastGameScore,
                HighScore = highScore,
                LeftHanded = leftHanded
            };
            var json = JsonSerialization.ToJson(serializableData);
            // DVIR - upload json to aws & send to npc
        }

        public void UpdateStatistics(int enemyType)
        {
            totalGamesPlayed++;
            consecutiveGamesPlayed++;
            killedLastGameBy = enemyType;
            lastGameScore = new ScoreInfo(GameData.Instance.day, GameData.Instance.secondsSinceGameStarted);
            if (lastGameScore.daysSurvived > highScore.daysSurvived ||
                (lastGameScore.daysSurvived == highScore.daysSurvived && lastGameScore.secondsSurvived > highScore.secondsSurvived))
            {
                highScore = lastGameScore;
            }
            SaveToJson();
        }
    }
}