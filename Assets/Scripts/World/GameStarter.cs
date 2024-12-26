using System;
using System.Collections;
using AWSUtils;
using Crops;
using DG.Tweening;
using Player;
using Stores;
using Towers;
using UI.GameUI;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering.Universal;
using Utils;
using Utils.Data;

namespace World
{
    public class GameStarter : Singleton<GameStarter>
    {
        private static readonly Vector2 PlayerMenuPos = new Vector2(-14, -12f);
        private static readonly Vector2 PlayerStartPos = new Vector2(-14, -9.5f);
        [SerializeField] private Transform boatWithPlayer;
        [SerializeField] private Transform anchoredBoat;
        [SerializeField] private Transform player;
        [SerializeField] private GameObject game;
        [SerializeField] private GameObject gameCanvas;
        [SerializeField] private GameObject menuCanvas;
        [SerializeField] private GameObject newGameButton;
        [SerializeField] private GameObject continueButton;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private CashTextBehavior cashBehaviour;
        [SerializeField] private DayNightRollBehaviour dayNightRollBehaviour;
        [SerializeField] private UpgradeUIBehaviour speedUIBehaviour;
        [SerializeField] private UpgradeUIBehaviour regenUIBehaviour;
        [SerializeField] private UpgradeUIBehaviour staminaUIBehaviour;
        [SerializeField] private UpgradeUIBehaviour knockbackUIBehaviour;
        [SerializeField] private HealingMatBehaviour healingMatBehaviour;
        [SerializeField] private ActButtonBehavior actButtonBehavior;
        [SerializeField] private CropManager cropManager;
        [SerializeField] private FarmingManager farmingManager;
        [SerializeField] private MaterialManager materialManager;
        [SerializeField] private TowerBuildManager towerBuildManager;
        [SerializeField] private NPCSpeech npcBottom;
        [SerializeField] private NPCSpeech npcMid;
        [SerializeField] private ToolBuyer[] tools;
        [SerializeField] private UpgradeBuyer[] upgrades;
        private float xOffsetBetweenNewGameAndContinue = 260f;
        private Collider2D _collider;
        private Vector2 _boatStartPos;

        private string _savedGameJson;
        private string _dummyJson = "{\n    \"HealthUpgradeLevel\": 0,\n    \"RegenUpgradeLevel\": 0,\n    \"SpeedUpgradeLevel\": 0,\n    \"StaminaUpgradeLevel\": 0,\n    \"KnockbackUpgradeLevel\": 0,\n    \"SwordLevel\": 0,\n    \"HoeLevel\": 0,\n    \"HammerLevel\": 1,\n    \"Cash\": 99879,\n    \"Day\": 1,\n    \"CurHealth\": 6,\n    \"SecondsSinceGameStarted\": 108.235,\n    \"InventoryCrops\": [\n        2,\n        0,\n        0,\n        0,\n        0\n    ],\n    \"InventoryMaterials\": [\n        1,\n        0,\n        0,\n        0,\n        0\n    ],\n    \"CropsInStore\": [\n        5,\n        0,\n        0,\n        0,\n        0\n    ],\n    \"MaterialsInStore\": [\n        1,\n        0,\n        0,\n        0,\n        0\n    ],\n    \"ThisDayEnemies\": [\n        0,\n        0,\n        0,\n        0,\n        0,\n        0\n    ],\n    \"ThisNightEnemies\": [\n        5,\n        4,\n        0,\n        0,\n        0,\n        0\n    ],\n    \"Towers\": [\n        [],\n        [],\n        [],\n        [],\n        [],\n        [\n            {\n                \"material\": 0,\n                \"progress\": 1.000658,\n                \"health\": 5\n            }\n        ],\n        [],\n        [\n            {\n                \"material\": 0,\n                \"progress\": 0.2982577,\n                \"health\": 5\n            }\n        ],\n        []\n    ],\n    \"PlantedCrops\": [\n        {\n            \"Key\": {\n                \"x\": -3,\n                \"y\": 39\n            },\n            \"Value\": {\n                \"cropType\": 0,\n                \"growthProgress\": 0.4730192,\n                \"destroyProgress\": 0\n            }\n        }\n    ]\n}";
        
