using System;
using System.Collections;
using UnityEngine;
using Utils.Data;
using World;
using Random = UnityEngine.Random;

namespace Enemies.Chicken
{
    public class ChickenSoundManager: EnemySoundManager
    {
        [SerializeField] private AudioClip eatingSound;
        
        private ChickenEatingManager _chickenEatingManager;
        private IEnumerator _eatingCR;

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
                    _eatingCR = PlayEatingSoundCR();
                    StartCoroutine(_eatingCR);
                }
            }
        }

        protected override IEnumerator PlayWalkingSoundCR()
        {
            while (true)
            {
                if (EnemyHealthManager.IsDead) break;
                if (!_chickenEatingManager.IsEating && !IsPlayingImportantSound)
                {
                    AudioSource.volume = SettingsBehaviour.Instance.SFXVolume;
                    AudioSource.clip = walkingSound;
                    AudioSource.Play();
                    yield return new WaitForSeconds(Random.Range(2f, 5f));
                }
                else
                {
                    yield return null;
                }
            }
            yield return null;
        }

        private IEnumerator PlayEatingSoundCR()
        {
            while (true)
            {
                if (_chickenEatingManager.IsEating && !IsPlayingImportantSound)
                {
                    AudioSource.volume = SettingsBehaviour.Instance.SFXVolume;
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