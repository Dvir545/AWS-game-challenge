using System.Collections.Generic;

namespace Utils.Data
{
    struct EnemyInfo
    {
        public int maxHealth;
        public int damageMultiplier;
        public int cashDrop;
    }
    
    public class EnemyData: Singleton<EnemyData>
    {
        private Dictionary<Enemy, EnemyInfo> _enemies = new() {
            {Enemy.Slime, new EnemyInfo
            {
                maxHealth = 5,
                damageMultiplier = 1,
                cashDrop = 5
            }},
            {Enemy.Skeleton, new EnemyInfo
            {
                maxHealth = 2,  // todo 10
                damageMultiplier = 1,
                cashDrop = 10
            }}
        };

        public static int GetMaxHealth(Enemy enemy) => Instance._enemies[enemy].maxHealth;
        public static int GetDamageMultiplier(Enemy enemy) => Instance._enemies[enemy].damageMultiplier;
        public static int GetCashDrop(Enemy enemy) => Instance._enemies[enemy].cashDrop;
    }
}