        public bool GameStarted { get; private set; }
        public bool GameContinued { get; private set; }
        

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _boatStartPos = boatWithPlayer.position;
            GameStatistics.Instance.Init("");
            SoundManager.Instance.SyncMusicVolume();
            SoundManager.Instance.PlayOcean();
            SoundManager.Instance.StartEntryMusicCR();
        }

        public void Init()
        {
            _collider.enabled = true;
            GameStarted = false;
            GameContinued = false;
            SoundManager.Instance.Init();
            SoundManager.Instance.PlayOcean();
            SoundManager.Instance.PlayEntryMusic(1f);
            anchoredBoat.gameObject.SetActive(false);
            boatWithPlayer.gameObject.SetActive(false);
            boatWithPlayer.position = _boatStartPos;
            game.SetActive(false);
            gameCanvas.SetActive(false);
            player.gameObject.SetActive(false);
            player.transform.localPosition = PlayerMenuPos;
            continueButton.SetActive(false);
            menuCanvas.SetActive(true);
            globalLight.intensity = 1f;
            _savedGameJson = _dummyJson; // DVIR - try load from dynamo
            if (_savedGameJson != null)
            {
                continueButton.SetActive(true);
                var newGamePos = newGameButton.transform.localPosition;
                newGamePos = new Vector3(- xOffsetBetweenNewGameAndContinue, 
                    newGamePos.y, newGamePos.z);
                newGameButton.transform.localPosition = newGamePos;
                var continuePos = continueButton.transform.localPosition;
                continuePos = new Vector3(xOffsetBetweenNewGameAndContinue, continuePos.y, continuePos.z);
                continueButton.transform.localPosition = continuePos;
            }
        }

        private void SetupGame()
        {
            SoundManager.Instance.Init(stop: true);
            player.GetComponent<PlayerHealthManager>().Init();
            cashBehaviour.Init();
            actButtonBehavior.Init();
            dayNightRollBehaviour.Init();
            speedUIBehaviour.Init();
            regenUIBehaviour.Init();
            staminaUIBehaviour.Init();
            knockbackUIBehaviour.Init();
            healingMatBehaviour.Init();
            cropManager.Init();
            farmingManager.Init();
            materialManager.Init();
            towerBuildManager.Init();
            foreach (var tool in tools)
            {
                tool.Init();
            }
            foreach (var upgrade in upgrades)
            {
                upgrade.Init();
            }
            anchoredBoat.gameObject.SetActive(true);
            player.gameObject.SetActive(true);
            boatWithPlayer.gameObject.SetActive(false);
            SetPlayerPosition();
            game.SetActive(true);
            player.GetComponent<PlayerSoundManager>().Init();
            npcBottom.Init();
            npcMid.Init();
            gameCanvas.SetActive(true);
        }

        private void GameStart()
        {
            _collider.enabled = false;
            SoundManager.Instance.StartGame();
            Debug.Log("Game Started");
            GameStarted = true;
            DayNightManager.Instance.StartGame();
        }

        public void PressedStartNewGame()
        {
            SoundManager.Instance.StopEntryMusicCR();
            menuCanvas.SetActive(false);
            GameData.Instance.NewGame();
            boatWithPlayer.gameObject.SetActive(true);
            boatWithPlayer.DOMoveX(anchoredBoat.position.x, 6f).SetEase(Ease.OutQuad).OnComplete(SetupGame);
        }
        
        public void PressedContinue()
        {
            menuCanvas.SetActive(false);
            GameContinued = true;
            ContinueFromSavedJson();
        }

        public void ContinueFromSavedJson()
        {
            GameData.Instance.LoadFromJson(_savedGameJson);
            SetupGame();
        }
        
        private void SetPlayerPosition()
        {
            player.transform.localPosition = PlayerStartPos;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                GameStart();
            }
        }
    }
}
