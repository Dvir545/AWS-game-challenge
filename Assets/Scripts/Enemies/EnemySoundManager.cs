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
        [SerializeField] private float secondsBetweenWalks = 0.3f;
        protected AudioSource AudioSource;
        private EnemyMovementManager _enemyMovementManager;
        protected EnemyHealthManager EnemyHealthManager;
        protected IEnumerator WalkingCR;

        protected bool IsPlayingImportantSound => AudioSource.isPlaying && 
                                                  (AudioSource.clip == hitSound || AudioSource.clip == deathSound);
        
        protected virtual void Awake()
        {
            AudioSource = GetComponent<AudioSource>();
            _enemyMovementManager = GetComponent<EnemyMovementManager>();
            EnemyHealthManager = GetComponent<EnemyHealthManager>();
        }

        private void Start()
        {
            WalkingCR = PlayWalkingSoundCR();
            StartCoroutine(WalkingCR);
        }

        public void PlayHitSound()
        {
            // AudioSource.clip = hitSound;
            AudioSource.pitch = Random.Range(0.8f, 1.2f);
            AudioSource.PlayOneShot(hitSound);
        }
        
        public virtual float PlayDeathSound()
        {
            if (AudioSource.isPlaying)
            {
                AudioSource.Stop();
            }
            AudioSource.clip = deathSound;
            AudioSource.pitch = Random.Range(0.8f, 1.2f);
            AudioSource.Play();
            return deathSound.length;
        }

        protected bool IsEnemyMoving
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

        protected virtual IEnumerator PlayWalkingSoundCR()
        {
            while (true)
            {
                if (EnemyHealthManager.IsDead) break;
                if (IsEnemyMoving && !IsPlayingImportantSound)
                {
                    AudioSource.clip = walkingSound;
                    AudioSource.pitch = Random.Range(0.6f, 1.1f);
                    AudioSource.Play();
                    yield return new WaitForSeconds(secondsBetweenWalks);
                }
                else
                {
                    yield return null;
                }
            }
            yield return null;
        }
    }
}
