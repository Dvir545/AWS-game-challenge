using Enemies;
using UnityEngine;
using Utils;

namespace UI.WarningSign
{
    public class WarningSignManager: MonoBehaviour
    {
        [SerializeField] private Transform warningSignParent;
        private void Start()
        {
            EventManager.Instance.StartListening(EventManager.CropBeingDestroyed, StartWarningSign);
            EventManager.Instance.StartListening(EventManager.TowerUnderAttack, StartWarningSign);
            EventManager.Instance.StartListening(EventManager.EnemySpawned, StartWarningSign);
            EventManager.Instance.StartListening(EventManager.CropReadyForHarvest, StartWarningSign);
            EventManager.Instance.StartListening(EventManager.CropStoppedBeingDestroyed, StopWarningSign);
            EventManager.Instance.StartListening(EventManager.CropHarvested, StopWarningSign);
            EventManager.Instance.StartListening(EventManager.TowerStoppedBeingUnderAttack, StopWarningSign);
            EventManager.Instance.StartListening(EventManager.EnemyKilled, StopWarningSign);
        }

        private void StartWarningSign(object arg0)
        {
            if (arg0 is (Transform target, WarningSignType type))
            {
                WarningSignPool.Instance.GetWarningSign(warningSignParent, target, type: type);
            }
        }
        private void StopWarningSign(object arg0)
        {
            if (arg0 is Transform target)
            {
                WarningSignPool.Instance.ReleaseWarningSign(target);
            }
        }
    }
}