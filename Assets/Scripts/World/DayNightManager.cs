using System;
using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using Enemies;
using Player;
using Stores;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utils;
using Utils.Data;
using Random = UnityEngine.Random;

namespace World
{
    public class DayNightManager : Singleton<DayNightManager>
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private TextMeshProUGUI waveDeclarationText;
        
        public bool GameStarted { get; private set; }
        private DayNightData.Cycle _currentCycle;
        private float _curDayProgress = 0f;
        private Coroutine _dayNightCycleCR;
        private Coroutine _spawnEnemiesCR;
        
        private float _textFadeDuration = 2f;
        private float _textFadeDelay = 1f;
        private Tween _darkenTween;
        private Tween _lightenTween;
        public bool DayTime { get; private set; } = true;
        public bool NightTime { get; private set; } = false;

        private int _totalEnemies;
        private int _remainingEnemies;
        private Tween _waveDeclarationTween;
        [SerializeField] private PlayerHealthManager playerHealthManager;

        void Start()
        {
            _currentCycle = DayNightData.GetCycle(GameData.Instance.day, GameData.Instance.thisDayEnemies, GameData.Instance.thisNightEnemies);
        }

        private void Update()
        {
            if (GameStarted && !playerHealthManager.IsDead)
                GameData.Instance.secondsSinceGameStarted += Time.deltaTime;
        }


        public void StartGame()
        {
            GameStarted = true;
            _dayNightCycleCR = StartCoroutine(DayNightCycle());
        }

        public void JumpToNight()
        {
            if (!GameStarted) return;
            if (NightTime) return;
            if (_dayNightCycleCR != null)
            {
                StopCoroutine(_dayNightCycleCR);
            }
            if (_darkenTween != null)
            {
                _darkenTween.Kill();
            }

            globalLight.intensity = Constants.NightLightIntensity;
            DayTime = false;
            NightTime = true;
            DayNightRollBehaviour.Instance.JumpToNight();
            _dayNightCycleCR = StartCoroutine(DayNightCycle());
        }

        public IEnumerator DayNightCycle()
        {
            while (!playerHealthManager.IsDead)
            {
                if (!NightTime)
                {
                    // daytime
                    StartDay();
                    while (_curDayProgress < 1 - DayNightRollBehaviour.Instance.ChangeLightProgressDuration)
                    {
                        var progress = Time.deltaTime / _currentCycle.DayWave.DurationInSeconds;
                        _curDayProgress += progress;
                        DayNightRollBehaviour.Instance.AddProgress(progress);
                        yield return null;
                    }

                    var remainingTime = (1 - _curDayProgress) * _currentCycle.DayWave.DurationInSeconds;
                    EndDay(remainingTime);
                    while (_curDayProgress < 1)
                    {
                        var progress = Time.deltaTime / _currentCycle.DayWave.DurationInSeconds;
                        _curDayProgress += progress;
                        DayNightRollBehaviour.Instance.AddProgress(progress);
                        yield return null;
                    }
                }

                // night time
                StartNight();
                // wait for all enemies to be destroyed
                while (_remainingEnemies > 0)
                {
                    yield return null;
                }

                // transition to day
                _currentCycle = DayNightData.GetCycle(GameData.Instance.day, GameData.Instance.thisDayEnemies, GameData.Instance.thisNightEnemies);
                GetNextCycleAsync(EnemySpawnData.Instance);
                GameData.Instance.day++;
                _curDayProgress = 0;
                EndNight(DayNightRollBehaviour.Instance.ChangeLightProgressDuration *
                         _currentCycle.DayWave.DurationInSeconds);
                while (_curDayProgress < DayNightRollBehaviour.Instance.ChangeLightProgressDuration)
                {
                    var progress = Time.deltaTime / _currentCycle.DayWave.DurationInSeconds;
                    _curDayProgress += progress;
                    DayNightRollBehaviour.Instance.AddProgress(progress);
                    yield return null;
                }
            }
        }
        
        private struct EnemySpawn
        {
            public Enemy Enemy;
            public Vector2 Position;
            public float SpawnTime;
        }

        private void SpawnEnemy(EnemySpawn enemySpawn, Vector2[] positionOptions)
        {
            Vector2 spawnPosition;
            do
            {
                spawnPosition =  positionOptions[Random.Range(0, positionOptions.Length)];
            } while (Vector2.Distance(spawnPosition, playerTransform.position) < Constants.MinEnemySpawnDistance);
            var enemy = EnemyPool.Instance.GetEnemy(enemySpawn.Enemy, spawnPosition);
        }

