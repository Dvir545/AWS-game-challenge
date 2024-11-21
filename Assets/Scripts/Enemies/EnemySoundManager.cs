using UnityEngine;

namespace Enemies
{
    public class EnemySoundManager : MonoBehaviour
    {
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip deathSound;
        private AudioSource _audioSource;
        
        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }
        
        public void PlayHitSound()
        {
            _audioSource.clip = hitSound;
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.Play();
        }
        
        public void PlayDeathSound()
        {
            _audioSource.clip = deathSound;
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.Play();
        }
    }
}
