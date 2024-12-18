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
    
    class SerializableUserGamesData
    {
        public int TotalGamesPlayed;
        public int ConsecutiveGamesPlayed;
        public int KilledLastGameBy;
        public ScoreInfo LastGameScore;
        public ScoreInfo HighScore;
    }
    
    public class UserGamesData: Singleton<UserGamesData>
    {
        public int totalGamesPlayed;
        public int consecutiveGamesPlayed;
        public int killedLastGameBy;
        public ScoreInfo lastGameScore;
        public ScoreInfo highScore;
        
        public UserGamesData()
        {
            NewUser();
        }

        private void NewUser()
        {
            totalGamesPlayed = 0;
            consecutiveGamesPlayed = 0;
            killedLastGameBy = 0;
            lastGameScore = new ScoreInfo(0, 0);
            highScore = new ScoreInfo(0, 0);
        }
        
        public void LoadFromJson(string json)
        {
            // DVIR - use this function after login to load user data from aws
            var data = JsonSerialization.FromJson<SerializableUserGamesData>(json);
            totalGamesPlayed = data.TotalGamesPlayed;
            consecutiveGamesPlayed = 0;  // this field is reset after each entry to the app
            killedLastGameBy = data.KilledLastGameBy;
            lastGameScore = data.LastGameScore;
            highScore = data.HighScore;
        }
        
        public void SaveToJson()
        {
            var serializableData = new SerializableUserGamesData
            {
                TotalGamesPlayed = totalGamesPlayed,
                ConsecutiveGamesPlayed = consecutiveGamesPlayed,
                KilledLastGameBy = killedLastGameBy,
                LastGameScore = lastGameScore,
                HighScore = highScore
            };
            var json = JsonSerialization.ToJson(serializableData);
            // DVIR - upload json to aws & send to npc
        }
    }
}