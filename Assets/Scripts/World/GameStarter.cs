using System;
using DG.Tweening;
using UnityEngine;
using Utils;
using Utils.Data;

namespace World
{
    public class GameStarter : Singleton<GameStarter>
    {
        private static readonly Vector2 PlayerStartPos = new Vector2(-14, -9.5f);
        [SerializeField] private Transform boatWithPlayer;
        [SerializeField] private Transform anchoredBoat;
        [SerializeField] private Transform player;
        [SerializeField] private GameObject game;
        [SerializeField] private GameObject gameCanvas;
        [SerializeField] private GameObject menuCanvas;
        [SerializeField] private GameObject newGameButton;
        [SerializeField] private GameObject continueButton;
        private float xOffsetBetweenNewGameAndContinue = 300f;

        private string _savedGameJson;
        private string _dummyJson = "{\n    \"HealthUpgradeLevel\": 0,\n    \"RegenUpgradeLevel\": 0,\n    \"SpeedUpgradeLevel\": 0,\n    \"SwordLevel\": 0,\n    \"HoeLevel\": 0,\n    \"HammerLevel\": 1,\n    \"Cash\": 99879,\n    \"Day\": 1,\n    \"CurHealth\": 6,\n    \"SecondsSinceGameStarted\": 108.235,\n    \"InventoryCrops\": [\n        2,\n        0,\n        0,\n        0,\n        0\n    ],\n    \"InventoryMaterials\": [\n        1,\n        0,\n        0,\n        0,\n        0\n    ],\n    \"CropsInStore\": [\n        5,\n        0,\n        0,\n        0,\n        0\n    ],\n    \"MaterialsInStore\": [\n        1,\n        0,\n        0,\n        0,\n        0\n    ],\n    \"ThisDayEnemies\": [\n        0,\n        0,\n        0,\n        0,\n        0,\n        0\n    ],\n    \"ThisNightEnemies\": [\n        5,\n        4,\n        0,\n        0,\n        0,\n        0\n    ],\n    \"Towers\": [\n        [],\n        [],\n        [],\n        [],\n        [],\n        [\n            {\n                \"material\": 0,\n                \"progress\": 1.000658,\n                \"health\": 5\n            }\n        ],\n        [],\n        [\n            {\n                \"material\": 0,\n                \"progress\": 0.2982577,\n                \"health\": 5\n            }\n        ],\n        []\n    ],\n    \"PlantedCrops\": [\n        {\n            \"Key\": {\n                \"x\": -3,\n                \"y\": 39\n            },\n            \"Value\": {\n                \"cropType\": 0,\n                \"growthProgress\": 0.4730192,\n                \"destroyProgress\": 0\n            }\n        }\n    ]\n}";
        
        public bool GameStarted { get; private set; }

        private void Awake()
        {
            anchoredBoat.gameObject.SetActive(false);
            game.SetActive(false);
            gameCanvas.SetActive(false);
            player.gameObject.SetActive(false);
            continueButton.SetActive(false);
            _savedGameJson = _dummyJson; // DVIR - try load from dynamo
            if (_savedGameJson != null)
            {
                continueButton.SetActive(true);
                var newGamePos = newGameButton.transform.localPosition;
                newGamePos = new Vector3(newGamePos.x - xOffsetBetweenNewGameAndContinue, 
                    newGamePos.y, newGamePos.z);
                newGameButton.transform.localPosition = newGamePos;
                var continuePos = continueButton.transform.localPosition;
                continuePos = new Vector3(continuePos.x + xOffsetBetweenNewGameAndContinue, continuePos.y, continuePos.z);
                continueButton.transform.localPosition = continuePos;
            }
        }

        private void PositionPlayer()
        {
            anchoredBoat.gameObject.SetActive(true);
            player.gameObject.SetActive(true);
            boatWithPlayer.gameObject.SetActive(false);
            SetPlayerPosition();
            game.SetActive(true);
            gameCanvas.SetActive(true);
        }

        private void GameStart()
        {
            GetComponent<Collider2D>().enabled = false;
            Debug.Log("Game Started");
            GameStarted = true;
            DayNightManager.Instance.StartGame();
        }

        public void PressedStartNewGame()
        {
            menuCanvas.SetActive(false);
            GameData.Instance.NewGame();
            boatWithPlayer.DOMoveX(anchoredBoat.position.x, 8f).SetEase(Ease.OutQuad).OnComplete(PositionPlayer);
        }
        
        public void PressedContinue()
        {
            menuCanvas.SetActive(false);
            ContinueFromSavedJson();
        }

        public void ContinueFromSavedJson()
        {
            GameData.Instance.LoadFromJson(_savedGameJson);
            PositionPlayer();
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
