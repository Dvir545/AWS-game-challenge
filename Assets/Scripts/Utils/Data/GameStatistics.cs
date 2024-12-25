using System;
using Unity.Serialization.Json;
using UnityEngine;

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
        public float SfxVolume;
        public float MusicVolume;
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
        public bool leftHanded { get; private set; }
        public float sfxVolume { get; private set; }
        public float musicVolume { get; private set; }

        public void Init(string username)
        {
            this.username = username;
            totalGamesPlayed = 0;
            consecutiveGamesPlayed = 0;
            killedLastGameBy = 0;
            lastGameScore = new ScoreInfo(0, 0);
            highScore = new ScoreInfo(0, 0);
            leftHanded = PlayerPrefs.GetInt("leftHanded", 0) == 1;
            sfxVolume = PlayerPrefs.GetFloat("sfxVolume", .5f);
            musicVolume = PlayerPrefs.GetFloat("musicVolume", .5f);
        }
        
        public void SetLeftHanded(bool leftHanded)
        {
            this.leftHanded = leftHanded;
            PlayerPrefs.SetInt("leftHanded", leftHanded ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        public void SetSfxVolume(float sfxVolume)
        {
            this.sfxVolume = sfxVolume;
            PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
            PlayerPrefs.Save();
        }
        
        public void SetMusicVolume(float musicVolume)
        {
            this.musicVolume = musicVolume;
            PlayerPrefs.SetFloat("musicVolume", musicVolume);
            PlayerPrefs.Save();
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
            SetLeftHanded(data.LeftHanded);
            SetSfxVolume(data.SfxVolume);
            SetMusicVolume(data.MusicVolume);
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
                LeftHanded = leftHanded,
                SfxVolume = sfxVolume,
                MusicVolume = musicVolume
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