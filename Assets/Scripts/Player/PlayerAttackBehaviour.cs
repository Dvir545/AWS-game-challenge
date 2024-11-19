using UnityEngine;

namespace Player
{
    public class PlayerAttackBehaviour : StateMachineBehaviour
    {
        [SerializeField] private int[] attackFrames;
        private PlayerAttackManager _playerAttackManager;
    
        public void Init(PlayerAttackManager playerAttackManager)
        {
            _playerAttackManager = playerAttackManager;
        }
    
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            int currentFrame = (int)(stateInfo.normalizedTime * attackFrames.Length);
            if (System.Array.IndexOf(attackFrames, currentFrame) != -1 && !_playerAttackManager.IsAttacking)
            {
                _playerAttackManager.StartAttack();
            }
            else if (System.Array.IndexOf(attackFrames, currentFrame) == -1 && _playerAttackManager.IsAttacking)
            {
                _playerAttackManager.StopAttack();
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }
}
