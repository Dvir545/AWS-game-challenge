using System.Collections;
using UnityEngine;
using Utils;

namespace Player
{
    public class PlayerAction : MonoBehaviour
    {
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private Animator toolAnimator;
        [SerializeField] private float minAttackTime;
        private static readonly int AnimationActing = Animator.StringToHash("acting");
        private  static readonly int AnimationActType = Animator.StringToHash("act_type");
        private float _actTime = 0;
        
        public void StartActing()
        {
            playerAnimator.SetBool(AnimationActing, true);
            toolAnimator.SetBool(AnimationActing, true);
            int curTool = (int)GameData.GetCurTool();
            playerAnimator.SetInteger(AnimationActType, curTool);
            toolAnimator.SetInteger(AnimationActType, curTool);
            _actTime = Time.time;
        }
        
        public void StopActing()
        {
            _actTime = Time.time - _actTime;
            StartCoroutine(StopActingCoroutine());
        }

        private IEnumerator StopActingCoroutine()
        {
            if (_actTime < minAttackTime)
            {
                yield return new WaitForSeconds(minAttackTime - _actTime);
            }
            playerAnimator.SetBool(AnimationActing, false);
            toolAnimator.SetBool(AnimationActing, false);
            _actTime = 0;
        }
    }
}