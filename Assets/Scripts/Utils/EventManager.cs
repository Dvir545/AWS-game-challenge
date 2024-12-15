using System.Collections.Generic;
using UnityEngine.Events;

namespace Utils
{
    public class EventManager : Singleton<EventManager>
    {
        public const string MaxHealthIncreased = "MaxHealthIncreased";
        public const string HealthChanged = "HealthChanged";
        public const string CashChanged = "CashChanged";
        public const string PlayerDied = "PlayerDied";
        public const string PlayerGotHit  = "PlayerGotHit";
        public const string PlayerHitEnemy = "PlayerHitEnemy";
        public const string TowerBuilt = "TowerBuilt";
        public const string CropHarvested = "CropHarvested";
        public const string ActiveToolChanged = "ActiveToolChanged";
        public const string AbilityUpgraded = "AbilityUpgraded";
        public const string CropBeingDestroyed = "CropBeingDestroyed";
        public const string CropStoppedBeingDestroyed = "CropStoppedBeingDestroyed";
        public const string TowerUnderAttack = "TowerUnderAttack";
        public const string TowerStoppedBeingUnderAttack = "TowerStoppedBeingUnderAttack";

        protected EventManager()
        {
            Init();
        } // guarantee this will be always a singleton only - can't use the constructor!

        public class FloatEvent : UnityEvent<object> {} //empty class; just needs to exist


        private Dictionary <string, FloatEvent> _eventDictionary;

        private void Init ()
        {
            if (_eventDictionary == null)
            {
                _eventDictionary = new Dictionary<string, FloatEvent>();
            }
        }
	
        public void StartListening (string eventName, UnityAction<object> listener)
        {
            FloatEvent thisEvent = null;
            if (_eventDictionary.TryGetValue (eventName, out thisEvent))
            {
                thisEvent.AddListener (listener);
            } 
            else
            {
                thisEvent = new FloatEvent ();
                thisEvent.AddListener (listener);
                _eventDictionary.Add (eventName, thisEvent);
            }
        }
	
        public void StopListening (string eventName, UnityAction<object> listener)
        {
            FloatEvent thisEvent = null;
            if (_eventDictionary.TryGetValue (eventName, out thisEvent))
            {
                thisEvent.RemoveListener (listener);
            }
        }
	
        public void TriggerEvent (string eventName, object obj)
        {
            FloatEvent thisEvent = null;
            if (_eventDictionary.TryGetValue (eventName, out thisEvent))
            {
                thisEvent.Invoke (obj);
            }
        }
    }
}