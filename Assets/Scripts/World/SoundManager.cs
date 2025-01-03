using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Player;
using UnityEngine;
using Utils;
using Utils.Data;
using Random = UnityEngine.Random;

namespace World
{
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private GameObject game;
        [SerializeField] private PlayerHealthManager playerHealthManager;
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
        private int _currentDaySongIndex = 0;
        private Coroutine _dayBackgroundCoroutine;
        [SerializeField] private AudioSource nightBackgroundMusic;
        [SerializeField] private List<AudioClip> nightPlaylist;
        private int _currentNightSongIndex = 0;
        private Coroutine _nightBackgroundCoroutine;
        private bool _playingDayMusic;
        private List<AudioSource> _musicSources = new();
        private float dayBeforeSample;
        private float nightBeforeSample;

        public void Init(bool stop=false)
        {
            // _currentDaySongIndex = 0;
            // _currentNightSongIndex = 0;
            dayBeforeSample = 0;
            nightBeforeSample = 0;
            if (stop)
                StopEntryMusicCR();
            if (pausedSources.ContainsKey(dayBackgroundMusic))
                pausedSources.Remove(dayBackgroundMusic);
            if (pausedSources.ContainsKey(nightBackgroundMusic))
                    pausedSources.Remove(nightBackgroundMusic);
        }

        public void SyncMusicVolume()
        {
            Debug.Log("sync music volume");
            if (_musicSources.Count == 0)
            {
                _musicSources.Add(ocean);
                _musicSources.Add(entryMusic);
                _musicSources.Add(dayBackgroundMusic);
                _musicSources.Add(nightBackgroundMusic);
            }
            foreach (var source in _musicSources)
            {
                if (game.activeSelf)
                    source.volume = SettingsBehaviour.Instance.MusicVolume;
                else
                    source.volume = PlayerPrefs.GetFloat(Constants.MusicVolumePlayerPref, 0.5f);
            }
        }

        public void PlaySFX(AudioSource source, AudioClip clip, bool randomPitch = true, float pitchRange = .3f)
        {
            source.volume = SettingsBehaviour.Instance.SFXVolume;
            source.clip = clip;
            if (randomPitch)
            {
                source.pitch = Random.Range(1f - pitchRange, 1f + pitchRange);
            }
            source.Play();
        }
        
        private void PlayMusic(AudioSource source, float duration=1f, bool restart=false)
        {
            Debug.Log("play music");
            if (!source.isPlaying)
            {
                if (restart)
                {
                    source.Stop();
                }
                source.Play();
                if (source.volume == 0 && SettingsBehaviour.Instance.MusicVolume > 0)
                {
                    source.DOFade(SettingsBehaviour.Instance.MusicVolume, duration);
                }
            }
        }
        
        private void PauseMusic(AudioSource source, float fadeDuration = 1f, bool stop=false)
        {
            Debug.Log("pause music");
            if (source.isPlaying)
                if (stop)
                    source.DOFade(0, fadeDuration).OnComplete(source.Stop);
                else
                    source.DOFade(0, fadeDuration).OnComplete(source.Pause);
        }
    
        public void ButtonPress()
        {
            ui.volume = SettingsBehaviour.Instance.SFXVolume;
            ui.clip = buttonPress;
            ui.pitch = _buttonPitch;
            ui.Play();
        }
    
