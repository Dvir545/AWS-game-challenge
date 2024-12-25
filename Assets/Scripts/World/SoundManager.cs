using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Utils;
using Utils.Data;

namespace World
{
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private AudioSource declaration;
        [SerializeField] private AudioSource ui;
        [SerializeField] private AudioClip buttonPress;
        [SerializeField] private AudioClip buttonRelease;
        [SerializeField] private AudioClip shortButton;
        [SerializeField] private AudioClip morningStarted;
        [SerializeField] private AudioClip nightStarted;
        [SerializeField] private AudioClip gotMoney;
        [SerializeField] private AudioClip purchase;
        [SerializeField] private AudioClip cantPurchase;
        [SerializeField] private AudioClip openStore;
        [SerializeField] private AudioClip towerBuilt;
        private float _buttonPitch = 1f;
        
        [Header("music")]
        [SerializeField] private AudioSource ocean;
        [SerializeField] private AudioSource entryMusic;
        private List<AudioSource> _musicSources = new();

        public void SyncMusicVolume()
        {
            if (_musicSources.Count == 0)
            {
                _musicSources.Add(ocean);
                _musicSources.Add(entryMusic);
            }
            foreach (var source in _musicSources)
            {
                source.volume = GameStatistics.Instance.musicVolume;
            }
        }

        public void PlaySFX(AudioSource source, AudioClip clip, bool randomPitch = true, float pitchRange = .3f)
        {
            source.volume = GameStatistics.Instance.sfxVolume;
            source.clip = clip;
            if (randomPitch)
            {
                source.pitch = Random.Range(1f - pitchRange, 1f + pitchRange);
            }
            source.Play();
        }
        
        private void PlayMusic(AudioSource source)
        {
            if (!source.isPlaying)
            {
                source.Play();
                if (source.volume == 0 && GameStatistics.Instance.musicVolume > 0)
                {
                    source.DOFade(GameStatistics.Instance.musicVolume, 1f);
                }
            }
        }
        
        private void StopMusic(AudioSource source, float fadeDuration = 1f)
        {
            source.DOFade(0, fadeDuration).OnComplete(source.Pause);
        }
    
        public void ButtonPress()
        {
            ui.volume = GameStatistics.Instance.sfxVolume;
            ui.clip = buttonPress;
            ui.pitch = _buttonPitch;
            ui.Play();
        }
    
        public void ButtonRelease()
        {
            ui.volume = GameStatistics.Instance.sfxVolume;
            ui.clip = buttonRelease;
            ui.pitch = _buttonPitch;
            ui.Play();
            _buttonPitch = Random.Range(.7f, 1.3f);
        }
        
        public void ShortButton()
        {
            PlaySFX(ui, shortButton);
        }
    
        public void Healed()
        {
            PlaySFX(ui, buttonRelease);
        }
    
        public void GotMoney()
        {
            PlaySFX(ui, gotMoney);
        }
    
        public void Purchase()
        {
            PlaySFX(ui, purchase);
        }
    
        public void CantPurchase()
        {
            PlaySFX(ui, cantPurchase, false);
        }
        
        public void OpenStore()
        {
            PlaySFX(ui, openStore);
        }
        
        public void TowerBuilt()
        {
            PlaySFX(ui, towerBuilt);
        }
    
        public void DayStarted()
        {
            PlaySFX(declaration, morningStarted, true, .2f);
        }
    
        public void NightStarted()
        {
            PlaySFX(declaration, nightStarted, true, .2f);
        }
        
        public void PlayOcean()
        {
            PlayMusic(ocean);
        }
        
        public void StopOcean()
        {
            StopMusic(ocean);
        }
        
        public void PlayEntryMusic()
        {
            PlayMusic(entryMusic);
        }
        
        public void StopEntryMusic()
        {
            StopMusic(entryMusic, .3f);
        }
        
        private Coroutine _entryMusicCoroutine;
        private IEnumerator PlayEntryMusicCR(bool immediate = false)
        {
            if (!immediate)
            {
                yield return  new WaitForSeconds(4f);
            }
            PlayEntryMusic();
        }
        
        public void StartEntryMusicCR(bool immediate = false)
        {
            if (_entryMusicCoroutine != null)
            {
                return;
            }
            _entryMusicCoroutine = StartCoroutine(PlayEntryMusicCR(immediate));
        }
        public void StopEntryMusicCR()
        {
            if (_entryMusicCoroutine != null)
            {
                StopCoroutine(_entryMusicCoroutine);
            }
            StopEntryMusic();
            _entryMusicCoroutine = null;
        }
    
    }
}
