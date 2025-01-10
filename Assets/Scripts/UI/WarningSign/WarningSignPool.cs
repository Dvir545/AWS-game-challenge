using System.Collections;
using System.Collections.Generic;
using Enemies;
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
        [SerializeField] private Sprite harvestSprite;
        private static ObjectPool<GameObject> _warningSignPool;
        
        private Dictionary<Transform, GameObject> _tgt2Sign = new();
        private List<GameObject> _importantWarnings = new();

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
                maxSize: 200
            );

        }

        public void ReleaseAll()
        {
            foreach (var sign in _tgt2Sign.Values)
            {
                _warningSignPool.Release(sign);
            }
            _tgt2Sign.Clear();
            _importantWarnings = new List<GameObject>();
        }
        
        // Coroutine to wait for end of frame and then set as last sibling
        private IEnumerator SetAsLastSiblingNextFrame(Transform childTransform)
        {
            // Wait until the end of the frame
            yield return new WaitForEndOfFrame();

            // Set as last sibling
            childTransform.SetAsLastSibling();
        }

        public GameObject GetWarningSign(Transform parent, Transform target, WarningSignType type)
        {
            var warningSign = _warningSignPool.Get();
            warningSign.transform.SetParent(parent);
            var sprite = type switch
            {
                WarningSignType.Warning => destroySignSprite,
                WarningSignType.Enemy => enemySignSprite,
                WarningSignType.Harvest => harvestSprite,
                _ => null
            };
            warningSign.transform.GetChild(4).GetComponent<Image>().sprite = sprite;
            warningSign.GetComponent<WarningSignBehaviour>().Init(target, type == WarningSignType.Warning);
            if (_tgt2Sign.ContainsKey(target))
            {
                _warningSignPool.Release(_tgt2Sign[target]);
                _tgt2Sign.Remove(target);
            }
            _tgt2Sign.Add(target, warningSign);
            
            // move all important warnings to the top
            if (type == WarningSignType.Warning)
                _importantWarnings.Add(warningSign);
            foreach (var importantWarning in _importantWarnings)
                StartCoroutine(SetAsLastSiblingNextFrame(importantWarning.transform));
            
            return warningSign;
        }

        public void ReleaseWarningSign(Transform target)
        {
            if (!_tgt2Sign.ContainsKey(target)) return;
            _warningSignPool.Release(_tgt2Sign[target]);
            var obj = _tgt2Sign[target];
            if (_importantWarnings.Contains(obj))
                _importantWarnings.Remove(obj);
            _tgt2Sign.Remove(target);
        }
    }
}