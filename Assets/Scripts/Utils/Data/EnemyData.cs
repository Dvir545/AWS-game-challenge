using System.Collections.Generic;

namespace Utils.Data
{
    struct EnemyInfo
    {
        public int maxHealth;
        public int damageMultiplier;
        public int cashDrop;
        public float speed;
    }
    
    public class EnemyData: Singleton<EnemyData>
    {
        private Dictionary<Enemy, EnemyInfo> _enemies = new() {
            {Enemy.Slime, new EnemyInfo
            {
                maxHealth = 5,
                damageMultiplier = 1,
                cashDrop = 5,
                speed = 1
            }},
            {Enemy.Skeleton, new EnemyInfo
            {
                maxHealth = 10,
                damageMultiplier = 1,
                cashDrop = 10,
                speed = 1
            }},
            {Enemy.Chicken, new EnemyInfo
            {
                maxHealth = 8,
                damageMultiplier = 1,
                cashDrop = 25,
                speed = 2
            }}
        };

        public static int GetMaxHealth(Enemy enemy) => Instance._enemies[enemy].maxHealth;
        public static int GetDamageMultiplier(Enemy enemy) => Instance._enemies[enemy].damageMultiplier;
        public static int GetCashDrop(Enemy enemy) => Instance._enemies[enemy].cashDrop;

        public static float GetSpeed(Enemy enemy)
        {
            return Instance._enemies[enemy].speed;
        }
    }
}