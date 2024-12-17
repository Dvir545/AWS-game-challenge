using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World;

public class GameStarter : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Game Started");
            DayNightManager.Instance.StartGame();
            gameObject.SetActive(false);
        }
    }
}
