using UnityEngine;
using UnityEngine.Pool;
using Utils;

namespace Enemies.Goblin
{
    public class ArrowPool: Singleton<ArrowPool>
    {
        [SerializeField] private GameObject arrowPrefab;
        private static ObjectPool<GameObject> _arrowPool;
        
        private void Awake()
        {
            _arrowPool = new ObjectPool<GameObject>(
                createFunc: () => CreateArrow(),
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: Destroy,
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 100
            );
        }
        
        private GameObject CreateArrow()
        {
            GameObject arrow = Instantiate(arrowPrefab, transform);
            arrow.transform.SetParent(transform);
            arrow.SetActive(false);
            return arrow;
        }

        public void SpawnArrow(Vector2 position, Vector2 direction, FacingDirection facingDirection, Transform shooter)
        {
            GameObject arrow = _arrowPool.Get();
            arrow.transform.position = position;
            var arrowBehaviour = arrow.GetComponent<ArrowBehaviour>();
            arrowBehaviour.Initialize(direction.normalized, facingDirection, shooter);
        }

        public void ReleaseArrow(GameObject arrow)
        {
            _arrowPool.Release(arrow);
        }
    }
}