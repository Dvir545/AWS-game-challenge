using System;
using System.Collections;
using Player;
using Towers;
using Unity.VisualScripting;
using UnityEngine;
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

        private void Update()
        {
            if (_enemyHealthManager.IsDead)
            {
                SetAttacking(false);
                if (progressBarBehavior.IsWorking)
                    progressBarBehavior.StopWork();
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
                    _curTarget.DecTopFloorHealth(Time.deltaTime);
                    var destroyProgress = _curTarget.GetDestroyProgress();
                    if (!progressBarBehavior.IsWorking)
                        progressBarBehavior.StartWork(destroyProgress);
                    else
                        progressBarBehavior.UpdateProgress(destroyProgress);
                }
            }
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