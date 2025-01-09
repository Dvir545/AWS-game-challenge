using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils.Data
{
    public class EnemySpawnData: Singleton<EnemySpawnData>
    {
        [SerializeField] private GameObject defaultSpawns;
        [SerializeField] private Dictionary<Enemy, GameObject> specificSpawnPositions = new();  // override default spawns
        
        [Header("Enemy Power Points")]
        [SerializeField] private int slimePower = 2;
        [SerializeField] private int skeletonPower = 3;
        [SerializeField] private int goblinPower = 5;
        [SerializeField] private int chickenPower = 5;
        [SerializeField] private int orcPower = 8;
        [SerializeField] private int demonPower = 20;
        
        public Dictionary<Enemy, Vector2[]> EnemySpawnPositions { get; private set; }


        private void Awake()
        {
            var defaultPositions = GetPositions(defaultSpawns);
            EnemySpawnPositions = new Dictionary<Enemy, Vector2[]>
            {
                { Enemy.Slime , specificSpawnPositions.ContainsKey(Enemy.Slime) ? GetPositions(specificSpawnPositions[Enemy.Slime]) : defaultPositions},
                { Enemy.Skeleton , specificSpawnPositions.ContainsKey(Enemy.Skeleton) ? GetPositions(specificSpawnPositions[Enemy.Skeleton]) : defaultPositions},
                { Enemy.Goblin , specificSpawnPositions.ContainsKey(Enemy.Goblin) ? GetPositions(specificSpawnPositions[Enemy.Goblin]) : defaultPositions},
                { Enemy.Chicken , specificSpawnPositions.ContainsKey(Enemy.Chicken) ? GetPositions(specificSpawnPositions[Enemy.Chicken]) : defaultPositions},
                { Enemy.Orc , specificSpawnPositions.ContainsKey(Enemy.Orc) ? GetPositions(specificSpawnPositions[Enemy.Orc]) : defaultPositions},
                { Enemy.Demon , specificSpawnPositions.ContainsKey(Enemy.Demon) ? GetPositions(specificSpawnPositions[Enemy.Demon]) : defaultPositions}
            };
        }
        
        private static Vector2[] GetPositions(GameObject transformsParent)
        {
            var transforms = transformsParent.GetComponentsInChildren<Transform>();
            Vector2[] positions = new Vector2[transforms.Length];
            for (int i = 0; i < transforms.Length; i++)
            {
                positions[i] = transforms[i].position;
            }

            return positions;
        }

        private int GetPower(Enemy enemy)
        {
            return enemy switch
            {
                Enemy.Slime => slimePower,
                Enemy.Skeleton => skeletonPower,
                Enemy.Goblin => goblinPower,
                Enemy.Chicken => chickenPower,
                Enemy.Orc => orcPower,
                Enemy.Demon => demonPower,
                _ => 0
            };
        }
        
        public Tuple<Enemy, int> GetRandomEnemy(int points)
        {
            var enemies = new List<Enemy>();
            foreach (var enemy in (Enemy[]) Enum.GetValues(typeof(Enemy)))
            {
                if (enemy == Enemy.EvilBall) continue;
                if (GetPower(enemy) <= points)
                {
                    enemies.Add(enemy);
                }
            }

            var selectedEnemy = enemies[UnityEngine.Random.Range(0, enemies.Count)];
            return new Tuple<Enemy, int>(selectedEnemy, GetPower(selectedEnemy));
        }
    }
    
    public class EnemySpawns
    {
        private readonly Dictionary<Enemy, int> _spawnAmounts;
        private readonly Dictionary<Enemy, Vector2[]> _spawnPositions;

        public EnemySpawns(int slimes = 0, int skeletons = 0, int chickens = 0, int orcs = 0, int goblins = 0, int demons = 0)
        {
            _spawnAmounts = new Dictionary<Enemy, int>
            {
                { Enemy.Slime, slimes },
                { Enemy.Skeleton, skeletons },
                { Enemy.Chicken, chickens },
                { Enemy.Orc, orcs },
                { Enemy.Goblin, goblins },
                { Enemy.Demon, demons }
            };

            _spawnPositions = EnemySpawnData.Instance.EnemySpawnPositions;
        }
        
        public int[] SpawnAmounts => _spawnAmounts.Values.ToArray();
        public Vector2[][] SpawnPositions => _spawnPositions.Values.ToArray();

        public EnemySpawns(int[] amounts)
        {
            if (amounts.Length != 6)
            {
                Debug.LogError($"[EnemySpawns] Invalid amounts array length: {amounts.Length}");
                return;
            }

            _spawnAmounts = new Dictionary<Enemy, int>
            {
                { Enemy.Slime, amounts[0] },
                { Enemy.Skeleton, amounts[1] },
                { Enemy.Chicken, amounts[2] },
                { Enemy.Orc, amounts[3] },
                { Enemy.Goblin, amounts[4] },
                { Enemy.Demon, amounts[5] }
            };

            _spawnPositions = EnemySpawnData.Instance.EnemySpawnPositions;
        }

        public void AddEnemy(Enemy enemy, int amount = 1)
        {
            if (!_spawnAmounts.ContainsKey(enemy))
            {
                _spawnAmounts[enemy] = 0;
            }
            _spawnAmounts[enemy] += amount;
        }

        public int GetSpawnAmount(Enemy enemy)
        {
            return _spawnAmounts.TryGetValue(enemy, out int amount) ? amount : 0;
        }

        public Vector2[] GetSpawnPositions(Enemy enemy)
        {
            return _spawnPositions[enemy];
        }

        public int TotalSpawnsAmount()
        {
            return _spawnAmounts.Values.Sum();
        }
    }

}