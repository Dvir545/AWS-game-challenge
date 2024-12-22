using System;
using Crops;
using Player;
using UI.GameUI;
using UnityEngine;
using Utils;
using World;

namespace Enemies.Chicken
{
    public class ChickenEatingManager: MonoBehaviour
    {
        private FarmingManager _farmingManager;
        private ChickenMovementManager _chickenMovementManager;
        private EnemyHealthManager _chickenHealthManager;
        [SerializeField] private ProgressBarBehavior progressBarBehavior;
        public bool IsEating { get; private set; }
        private bool _isStandingOnFarmTile;
        private Vector2Int _farmCellPos;

        private void Awake()
        {
            _chickenMovementManager = GetComponent<ChickenMovementManager>();
            _chickenHealthManager = GetComponent<EnemyHealthManager>();
            _farmingManager = FindObjectOfType<FarmingManager>();
        }
        
        private void Start()
        {
            progressBarBehavior.SetType(ProgressBarType.Evil);
        }

        private void Update()
        {
            if (_chickenHealthManager.IsDead)
            {
                IsEating = false;
                if (progressBarBehavior.IsWorking)
                    progressBarBehavior.StopWork();
                return;
            }
            if (!_chickenMovementManager.IsMoving)
            {
                (_isStandingOnFarmTile, _farmCellPos) = _farmingManager.IsStandingOnFarmTile(transform);
                if (_isStandingOnFarmTile)
                {
                    bool isDestroyed = _farmingManager.IncDestroyProgress(_farmCellPos);
                    IsEating = true;
                    if (!isDestroyed)
                    {
                        var destroyProgress = _farmingManager.Farms[_farmCellPos].GetDestroyProgress();
                        if (!progressBarBehavior.IsWorking)
                            progressBarBehavior.StartWork(destroyProgress);
                        else
                            progressBarBehavior.UpdateProgress(destroyProgress);
                    }
                    else
                    {
                        progressBarBehavior.StopWork();
                    }
                }
                else
                {
                    IsEating = false;
                }
            } 
            else
            {
                IsEating = false;
            }
            if (!IsEating && progressBarBehavior.IsWorking)
            {
                progressBarBehavior.StopWork();
            }
        }
    }
}