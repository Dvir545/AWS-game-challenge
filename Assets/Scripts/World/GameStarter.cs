using System;
using UnityEngine;
using Utils.Data;

namespace World
{
    public class GameStarter : MonoBehaviour
    {
        public void ContinueFromSavedJson()
        {
            string json = null; // DVIR - load from dynamo
            if (json == null)
            {
                Debug.Log("No saved game found");
                return;
            }
            else
            {
                GetComponent<Collider2D>().enabled = false;
                GameData.Instance.LoadFromJson(json);
            }
        }
        
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
}
