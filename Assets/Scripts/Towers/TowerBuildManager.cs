using System;
using System.Collections.Generic;
using Amazon.GameLift.Model;
using Player;
using UI.GameUI;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Utils.Data;

namespace Towers
{
    public class TowerBuildManager: MonoBehaviour
    {
        [SerializeField] private GameObject towersParent;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private ProgressBarBehavior progressBarBehavior;
        
        private TowerBuild[] _towerBuilds;
        private Collider2D[] _towerColliders;
        private Collider2D _playerCollider;
        [SerializeField] private MaterialManager materialManager;

        public void Init()
        {
            if (_towerBuilds == null)
            {
                _towerBuilds = new TowerBuild[towersParent.transform.childCount];
                _towerColliders = new Collider2D[_towerBuilds.Length];
                for (int i = 0; i < _towerBuilds.Length; i++)
                {
                    var child = towersParent.transform.GetChild(i);
                    _towerBuilds[i] = child.GetComponent<TowerBuild>();
                    _towerColliders[i] = child.GetComponent<Collider2D>();
                }
                _playerCollider = playerTransform.GetComponent<Collider2D>();
            }
            // load existing tower levels
            for (int i = 0; i < _towerBuilds.Length; i++)
            {
                _towerBuilds[i].Init(i);
                var floors = GameData.Instance.towers[i];
                foreach (var floor in floors)
                {
                    _towerBuilds[i].AddFloor(floor);
                }
            }
        }

        public bool IsBuilding { get; private  set; }
        
        public void StartBuilding()
        {
            IsBuilding = true;
        }
        
        public void StopBuilding()
        {
            IsBuilding = false;
        }

        private void Update()
        {
            if (IsBuilding)
            {
                Build();
            }
        }
        
        private void Build()
        {
            bool isStandingNearTower;
            int i;
            (isStandingNearTower, i) = IsStandingNearTower();
            if (isStandingNearTower && _towerBuilds[i].CanBuild())
            {
                if (_towerBuilds[i].IsInProgress())
                {
                    _towerBuilds[i].AddToProgress(playerData.GetProgressSpeedMultiplier * Time.deltaTime);
                    if (progressBarBehavior.IsWorking)
                        progressBarBehavior.UpdateProgress(_towerBuilds[i].GetProgress());
                    else
                        progressBarBehavior.StartWork(_towerBuilds[i].GetProgress());
                }
                else if (materialManager.HasMaterials())  // can build new tower
                {
                    var material = materialManager.GetBestAvailableMaterial();
                    _towerBuilds[i].StartBuild(material);
                    materialManager.RemoveMaterial(material);
                    progressBarBehavior.StartWork(_towerBuilds[i].GetProgress());
                }
            } else if (progressBarBehavior.IsWorking)
            {
                progressBarBehavior.StopWork();
            }
        }

        private Tuple<bool, int> IsStandingNearTower()
        {
            for (int i = 0; i < _towerColliders.Length; i++)
            {
                if (_towerColliders[i].bounds.Intersects(_playerCollider.bounds))
                {
                    return Tuple.Create(true, i);
                }
            }
            return Tuple.Create(false, -1);
        }

        public Transform[] GetBuiltTowerTransforms()
        {
            if (_towerBuilds == null)
                return Array.Empty<Transform>();
            var transforms = new List<Transform>();
            for (int i = 0; i < _towerBuilds.Length; i++)
            {
                if (_towerBuilds[i].IsBuilt)
                    transforms.Add(_towerBuilds[i].transform);
            }
            return transforms.ToArray();
        }

        public void ResetTowers()
        {
            for (int i = 0; i < _towerBuilds.Length; i++)
            {
                _towerBuilds[i].Reset();
            }
        }
    }
}