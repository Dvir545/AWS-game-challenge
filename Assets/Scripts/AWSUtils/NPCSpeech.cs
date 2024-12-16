using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AWSUtils
{
    public enum NPCType
    {
        Start,
        Mid
    }

    [Serializable]
    public class NPCResponse
    {
        public string response;
    }

    [Serializable]
    public class StartNPCRequest
    {
        public int totalGamesPlayed;
        public int consecutiveGamesPlayed;
        public string killedLastGameBy;
        public int daysSurvivedLastGame;
        public int daysSurvivedHighScore;
    }

    [Serializable]
    public class MidNPCRequest
    {
        public PlayerStatus playerStatus;
    }

    [Serializable]
    public class PlayerStatus
    {
        public int health;
        public int money;
        public Inventory inventory;
        public WorldStatus worldStatus;
        public LastRoundActivity lastRoundActivity;
        public NextRoundEnemies nextRoundEnemies;
        public List<string> previousResponses;
    }

    [Serializable]
    public class Inventory
    {
        public Crops crops;
        public TowerMaterials towerMaterials;
    }

    [Serializable]
    public class Crops
    {
        public int wheat;
        public int carrot;
        public int tomato;
        public int corn;
        public int pumpkin;
    }

    [Serializable]
    public class TowerMaterials
    {
        public int wood;
        public int stone;
        public int iron;
        public int gold;
        public int diamond;
    }

    [Serializable]
    public class WorldStatus
    {
        public int daysSurvived;
        public int availableTowerSpots;
        public int existingTowers;
        public Shops shops;
    }

    [Serializable]
    public class Shops
    {
        public SeedShop seedShopAvailableSeeds;
        public ResourcesShop resourcesShopAvailableMaterials;
        public ToolShop toolShopAvailableUpgradeLevel;
        public UtilityShop utilityShopAvailableUpgradeLevels;
    }

    [Serializable]
    public class SeedShop
    {
        public int wheat;
        public int carrot;
        public int tomato;
        public int corn;
        public int pumpkin;
    }

    [Serializable]
    public class ResourcesShop
    {
        public int wood;
        public int stone;
        public int iron;
        public int gold;
        public int diamond;
    }

    [Serializable]
    public class ToolShop
    {
        public int hoe;
        public int hammer;
        public int sword;
    }

    [Serializable]
    public class UtilityShop
    {
        public int health;
        public int speed;
        public int healthRegen;
    }

    [Serializable]
    public class LastRoundActivity
    {
        public int damageTaken;
        public int cropsPlanted;
        public int cropsHarvested;
        public int cropsDestroyed;
        public int towersBuilt;
        public int towersDestroyed;
    }

    [Serializable]
    public class NextRoundEnemies
    {
        public int slime;
        public int skeleton;
        public int goblinArcher;
        public int chicken;
        public int orc;
        public int demon;
    }

    public class NPCSpeech : MonoBehaviour
    {
        [SerializeField] private SpeechBubbleBehaviour speechBubbleBehaviour;
        [SerializeField] private NPCType npcType;
        [SerializeField] private string apiKey = "your-api-key-here";

        private const string API_URL_PREFIX = "https://creolt9mzl.execute-api.us-east-1.amazonaws.com/dev/";
        private string _apiURL;
        private List<string> _previousResponses = new List<string>();
        private bool _isWaitingForResponse = false;

        private const float REFRESH_INTERVAL = 10f;
        private float _timeSinceLastRefresh = 0f;
        private const string DEFAULT_MESSAGE = "...";

        private void Awake()
        {
            var urlPostfix = npcType == NPCType.Start ? "startnpc" : "storenpc";
            _apiURL = API_URL_PREFIX + urlPostfix;
            Debug.Log($"Initialized {npcType} NPC with URL: {_apiURL}");
        }

        private void Update()
        {
            if (npcType == NPCType.Mid)
            {
                _timeSinceLastRefresh += Time.deltaTime;
                if (_timeSinceLastRefresh >= REFRESH_INTERVAL && !_isWaitingForResponse)
                {
                    _timeSinceLastRefresh = 0f;
                    SendNPCRequest();
                }
            }
        }

        public void TriggerNPCSpeech()
        {
            if (!_isWaitingForResponse)
            {
                _timeSinceLastRefresh = 0f;
                SendNPCRequest();
            }
        }

        private void Start() => SendNPCRequest();

        private void Speak(string text)
        {
            speechBubbleBehaviour.SetText(text);
            if (npcType == NPCType.Mid && !_isWaitingForResponse)
            {
                _previousResponses.Add(text);
                if (_previousResponses.Count > 5)
                {
                    _previousResponses.RemoveAt(0);
                }
            }
        }

        private string GetRequestJson()
        {
            if (npcType == NPCType.Start)
            {
                var request = new StartNPCRequest
                {
                    totalGamesPlayed = 0,
                    consecutiveGamesPlayed = 0,
                    killedLastGameBy = "none",
                    daysSurvivedLastGame = 0,
                    daysSurvivedHighScore = 0
                };
                return JsonUtility.ToJson(request);
            }
            else
            {
                var request = new MidNPCRequest
                {
                    playerStatus = new PlayerStatus
                    {
                        health = 4,
                        money = 1200,
                        inventory = new Inventory
                        {
                            crops = new Crops { wheat = 10, carrot = 5, tomato = 3, corn = 4, pumpkin = 0 },
                            towerMaterials = new TowerMaterials { wood = 3, stone = 2, iron = 1, gold = 0, diamond = 0 }
                        },
                        worldStatus = new WorldStatus
                        {
                            daysSurvived = 14,
                            availableTowerSpots = 3,
                            existingTowers = 3,
                            shops = new Shops
                            {
                                seedShopAvailableSeeds = new SeedShop { wheat = 100, carrot = 75, tomato = 50, corn = 40, pumpkin = 25 },
                                resourcesShopAvailableMaterials = new ResourcesShop { wood = 200, stone = 150, iron = 100, gold = 50, diamond = 20 },
                                toolShopAvailableUpgradeLevel = new ToolShop { hoe = 2, hammer = 3, sword = 3 },
                                utilityShopAvailableUpgradeLevels = new UtilityShop { health = 2, speed = 1, healthRegen = 3 }
                            }
                        },
                        lastRoundActivity = new LastRoundActivity
                        {
                            damageTaken = 3,
                            cropsPlanted = 20,
                            cropsHarvested = 12,
                            cropsDestroyed = 4,
                            towersBuilt = 8,
                            towersDestroyed = 2
                        },
                        nextRoundEnemies = new NextRoundEnemies
                        {
                            slime = 4,
                            skeleton = 2,
                            goblinArcher = 1,
                            chicken = 3,
                            orc = 1,
                            demon = 0
                        },
                        previousResponses = _previousResponses
                    }
                };
                return JsonUtility.ToJson(request);
            }
        }

        private void SendNPCRequest()
        {
            try
            {
                _isWaitingForResponse = true;
                Speak(DEFAULT_MESSAGE);
                StartCoroutine(SendNPCRequestCoroutine());
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send NPC request: {ex.Message}");
                Speak("Sorry, I don't want to talk with you right now...");
                _isWaitingForResponse = false;
            }
        }

        private IEnumerator SendNPCRequestCoroutine()
        {
            string jsonBody = GetRequestJson();
            Debug.Log($"Sending request: {jsonBody}");

            using var request = new UnityWebRequest(_apiURL, "POST")
            {
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonBody)),
                downloadHandler = new DownloadHandlerBuffer()
            };

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-api-key", apiKey);

            yield return request.SendWebRequest();

            _isWaitingForResponse = false;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API request failed for {npcType}: {request.error}\nResponse: {request.downloadHandler.text}");
                Speak("Sorry, I'm not feeling well...");
                yield break;
            }

            try
            {
                var response = JsonUtility.FromJson<NPCResponse>(request.downloadHandler.text);
                Speak(response.response);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to process {npcType} response: {ex.Message}\nResponse: {request.downloadHandler.text}");
                Speak("WHO AM I?! WHERE AM I?!");
            }
        }
    }
}