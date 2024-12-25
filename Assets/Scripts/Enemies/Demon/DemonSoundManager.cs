using System;
using System.Collections;
using System.Diagnostics;
using DG.Tweening;
using UnityEngine;
using Utils;
using Utils.Data;
using World;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Enemies.Demon
{
    public class DemonSoundManager: EnemySoundManager
    {
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioClip killedYouSound;

        private bool _hitPlayer;
        [SerializeField] private AudioSource walkingAudioSource;
        private Coroutine _attackSoundCR;
        private bool _attackSoundPlaying;
        
        protected override void Awake()
        {
            base.Awake();
            EventManager.Instance.StartListening(EventManager.PlayerDied, PlayKilledYouSound);
        }

        private void PlayKilledYouSound(object arg0)
        {
            StartCoroutine(PlayKilledYouSoundCR());
        }

        private IEnumerator PlayKilledYouSoundCR()
        {
            if (!_hitPlayer) yield return null;
            while (_attackSoundPlaying)
            {
                    yield return null;
            }
            if (_attackSoundCR != null)
            {
                StopCoroutine(_attackSoundCR);
            }
            AudioSource.PlayOneShot(killedYouSound);
        }

        public void PlayAttackSound()
        {
            if (_attackSoundCR != null)
            {
                StopCoroutine(_attackSoundCR);
            }
            _attackSoundCR = StartCoroutine(PlayAttackSoundCR());
        }

        private IEnumerator PlayAttackSoundCR()
        {
            _attackSoundPlaying = true;
            _hitPlayer = true;
            SoundManager.Instance.PlaySFX(AudioSource, attackSound, true, 0.2f);
            yield return new WaitForSeconds(attackSound.length/2);
            _attackSoundPlaying = false;
            if (!EnemyHealthManager.IsDead && Random.Range(0, 5) == 0)
            {
                AudioSource.pitch = 1;
                AudioSource.PlayOneShot(killedYouSound);
            }
        }

        protected override IEnumerator PlayWalkingSoundCR()
        {
            walkingAudioSource.volume = GameStatistics.Instance.sfxVolume;
            walkingAudioSource.clip = walkingSound;
            walkingAudioSource.loop = true;
            walkingAudioSource.Play();
            yield return null;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                PlayAttackSound();
            }
        }
        
        public override float PlayDeathSound()
        {
            walkingAudioSource.DOFade(0, 1f);
            return base.PlayDeathSound();
        }
    }
}