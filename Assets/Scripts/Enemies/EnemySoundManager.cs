using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace Enemies
    {
    public class EnemySoundManager : MonoBehaviour
    {
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] protected AudioClip walkingSound;
        protected AudioSource AudioSource;
        private EnemyMovementManager _enemyMovementManager;
        protected Coroutine WalkingCR;

        private bool IsPlayingImportantSound => AudioSource.isPlaying && 
                                                (AudioSource.clip == hitSound || AudioSource.clip == deathSound);
        
        private void Awake()
        {
            AudioSource = GetComponent<AudioSource>();
            _enemyMovementManager = GetComponent<EnemyMovementManager>();
            WalkingCR = StartCoroutine(PlayWalkingSound());
        }
        
        public void PlayHitSound()
        {
            AudioSource.clip = hitSound;
            AudioSource.pitch = Random.Range(0.8f, 1.2f);
            AudioSource.Play();
        }
        
        public void PlayDeathSound()
        {
            AudioSource.clip = deathSound;
            AudioSource.pitch = Random.Range(0.8f, 1.2f);
            AudioSource.Play();
        }
        
        private bool IsEnemyMoving
        {
            get
            {
                try
                {
                    return _enemyMovementManager?.IsMoving ?? false;
                }
                catch (NullReferenceException)
                {
                    return false;
                }
            }
        }

        private IEnumerator PlayWalkingSound()
        {
            while (true)
            {
                if (IsEnemyMoving && !IsPlayingImportantSound)
                {
                    AudioSource.clip = walkingSound;
                    AudioSource.pitch = Random.Range(0.6f, 1.1f);
                    AudioSource.Play();
                    yield return new WaitForSeconds(0.3f);
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}
