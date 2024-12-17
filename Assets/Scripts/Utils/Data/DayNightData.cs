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
                daySpawnDurations[i] = dayDurations[i] * (1 - DayNightRollBehaviour.Instance.ChangeLightProgressDuration);
                nightSpawnDurations[i] = Constants.FirstNightDurationInSeconds - Constants.NightSecondsReductionPerCycle * i;
            }
            var daySpawns = new List<EnemySpawns>()
            {
                new(), new(), new(), new(), new(chickens:1),
                new(), new(chickens:2), new(chickens:0), new(chickens:1), new(chickens:3)
            };
            var nightSpawns = new List<EnemySpawns>()
            {
                new(5), 
                new(8, 1), 
                new(5, 4), 
                new(6, 5, 2), 
                new(5, 6, 5), 
                new(0, 8, 5, 0, 2), 
                new(30),
                new(0, 15, 3),
                new(10, 10, 10, 10, 10), 
                new(demons:1)
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
                    }
                });
            }
            return cycles;
        }

        private static readonly List<Cycle> FirstCycles = DefineCycles();

        public static Cycle GetCycle(int cycleNum, EnemySpawnData spawnData) // should be called in coroutine to avoid lag
        {
            if (cycleNum < FirstCycles.Count)
                return FirstCycles[cycleNum];

            var dayEnemySpawns = new EnemySpawns(chickens: UnityEngine.Random.Range(0, cycleNum));
            var nightEnemySpawns = new EnemySpawns();
            var enemyPowerPoints = Mathf.RoundToInt(Mathf.Pow(cycleNum, 2) / 4);
            while (enemyPowerPoints > 0)
            {
                var (enemy, points) = spawnData.GetRandomEnemy(enemyPowerPoints);
                enemyPowerPoints -= points;
                nightEnemySpawns.AddEnemy(enemy);
            }

            return new Cycle()
            {
                DayWave = new DayWave()
                {
                    DurationInSeconds = Constants.MinDayDurationInSeconds,
                    SpawnDurationInSeconds = Constants.MinDayDurationInSeconds * (1 - DayNightRollBehaviour.Instance.ChangeLightProgressDuration),
                    EnemySpawns = dayEnemySpawns
                },
                NightWave = new NightWave()
                {
                    SpawnDurationInSeconds = Constants.MinNightDurationInSeconds,
                    EnemySpawns = nightEnemySpawns
                }
            };
        }
    }
}