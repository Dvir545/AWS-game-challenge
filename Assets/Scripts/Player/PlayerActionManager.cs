using System.Collections;
using Crops;
using Towers;
using UI.GameUI;
using UnityEngine;
using Utils;
using Utils.Data;

namespace Player
{
    public class PlayerActionManager : MonoBehaviour
    {
        [SerializeField] private FarmingManager farmingManager;
        [SerializeField] private PlayerAttackManager playerAttackManager;
        [SerializeField] private TowerBuildManager towerBuildManager;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private ProgressBarBehavior progressBarBehavior;
        private float _attackStaminaProgress = 0f;

        private bool _canAct = true;
        public bool isActing = false;
        private Coroutine _cooldownCR;
        private bool _onCooldown = false;
        public bool IsActing => _canAct && !_onCooldown && isActing;
        
        private void Awake()
        {
            EventManager.Instance.StartListening(EventManager.PlayerGotHit, GotHit);
            EventManager.Instance.StartListening(EventManager.PlayerDied, Die);
        }

        private void Die(object arg0)
        {
            if (_cooldownCR != null)
            {
                StopCoroutine(_cooldownCR);
                _cooldownCR = null;
                progressBarBehavior.SetType(ProgressBarType.Default);
            }

            DisableActions();
            _canAct = false;
        }

        public void StartActing()
        {
            isActing = true;
            if (progressBarBehavior.GetType() != ProgressBarType.Cooldown)
            {
                switch (playerData.GetCurTool())
                {
                    case HeldTool.Sword:
                        progressBarBehavior.SetType(ProgressBarType.Stamina);
                        break;
                    default:
                        progressBarBehavior.SetType(ProgressBarType.Default);
                        break;
                }
            }
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
            if (progressBarBehavior.IsWorking && !_onCooldown)
                progressBarBehavior.StopWork();
        }

        private void Update()
        {
            if (_onCooldown) return;
            if (IsActing)
            {
                HeldTool curTool = playerData.GetCurTool();
                if (curTool == HeldTool.Sword && !playerAttackManager.IsAttacking)
                {
                    playerAttackManager.StartAttack();
                    _attackStaminaProgress +=
                        Constants.BaseStaminaProgressIncPerSingleAttack / playerData.StaminaMultiplier; // start boost
                    if (_attackStaminaProgress >= 1f)
                    {
                        _attackStaminaProgress = 1f;
                        playerAttackManager.StopAttack();
                        _cooldownCR = StartCoroutine(ActCooldown());
                    }
                    else
                        progressBarBehavior.StartWork(_attackStaminaProgress);
                }
                else if (curTool != HeldTool.Sword && playerAttackManager.IsAttacking)
                {
                    progressBarBehavior.StopWork();
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

                if (curTool == HeldTool.Sword && playerAttackManager.IsAttacking)
                {
                    if (_attackStaminaProgress < 1f)
                    {
                        _attackStaminaProgress +=
                            Time.deltaTime / (Constants.BaseAttackDuration * playerData.StaminaMultiplier);
                        if (_attackStaminaProgress >= 1f)
                        {
                            _attackStaminaProgress = 1f;
                            playerAttackManager.StopAttack();
                            _cooldownCR = StartCoroutine(ActCooldown());
                        } else
                            progressBarBehavior.UpdateProgress(_attackStaminaProgress);
                    }
                }
            }
            if (!playerAttackManager.IsAttacking)
            {
                if (_attackStaminaProgress > 0f)
                {
                    _attackStaminaProgress -=
                        Time.deltaTime / (Constants.BaseAttackDuration * playerData.StaminaMultiplier);
                    if (_attackStaminaProgress <= 0f)
                    {
                        _attackStaminaProgress = 0f;
                    }
                }
                else if (progressBarBehavior.IsWorking && !farmingManager.IsFarming && !towerBuildManager.IsBuilding)
                {
                    progressBarBehavior.StopWork();
                }
            }
        }

        private IEnumerator ActCooldown()
        {
            _onCooldown = true;
            EventManager.Instance.TriggerEvent(EventManager.Cooldown, null);
            progressBarBehavior.SetType(ProgressBarType.Cooldown);  // todo add upgrade level
            progressBarBehavior.StartWork(1f);
            yield return null;
            while (_attackStaminaProgress > 0f)
            {
                _attackStaminaProgress -= Time.deltaTime / Constants.BaseActCooldownDuration;
                if (_attackStaminaProgress < 0) _attackStaminaProgress = 0;
                progressBarBehavior.UpdateProgress(_attackStaminaProgress);
                yield return null;
            }
            progressBarBehavior.SetType(playerData.GetCurTool() == HeldTool.Sword
                ? ProgressBarType.Stamina
                : ProgressBarType.Default);
            _onCooldown = false;
            progressBarBehavior.StopWork();
            yield return null;
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
            _attackStaminaProgress = 0f;
            _onCooldown = false;
        }
    }
}