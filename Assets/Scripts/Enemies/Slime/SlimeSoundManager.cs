using UnityEngine;

namespace Enemies.Slime
{
    public class SlimeSoundManager: EnemySoundManager
    {
        private void Start()
        {
            StopCoroutine(WalkingCR);
        }
        
        public void PlayJumpSound()
        {
            AudioSource.clip = walkingSound;
            AudioSource.pitch = Random.Range(0.8f, 1.2f);
            AudioSource.Play();
        }
        
        public void PlayLandSound() => PlayJumpSound();
    }
}