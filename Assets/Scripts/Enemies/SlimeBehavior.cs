using UnityEngine;
using System.Collections;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class SlimeBehavior : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private static readonly int AnimationMoving = Animator.StringToHash("moving");
        private static readonly int AnimationJumpSpeedMultiplier = Animator.StringToHash("jumpSpeedMultiplier");
        private static readonly int AnimationGotHit = Animator.StringToHash("hit");
        private static readonly int AnimationDeath = Animator.StringToHash("die");
        
        [SerializeField] private float jumpMinDistance;
        [SerializeField] private float jumpMaxDistance;
        [SerializeField] private float jumpSpeed;
        [SerializeField] private float jumpDestAngleVariance;  // how much the jump destination can vary from the player
        [SerializeField] private float jumpMinCooldown;
        [SerializeField] private float jumpMaxCooldown;
        private float _jumpCooldown;
        private bool _isJumping;
        [SerializeField] private GameObject players;
        private GameObject[] _players;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _isJumping = false;
            // set the players array to the children of the players object
            _players = new GameObject[players.transform.childCount];
            for (int i = 0; i < players.transform.childCount; i++)
            {
                _players[i] = players.transform.GetChild(i).gameObject;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isJumping)
            {
                // jump towards one of the players
                StartCoroutine(Jump());
            }
        }

        private IEnumerator Jump()
        {
            _isJumping = true;
            _jumpCooldown = Random.Range(jumpMinCooldown, jumpMaxCooldown);
            yield return new WaitForSeconds(_jumpCooldown);
            animator.SetBool(AnimationMoving, true);
            Vector2 towards = GetClosestPlayer().transform.position;
            // set facing to look at the player
            if (!Facing(transform.position, towards))
            {
                transform.localScale =
                    new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
            float jumpDistance = Random.Range(jumpMinDistance, jumpMaxDistance);
            float jumpDuration = jumpDistance / jumpSpeed;
            float jumpCharge = jumpDuration / 6f;
            jumpDuration -= jumpCharge;
            yield return new WaitForSeconds(jumpCharge);
            // add some randomness to the jump destination
            float angle = Random.Range(-jumpDestAngleVariance, jumpDestAngleVariance);
            towards = Quaternion.Euler(0, 0, angle) * towards;
            Vector2 destination = transform.position + (Vector3)(towards - (Vector2)transform.position).normalized * jumpDistance;
            animator.SetFloat(AnimationJumpSpeedMultiplier, 1/jumpDistance);
            transform.DOMove(destination, jumpDuration).SetEase(Ease.OutSine).OnComplete(() =>
            {
                _isJumping = false;
                animator.SetBool(AnimationMoving, false);
            });
        }

        private bool Facing(Vector2 pos, Vector2 towards)
        {
            float currentFacing = math.sign(transform.localScale.x);
            return (pos.x - towards.x) * currentFacing < 0;;
        }

        private GameObject GetClosestPlayer()
        {
            GameObject closestPlayer = null;
            float closestDistance = Mathf.Infinity;
            foreach (GameObject player in _players)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }
            return closestPlayer;
        }
        
    }
}
