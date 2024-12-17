using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Utils;

namespace UI.WarningSign
{
    public class WarningSignPool: Singleton<WarningSignPool>
    {
        [SerializeField] private GameObject warningSignPrefab;
        [SerializeField] private Sprite enemySignSprite;
        [SerializeField] private Sprite destroySignSprite;
        private static ObjectPool<GameObject> _warningSignPool;
        
        private Dictionary<Transform, GameObject> _tgt2Sign = new();

        private GameObject CreateWarningSign()
        {
            GameObject warningSign = Instantiate(warningSignPrefab, transform);
            warningSign.transform.SetParent(transform);
            warningSign.SetActive(false);
            return warningSign;
        }
        private void Awake()
        {
            _warningSignPool = new ObjectPool<GameObject>(
                createFunc: CreateWarningSign,
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: Destroy,
                collectionCheck: false,
                defaultCapacity: 4,
                maxSize: 100
            );
        }
        
        public GameObject GetWarningSign(Transform parent, Transform target, bool enemy=false)
        {
            var warningSign = _warningSignPool.Get();
            warningSign.transform.SetParent(parent);
            warningSign.GetComponent<WarningSignBehaviour>().Init(target);
            var sprite = enemy ? enemySignSprite : destroySignSprite;
            warningSign.transform.GetChild(4).GetComponent<Image>().sprite = sprite;
            _tgt2Sign.Add(target, warningSign);
            return warningSign;
        }

        public void ReleaseWarningSign(Transform target)
        {
            if (!_tgt2Sign.ContainsKey(target)) return;
            _warningSignPool.Release(_tgt2Sign[target]);
            _tgt2Sign.Remove(target);
        }
    }
}