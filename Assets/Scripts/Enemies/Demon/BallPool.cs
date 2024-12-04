using UnityEngine;
using UnityEngine.Pool;
using Utils;

namespace Enemies.Demon
{
    public class BallPool: Singleton<BallPool>
    {
        [SerializeField] private GameObject ballPrefab;
        private static ObjectPool<GameObject> _ballPool;

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
        
        public GameObject GetBall()
        {
            return _ballPool.Get();
        }

        public void ReleaseBall(GameObject ball)
        {
            _ballPool.Release(ball);
        }
    }
}