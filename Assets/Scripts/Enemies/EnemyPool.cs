using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using Utils;
using Utils.Data;

namespace Enemies
{
    public class EnemyPool: Singleton<EnemyPool>
    {
        private static Dictionary<Enemy, ObjectPool<GameObject>> _enemyPools;
        public int EnemyCount { get; set; }
        private Dictionary<GameObject, Enemy> _enemies = new Dictionary<GameObject, Enemy>();

        private GameObject CreateEnemy(Enemy type)
        {
            GameObject enemy = Instantiate(PrefabData.Instance.GetEnemyPrefab(type), transform);
            enemy.transform.SetParent(transform);
            enemy.SetActive(false);
            return enemy;
        }
        
        private void Awake()
        {
            _enemyPools  = new Dictionary<Enemy, ObjectPool<GameObject>>();
            foreach (Enemy enemy in Enum.GetValues(typeof(Enemy)))
            {
                _enemyPools.Add(enemy, new ObjectPool<GameObject>(
                    createFunc: () => CreateEnemy(enemy),
                    actionOnGet: obj => obj.SetActive(true),
                    actionOnRelease: obj => obj.SetActive(false),
                    actionOnDestroy: Destroy,
                    collectionCheck: false,
                    defaultCapacity: 15,
                    maxSize: 1000
                ));
            }
        }
        
        public GameObject GetEnemy(Enemy type, Vector2 spawnPosition)
        {
            GameObject enemy = _enemyPools[type].Get();
            enemy.transform.position = spawnPosition;
            enemy.GetComponent<EnemyHealthManager>().Reset();
            enemy.GetComponent<EnemyMovementManager>().Reset();
            // fade in
            var body = enemy.transform.GetChild(0);
            var sr = body.GetComponent<SpriteRenderer>();
            sr.color = new Color(1, 1, 1, 0);
            sr.DOFade(1, 0.5f);
            EventManager.Instance.TriggerEvent(EventManager.EnemySpawned, (body.transform, true));
            EnemyCount++;
            _enemies.Add(enemy, type);
            return enemy;
        }

        public void ReleaseEnemy(GameObject enemy, Enemy type)
        {
            var sr = enemy.transform.GetChild(0).GetComponent<SpriteRenderer>();
            sr.color = new Color(1, 1, 1, 0);
            _enemyPools[type].Release(enemy);
            _enemies.Remove(enemy);
            EnemyCount--;
        }

        public void ReleaseAll()
        {
            var enemies = new List<GameObject>(_enemies.Keys);
            foreach (var enemy in enemies)
            {
                ReleaseEnemy(enemy, _enemies[enemy]);
            }
        }
    }
}