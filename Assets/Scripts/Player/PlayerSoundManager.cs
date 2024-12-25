using UnityEngine;
using Utils;
using Utils.Data;
using World;
using Random = UnityEngine.Random;

namespace Player
{
    public class PlayerSoundManager: MonoBehaviour
    {
        [SerializeField] private AudioSource action;  // including walk
        [SerializeField] private AudioSource hits;
        [SerializeField] private AudioSource gotHit;
        [SerializeField] private AudioSource cooldown;
        
        [SerializeField] private AudioClip walkAudio;
        private const float WalkDelay = .25f;
        [SerializeField] private AudioClip swordAudio;
        private const float SwordDelay = .3f;
        [SerializeField] private AudioClip hoeAudio;
        private const float HoeDelay = .45f;
        [SerializeField] private AudioClip hammerAudio;
        private const float HammerDelay = .47f;
        private float _delay;
        
        [SerializeField] private AudioClip hitsAudio;
        [SerializeField] private AudioClip gotHitAudio;
        [SerializeField] private AudioClip cooldownAudio;

        private PlayerMovement _playerMovement;
        private PlayerActionManager _playerActionManager;
        private PlayerData _playerData;
        private PlayerHealthManager _playerHealthManager;
        
        public void Init()
        {
            if (_playerData == null)
            {
                _playerData = FindObjectOfType<PlayerData>();
                _playerMovement = GetComponent<PlayerMovement>();
                _playerActionManager = GetComponent<PlayerActionManager>();
                _playerHealthManager = GetComponent<PlayerHealthManager>();
            }
            EventManager.Instance.StartListening(EventManager.PlayerGotHit, GotHit);
            EventManager.Instance.StartListening(EventManager.PlayerHitEnemy, Hits);
            EventManager.Instance.StartListening(EventManager.Cooldown, Cooldown);
        }

        private void Cooldown(object arg0)
        {
            SoundManager.Instance.PlaySFX(cooldown, cooldownAudio);
        }

        public void Hits(object argo)
        {
            SoundManager.Instance.PlaySFX(hits, hitsAudio);
        }

        public void GotHit(object arg0)
        {
            SoundManager.Instance.PlaySFX(gotHit, gotHitAudio);
        }

        private void Update()
        {
            if (_playerHealthManager.IsDead) return;
            if (_delay > 0)
            {
                _delay -= Time.deltaTime;
                return;
            }
            if (_playerActionManager.IsActing)
            {
                switch (_playerData.GetCurTool())
                {
                    case HeldTool.Sword: 
                        action.clip = swordAudio;
                        _delay = SwordDelay;
                        break;
                    case HeldTool.Hoe: 
                        action.clip = hoeAudio;
                        _delay = HoeDelay;
                        break;
                    case HeldTool.Hammer: 
                        action.clip = hammerAudio;
                        _delay = HammerDelay;
                        break;
                }
            } else if (_playerMovement.IsMoving)
            {
                action.clip = walkAudio;
                _delay = WalkDelay;
            }
            else
            {
                action.clip = null;
            }

            if (action.clip != null)
            {
                action.volume = GameStatistics.Instance.sfxVolume;
                action.pitch = Random.Range(0.7f, 1.3f);
                action.Play();
            }
        }

        public void Reset()
        {
            _delay = 0f;
        }
    }
}