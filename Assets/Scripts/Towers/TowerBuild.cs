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
        private GameObject _newFloor;
        private SpriteRenderer _towerBaseSpriteRenderer;
        private Transform _newFloorBody;
        private GameObject _topConstruction;
        private bool _isUnderAttack;  // can't build while tower is under attack
        
        // private const int MaxLevel = 3;  // The maximum level of the tower
        private const float YOffsetBetweenFloors = 0.9375f;  // The offset between the floors
        
        private int _towerIndex;
        private List<TowerData> _towerDatas = new();
        private int CurrentLevel => GameData.Instance.towers[_towerIndex].Count;
        private float CurBuildProgress
        {
            get => GameData.Instance.towers[_towerIndex][CurrentLevel - 1].progress;
            set => GameData.Instance.towers[_towerIndex][CurrentLevel - 1].progress = value;
        }

        private TowerFloorAnimationManager _newFloorAnimationManager;
        private Collider2D _newFloorAttackZone;
        private WarningSignBehaviour _warningSign;
        
        public bool IsBuilt { get; private set; }

        private void Awake()
        {
            _topConstruction = transform.GetChild(0).gameObject;
            _towerBaseSpriteRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();
        }

        public void Init(int index)
        {
            _towerIndex = index;
        }

        public bool CanBuild()
        {
            return !_isUnderAttack && (CurrentLevel < Constants.MaxTowerLevels || (CurrentLevel == Constants.MaxTowerLevels && CurBuildProgress is >= 0 and < 1));
        }
    
        public bool IsInProgress()
        {
            return CurrentLevel != 0 && CurBuildProgress is > 0 and < 1;
        }
    
        public float GetProgress()
        {
            return CurBuildProgress;
        }

        public void AddToProgress(float time)  // Returns true if the tower has been built
        {
            CurBuildProgress += time / TowersData.Instance.GetTowerData(_towerDatas[CurrentLevel-1].TowerMaterial).SecondsToBuild;
            if (CurBuildProgress >= 1f)
            {
                OnBuildCompleted();
            }
        }
        
        public void AddFloor(TowerLevelInfo towerLevelInfo)  // instantly
        {
            StartBuild((TowerMaterial)towerLevelInfo.material, towerLevelInfo.health, addToGameData:false);
            CurBuildProgress = towerLevelInfo.progress;
            if (CurBuildProgress >= 1f)
            {
                OnBuildCompleted();
            }
        }

        public void StartBuild(TowerMaterial material, float health = 0, bool addToGameData = true)
        {
            _topConstruction.SetActive(true);
            var towerData = TowersData.Instance.GetTowerData(material);
            _towerDatas.Add(towerData);
            if (addToGameData)
                GameData.Instance.towers[_towerIndex].Add(new TowerLevelInfo(
                    (int)material, 
                    0,
                    health == 0 ? towerData.SecondsToDestroy: health));
            var towerHeight = YOffsetBetweenFloors * (CurrentLevel-1);
            _newFloor = Instantiate(towerData.GetFloorPrefab(),
                transform.position + new Vector3(0, towerHeight, 0), Quaternion.identity);
            _newFloor.transform.SetParent(transform);
            _newFloorBody = _newFloor.transform.GetChild(0);
            _newFloorAttackZone = _newFloor.transform.GetChild(2).GetComponent<Collider2D>();
            _newFloorAttackZone.transform.position -= new Vector3(0, towerHeight, 0);
            _newFloorAnimationManager = _newFloor.GetComponent<TowerFloorAnimationManager>();
            _newFloorAnimationManager.Init(material, CurrentLevel-1, towerData.SecondsToAttack);
            _newFloorAttackZone.GetComponent<TowerFloorAttackZoneBehaviour>().Init(towerData.Range, towerData.Damage, towerData.SecondsToAttack);
            _floors.Add(_newFloor);
            
            AddToProgress(Time.deltaTime);
        }

        private void OnBuildCompleted()
        {
            _topConstruction.SetActive(false);
            _newFloorBody.gameObject.SetActive(true);
            _newFloorAttackZone.enabled = true;
            if (CurrentLevel == 1)  // first floor
            {
                _newFloorBody.GetComponent<Collider2D>().enabled = true;
                _newFloorBody.GetComponent<NavMeshObstacle>().enabled = true;
            }
            _newFloorAnimationManager.StartTower();
            _topConstruction = _floors[CurrentLevel-1].transform.GetChild(1).gameObject;
            _topConstruction.GetComponent<SpriteRenderer>().sortingOrder = CurrentLevel;
            EventManager.Instance.TriggerEvent(EventManager.TowerBuilt, this);
            IsBuilt = true;
        }
        
        public void SetUnderAttack(bool isUnderAttack)
        {
            switch (isUnderAttack)
            {
                case true when !_isUnderAttack:
                    EventManager.Instance.TriggerEvent(EventManager.TowerUnderAttack, (transform, false));
                    break;
                case false when _isUnderAttack:
                    EventManager.Instance.TriggerEvent(EventManager.TowerStoppedBeingUnderAttack, transform);
                    break;
            }

            _isUnderAttack = isUnderAttack;
        }

        public float GetDestroyProgress()
        {
            if (CurrentLevel == 0)
                return 0;
            var maxHealth = _towerDatas[CurrentLevel - 1].SecondsToDestroy;
            var curHealth = GameData.Instance.towers[_towerIndex][CurrentLevel - 1].health;
            return 1 - curHealth / maxHealth;
        }
        
        private int GetWorstFloor()
        {
            var worstFloor = 0;
            for (int i = 1; i < CurrentLevel; i++)
            {
                if (GameData.Instance.towers[_towerIndex][i].progress >= 1f
                    && _towerDatas[i].TowerMaterial < _towerDatas[worstFloor].TowerMaterial)
                    worstFloor = i;
            }
            return worstFloor;
        }

        private void DestroyWorstFloor()
        {
            var worstFloor = GetWorstFloor();
            // move down all the floors above the worst one
            for (int i = worstFloor + 1; i < CurrentLevel; i++)
            {
                _floors[i].transform.position -= new Vector3(0, YOffsetBetweenFloors, 0);
                _floors[i].GetComponent<SpriteRenderer>().sortingOrder = i;
            }
            Destroy(_floors[worstFloor]);
            _floors.RemoveAt(worstFloor);
            _towerDatas.RemoveAt(worstFloor);
            GameData.Instance.towers[_towerIndex].RemoveAt(worstFloor);
            if (CurrentLevel == 0)
            {
                _topConstruction = transform.GetChild(0).gameObject;
                IsBuilt = false;
            }
            else
            {
                _topConstruction = _floors[CurrentLevel - 1].transform.GetChild(1).gameObject;
            }
            if (CurBuildProgress > 0)
            {
                _newFloor.transform.position -= new Vector3(0, YOffsetBetweenFloors, 0);
                _topConstruction.GetComponent<SpriteRenderer>().sortingOrder = CurrentLevel;
                _newFloorAnimationManager.Init(_towerDatas[CurrentLevel-1].TowerMaterial, CurrentLevel-1, _towerDatas[CurrentLevel-1].SecondsToAttack);
                _topConstruction.SetActive(true);
            }
        }

        public void DecWorstFloorHealth(float health)
        {
            if (CurrentLevel == 0)
                return;
            GameData.Instance.towers[_towerIndex][GetWorstFloor()].health -= health;
            if (GameData.Instance.towers[_towerIndex][GetWorstFloor()].health <= 0)
            {
                DestroyWorstFloor();
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

        public void Reset()
        {
            _towerDatas.Clear();
            _floors.ForEach(Destroy);
            _floors.Clear();
            _topConstruction = transform.GetChild(0).gameObject;
            _topConstruction.SetActive(false);
            IsBuilt = false;
            _isUnderAttack = false;
        }
    }
}
