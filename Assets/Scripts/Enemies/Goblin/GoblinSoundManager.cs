using System.Collections;
using System.Collections.Generic;
using Enemies;
using UnityEngine;

public class GoblinSoundManager : EnemySoundManager
{
    [SerializeField] private AudioClip shootSound;
    
    public void PlayShootSound()
    {
        AudioSource.clip = shootSound;
        AudioSource.pitch = Random.Range(0.8f, 1.2f);
        AudioSource.Play();
    }
}
