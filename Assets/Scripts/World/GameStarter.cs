using System;
using AWSUtils;
using Crops;
using DG.Tweening;
using Player;
using Stores;
using Towers;
using UI.GameUI;
using UnityEngine;
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
        private bool _initialized = false;
        
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

        private void OnEnable()
        {
            if (GameStatistics.Instance != null)
            {
                GameStatistics.Instance.OnGameDataLoaded += HandleGameDataLoaded;
            }
        }

        private void OnDisable()
        {
            if (GameStatistics.Instance != null)
            {
                GameStatistics.Instance.OnGameDataLoaded -= HandleGameDataLoaded;
            }
        }

        private void HandleGameDataLoaded()
        {
            if (GameStatistics.Instance.LoadedGameData?.CurrentGameState != null)
            {
                var gameState = GameStatistics.Instance.LoadedGameData.CurrentGameState;
                bool isDefaultGameState = gameState.day == 0 && gameState.curHealth == 0 && gameState.cash == 0;

                if (isDefaultGameState)
                {
                    Debug.Log("No valid save data found. Hiding continue button.");
                    continueButton.SetActive(false);
                }
                else
                {
                    EnableContinueButton();
                }
            }
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

            if (GameStatistics.Instance?.LoadedGameData?.CurrentGameState != null)
            {
                HandleGameDataLoaded();
            }
            
            _initialized = true;
        }

        private void EnableContinueButton()
        {
            if (!continueButton.activeSelf && menuCanvas.activeSelf)
            {
                Debug.Log("Enabling continue button - save data found");
                continueButton.SetActive(true);
                
                var newGamePos = newGameButton.transform.localPosition;
                newGamePos = new Vector3(-xOffsetBetweenNewGameAndContinue, newGamePos.y, newGamePos.z);
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
            player.GetComponent<PlayerAnimationManager>().Init();
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
            var gameState = GameStatistics.Instance.LoadedGameData?.CurrentGameState;
            if (gameState == null)
            {
                Debug.LogError("Attempted to continue game without valid save data!");
                return;
            }

            menuCanvas.SetActive(false);
            GameContinued = true;
            GameData.Instance.LoadFromGameState(gameState);
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
