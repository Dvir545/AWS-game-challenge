using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Utils;
using Utils.Data;
using Player;
using UI.GameUI;
using World;

namespace AWSUtils
{
    public class NPCSpeech : MonoBehaviour
    {
        [SerializeField] private SpeechBubbleBehaviour speechBubbleBehaviour;
        [SerializeField] private NPCType npcType;
        [SerializeField] private string apiKey = "eVZBuSzrn113f2bFvQjTZ9tXmNyhHGxU3YcwPmWT";
        [SerializeField] private Camera mainCamera;

        private PlayerData playerData;
        private const string API_URL_PREFIX = "https://creolt9mzl.execute-api.us-east-1.amazonaws.com/dev/";
        private string _apiURL;
        private List<string> _previousResponses = new List<string>();
        private bool _isWaitingForResponse = false;
        private string _lastDayResponse = "";
        private string _currentText = "";
        private bool _isNightTime = false;
        private const string NIGHT_MESSAGE_STORE = "Z Z Z...";
        private const string NIGHT_MESSAGE_START = "Slay these monsters!";
        private const string DEFAULT_MESSAGE = "...";
        private const int MAX_RETRIES = 3;
        private const float RETRY_DELAY = 2f;
        
        private GameObject _canvas;
        private float _showSpeechBubbleRadius = 8f;
        private Transform _playerTransform;
        private CanvasGroup _speechBubbleCanvasGroup;
        private RectTransform _speechBubbleRectTransform;
        private bool _isPlayerBehind;

        private void Awake()
        {
            _canvas = transform.GetChild(0).gameObject;
            _canvas.SetActive(false);
            _speechBubbleCanvasGroup = _canvas.GetComponent<CanvasGroup>();
            _playerTransform = GameObject.FindWithTag("Player").transform;
            var urlPostfix = npcType == NPCType.Start ? "startnpc" : "storenpc";
            _apiURL = API_URL_PREFIX + urlPostfix;
            Debug.Log($"Initialized {npcType} NPC with URL: {_apiURL}");
            
            playerData = FindObjectOfType<PlayerData>();
            if (playerData == null)
            {
                Debug.LogError("Could not find PlayerData component!");
            }

            if (npcType == NPCType.Start)
            {
                _speechBubbleRectTransform = speechBubbleBehaviour.GetComponent<RectTransform>();
                GameStatistics.Initialize();
            }
        }

        public void TriggerNPCSpeech()
        {
            if (!_isWaitingForResponse && !_isNightTime)
            {
                SendNPCRequest();
            }
        }

        private void Start()
        {
            if (npcType == NPCType.Start && GameStarter.Instance.GameContinued)
                return;
            SendNPCRequest();
        } 
        
        private void Update()
        {
            if (string.IsNullOrEmpty(_currentText))
                return;
            var distanceToPlayer = Vector2.Distance(_playerTransform.position, transform.position);
            if (!IsSpeechBubbleVisible() && distanceToPlayer <= _showSpeechBubbleRadius)
            {
                ShowSpeechBubble();
            }
            else if (IsSpeechBubbleVisible() && distanceToPlayer > _showSpeechBubbleRadius)
            {
                HideSpeechBubble();
            }
            if (IsSpeechBubbleVisible())
            {
                UpdateSpeechBubbleTransparency();
            }
        }

        private void UpdateSpeechBubbleTransparency()
        {
            var offset = new Vector2(0f, 50f);
            Vector2 screenPoint = mainCamera.WorldToScreenPoint(_playerTransform.position);
            screenPoint += offset;
            bool isPlayerBehind = RectTransformUtility.RectangleContainsScreenPoint(
                _speechBubbleRectTransform, 
                screenPoint, 
                mainCamera
            );
    
            // Set the appropriate alpha
            _speechBubbleCanvasGroup.alpha = isPlayerBehind ? 0.2f : 1f;
        }

        private void ShowSpeechBubble() => _canvas.SetActive(true);
        private bool IsSpeechBubbleVisible() => _canvas.activeSelf;
        private void HideSpeechBubble() => _canvas.SetActive(false);

        private void Speak(string text)
        {
            _currentText = text;
            speechBubbleBehaviour.SetText(text);
        }

