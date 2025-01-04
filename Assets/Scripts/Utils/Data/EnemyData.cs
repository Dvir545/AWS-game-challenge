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
        public float pushForceMultiplier;
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
                knockbackForceMultiplier = .5f,
                towerDamageMultiplier = 2/3f,
                pushForceMultiplier = 1
            }},
            {Enemy.Skeleton, new EnemyInfo
            {
                maxHealth = 10,
                damageMultiplier = 1,
                cashDrop = 10,
                speed = 1,
                knockbackForceMultiplier = 1,
                towerDamageMultiplier = 2/3f,
                pushForceMultiplier = 1
            }},
            {Enemy.Chicken, new EnemyInfo
            {
                maxHealth = 8,
                damageMultiplier = 1,
                cashDrop = 25,
                speed = 2,
                knockbackForceMultiplier = 1,
                towerDamageMultiplier = 2/3f,
                pushForceMultiplier = 1
            }},
            { Enemy.Orc, new EnemyInfo
            {
                maxHealth = 25,
                damageMultiplier = 1,
                cashDrop = 30,
                speed = 0.75f,
                knockbackForceMultiplier = .5f,
                towerDamageMultiplier = 1/3f,
                pushForceMultiplier = 1.2f
            }},
            { Enemy.Goblin, new EnemyInfo
            {
                maxHealth = 10,
                damageMultiplier = 1,
                cashDrop = 20,
                speed = 1.5f,
                knockbackForceMultiplier = 1,
                towerDamageMultiplier = 2/3f,
                pushForceMultiplier = 1
            }},
            { Enemy.Demon, new EnemyInfo
            {
                maxHealth = 50,
                damageMultiplier = 1,
                cashDrop = 100,
                speed = .6f,
                knockbackForceMultiplier = -.2f,
                towerDamageMultiplier = 1/5f,
                pushForceMultiplier = 4
            }},
            { Enemy.EvilBall, new EnemyInfo
            {
                maxHealth = 1,
                damageMultiplier = 1,
                cashDrop = 0,
                speed = 1f,
                knockbackForceMultiplier = 0,
                towerDamageMultiplier = 0,
                pushForceMultiplier = 2
            }}
        };

        public static int GetMaxHealth(Enemy enemy) => Instance._enemies[enemy].maxHealth;
        public static int GetDamageMultiplier(Enemy enemy) => Instance._enemies[enemy].damageMultiplier;
        public static int GetCashDrop(Enemy enemy) => Instance._enemies[enemy].cashDrop;
        public static float GetSpeed(Enemy enemy) => Instance._enemies[enemy].speed;
        public static float GetKnockbackForceMultiplier(Enemy enemy) =>
            Instance._enemies[enemy].knockbackForceMultiplier;

        public static float GetTowerDamageMultiplier(Enemy enemy) => Instance._enemies[enemy].towerDamageMultiplier;
        public static float GetPushForceMultiplier(Enemy enemy) => Instance._enemies[enemy].pushForceMultiplier;
    }
}