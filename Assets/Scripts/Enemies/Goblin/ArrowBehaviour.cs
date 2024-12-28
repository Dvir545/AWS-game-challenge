using Player;
using UnityEngine;
using Utils;
using Utils.Data;

namespace Enemies.Goblin
{
    public class ArrowBehaviour: MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        private Vector2 _direction;
        [SerializeField] private float lifetime = 3f;
        private AudioSource _audioSource;
        [SerializeField] private Sprite arrowDownSprite;
        [SerializeField] private Sprite arrowRightSprite;
        private SpriteRenderer _spriteRenderer;
        [SerializeField] private BoxCollider2D downCollider;
        [SerializeField] private BoxCollider2D rightCollider;
        private BoxCollider2D _collider;
        private Transform _shooter;
        
        private void Awake()
        {
            _audioSource = GetComponentInParent<AudioSource>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
        }

        public void Initialize(Vector2 direction, FacingDirection facingDirection, Transform shooter)
        {
            _shooter = shooter;
            _direction = direction.normalized;
            switch (facingDirection)
            {
                case FacingDirection.Down:
                    _spriteRenderer.sprite = arrowDownSprite;
                    _collider.offset = downCollider.offset;
                    _collider.size = downCollider.size;
                    transform.localScale = new Vector3(1, 1, 1);
                    break;
                case FacingDirection.Up:
                    _spriteRenderer.sprite = arrowDownSprite;
                    _collider.offset = downCollider.offset;
                    _collider.size = downCollider.size;
                    transform.localScale = new Vector3(1, -1, 1);
                    break;
                case FacingDirection.Right:
                    _spriteRenderer.sprite = arrowRightSprite;
                    _collider.offset = rightCollider.offset;
                    _collider.size = rightCollider.size;
                    transform.localScale = new Vector3(-1, 1, 1);
                    break;
                case FacingDirection.Left:
                    _spriteRenderer.sprite = arrowRightSprite;
                    _collider.offset = rightCollider.offset;
                    _collider.size = rightCollider.size;
                    transform.localScale = new Vector3(1, 1, 1);
                    break;
            }
        }

        private void Update()
        {
            transform.Translate(_direction * (speed * Time.deltaTime));
            lifetime -= Time.deltaTime;
            if (lifetime <= 0)
            {
                DisableArrow();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.transform == _shooter) return;
            if (other.CompareTag("Player") || other.CompareTag("Enemy"))
            {
                var hitDirection = (other.transform.position - transform.position).normalized;
                if (other.CompareTag("Player"))
                    other.GetComponent<PlayerHealthManager>().GotHit(Enemy.Goblin, hitDirection);
                else
                {
                    var damage = EnemyData.GetDamageMultiplier(Enemy.Goblin);
                    other.GetComponent<EnemyHealthManager>().TakeDamage(damage, hitDirection);
                }
                _audioSource.volume = GameStatistics.Instance.sfxVolume;
                _audioSource.Play();
                DisableArrow();
            }
        }
        
        private void DisableArrow()
        {
            ArrowPool.Instance.ReleaseArrow(gameObject);
            lifetime = 3f;
        }
    }
}