        public void ButtonRelease()
        {
            ui.volume = SettingsBehaviour.Instance.SFXVolume;
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
        
        public void Error() => CantPurchase();
        
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
        
        public void PlayEntryMusic(float duration=0f, bool restart=false)
        {
            PlayMusic(entryMusic, duration, restart);
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
        
        public void StartGame()
        {
            StopOcean();
            PlayMusic(dayBackgroundMusic, 0f, restart: true);
            PlayMusic(nightBackgroundMusic, 0f, restart: true);
            PauseMusic(nightBackgroundMusic, 0f);
            _playingDayMusic = true;
        }
        
        public void PauseBackgroundSong(float duration=0f)
        {
            PauseMusic(dayBackgroundMusic, duration);
            PauseMusic(nightBackgroundMusic, duration);
        }

        public void SwitchBackgroundMusic(bool day, float duration = Constants.ChangeLightDurationInSeconds)
        {
            if (day)
            {
                if (nightBackgroundMusic.isPlaying)
                {
                    PauseMusic(nightBackgroundMusic, duration);
                    if (activeAudioSources.Contains(nightBackgroundMusic))
                        activeAudioSources.Remove(nightBackgroundMusic);
                }
                PlayMusic(dayBackgroundMusic, duration);
                activeAudioSources.Add(dayBackgroundMusic);
            }
            else
            {
                if (dayBackgroundMusic.isPlaying)
                {
                    PauseMusic(dayBackgroundMusic, duration);
                    if (activeAudioSources.Contains(dayBackgroundMusic))
                        activeAudioSources.Remove(dayBackgroundMusic);
                }
                PlayMusic(nightBackgroundMusic, duration);
                activeAudioSources.Add(nightBackgroundMusic);
            }
            _playingDayMusic = day;
        }

        public void StopGameMusic()
        {
            if (_dayBackgroundCoroutine != null)
            {
                StopCoroutine(_dayBackgroundCoroutine);
            }
            if (_nightBackgroundCoroutine != null)
            {
                StopCoroutine(_nightBackgroundCoroutine);
            }
            PauseMusic(dayBackgroundMusic, stop: true);
            PauseMusic(nightBackgroundMusic, stop: true);
        }
        
        private HashSet<AudioSource> activeAudioSources = new HashSet<AudioSource>();
        private Dictionary<AudioSource, float> pausedSources = new Dictionary<AudioSource, float>();
        private bool isInitialized = false;
        private bool notInFocus = false;
        private bool wasGamePaused = false;
        private bool waitingForGameResume = false;

        private void Awake()
        {
            // Register pause/resume handlers
            Application.focusChanged += OnFocusChanged;
        }

        private void Start()
        {
            isInitialized = true;
        }
        
        private void Update()
        {
            // Track which sources are actually playing
            TrackAudioSource(ocean);
            TrackAudioSource(entryMusic);
            TrackAudioSource(dayBackgroundMusic);
            TrackAudioSource(nightBackgroundMusic);
            
            // change music in playlist
            if (!GameStarter.Instance.GameStarted) return;

            if (dayBeforeSample < dayBackgroundMusic.timeSamples)
            {
                dayBeforeSample = dayBackgroundMusic.timeSamples - 1f;
            }
            else if (dayBeforeSample > dayBackgroundMusic.timeSamples)  // song ended
            {
                dayBackgroundMusic.Stop();
                _currentDaySongIndex = (_currentDaySongIndex + 1) % dayPlaylist.Count;
                dayBackgroundMusic.clip = dayPlaylist[_currentDaySongIndex];
                dayBackgroundMusic.Play();
                dayBeforeSample = 0;
            }
            
            if (nightBeforeSample < nightBackgroundMusic.timeSamples)
            {
                nightBeforeSample = nightBackgroundMusic.timeSamples - 1f;
            }
            else if (nightBeforeSample > nightBackgroundMusic.timeSamples)  // song ended
            {
                nightBackgroundMusic.Stop();
                _currentNightSongIndex = (_currentNightSongIndex + 1) % dayPlaylist.Count;
                nightBackgroundMusic.clip = nightPlaylist[_currentNightSongIndex];
                nightBackgroundMusic.Play();
                nightBeforeSample = 0;
            }
        }
        
        private void TrackAudioSource(AudioSource source)
        {
            if (source != null)
            {
                if (source.isPlaying && !activeAudioSources.Contains(source))
                {
                    activeAudioSources.Add(source);
                }
                else if (!source.isPlaying && activeAudioSources.Contains(source) && !pausedSources.ContainsKey(source))
                {
                    activeAudioSources.Remove(source);
                }
            }
        }

        private void OnDestroy()
        {
            Application.focusChanged -= OnFocusChanged;
        }
        
        private void OnFocusChanged(bool hasFocus)
        {
            if (!isInitialized) return;
        
            if (!hasFocus)
            {
                wasGamePaused = game.activeSelf && !playerHealthManager.IsDead;  // because SettingsBehaviour pauses if game.activeSelf
                if (wasGamePaused)
                    PauseAllMusic();
            }
            else {
                waitingForGameResume = true;
            }
        }

            public void PauseAllMusic()
            {
                foreach (var source in activeAudioSources)
                {
                    if (source != null)
                    {
                        pausedSources[source] = source.time;
                        source.Pause();
                    }
                }
            }
        
            public void ResumeAllMusic()
            {
                foreach (var kvp in pausedSources)
                {
                    if (kvp.Key != null && activeAudioSources.Contains(kvp.Key))
                    {
                        kvp.Key.time = kvp.Value;
                        kvp.Key.Play();
                    }
                }
                pausedSources.Clear();
            }
    }
}
