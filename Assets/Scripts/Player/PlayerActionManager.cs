using System.Collections;
using Crops;
using Towers;
using UI.GameUI;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using World;

namespace Player
{
    public class PlayerActionManager : MonoBehaviour
    {
        [SerializeField] private FarmingManager farmingManager;
        [SerializeField] private PlayerAttackManager playerAttackManager;
        [SerializeField] private TowerBuildManager towerBuildManager;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private ProgressBarBehavior progressBarBehavior;
        private bool _canAct = true;
        public bool isActing = false;
        public bool IsActing => _canAct && isActing;
        
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
            isActing = true;
        }
        
        public void StopActing()
        {
            isActing = false;
            DisableActions();
        }

        public void SwitchActing()
        {
            if (!IsActing) return;
            StartActing();
        }

        private void DisableActions()
        {
            playerAttackManager.StopAttack();
            farmingManager.StopFarming();
            towerBuildManager.StopBuilding();
            if (progressBarBehavior.IsWorking)
                progressBarBehavior.StopWork();
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
                    farmingManager.StopFarming();
                }
                if (curTool == HeldTool.Hammer && !towerBuildManager.IsBuilding)
                {
                    towerBuildManager.StartBuilding();
                }
                else if (curTool != HeldTool.Hammer && towerBuildManager.IsBuilding)
                {
                    towerBuildManager.StopBuilding();
                }
            } else if (progressBarBehavior.IsWorking)
            {
                progressBarBehavior.StopWork();
            }
        }
        
        private void GotHit(object arg0)
        {
            if (arg0 is (float hitTime, Vector3 hitDirection, float pushForceMultiplier))
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

        public void Reset()
        {
            _canAct = true;
            isActing = false;
        }
    }
}