using System;
using System.Collections;
using Player;
using Towers;
using UI.GameUI;
using Unity.VisualScripting;
using UnityEngine;
using Utils;
using Utils.Data;

namespace Enemies.Orc
{
    public class OrcAttackManager: MonoBehaviour
    {
        private EnemyHealthManager _enemyHealthManager;
        [SerializeField] private ProgressBarBehavior progressBarBehavior;
        private TowerBuild _curTarget;
        public bool _isAttacking;

        private void Awake()
        {
            _enemyHealthManager = GetComponent<EnemyHealthManager>();
        }

        private void Start()
        {
            progressBarBehavior.SetType(ProgressBarType.Evil);
        }

        private void Update()
        {
            if (_enemyHealthManager.IsDead)
            {
                SetAttacking(false);
                if (progressBarBehavior.IsWorking)
                    progressBarBehavior.StopWork();
                if (_curTarget != null)
                    _curTarget.SetUnderAttack(false);
                return;
            }
            if (_isAttacking)
            {
                if (!_curTarget.IsBuilt)
                {
                    SetAttacking(false);
                    progressBarBehavior.StopWork();
                }
                else
                {
                    _curTarget.DecWorstFloorHealth(Time.deltaTime);
                    var destroyProgress = _curTarget.GetDestroyProgress();
                    
                    if (!progressBarBehavior.IsWorking)
                        progressBarBehavior.StartWork(destroyProgress);
                    else
                        progressBarBehavior.UpdateProgress(destroyProgress);
                }
            } else if (progressBarBehavior.IsWorking)
                progressBarBehavior.StopWork();
        }

        public void StartAttacking(Transform tower)
        {
            _curTarget = tower.GetComponent<TowerBuild>();
            SetAttacking(true);
        }

        public void SetAttacking(bool isAttacking)
        {
            _isAttacking = isAttacking;
            if (_curTarget != null)
                _curTarget.SetUnderAttack(isAttacking);
        }

        public bool IsAttacking()
        {
            return _isAttacking;
        }
    }
}