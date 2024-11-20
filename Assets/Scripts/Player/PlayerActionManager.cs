using System.Collections;
using UnityEngine;
using Utils;
using World;

namespace Player
{
    public class PlayerActionManager : MonoBehaviour
    {
        [SerializeField] private float minActTime;
        private float _actTime = 0;
        [SerializeField] private FarmingManager farmingManager;
        [SerializeField] private PlayerAttackManager playerAttackManager;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private ProgressBarBehavior progressBarBehavior;
        private bool _canAct = true;
        public bool _isActing = false;
        public bool IsActing => _canAct && _isActing;
        
        private void Awake()
        {
            EventManager.Instance.StartListening(EventManager.PlayerGotHit, GotHit);
            EventManager.Instance.StartListening(EventManager.PlayerDied, Die);
        }

        private void Die(object arg0)
        {
            DisableActions();
            _canAct = false;
        }

        public void StartActing()
        {
            _isActing = true;
            _actTime = Time.time;
        }
        
        public void StopActing()
        {
            _actTime = Time.time - _actTime;
            StartCoroutine(StopActingCoroutine());
        }

        public void SwitchActing()
        {
            if (!IsActing) return;
            StartActing();
        }

        private IEnumerator StopActingCoroutine()
        {
            if (_actTime < minActTime)
            {
                yield return new WaitForSeconds(minActTime - _actTime);
            }
            _isActing = false;
            DisableActions();
            _actTime = 0;
        }

        private void DisableActions()
        {
            playerAttackManager.StopAttack();
            farmingManager.StopFarming();
        }

        private void Update()
        {
            if (IsActing)
            {
                HeldTool curTool = playerData.GetCurTool();
                if (curTool == HeldTool.Sword && !playerAttackManager.IsAttacking)
                {
                    playerAttackManager.StartAttack();
                }
                else if (curTool != HeldTool.Sword && playerAttackManager.IsAttacking)
                {
                    playerAttackManager.StopAttack();
                }
                if (curTool == HeldTool.Hoe && !farmingManager.IsFarming)
                {
                    farmingManager.StartFarming();
                }
                else if (curTool != HeldTool.Hoe && farmingManager.IsFarming)
                {
                    farmingManager.StartFarming();
                }
            } else if (progressBarBehavior.IsWorking)
            {
                progressBarBehavior.StopWork();
            }
        }
        
        private void GotHit(object arg0)
        {
            if (arg0 is (float hitTime, Vector2 hitDirection))
            {
                StartCoroutine(GotHitCoroutine(hitTime));
            }
        }

        private IEnumerator GotHitCoroutine(float hitTime)
        {
            _canAct = false;
            DisableActions();
            yield return new WaitForSeconds(hitTime);
            _canAct = true;
        }
    }
}