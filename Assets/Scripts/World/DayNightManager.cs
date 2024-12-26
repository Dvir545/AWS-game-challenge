using System;
using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using Enemies;
using Player;
using Stores;
using TMPro;
using Towers;
using UI;
using UI.GameUI;
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
        
        private DayNightData.Cycle _currentCycle;
        private float _curDayPhaseProgress = 0f;
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
        private DayPhase _currentDayPhase;
        public DayPhase CurrentDayPhase => _currentDayPhase;

        private void Update()
        {
            if (GameStarter.Instance.GameStarted && !playerHealthManager.IsDead)
                GameData.Instance.secondsSinceGameStarted += Time.deltaTime;
        }


        public void StartGame()
        {
            NightTime = false;
            DayTime = true;
            _currentDayPhase = DayPhase.Day;
            _currentCycle = DayNightData.GetCycle(GameData.Instance.day, GameData.Instance.thisDayEnemies, GameData.Instance.thisNightEnemies);
            if (GameData.Instance.day >= DayNightData.FirstCycles.Count - 1)
            {
                GetNextCycleAsync(EnemySpawnData.Instance);
            }
            if (_dayNightCycleCR != null)
            {
                StopCoroutine(_dayNightCycleCR);
            }
            _dayNightCycleCR = StartCoroutine(DayNightCycle());
        }

        public void JumpToNight()
        {
            if (!GameStarter.Instance.GameStarted) return;
            if (NightTime) return;
            if (_currentDayPhase == DayPhase.Night) return;
            if (_dayNightCycleCR != null)
            {
                StopCoroutine(_dayNightCycleCR);
            }
            if (_spawnEnemiesCR != null)
            {
                StopCoroutine(_spawnEnemiesCR);
            }
            if (_darkenTween != null)
            {
                _darkenTween.Kill();
            }
            if (_lightenTween != null)
            {
                _lightenTween.Kill();
            }
            MinimapBehaviour.Instance.StopTweens();

            if (_currentDayPhase == DayPhase.NightEnd)  // still on night end of previous day
            {
                StartDay(force: true);
            }

            if (_currentDayPhase == DayPhase.Day)
            {
                EndDay(force: true);
            }
            if (_currentDayPhase == DayPhase.DayEnd)
            {
                DayNightRollBehaviour.Instance.JumpToNight();
                MinimapBehaviour.Instance.JumpToNight();
                StartNight();
                SoundManager.Instance.SwitchBackgroundMusic(false, 0f);
                _dayNightCycleCR = StartCoroutine(DayNightCycle());
            }
        }

        public IEnumerator DayNightCycle()
        {
            while (!playerHealthManager.IsDead)
            {
                if (!NightTime)
                {
                    // daytime
                    StartDay();
                    _curDayPhaseProgress = 0;
                    while (_curDayPhaseProgress < 1)
                    {
                        var progress = Time.deltaTime / (_currentCycle.DayWave.DurationInSeconds - 2*Constants.ChangeLightDurationInSeconds);
                        _curDayPhaseProgress += progress;
                        DayNightRollBehaviour.Instance.AddProgress(progress);
                        yield return null;
                    }

                    _curDayPhaseProgress = 0;
                    EndDay();
                    while (_curDayPhaseProgress < 1)
                    {
                        var progress = Time.deltaTime / Constants.ChangeLightDurationInSeconds;
                        _curDayPhaseProgress += progress;
                        DayNightRollBehaviour.Instance.AddProgress(progress);
                        yield return null;
                    }
                    // night time
                    StartNight();
                }
                
                // wait for all enemies to be destroyed
                while (_remainingEnemies > 0)
                {
                    yield return null;
                }

                // transition to day
                EndNight();
                _curDayPhaseProgress = 0;
                while (_curDayPhaseProgress < 1)
                {
                    var progress = Time.deltaTime / Constants.ChangeLightDurationInSeconds;
                    _curDayPhaseProgress += progress;
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
                _totalEnemies = totalSpawnAmounts + EnemyPool.Instance.EnemyCount;
                _remainingEnemies = totalSpawnAmounts + EnemyPool.Instance.EnemyCount;
            }
            var enemySpawns = new EnemySpawn[totalSpawnAmounts];
            // randomize spawn time and position for each enemy
            int i = 0;
            for (int enemyType = 0; enemyType < spawnAmounts.Length; enemyType++)
            {
                var spawnAmount = spawnAmounts[enemyType];
                for (int j = 0; j < spawnAmount; j++)
                {
                    var enemySpawn = new EnemySpawn
                    {
                        Enemy = (Enemy)enemyType,
                        SpawnTime = Random.Range(0, spawnDurationInSeconds)
                    };
                    enemySpawns[i++] = enemySpawn;
                }
            }

            yield return null;
            // sort by spawn time
            Array.Sort(enemySpawns, (a, b) => a.SpawnTime.CompareTo(b.SpawnTime));
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

        private void EndNight()
        {
            Debug.Log("Ending night");
            _currentDayPhase = DayPhase.NightEnd;
            GameData.Instance.day++;
            _currentCycle = DayNightData.GetCycle(GameData.Instance.day, GameData.Instance.thisDayEnemies, GameData.Instance.thisNightEnemies);
            GetNextCycleAsync(EnemySpawnData.Instance);
            _lightenTween = DOTween.To(
                () => globalLight.intensity,
                x => globalLight.intensity = x,
                Constants.DayLightIntensity,
                Constants.ChangeLightDurationInSeconds
            );
            MinimapBehaviour.Instance.LightenMap(Constants.ChangeLightDurationInSeconds);
            SoundManager.Instance.SwitchBackgroundMusic(true);
            NightTime = false;
            EventManager.Instance.TriggerEvent(EventManager.NightEnded, null);
        }

        private void EndDay(bool force=false)
        {
            Debug.Log("Ending day");
            _currentDayPhase = DayPhase.DayEnd;
            if (!force)
            {
                NightTime = false;
                DayTime = false;
                _darkenTween = DOTween.To(
                    () => globalLight.intensity, 
                    x => globalLight.intensity = x, 
                    Constants.NightLightIntensity, 
                    Constants.ChangeLightDurationInSeconds
                );
                MinimapBehaviour.Instance.DarkenMap(Constants.ChangeLightDurationInSeconds);
                SoundManager.Instance.SwitchBackgroundMusic(false);
            }
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
            if (DayTime)
                SoundManager.Instance.DayStarted();
            else
                SoundManager.Instance.NightStarted();
            _waveDeclarationTween = waveDeclarationText.DOFade(1, _textFadeDuration).OnComplete(() =>
            {
                waveDeclarationText.DOFade(0, _textFadeDuration).SetDelay(_textFadeDelay).OnComplete(() =>
                {
                    waveDeclarationText.gameObject.SetActive(false);
                });
            });
        }

        private void StartDay(bool force=false)
        {
            Debug.Log($"Starting day {GameData.Instance.day + 1}");
            _currentDayPhase = DayPhase.Day;
            if (GameData.Instance.day > 0)
                GeneralStoreManager.Instance.UpdateStock(_currentCycle.NewCrops, _currentCycle.NewMaterials);
            if (!force)
            {
                waveDeclarationText.text = $"- DAY {GameData.Instance.day + 1} -";
                _spawnEnemiesCR = StartCoroutine(SpawnEnemies());
                DayTime = true;
                NightTime = false;
                ShowWaveDeclaration();
                EventManager.Instance.TriggerEvent(EventManager.DayStarted, null);
            }
        }

        private void StartNight()
        {
            Debug.Log("Starting night");
            _currentDayPhase = DayPhase.Night;
            DayTime = false;
            NightTime = true;
            globalLight.intensity = Constants.NightLightIntensity;
            _spawnEnemiesCR = StartCoroutine(SpawnEnemies());
            waveDeclarationText.text = $"- NIGHT -";
            ShowWaveDeclaration();
            EventManager.Instance.TriggerEvent(EventManager.NightStarted, null);
        }

        private async void GetNextCycleAsync(EnemySpawnData spawnData)
        {
            var cycleNum = GameData.Instance.day + 1;
            // await Task.Run(() =>
            // {
            DayNightData.WarmupCycle(cycleNum, spawnData);
            // });
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
