using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utils;
using Utils.Data;

namespace World
{
    public class DayNightManager : Singleton<DayNightManager>
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private TextMeshProUGUI waveDeclarationText;
        
        private DayNightData.Cycle _currentCycle;
        private int _currentCycleIndex = 0;
        private DayNightData.Cycle _nextCycle;
        private Coroutine _dayNightCycleCoroutine;
        private Coroutine _spawnEnemiesCR;
        
        private float _textFadeDuration = 3f;
        private float _textFadeDelay = 5f;

        void Start()
        {
            _currentCycle = DayNightData.GetCycle(_currentCycleIndex, EnemySpawnData.Instance);
        }

        public void StartDayNightCycle()
        {
            _dayNightCycleCoroutine = StartCoroutine(DayNightCycle());
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

        private IEnumerator SpawnEnemies(bool day)
        {
            var enemySpawnData = day? _currentCycle.DayWave.EnemySpawns : _currentCycle.NightWave.EnemySpawns;
            var spawnDurationInSeconds = day? _currentCycle.DayWave.SpawnDurationInSeconds : _currentCycle.NightWave.SpawnDurationInSeconds;
            var spawnPositionOptions = enemySpawnData.SpawnPositions;
            var spawnAmounts = enemySpawnData.SpawnAmounts;
            var totalSpawnAmounts = enemySpawnData.TotalSpawnsAmount();
            var enemySpawns = new EnemySpawn[totalSpawnAmounts];
            // randomize spawn time and position for each enemy
            foreach (var enemyType in spawnAmounts.Keys)
            {
                var spawnAmount = spawnAmounts[enemyType];
                for (int i = 0; i < spawnAmount; i++)
                {
                    var enemySpawn = new EnemySpawn
                    {
                        Enemy = enemyType,
                        SpawnTime = Random.Range(0, spawnDurationInSeconds)
                    };
                    enemySpawns[i] = enemySpawn;
                }
            }
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
                    SpawnEnemy(enemySpawn, spawnPositionOptions[enemySpawn.Enemy]);
                    currentEnemyIndex++;
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator DayNightCycle()
        {
            while (true)
            {
                GetCycleAsync(EnemySpawnData.Instance);
                // daytime
                StartDay();
                yield return new WaitForSeconds(_currentCycle.DayWave.DurationInSeconds - Constants.Day2NightTransitionInSeconds);
                
                // transition to night
                EndDay();
                yield return  new WaitForSeconds(Constants.Day2NightTransitionInSeconds);
                
                // night time
                StartNight();
                yield return new WaitForSeconds(_currentCycle.NightWave.SpawnDurationInSeconds);
                
                // wait for all enemies to be destroyed
                while (EnemyPool.Instance.EnemyCount > 0)
                {
                    yield return null;
                }
                
                // transition to day
                _currentCycle = _nextCycle;
                _currentCycleIndex++;
                EndNight();
            }
        }

        private void EndNight()
        {
            DOTween.To(
                () => globalLight.intensity,
                x => globalLight.intensity = x,
                Constants.DayLightIntensity,
                Constants.Day2NightTransitionInSeconds
            );
        }

        private void EndDay()
        {
            DOTween.To(
                () => globalLight.intensity, 
                x => globalLight.intensity = x, 
                Constants.NightLightIntensity, 
                Constants.Day2NightTransitionInSeconds
            );
        }
        
        private void ShowWaveDeclaration()
        {
            waveDeclarationText.gameObject.SetActive(true);
            waveDeclarationText.DOFade(1, _textFadeDuration).OnComplete(() =>
            {
                waveDeclarationText.DOFade(0, _textFadeDuration).SetDelay(_textFadeDelay);
                waveDeclarationText.gameObject.SetActive(false);
            });
        }

        private void StartDay()
        {
            waveDeclarationText.text = $"- DAY {_currentCycleIndex + 1} -";
            _spawnEnemiesCR = StartCoroutine(SpawnEnemies(day: true));
            ShowWaveDeclaration();
        }

        private void StartNight()
        {
            _spawnEnemiesCR = StartCoroutine(SpawnEnemies(day: false));
            waveDeclarationText.text = $"- NIGHT -";
            ShowWaveDeclaration();
        }

        private async void GetCycleAsync(EnemySpawnData spawnData)
        {
            await Task.Run(() =>
            {
                _nextCycle = DayNightData.GetCycle(_currentCycleIndex, spawnData);
            });
        }
    }
}
