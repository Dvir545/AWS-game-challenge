using System.Collections;
using UnityEngine;

namespace Enemies.Orc
{
    public class OrcSoundManager: EnemySoundManager
    {
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioSource attackAudioSource;
        
        private OrcAttackManager _orcAttackManager;
        private Coroutine _attackCR;

        protected override void Awake()
        {
            _orcAttackManager = GetComponent<OrcAttackManager>();
            base.Awake();
        }
        
        private void Update()
        {
            if (EnemyHealthManager.IsDead && _attackCR != null)
            {
                StopCoroutine(_attackCR);
                _attackCR = null;
                return;
            }
            if (!_orcAttackManager.IsAttacking())
            {
                if (_attackCR != null)
                {
                    StopCoroutine(_attackCR);
                    _attackCR = null;
                    WalkingCR = StartCoroutine(PlayWalkingSoundCR());
                }
            }
            else
            {
                if (WalkingCR != null)
                {
                    StopCoroutine(WalkingCR);
                    WalkingCR = null;
                    _attackCR = StartCoroutine(PlayAttackSoundCR());
                }
            }
        }

        private IEnumerator PlayAttackSoundCR()
        {
            while (true)
            {
                if (_orcAttackManager.IsAttacking())
                {
                    attackAudioSource.clip = attackSound;
                    attackAudioSource.pitch = Random.Range(0.6f, 1.1f);
                    attackAudioSource.Play();
                    yield return new WaitForSeconds(0.6f);
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}