using System;
using Unity.Serialization.Json;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    [Serializable]
    internal class LambdaResponse
    {
        public int statusCode;
        public Headers headers;
        public string body;
    }

    [Serializable]
    internal class Headers
    {
        public string Content_Type;
        public string Access_Control_Allow_Origin;
    }
    
    [Serializable]
    internal class GameDataResponse
    {
        public SavedGameData gameData;
    }

    [Serializable]
    public class SaveRequest
    {
        public string username;
        public SavedGameData gameData;
    }

    [Serializable]
    public class SavedGameData
    {
        public string username;
        public float TotalGamesPlayed;
        public float ConsecutiveGamesPlayed;
        public float KilledLastGameBy;
        public ScoreInfo LastGameScore;
        public ScoreInfo HighScore;
        public bool LeftHanded;
        public GameState CurrentGameState;
    }

    public class GameStatistics : Singleton<GameStatistics>
    {
        public delegate void GameDataLoadedHandler();
        public event GameDataLoadedHandler OnGameDataLoaded;

        public SavedGameData LoadedGameData { get; private set; }
        private const string SAVE_API_URL = "https://ty3pjcq6x8.execute-api.us-east-1.amazonaws.com/saves/new-save";
        private const string LOAD_SAVE_API_URL = "https://ty3pjcq6x8.execute-api.us-east-1.amazonaws.com/saves/get-save";
        private const int MAX_RETRIES = 3;
        private const float RETRY_DELAY = 2f;
        private bool _isSaving = false;

        public string username;
        public int totalGamesPlayed;
        public int consecutiveGamesPlayed;
        public int killedLastGameBy;
        public ScoreInfo lastGameScore;
        public ScoreInfo highScore;
        public bool leftHanded;

        private void OnEnable()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StartListening(EventManager.DayStarted, OnDayStarted);
            }
        }

        private void OnDisable()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening(EventManager.DayStarted, OnDayStarted);
            }
        }

        private void OnDayStarted(object _)
        {
            Debug.Log("********** OnDayStarted triggered - Attempting auto-save **********");
            SaveToJson();
        }

        public void Init(string username)
        {
            Debug.Log($"********** Initializing GameStatistics for user: {username} **********");
            this.username = username;
            totalGamesPlayed = 0;
            consecutiveGamesPlayed = 0;
            killedLastGameBy = 0;
            lastGameScore = new ScoreInfo(0, 0);
            highScore = new ScoreInfo(0, 0);
            leftHanded = true;

            StartCoroutine(LoadUserDataWithRetry(0));
        }

        private TowerArrayWrapper[] SerializeTowers(List<TowerLevelInfo>[] towers)
        {
            var serializedTowers = new TowerArrayWrapper[Constants.TowerCount];
            for (int i = 0; i < Constants.TowerCount; i++)
            {
                serializedTowers[i] = new TowerArrayWrapper();
                if (towers[i] != null)
                {
                    serializedTowers[i].towers = towers[i].ToList();
                }
            }
            return serializedTowers;
        }

        private List<TowerLevelInfo>[] DeserializeTowers(TowerArrayWrapper[] serializedTowers)
        {
            var towers = new List<TowerLevelInfo>[Constants.TowerCount];
            for (int i = 0; i < Constants.TowerCount; i++)
            {
                towers[i] = new List<TowerLevelInfo>();
                if (serializedTowers != null && i < serializedTowers.Length && serializedTowers[i] != null)
                {
                    towers[i] = serializedTowers[i].towers;
                }
            }
            return towers;
        }

        private IEnumerator LoadUserDataWithRetry(int retryCount)
        {
            Debug.Log($"********** Attempting to load user data (attempt {retryCount + 1}/{MAX_RETRIES}) **********");
            var requestData = new LoadSaveRequest { username = this.username };
            string jsonData = JsonUtility.ToJson(requestData);
            Debug.Log($"********** Load request JSON: {jsonData} **********");

            using (UnityWebRequest www = new UnityWebRequest(LOAD_SAVE_API_URL, "POST"))
            {
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                
                string authToken = PlayerPrefs.GetString("IdToken");
                if (!string.IsNullOrEmpty(authToken))
                {
                    Debug.Log("********** Auth token found and set for load request **********");
                    www.SetRequestHeader("Authorization", authToken);
                }
                else 
                {
                    Debug.Log("********** WARNING: No auth token found for load request! **********");
                }

                Debug.Log($"********** Sending load request for user: {username} **********");
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"********** Error loading save data: {www.error} **********");
                    Debug.LogError($"********** Response: {www.downloadHandler.text} **********");
                    
                    if (retryCount < MAX_RETRIES - 1)
                    {
                        Debug.Log($"********** Retrying load request in {RETRY_DELAY} seconds... **********");
                        yield return new WaitForSeconds(RETRY_DELAY);
                        StartCoroutine(LoadUserDataWithRetry(retryCount + 1));
                        yield break;
                    }
                    LoadedGameData = null;
                }
                else
                {
                    try
                    {
                        var response = JsonUtility.FromJson<GameDataResponse>(www.downloadHandler.text);

                        if (response != null && response.gameData != null)
                        {
                            LoadedGameData = response.gameData;
                            var gameData = response.gameData;
                            username = gameData.username;
                            totalGamesPlayed = Mathf.RoundToInt(gameData.TotalGamesPlayed);
                            consecutiveGamesPlayed = Mathf.RoundToInt(gameData.ConsecutiveGamesPlayed);
                            killedLastGameBy = Mathf.RoundToInt(gameData.KilledLastGameBy);
                            lastGameScore = gameData.LastGameScore;
                            highScore = gameData.HighScore;
                            leftHanded = gameData.LeftHanded;

                            if (gameData.CurrentGameState != null && gameData.CurrentGameState.towers != null)
                            {
                                var deserializedTowers = DeserializeTowers(gameData.CurrentGameState.towers);
                                gameData.CurrentGameState.towers = gameData.CurrentGameState.towers;
                            }

                            Debug.Log($"********** Successfully loaded game data: TotalGames={totalGamesPlayed}, LastScore={lastGameScore.daysSurvived} days **********");
                            OnGameDataLoaded?.Invoke();
                        }
                        else
                        {
                            Debug.LogWarning($"********** No save data found for user: {username} **********");
                            LoadedGameData = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"********** Error parsing save data response: {e.Message} **********");
                        Debug.LogError($"********** Raw response was: {www.downloadHandler.text} **********");
                        LoadedGameData = null;
                    }
                }
            }
        }

        private IEnumerator SaveGameCoroutine()
        {
            Debug.Log("********** Starting SaveGameCoroutine **********");
            if (_isSaving)
            {
                Debug.Log("********** Save already in progress, skipping **********");
                yield break;
            }
            _isSaving = true;

            SaveRequest saveRequest = null;
            string jsonData = "";
            
            Debug.Log($"********** Preparing save data for user: {username} **********");
            Debug.Log($"********** Current game state - Day: {GameData.Instance.day}, Health: {GameData.Instance.curHealth}, Cash: {GameData.Instance.cash} **********");

            try 
            {
                saveRequest = new SaveRequest
                {
                    username = username,
                    gameData = new SavedGameData
                    {
                        username = username,
                        TotalGamesPlayed = totalGamesPlayed,
                        ConsecutiveGamesPlayed = consecutiveGamesPlayed,
                        KilledLastGameBy = killedLastGameBy,
                        LastGameScore = lastGameScore,
                        HighScore = highScore,
                        LeftHanded = leftHanded,
                        CurrentGameState = new GameState
                        {
                            healthUpgradeLevel = GameData.Instance.healthUpgradeLevel,
                            regenUpgradeLevel = GameData.Instance.regenUpgradeLevel,
                            speedUpgradeLevel = GameData.Instance.speedUpgradeLevel,
                            staminaUpgradeLevel = GameData.Instance.staminaUpgradeLevel,
                            knockbackUpgradeLevel = GameData.Instance.knockbackUpgradeLevel,
                            swordLevel = GameData.Instance.swordLevel,
                            hoeLevel = GameData.Instance.hoeLevel,
                            hammerLevel = GameData.Instance.hammerLevel,
                            cash = GameData.Instance.cash,
                            day = GameData.Instance.day,
                            curHealth = GameData.Instance.curHealth,
                            secondsSinceGameStarted = GameData.Instance.secondsSinceGameStarted,
                            crops = GameData.Instance.crops,
                            materials = GameData.Instance.materials,
                            cropsInStore = GameData.Instance.cropsInStore,
                            materialsInStore = GameData.Instance.materialsInStore,
                            thisDayEnemies = GameData.Instance.thisDayEnemies,
                            thisNightEnemies = GameData.Instance.thisNightEnemies,
                            towers = SerializeTowers(GameData.Instance.towers),
                            plantedCrops = GameData.Instance.plantedCrops.Select(kvp => new PlantedCropKeyValue 
                            { 
                                Key = new Vector2IntSerializable(kvp.Key),
                                Value = kvp.Value
                            }).ToList()
                        }
                    }
                };

                jsonData = JsonUtility.ToJson(saveRequest);
                Debug.Log($"********** Prepared JSON data length: {jsonData.Length} **********");
            }
            catch (Exception e)
            {
                Debug.LogError($"********** CRITICAL ERROR preparing save data: {e.Message} **********");
                Debug.LogError($"********** Stack trace: {e.StackTrace} **********");
                _isSaving = false;
                yield break;
            }

            UnityWebRequest www = null;
            try
            {
                www = new UnityWebRequest(SAVE_API_URL, "POST");
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                
                string authToken = PlayerPrefs.GetString("IdToken");
                if (!string.IsNullOrEmpty(authToken))
                {
                    Debug.Log("********** Auth token found and set in request header **********");
                    www.SetRequestHeader("Authorization", authToken);
                }
                else 
                {
                    Debug.Log("********** WARNING: No auth token found! **********");
                }

                Debug.Log($"********** Sending save request to {SAVE_API_URL} **********");
            }
            catch (Exception e)
            {
                Debug.LogError($"********** CRITICAL ERROR creating web request: {e.Message} **********");
                Debug.LogError($"********** Stack trace: {e.StackTrace} **********");
                if (www != null) www.Dispose();
                _isSaving = false;
                yield break;
            }

            using (www)
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"********** SAVE FAILED - Error: {www.error} **********");
                    Debug.LogError($"********** Response Code: {www.responseCode} **********");
                    Debug.LogError($"********** Response: {www.downloadHandler.text} **********");
                }
                else
                {
                    Debug.Log($"********** Game saved successfully **********");
                    Debug.Log($"********** Response: {www.downloadHandler.text} **********");
                }
            }

            _isSaving = false;
            Debug.Log("********** SaveGameCoroutine completed **********");
        }

        public void SaveToJson()
        {
            Debug.Log("********** SaveToJson called - Starting save process **********");
            StartCoroutine(SaveGameCoroutine());
        }

        public void UpdateStatistics(int enemyType)
        {
            Debug.Log("********** UpdateStatistics called - Player died **********");
            Debug.Log($"********** Enemy type: {enemyType}, Days survived: {GameData.Instance.day}, Time survived: {GameData.Instance.secondsSinceGameStarted} **********");
            
            totalGamesPlayed++;
            consecutiveGamesPlayed++;
            killedLastGameBy = enemyType;
            lastGameScore = new ScoreInfo(GameData.Instance.day, GameData.Instance.secondsSinceGameStarted);
            if (lastGameScore.daysSurvived > highScore.daysSurvived ||
                            (lastGameScore.daysSurvived == highScore.daysSurvived && lastGameScore.secondsSurvived > highScore.secondsSurvived))
                        {
                            highScore = lastGameScore;
                            Debug.Log("********** New high score achieved! **********");
                            EventManager.Instance.TriggerEvent(EventManager.NewHighScore, null);
                        }
                        SaveToJson();
                    }
                }

                [Serializable]
                internal class LoadSaveRequest
                {
                    public string username;
                }
            }
// Save on death

// TODO: Add manual saving functionality and save on quit
// Potential implementation points:
// 1. Add a save button to the pause menu  
// 2. Hook into the game's quit process to save before closing
// 3. Add auto-save on certain achievements or milestones
