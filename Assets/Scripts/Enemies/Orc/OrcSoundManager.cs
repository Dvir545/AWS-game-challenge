using System.Collections;
using UnityEngine;
using Utils.Data;

namespace Enemies.Orc
{
    public class OrcSoundManager: EnemySoundManager
    {
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioSource attackAudioSource;
        
        private OrcAttackManager _orcAttackManager;
        private IEnumerator _attackCR;

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
                    WalkingCR = PlayWalkingSoundCR();
                    StartCoroutine(WalkingCR);
                }
            }
            else
            {
                if (WalkingCR != null)
                {
                    StopCoroutine(WalkingCR);
                    WalkingCR = null;
                    _attackCR = PlayAttackSoundCR();
                    StartCoroutine(_attackCR);
                }
            }
        }

        private IEnumerator PlayAttackSoundCR()
        {
            while (true)
            {
                if (_orcAttackManager.IsAttacking())
                {
                    attackAudioSource.volume = GameStatistics.Instance.sfxVolume;
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