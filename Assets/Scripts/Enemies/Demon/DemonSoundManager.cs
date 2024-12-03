using UnityEngine;

namespace Enemies.Demon
{
    public class DemonSoundManager: EnemySoundManager
    {
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioClip landedHitSound;
    }
}