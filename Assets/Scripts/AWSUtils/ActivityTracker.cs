using UnityEngine;
using Utils;

namespace AWSUtils
{
    public class ActivityTracker : MonoBehaviour
    {
        private static ActivityTracker instance;
        public static ActivityTracker Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ActivityTracker>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ActivityTracker");
                        instance = go.AddComponent<ActivityTracker>();
                    }
                }
                return instance;
            }
        }

        public LastRoundActivity CurrentActivity { get; private set; } = new LastRoundActivity();

        private void OnEnable()
        {
            EventManager.Instance.StartListening(EventManager.PlayerGotHit, OnPlayerGotHit);
            EventManager.Instance.StartListening(EventManager.CropHarvested, OnCropHarvested);
            EventManager.Instance.StartListening(EventManager.TowerBuilt, OnTowerBuilt);
            EventManager.Instance.StartListening(EventManager.DayStarted, OnDayStarted);
        }

        private void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.StopListening(EventManager.PlayerGotHit, OnPlayerGotHit);
            EventManager.Instance.StopListening(EventManager.CropHarvested, OnCropHarvested);
            EventManager.Instance.StopListening(EventManager.TowerBuilt, OnTowerBuilt);
            EventManager.Instance.StopListening(EventManager.DayStarted, OnDayStarted);
        }

        private void OnPlayerGotHit(object damage)
        {
            if (damage is int dmg)
            {
                CurrentActivity.damageTaken += dmg;
            }
        }

        private void OnCropHarvested(object _)
        {
            CurrentActivity.cropsHarvested++;
        }

        private void OnTowerBuilt(object _)
        {
            CurrentActivity.towersBuilt++;
        }

        public void TrackCropPlanted()
        {
            CurrentActivity.cropsPlanted++;
        }

        public void TrackCropDestroyed()
        {
            CurrentActivity.cropsDestroyed++;
        }

        public void TrackTowerDestroyed()
        {
            CurrentActivity.towersDestroyed++;
        }

        private void OnDayStarted(object _)
        {
            CurrentActivity = new LastRoundActivity();
        }
    }
}