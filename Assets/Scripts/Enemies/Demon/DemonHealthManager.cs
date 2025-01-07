using UnityEngine;

namespace Enemies.Demon
{
    public class DemonHealthManager: EnemyHealthManager
    {
        [SerializeField] private EvilBallManager evilBallManager;

        public override void TakeDamage(int damage, Vector2? hitDirection = null, bool tower = false)
        {
            base.TakeDamage(damage, hitDirection, tower);
            if (!tower)
                evilBallManager.curTimeToSpawnNewBall = 0;
        }
    }
}