        private string GetRequestJson()
        {
            if (npcType == NPCType.Start)
            {
                var request = new StartNPCRequest
                {
                    totalGamesPlayed = GameStatistics.Instance.totalGamesPlayed,
                    consecutiveGamesPlayed = GameStatistics.Instance.consecutiveGamesPlayed,
                    killedLastGameBy = GameStatistics.Instance.killedLastGameBy,
                    daysSurvivedLastGame = GameStatistics.Instance.daysSurvivedLastGame,
                    daysSurvivedHighScore = GameStatistics.Instance.daysSurvivedHighScore
                };
                return JsonUtility.ToJson(request);
            }
            else
            {
                var request = new MidNPCRequest
                {
                    playerStatus = new PlayerStatus
                    {
                        health = GameData.Instance.curHealth,
                        money = GameData.Instance.cash,
                        inventory = new Inventory
                        {
                            crops = new Crops 
                            { 
                                wheat = GameData.Instance.crops[0],
                                carrot = GameData.Instance.crops[1],
                                tomato = GameData.Instance.crops[2],
                                corn = GameData.Instance.crops[3],
                                pumpkin = GameData.Instance.crops[4]
                            },
                            towerMaterials = new TowerMaterials 
                            { 
                                wood = GameData.Instance.materials[0],
                                stone = GameData.Instance.materials[1],
                                iron = GameData.Instance.materials[2],
                                gold = GameData.Instance.materials[3],
                                diamond = GameData.Instance.materials[4]
                            }
                        },
                        worldStatus = new WorldStatus
                        {
                            daysSurvived = GameData.Instance.day,
                            availableTowerSpots = Constants.TowerCount - GetExistingTowersCount(),
                            existingTowers = GetExistingTowersCount(),
                            shops = new Shops
                            {
                                seedShopAvailableSeeds = new SeedShop 
                                { 
                                    wheat = GameData.Instance.cropsInStore[0],
                                    carrot = GameData.Instance.cropsInStore[1],
                                    tomato = GameData.Instance.cropsInStore[2],
                                    corn = GameData.Instance.cropsInStore[3],
                                    pumpkin = GameData.Instance.cropsInStore[4]
                                },
                                resourcesShopAvailableMaterials = new ResourcesShop
                                {
                                    wood = GameData.Instance.materialsInStore[0],
                                    stone = GameData.Instance.materialsInStore[1],
                                    iron = GameData.Instance.materialsInStore[2],
                                    gold = GameData.Instance.materialsInStore[3],
                                    diamond = GameData.Instance.materialsInStore[4]
                                },
                                toolShopAvailableUpgradeLevel = new ToolShop
                                {
                                    hoe = playerData.GetToolLevel(HeldTool.Hoe),
                                    hammer = playerData.GetToolLevel(HeldTool.Hammer),
                                    sword = playerData.GetToolLevel(HeldTool.Sword)
                                },
                                utilityShopAvailableUpgradeLevels = new UtilityShop
                                {
                                    health = playerData.GetUpgradeLevel(Upgrade.Health),
                                    speed = playerData.GetUpgradeLevel(Upgrade.Speed),
                                    healthRegen = playerData.GetUpgradeLevel(Upgrade.Regen)
                                }
                            }
                        },
                        lastRoundActivity = ActivityTracker.Instance.CurrentActivity,
                        nextRoundEnemies = new NextRoundEnemies
                        {
                            slime = GameData.Instance.thisDayEnemies[0],
                            skeleton = GameData.Instance.thisDayEnemies[1],
                            goblinArcher = GameData.Instance.thisDayEnemies[2],
                            chicken = GameData.Instance.thisDayEnemies[3],
                            orc = GameData.Instance.thisDayEnemies[4],
                            demon = GameData.Instance.thisDayEnemies[5]
                        },
                        previousResponses = _previousResponses
                    }
                };
                return JsonUtility.ToJson(request);
            }
        }

        private int GetExistingTowersCount()
        {
            int count = 0;
            foreach (var towerList in GameData.Instance.towers)
            {
                if (towerList.Count > 0)
                {
                    count++;
                }
            }
            return count;
        }

        private void OnEnable()
        {
            EventManager.Instance.StartListening(EventManager.DayStarted, OnDayStarted);
            EventManager.Instance.StartListening(EventManager.NightStarted, OnNightStarted);
        }

        private void OnDisable()
        {
            if (EventManager.Instance == null)
                return;
            EventManager.Instance.StopListening(EventManager.DayStarted, OnDayStarted);
            EventManager.Instance.StopListening(EventManager.NightStarted, OnNightStarted);
        }

        private void OnDayStarted(object _)
        {
            _isNightTime = false;
            if (npcType == NPCType.Mid)
            {
                TriggerNPCSpeech();
            }
            else if (npcType == NPCType.Start && !string.IsNullOrEmpty(_lastDayResponse))
            {
                Speak(_lastDayResponse);
            }
        }

        private void OnNightStarted(object _)
        {
            _isNightTime = true;
            if (npcType == NPCType.Mid)
            {
                Speak(NIGHT_MESSAGE_STORE);
            }
            else
            {
                if (!string.IsNullOrEmpty(_currentText))
                {
                    _lastDayResponse = _currentText;
                }
                Speak(NIGHT_MESSAGE_START);
            }
        }

        private void SendNPCRequest()
        {
            try
            {
                _isWaitingForResponse = true;
                // Speak(DEFAULT_MESSAGE);
                StartCoroutine(SendNPCRequestWithRetry(0));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send NPC request: {ex.Message}");
                Speak("Sorry, I don't want to talk with you right now...");
                _isWaitingForResponse = false;
            }
        }

        private IEnumerator SendNPCRequestWithRetry(int retryCount)
        {
            string jsonBody = GetRequestJson();
            Debug.Log($"Sending request (attempt {retryCount + 1}/{MAX_RETRIES}): {jsonBody}");

            using var request = new UnityWebRequest(_apiURL, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-api-key", apiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API request failed for {npcType}: {request.error}\nResponse: {request.downloadHandler.text}");
                
                if (retryCount < MAX_RETRIES - 1)
                {
                    Debug.Log($"Retrying request in {RETRY_DELAY} seconds...");
                    yield return new WaitForSeconds(RETRY_DELAY);
                    yield return StartCoroutine(SendNPCRequestWithRetry(retryCount + 1));
                    yield break;
                }
                
                _isWaitingForResponse = false;
                Speak("?");
                yield break;
            }

            try
            {
                var response = JsonUtility.FromJson<NPCResponse>(request.downloadHandler.text);
                if (npcType == NPCType.Start)
                {
                    _lastDayResponse = response.response;
                }
                else if (npcType == NPCType.Mid && !string.IsNullOrEmpty(response.response))
                {
                    _previousResponses = new List<string> { response.response };
                }
                Speak(response.response);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to process {npcType} response: {ex.Message}\nResponse: {request.downloadHandler.text}");
                Speak("Keep going, warrior!");
            }
            finally
            {
                _isWaitingForResponse = false;
            }
        }
    }
}