        private IEnumerator SpawnEnemies()
        {
            var enemySpawnData = NightTime? _currentCycle.NightWave.EnemySpawns : _currentCycle.DayWave.EnemySpawns;
            var spawnDurationInSeconds = NightTime? _currentCycle.NightWave.SpawnDurationInSeconds : _currentCycle.DayWave.SpawnDurationInSeconds;
            var spawnPositionOptions = enemySpawnData.SpawnPositions;
            var spawnAmounts = enemySpawnData.SpawnAmounts;
            var totalSpawnAmounts = enemySpawnData.TotalSpawnsAmount();
            if (NightTime)
            {
                _totalEnemies = totalSpawnAmounts;
                _remainingEnemies = totalSpawnAmounts;
            }
            var enemySpawns = new EnemySpawn[totalSpawnAmounts];
            // randomize spawn time and position for each enemy
            for (int enemyType = 0; enemyType < spawnAmounts.Length; enemyType++)
            {
                var spawnAmount = spawnAmounts[enemyType];
                for (int i = 0; i < spawnAmount; i++)
                {
                    var enemySpawn = new EnemySpawn
                    {
                        Enemy = (Enemy)enemyType,
                        SpawnTime = Random.Range(0, spawnDurationInSeconds)
                    };
                    enemySpawns[i] = enemySpawn;
                }
            }

            yield return null;
            // sort by spawn time
            System.Array.Sort(enemySpawns, (a, b) => a.SpawnTime.CompareTo(b.SpawnTime));
            // spawn enemies
            float elapsedTime = 0f;
            int currentEnemyIndex = 0;
            while (currentEnemyIndex < enemySpawns.Length)
            {
                if (elapsedTime >= enemySpawns[currentEnemyIndex].SpawnTime)
                {
                    var enemySpawn = enemySpawns[currentEnemyIndex];
                    SpawnEnemy(enemySpawn, spawnPositionOptions[(int)enemySpawn.Enemy]);
                    currentEnemyIndex++;
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        private void EndNight(float duration)
        {
            Debug.Log("Ending night");
            _lightenTween = DOTween.To(
                () => globalLight.intensity,
                x => globalLight.intensity = x,
                Constants.DayLightIntensity,
                duration
            );
            NightTime = false;
        }

        private void EndDay(float duration)
        {
            Debug.Log("Ending day");
            NightTime = false;
            DayTime = false;
            _darkenTween = DOTween.To(
                () => globalLight.intensity, 
                x => globalLight.intensity = x, 
                Constants.NightLightIntensity, 
                duration
            );
            EventManager.Instance.TriggerEvent(EventManager.DayEnded, null);
        }
        
        private void ShowWaveDeclaration()
        {
            if (_waveDeclarationTween != null)
            {
                _waveDeclarationTween.Kill();
            }
            waveDeclarationText.alpha = 0;
            waveDeclarationText.gameObject.SetActive(true);
            _waveDeclarationTween = waveDeclarationText.DOFade(1, _textFadeDuration).OnComplete(() =>
            {
                waveDeclarationText.DOFade(0, _textFadeDuration).SetDelay(_textFadeDelay).OnComplete(() =>
                {
                    waveDeclarationText.gameObject.SetActive(false);
                });
            });
        }

        private void StartDay()
        {
            Debug.Log($"Starting day {GameData.Instance.day + 1}");
            waveDeclarationText.text = $"- DAY {GameData.Instance.day + 1} -";
            _spawnEnemiesCR = StartCoroutine(SpawnEnemies());
            ShowWaveDeclaration();
            DayTime = true;
            NightTime = false;
            if (GameData.Instance.day > 0)
                GeneralStoreManager.Instance.UpdateStock(_currentCycle.NewCrops, _currentCycle.NewMaterials);
            EventManager.Instance.TriggerEvent(EventManager.DayStarted, null);
        }

        private void StartNight()
        {
            Debug.Log("Starting night");
            DayTime = false;
            NightTime = true;
            _spawnEnemiesCR = StartCoroutine(SpawnEnemies());
            waveDeclarationText.text = $"- NIGHT -";
            ShowWaveDeclaration();
            EventManager.Instance.TriggerEvent(EventManager.NightStarted, null);
        }

        private async void GetNextCycleAsync(EnemySpawnData spawnData)
        {
            await Task.Run(() =>
            {
                DayNightData.WarmupCycle(GameData.Instance.day + 1, spawnData);
            });
            GameData.Instance.SaveToJson();
        }

        public void EnemyDied(float waitTime)
        {
            if (NightTime)
            {
                StartCoroutine(EnemyDiedCR(waitTime));
            }
        }

        private IEnumerator EnemyDiedCR(float waitTime)
        {
            var elapsedTime = 0f;
            while (elapsedTime < waitTime)
            {
                var timePassed = Time.deltaTime;
                var progress = (timePassed / waitTime) / _totalEnemies;
                DayNightRollBehaviour.Instance.AddProgress(progress);
                elapsedTime += timePassed;
                yield return null;
            }
            _remainingEnemies--;
        }
    }
}
