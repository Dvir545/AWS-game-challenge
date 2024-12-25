using System.Collections;
using System.Collections.Generic;
using Enemies;
using UnityEngine;
using World;

public class GoblinSoundManager : EnemySoundManager
{
    [SerializeField] private AudioClip shootSound;
    
    public void PlayShootSound()
    {
        SoundManager.Instance.PlaySFX(AudioSource, shootSound, true, .2f);
    }
}
