using System.Collections.Generic;
using UnityEngine;

namespace Utils.Data
{
    struct EnemyInfo
    {
        public int maxHealth;
        public int damageMultiplier;
        public int cashDrop;
        public float speed;
        public float knockbackForceMultiplier;
        public float towerDamageMultiplier;
    }
    
    public class EnemyData: Singleton<EnemyData>
    {
        private Dictionary<Enemy, EnemyInfo> _enemies = new() {
            {Enemy.Slime, new EnemyInfo
            {
                maxHealth = 5,
                damageMultiplier = 1,
                cashDrop = 5,
                speed = 1,
                knockbackForceMultiplier = .3f,
                towerDamageMultiplier = 1
            }},
            {Enemy.Skeleton, new EnemyInfo
            {
                maxHealth = 10,
                damageMultiplier = 1,
                cashDrop = 10,
                speed = 1,
                knockbackForceMultiplier = 1,
                towerDamageMultiplier = 1
            }},
            {Enemy.Chicken, new EnemyInfo
            {
                maxHealth = 8,
                damageMultiplier = 1,
                cashDrop = 25,
                speed = 2,
                knockbackForceMultiplier = 1,
                towerDamageMultiplier = 1
            }},
            { Enemy.Orc , new EnemyInfo
            {
                maxHealth = 25,
                damageMultiplier = 1,
                cashDrop = 25,
                speed = 0.75f,
                knockbackForceMultiplier = .5f,
                towerDamageMultiplier = 1/3f
            }}
        };

        public static int GetMaxHealth(Enemy enemy) => Instance._enemies[enemy].maxHealth;
        public static int GetDamageMultiplier(Enemy enemy) => Instance._enemies[enemy].damageMultiplier;
        public static int GetCashDrop(Enemy enemy) => Instance._enemies[enemy].cashDrop;
        public static float GetSpeed(Enemy enemy) => Instance._enemies[enemy].speed;
        public static float GetKnockbackForceMultiplier(Enemy enemy) =>
            Instance._enemies[enemy].knockbackForceMultiplier;

        public static float GetTowerDamageMultiplier(Enemy enemy) => Instance._enemies[enemy].towerDamageMultiplier;
    }
}