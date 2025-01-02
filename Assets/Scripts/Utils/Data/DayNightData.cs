using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Utils.Data
{
    public static class DayNightData
    {

        public struct DayWave
        {
            public int DurationInSeconds;
            public float SpawnDurationInSeconds;
            public EnemySpawns EnemySpawns;
        }

        public struct NightWave
        {
            public float SpawnDurationInSeconds;
            public EnemySpawns EnemySpawns;
        }
        
        public struct Cycle
        {
            public DayWave DayWave;
            public NightWave NightWave;
            public int[] NewCrops;
            public int[] NewMaterials;
        }

        private const int NumPredefinedCycles = 10;

        private static List<Cycle> DefineCycles()
        {
            var dayDurations = new int[NumPredefinedCycles];
            var daySpawnDurations = new float[NumPredefinedCycles];
            var nightSpawnDurations = new float[NumPredefinedCycles];
            for (int i = 0; i < NumPredefinedCycles; i++)
            {
                dayDurations[i] = Mathf.Max(Constants.FirstDayDurationInSeconds - Constants.DaySecondsReductionPerCycle * i, Constants.MinDayDurationInSeconds);
                daySpawnDurations[i] = dayDurations[i] - Constants.ChangeLightDurationInSeconds;
                nightSpawnDurations[i] = Constants.FirstNightDurationInSeconds - Constants.NightSecondsReductionPerCycle * i;
            }
            var daySpawns = new List<EnemySpawns>()
            {
                // new(Constants.FirstDayEnemies), 
                new(chickens:1), // todo new(), 
                new(chickens:1), // todo new(), 
                new(), new(), new(chickens:1),
                new(), new(chickens:2), new(chickens:0), new(chickens:1), new(chickens:3)
            };
            var nightSpawns = new List<EnemySpawns>()
            {
                new(slimes: 1),//Constants.FirstNightEnemies), 
                new(8, 1), 
                new(5, 4), 
                new(6, 5, 0, 0, 2), 
                new(5, 6, 0, 0, 5), 
                new(0, 8, 0, 2, 5), 
                new(30),
                new(0, 15, 0, 0, 3),
                new(10, 8, 6, 3, 6), 
                new(demons:1)
            };

            var w = Constants.StoreWheatAmountPerCycle;
            var c = Constants.StoreCarrotAmountPerCycle;
            var t = Constants.StoreTomatoAmountPerCycle;
            var o = Constants.StoreCornAmountPerCycle;
            var p = Constants.StorePumpkinAmountPerCycle;
            var newCrops = new List<int[]>()
            {
                new []{w, 0, 0, 0, 0},
                new []{w, c, 0, 0, 0},
                new []{w, c, 0, 0, 0},
                new []{w, c, t, 0, 0},
                new []{w, c, t, 0, 0},
                new []{w, c, t, o, 0},
                new []{w, c, t, o, 0},
                new []{w, c, t, o, 0},
                new []{w, c, t, o, p},
                new []{w, c, t, o, p},
            };
            var newMaterials = new List<int[]>()
            {
                new [] { 1, 0, 0, 0, 0 },
                new [] { 1, 1, 0, 0, 0 },
                new [] { 1, 1, 0, 0, 0 },
                new [] { 1, 1, 1, 0, 0 },
                new [] { 1, 1, 1, 0, 0 },
                new [] { 1, 1, 1, 1, 0 },
                new [] { 1, 1, 1, 1, 0 },
                new [] { 1, 1, 1, 1, 0 },
                new [] { 1, 1, 1, 1, 1 },
                new [] { 1, 1, 1, 1, 1 },
            };
            var cycles = new List<Cycle>();
            for (int i = 0; i < NumPredefinedCycles; i++)
            {
                cycles.Add(new Cycle
                {
                    DayWave = new DayWave
                    {
                        DurationInSeconds = dayDurations[i],
                        SpawnDurationInSeconds = daySpawnDurations[i],
                        EnemySpawns = daySpawns[i]
                    },
                    NightWave = new NightWave
                    {
                        SpawnDurationInSeconds = nightSpawnDurations[i],
                        EnemySpawns = nightSpawns[i]
                    },
                    NewCrops = newCrops[i],
                    NewMaterials = newMaterials[i]
                });
            }
            return cycles;
        }

        public static readonly List<Cycle> FirstCycles = DefineCycles();

        public static void WarmupCycle(int cycleNum, EnemySpawnData spawnData)
        {
            Cycle cycle;
            if (cycleNum < FirstCycles.Count)
                cycle = FirstCycles[cycleNum];
            else
            {
                var dayEnemySpawns = new EnemySpawns(chickens: UnityEngine.Random.Range(0, Mathf.FloorToInt(cycleNum / 2)));
                var nightEnemySpawns = new EnemySpawns();
                var enemyPowerPoints = Mathf.RoundToInt(Mathf.Pow(cycleNum, 2) / 2);
                while (enemyPowerPoints > 0)
                {
                    var (enemy, points) = spawnData.GetRandomEnemy(enemyPowerPoints);
                    enemyPowerPoints -= points;
                    nightEnemySpawns.AddEnemy(enemy);
                }

                cycle = new Cycle()
                {
                    DayWave = new DayWave()
                    {
                        DurationInSeconds = Constants.MinDayDurationInSeconds,
                        SpawnDurationInSeconds = Constants.MinDayDurationInSeconds - Constants.ChangeLightDurationInSeconds,
                        EnemySpawns = dayEnemySpawns
                    },
                    NightWave = new NightWave()
                    {
                        SpawnDurationInSeconds = Constants.MinNightDurationInSeconds,
                        EnemySpawns = nightEnemySpawns
                    },
                    NewCrops = new [] {
                        Constants.StoreWheatAmountPerCycle, 
                        Constants.StoreCarrotAmountPerCycle, 
                        Constants.StoreTomatoAmountPerCycle, 
                        Constants.StoreCornAmountPerCycle, 
                        Constants.StorePumpkinAmountPerCycle
                    },
                    NewMaterials = new [] {1, 1, 1, 1, 1}
                };
            }

            GameData.Instance.thisDayEnemies = cycle.DayWave.EnemySpawns.SpawnAmounts;
            GameData.Instance.thisNightEnemies = cycle.NightWave.EnemySpawns.SpawnAmounts;
        }
        
        public static Cycle GetCycle(int day, int[] dayEnemies, int[] nightEnemies)
        {
            if (day < FirstCycles.Count)
                return FirstCycles[day];
            var dayEnemySpawns = new EnemySpawns(dayEnemies);
            var nightEnemySpawns = new EnemySpawns(nightEnemies);
            for (int i = 0; i < dayEnemies.Length; i++)
            {
                dayEnemySpawns.AddEnemy((Enemy) i, dayEnemies[i]);
            }
            for (int i = 0; i < nightEnemies.Length; i++)
            {
                nightEnemySpawns.AddEnemy((Enemy) i, nightEnemies[i]);
            }

            var dayDuration = Mathf.Max(Constants.MinDayDurationInSeconds,
                Constants.FirstDayDurationInSeconds -
                Constants.DaySecondsReductionPerCycle * (day - NumPredefinedCycles));
            return new Cycle()
            {
                DayWave = new DayWave()
                {
                    DurationInSeconds = dayDuration,
                    SpawnDurationInSeconds = dayDuration - Constants.ChangeLightDurationInSeconds,
                    EnemySpawns = dayEnemySpawns
                },
                NightWave = new NightWave()
                {
                    SpawnDurationInSeconds = Mathf.Max(Constants.MinNightDurationInSeconds,
                        Constants.FirstNightDurationInSeconds -
                        Constants.NightSecondsReductionPerCycle * (day - NumPredefinedCycles)),
                    EnemySpawns = nightEnemySpawns
                },
                NewCrops = new [] {
                    Constants.StoreWheatAmountPerCycle, 
                    Constants.StoreCarrotAmountPerCycle, 
                    Constants.StoreTomatoAmountPerCycle, 
                    Constants.StoreCornAmountPerCycle, 
                    Constants.StorePumpkinAmountPerCycle
                },
                NewMaterials = new [] {1, 1, 1, 1, 1}
            };
        }
    }
}