using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UI.WarningSign;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Utils;
using Utils.Data;

namespace Towers
{
    public class TowerBuild : MonoBehaviour, IWarnable
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
        private List<float> _floorsHealth = new();
        private GameObject _newFloor;
        private SpriteRenderer _towerBaseSpriteRenderer;
        private Transform _newFloorBody;
        private GameObject _topConstruction;
        private bool _isUnderAttack;  // can't build while tower is under attack
        
        // private const int MaxLevel = 3;  // The maximum level of the tower
        private const float YOffsetBetweenFloors = 0.9375f;  // The offset between the floors
        
        private List<TowerData> _towerDatas = new();
        private int _currentLevel = 0;  // The current level of the tower
        private float _curBuildTime = 0;  // The current build time of the tower
        private TowerFloorAnimationManager _newFloorAnimationManager;
        private Collider2D _newFloorAttackZone;
        private WarningSignBehaviour _warningSign;
        public bool IsBuilt { get; private set; }

        private void Awake()
        {
            _topConstruction = transform.GetChild(0).gameObject;
            _towerBaseSpriteRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();
        }

        public bool CanBuild()
        {
            return !_isUnderAttack && _currentLevel < Constants.MaxTowerLevels;
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
            _floorsHealth.Add(towerData.SecondsToDestroy);
            var towerHeight = YOffsetBetweenFloors * _currentLevel;
            _newFloor = Instantiate(towerData.GetFloorPrefab(),
                transform.position + new Vector3(0, towerHeight, 0), Quaternion.identity);
            _newFloor.transform.SetParent(transform);
            _newFloorBody = _newFloor.transform.GetChild(0);
            _newFloorAttackZone = _newFloor.transform.GetChild(2).GetComponent<Collider2D>();
            _newFloorAttackZone.transform.position -= new Vector3(0, towerHeight, 0);
            _newFloorAnimationManager = _newFloor.GetComponent<TowerFloorAnimationManager>();
            _newFloorAnimationManager.Init(material, _currentLevel, towerData.SecondsToAttack);
            _newFloorAttackZone.GetComponent<TowerFloorAttackZoneBehaviour>().Init(towerData.Range, towerData.Damage, towerData.SecondsToAttack);
            _floors.Add(_newFloor);
            
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
                _newFloorBody.GetComponent<NavMeshObstacle>().enabled = true;
            }
            _newFloorAnimationManager.StartTower();
            _topConstruction = _floors[_currentLevel].transform.GetChild(1).gameObject;
            _topConstruction.GetComponent<SpriteRenderer>().sortingOrder = _currentLevel+1;
            _curBuildTime = 0;
            _currentLevel++;
            EventManager.Instance.TriggerEvent(EventManager.TowerBuilt, this);
            IsBuilt = true;
        }
        
        public void SetUnderAttack(bool isUnderAttack)
        {
            switch (isUnderAttack)
            {
                case true when !_isUnderAttack:
                    EventManager.Instance.TriggerEvent(EventManager.TowerUnderAttack, transform);
                    break;
                case false when _isUnderAttack:
                    EventManager.Instance.TriggerEvent(EventManager.TowerStoppedBeingUnderAttack, transform);
                    break;
            }

            _isUnderAttack = isUnderAttack;
        }

        public float GetDestroyProgress()
        {
            if (_currentLevel == 0)
                return 0;
            var maxHealth = _towerDatas[_currentLevel - 1].SecondsToDestroy;
            var curHealth = _floorsHealth[_currentLevel - 1];
            return 1 - curHealth / maxHealth;
        }

        private void DestroyTopFloor()
        {
            Destroy(_floors[_currentLevel - 1]);
            _floors.RemoveAt(_currentLevel - 1);
            _floorsHealth.RemoveAt(_currentLevel - 1);
            _towerDatas.RemoveAt(_currentLevel - 1);
            _currentLevel--;
            if (_currentLevel == 0)
            {
                _topConstruction = transform.GetChild(0).gameObject;
                IsBuilt = false;
            }
            else
            {
                _topConstruction = _floors[_currentLevel - 1].transform.GetChild(1).gameObject;
            }
            if (_curBuildTime > 0)
            {
                _newFloor.transform.position -= new Vector3(0, YOffsetBetweenFloors, 0);
                _topConstruction.GetComponent<SpriteRenderer>().sortingOrder = _currentLevel;
                _newFloorAnimationManager.Init(_towerDatas[_currentLevel].TowerMaterial, _currentLevel, _towerDatas[_currentLevel].SecondsToAttack);
                _topConstruction.SetActive(true);
            }
        }

        public void DecTopFloorHealth(float health)
        {
            if (_currentLevel == 0)
                return;
            var newHealth = _floorsHealth[_currentLevel - 1] - health;
            if (newHealth <= 0)
            {
                DestroyTopFloor();
            }
            else
            {
                _floorsHealth[_currentLevel - 1] = newHealth;
            }
        }
        
        public void SetWarningSign(WarningSignBehaviour warningSign)
        {
            _warningSign = warningSign;
        }

        public bool IsVisible()
        {
            if (_towerBaseSpriteRenderer == null) return false;
            return _towerBaseSpriteRenderer.isVisible;
        }

        public void ShowWarningSign()
        {
            if (_warningSign == null)
                return;
            _warningSign.SetVisibility(false);
        }
        
        public void HideWarningSign()
        {
            if (_warningSign == null)
                return;
            _warningSign.SetVisibility(true);
        }
    }
}
