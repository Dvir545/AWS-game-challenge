using Towers;
using UI.GameUI;
using UnityEngine;
using Utils;

namespace Enemies.Orc
{
    public class OrcAttackManager: MonoBehaviour
    {
        private EnemyHealthManager _enemyHealthManager;
        private EnemyMovementManager _enemyMovementManager;
        [SerializeField] private ProgressBarBehavior progressBarBehavior;
        private TowerBuild _curTarget;
        private BoxCollider2D _hitTrigger;
        public bool _isAttacking;
        private float _colliderXOffset = 0f;
        private float _colliderXSize = 1.15f;
        private float _attackRightColliderXOffset = 0.3f;
        private float _attackRightColliderXSize = 1.75f;
        

        private void Awake()
        {
            _enemyHealthManager = GetComponent<EnemyHealthManager>();
            _enemyMovementManager = GetComponent<EnemyMovementManager>();
            foreach (var boxCollider2D in GetComponents<BoxCollider2D>())
            {
                if (boxCollider2D.isTrigger)
                {
                    _hitTrigger = boxCollider2D;
                    break;
                }
            }
            {
                
            }
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
            
            if (_isAttacking && (_enemyMovementManager.GetFacingDirection() == FacingDirection.Right || _enemyMovementManager.GetFacingDirection() == FacingDirection.Left))
            {
                _hitTrigger.offset = new Vector2(_attackRightColliderXOffset, _hitTrigger.offset.y);
                _hitTrigger.size = new Vector2(_attackRightColliderXSize, _hitTrigger.size.y);
            }
            else
            {
                _hitTrigger.offset = new Vector2(_colliderXOffset, _hitTrigger.offset.y);
                _hitTrigger.size = new Vector2(_colliderXSize, _hitTrigger.size.y);
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