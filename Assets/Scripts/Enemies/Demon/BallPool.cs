using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Utils;

namespace Enemies.Demon
{
    public class BallPool: Singleton<BallPool>
    {
        [SerializeField] private GameObject ballPrefab;
        private static ObjectPool<GameObject> _ballPool;
        private List<GameObject> _balls = new();

        private GameObject CreateBall()
        {
            GameObject ball = Instantiate(ballPrefab, transform);
            ball.transform.SetParent(transform);
            ball.SetActive(false);
            return ball;
        }
        private void Awake()
        {
            _ballPool = new ObjectPool<GameObject>(
                createFunc: () => CreateBall(),
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: Destroy,
                collectionCheck: false,
                defaultCapacity: 4,
                maxSize: 100
            );
        }

        public void ReleaseAll()
        {
            foreach (var ball in _balls)
            {
                _ballPool.Release(ball);
            }
            _balls.Clear();
        }

        public GameObject GetBall()
        {
            var ball = _ballPool.Get();
            _balls.Add(ball);
            return ball;
        }

        public void ReleaseBall(GameObject ball)
        {
            _ballPool.Release(ball);
            _balls.Remove(ball);
        }
    }
}