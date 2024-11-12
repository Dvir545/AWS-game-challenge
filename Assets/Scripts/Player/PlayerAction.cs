using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;
using World;

namespace Player
{
    public class PlayerAction : MonoBehaviour
    {
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private Animator toolAnimator;
        [SerializeField] private float minActTime;
        private static readonly int AnimationActing = Animator.StringToHash("acting");
        private  static readonly int AnimationActType = Animator.StringToHash("act_type");
        private float _actTime = 0;
        private bool _acting = false;
        [SerializeField] private FarmingManager farmingManager;
        private PlayerData _playerData;
        [SerializeField] private ProgressBarBehavior progressBarBehavior;
        
        private void Start()
        {
            _playerData = GetComponent<PlayerData>();
        }
        
        public void StartActing()
        {
            _acting = true;
            playerAnimator.SetBool(AnimationActing, true);
            toolAnimator.SetBool(AnimationActing, true);
            HeldTool curTool = _playerData.GetCurTool();
            int curToolNum = (int)curTool;
            playerAnimator.SetInteger(AnimationActType, curToolNum);
            toolAnimator.SetInteger(AnimationActType, curToolNum);
            _actTime = Time.time;
        }
        
        public void StopActing()
        {
            _actTime = Time.time - _actTime;
            StartCoroutine(StopActingCoroutine());
        }

        public void SwitchActing()
        {
            if (!_acting) return;
            StartActing();
        }

        private IEnumerator StopActingCoroutine()
        {
            if (_actTime < minActTime)
            {
                yield return new WaitForSeconds(minActTime - _actTime);
            }
            playerAnimator.SetBool(AnimationActing, false);
            toolAnimator.SetBool(AnimationActing, false);
            _acting = false;
            _actTime = 0;
        }

        private void Update()
        {
            if (_acting)
            {
                HeldTool curTool = _playerData.GetCurTool();
                switch (curTool)
                {
                    case HeldTool.Hoe:
                        farmingManager.Farm();
                        break;
                }
            } else if (progressBarBehavior.IsWorking)
            {
                progressBarBehavior.StopWork();
            }
        }
    }
}