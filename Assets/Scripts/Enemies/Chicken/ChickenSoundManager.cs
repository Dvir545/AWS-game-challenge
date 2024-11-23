using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies.Chicken
{
    public class ChickenSoundManager: EnemySoundManager
    {
        [SerializeField] private AudioClip eatingSound;
        
        private ChickenEatingManager _chickenEatingManager;
        private Coroutine _eatingCR;

        protected override void Awake()
        {
            _chickenEatingManager = GetComponent<ChickenEatingManager>();
            base.Awake();
        }

        private void Update()
        {
            if (!_chickenEatingManager.IsEating)
            {
                if (_eatingCR != null)
                {
                    StopCoroutine(_eatingCR);
                    _eatingCR = null;
                    WalkingCR = StartCoroutine(PlayWalkingSoundCR());
                }
            }
            else
            {
                if (WalkingCR != null)
                {
                    StopCoroutine(WalkingCR);
                    WalkingCR = null;
                    _eatingCR = StartCoroutine(PlayEatingSoundCR());
                }
            }
        }

        protected override IEnumerator PlayWalkingSoundCR()
        {
            while (true)
            {
                if (!_chickenEatingManager.IsEating && !IsPlayingImportantSound)
                {
                    AudioSource.clip = walkingSound;
                    AudioSource.Play();
                    yield return new WaitForSeconds(Random.Range(2f, 5f));
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator PlayEatingSoundCR()
        {
            while (true)
            {
                if (_chickenEatingManager.IsEating && !IsPlayingImportantSound)
                {
                    AudioSource.clip = eatingSound;
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