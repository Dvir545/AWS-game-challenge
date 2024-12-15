using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using Utils;
using Utils.Data;

namespace World
{
    public class EnemyPool: Singleton<EnemyPool>
    {
        private static Dictionary<Enemy, ObjectPool<GameObject>> _enemyPools;
        public int EnemyCount { get; set; }

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
            // fade in
            var sr = enemy.transform.GetChild(0).GetComponent<SpriteRenderer>();
            sr.DOFade(1, 0.5f);
            EnemyCount++;
            return enemy;
        }

        public void ReleaseEnemy(GameObject enemy, Enemy type)
        {
            var sr = enemy.transform.GetChild(0).GetComponent<SpriteRenderer>();
            sr.color = new Color(1, 1, 1, 0);
            _enemyPools[type].Release(enemy);
            EnemyCount--;
        }
    }
}