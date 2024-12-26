using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Utils;
using Utils.Data;
using Random = UnityEngine.Random;

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
        [SerializeField] private AudioClip towerDestroyed;
        [SerializeField] private AudioClip farmDestroyed;
        private float _buttonPitch = 1f;
        
        [Header("music")]
        [SerializeField] private AudioSource ocean;
        [SerializeField] private AudioSource entryMusic;
        [SerializeField] private AudioSource dayBackgroundMusic;
        [SerializeField] private List<AudioClip> dayPlaylist;
        private int _currentDaySongIndex;
        private Coroutine _dayBackgroundCoroutine;
        [SerializeField] private AudioSource nightBackgroundMusic;
        [SerializeField] private List<AudioClip> nightPlaylist;
        private int _currentNightSongIndex;
        private Coroutine _nightBackgroundCoroutine;
        private bool _backgroundMusicPaused;
        private bool _appPaused;
        private bool _playingDayMusic;
        private List<AudioSource> _musicSources = new();

        public void Init(bool stop=false)
        {
            _currentDaySongIndex = 0;
            _currentNightSongIndex = 0;
            _backgroundMusicPaused = false;
            if (stop)
                StopEntryMusicCR();
        }

        public void SyncMusicVolume()
        {
            if (_musicSources.Count == 0)
            {
                _musicSources.Add(ocean);
                _musicSources.Add(entryMusic);
                _musicSources.Add(dayBackgroundMusic);
                _musicSources.Add(nightBackgroundMusic);
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
        
        private void PlayMusic(AudioSource source, float duration=1f, bool restart=false)
        {
            if (!source.isPlaying)
            {
                if (restart)
                {
                    source.Stop();
                }
                source.Play();
                if (source.volume == 0 && GameStatistics.Instance.musicVolume > 0)
                {
                    source.DOFade(GameStatistics.Instance.musicVolume, duration);
                }
            }
        }
        
        private void PauseMusic(AudioSource source, float fadeDuration = 1f)
        {
            if (source.isPlaying)
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
        
        public void TowerDestroyed()
        {
            PlaySFX(ui, towerDestroyed);
        }
        
        public void FarmDestroyed()
        {
            PlaySFX(ui, farmDestroyed);
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
            PauseMusic(ocean);
        }
        
        public void PlayEntryMusic(float duration=0f)
        {
            PlayMusic(entryMusic, duration);
        }
        
        public void StopEntryMusic()
        {
            PauseMusic(entryMusic, .3f);
        }
        
        private Coroutine _entryMusicCoroutine;
        private IEnumerator PlayEntryMusicCR(bool immediate = false, float duration=0f)
        {
            if (!immediate)
            {
                yield return  new WaitForSeconds(3f);
            }
            PlayEntryMusic(duration);
        }
        
        public void StartEntryMusicCR(bool immediate = false, float duration=0f)
        {
            if (_entryMusicCoroutine != null)
            {
                return;
            }
            _entryMusicCoroutine = StartCoroutine(PlayEntryMusicCR(immediate, duration));
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
        
        private IEnumerator DayBackgroundMusicCR()
        {
            while (true)
            {
                while (!GameStarter.Instance.GameStarted || _backgroundMusicPaused || dayBackgroundMusic.isPlaying || 
                       _appPaused || DayNightManager.Instance.CurrentDayPhase != DayPhase.Day)
                {
                    yield return new WaitForSeconds(1f); // Check every second
                }
                dayBackgroundMusic.clip = dayPlaylist[_currentDaySongIndex];
                PlayMusic(dayBackgroundMusic);
                _currentDaySongIndex = (_currentDaySongIndex + 1) % dayPlaylist.Count;
                
                yield return null;
            }
        }
        
        private IEnumerator NightBackgroundMusicCR()
        {
            while (true)
            {
                while (!GameStarter.Instance.GameStarted || _backgroundMusicPaused || nightBackgroundMusic.isPlaying || 
                       _appPaused || DayNightManager.Instance.CurrentDayPhase != DayPhase.Night)
                {
                    yield return new WaitForSeconds(1f); // Check every second
                }
                nightBackgroundMusic.clip = nightPlaylist[_currentNightSongIndex];
                PlayMusic(nightBackgroundMusic);
                _currentNightSongIndex = (_currentNightSongIndex + 1) % nightPlaylist.Count;
                yield return null;
            }
        }
        
        public void StartGame()
        {
            StopOcean();
            PlayMusic(dayBackgroundMusic, 0f, restart: true);
            PlayMusic(nightBackgroundMusic, 0f, restart: true);
            _dayBackgroundCoroutine = StartCoroutine(DayBackgroundMusicCR());
            _nightBackgroundCoroutine = StartCoroutine(NightBackgroundMusicCR());
            PauseMusic(nightBackgroundMusic, 0f);
            _playingDayMusic = true;
        }
        
        public void PauseBackgroundSong(float duration=0f)
        {
            PauseMusic(dayBackgroundMusic, duration);
            PauseMusic(nightBackgroundMusic, duration);
            _backgroundMusicPaused = true;
        }
        
        public void ResumeBackgroundSong()
        {
            _backgroundMusicPaused = false;
            SwitchBackgroundMusic(_playingDayMusic, 0f);
        }

        public void SwitchBackgroundMusic(bool day, float duration = Constants.ChangeLightDurationInSeconds)
        {
            if (day)
            {
                if (nightBackgroundMusic.isPlaying)
                    PauseMusic(nightBackgroundMusic, duration);
                PlayMusic(dayBackgroundMusic, duration);
            }
            else
            {
                if (dayBackgroundMusic.isPlaying)
                    PauseMusic(dayBackgroundMusic, duration);
                PlayMusic(nightBackgroundMusic, duration);
            }
            _playingDayMusic = day;
        }

        private void OnApplicationPause(bool hasFocus)
        {
            _appPaused = true;
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            _appPaused = false;
        }
    }
}
