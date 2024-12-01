using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Utils.Data;

namespace Towers
{
    public class TowerBuild : MonoBehaviour
    {
        [Serializable]
        private struct TowerSpriteRenderers
        {
            public SpriteRenderer center;
            public SpriteRenderer left;
            public SpriteRenderer right;
            
            public void SetSprites(TowerSprites sprites)
            {
                center.sprite = sprites.Center;
                left.sprite = sprites.Left;
                right.sprite = sprites.Right;
            }
        }

        [SerializeField] private GameObject floorPrefab;
        private List<GameObject> _floors = new();
        private Transform _newFloorBody;
        private GameObject _topConstruction;
        
        // private const int MaxLevel = 3;  // The maximum level of the tower
        private const float YOffsetBetweenFloors = 0.9375f;  // The offset between the floors
        
        private List<TowerData> _towerDatas = new();
        private int _currentLevel = 0;  // The current level of the tower
        private float _curBuildTime = 0;  // The current build time of the tower
        private TowerFloorAnimationManager _newFloorAnimationManager;
        private Collider2D _newFloorAttackZone;

        private void Awake()
        {
            _topConstruction = transform.GetChild(0).gameObject;
        }

        public bool CanBuild()
        {
            return true; //_currentLevel < MaxLevel;
        }
    
        public bool IsInProgress()
        {
            return _curBuildTime > 0;
        }
    
        public float GetProgress()
        {
            if (_curBuildTime == 0)
                return 0;
            return _curBuildTime / TowersData.Instance.GetTowerData(_towerDatas[_currentLevel].TowerMaterial).SecondsToBuild;
        }

        public void AddToProgress(float time)  // Returns true if the tower has been built
        {
            _curBuildTime += time;
            if (_curBuildTime >=_towerDatas[_currentLevel].SecondsToBuild)
            {
                OnBuildCompleted();
            }
        }

        public void StartBuild(TowerMaterial material)
        {
            _topConstruction.SetActive(true);
            var towerData = TowersData.Instance.GetTowerData(material);
            _towerDatas.Add(towerData);
            var towerHeight = YOffsetBetweenFloors * _currentLevel;
            var newFloor = Instantiate(towerData.GetFloorPrefab(),
                transform.position + new Vector3(0, towerHeight, 0), Quaternion.identity);
            newFloor.transform.SetParent(transform);
            _newFloorBody = newFloor.transform.GetChild(0);
            _newFloorAttackZone = newFloor.transform.GetChild(2).GetComponent<Collider2D>();
            _newFloorAttackZone.transform.position -= new Vector3(0, towerHeight, 0);
            _newFloorAnimationManager = newFloor.GetComponent<TowerFloorAnimationManager>();
            _newFloorAnimationManager.Init(material, _currentLevel, towerData.SecondsToAttack);
            _newFloorAttackZone.GetComponent<TowerFloorAttackZoneBehaviour>().Init(towerData.Range, towerData.Damage, towerData.SecondsToAttack);
            _floors.Add(newFloor);
            
            AddToProgress(Time.deltaTime);
        }

        private void OnBuildCompleted()
        {
            _topConstruction.SetActive(false);
            _newFloorBody.gameObject.SetActive(true);
            _newFloorAttackZone.enabled = true;
            if (_currentLevel == 0)
            {
                _newFloorBody.GetComponent<Collider2D>().enabled = true;
            }
            _newFloorAnimationManager.StartTower();
            _topConstruction = _floors[_currentLevel].transform.GetChild(1).gameObject;
            _topConstruction.GetComponent<SpriteRenderer>().sortingOrder = _currentLevel+1;
            _curBuildTime = 0;
            _currentLevel++;
            EventManager.Instance.TriggerEvent(EventManager.TowerBuilt, this);
        }
    }
}
