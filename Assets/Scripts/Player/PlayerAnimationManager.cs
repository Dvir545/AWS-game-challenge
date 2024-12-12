using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Player
{
    public class PlayerAnimationManager: MonoBehaviour
    {
        [SerializeField] private Animator[] animators;
        [SerializeField] private SpriteRenderer[] spriteRenderers;  // for flipping
        private static readonly int AnimationMoving = Animator.StringToHash("moving");
        private static readonly int AnimationFacing = Animator.StringToHash("facing");
        private static readonly int AnimationActing = Animator.StringToHash("acting");
        private static readonly int AnimationActType = Animator.StringToHash("act_type");
        private static readonly int AnimationGotHit = Animator.StringToHash("hit");
        private static readonly int AnimationDeath = Animator.StringToHash("die");

        [SerializeField] private PlayerMovement playerMovement;
        [FormerlySerializedAs("playerAction")] [SerializeField] private PlayerActionManager playerActionManager;
        [SerializeField] private PlayerData playerData;
        private Coroutine _actStopCR;
        private float _actTime;
        [SerializeField] private float minActTime;

        bool IsMoving => playerMovement.IsMoving;
        int Facing => (int)playerMovement.GetFacingDirection();
        bool IsActing => playerActionManager.IsActing;
        int ActType => (int)playerData.GetCurTool();

        private void Awake()
        {
            EventManager.Instance.StartListening(EventManager.PlayerGotHit, GotHit);
            EventManager.Instance.StartListening(EventManager.PlayerDied, Die);
            EventManager.Instance.StartListening(EventManager.ActiveToolChanged, ChangeToolColor);
        }

        private void Start()
        {
            ChangeToolColor(null);
        }

        private void Die(object arg0)
        {
            foreach (var animator in animators)
            {
                animator.SetTrigger(AnimationDeath);
            }
        }

        private void Update()
        {
            foreach (var animator in animators)
            {
                UpdateAnimator(animator);
            }
        }

        private void UpdateAnimator(Animator animator)
        {
            animator.SetBool(AnimationMoving, IsMoving);
            animator.SetInteger(AnimationFacing, Facing);
            animator.SetInteger(AnimationActType, ActType);
            if (Facing == (int)FacingDirection.Right)
            {
                foreach (var spriteRenderer in spriteRenderers)
                {
                    spriteRenderer.flipX = true;
                }
            } else if (Facing == (int)FacingDirection.Left)
            {
                foreach (var spriteRenderer in spriteRenderers)
                {
                    spriteRenderer.flipX = false;
                }
            }

            if (!IsActing && animator.GetBool(AnimationActing) && _actStopCR == null)  // change to not acting
            {
                _actStopCR = StartCoroutine(StopActingCR());
            }

            if (IsActing)
            {
                if (_actStopCR != null) // change to acting while coroutine to stop it is working
                {
                    StopCoroutine(_actStopCR);
                    _actStopCR = null;
                }

                if (!animator.GetBool(AnimationActing))
                {
                    animator.SetBool(AnimationActing, true);
                    _actTime = 0;
                }
                _actTime += Time.deltaTime;
            }
        }

        private IEnumerator StopActingCR()
        {
            if (_actTime < minActTime)
                yield return new WaitForSeconds(minActTime - _actTime);
            foreach (var animator in animators)
            {
                animator.SetBool(AnimationActing, false);
            }
        }

        private void GotHit(object argo)
        {
            foreach (var animator in animators)
            {
                animator.SetTrigger(AnimationGotHit);
            }
        }

        public void ChangeToolColor(object arg0)
        {
            animators[1].GetComponent<SpriteRenderer>().color = playerData.GetCurToolColor();
        }
    }
}