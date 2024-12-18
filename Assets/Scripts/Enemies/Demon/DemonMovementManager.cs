using UnityEngine;

namespace Enemies.Demon
{
    public class DemonMovementManager: EnemyMovementManager
    {
        [SerializeField] private EvilBallManager evilBallManager;
        public override void Reset()
        {
            base.Reset();
            evilBallManager.Init();
        }
    }
}