using GamePlayer;
using UnityEngine;

namespace BattleSystem
{
    public enum SpawnMode
    {
        Frequency,
        Once
    }
    public class Difficulty
    {
        public float battleLength = 60; // 초단위

        public int EnemyID = 0;
        public int shopLevel = 1;

        public SpawnMode spawnMode = SpawnMode.Frequency;
        public int stageNumber;
        public int spawnFrequency;
        public int enemyHpMultiplier;
        public int enemyAttackMultiplier;

        public float SpawnInterval => 1f / spawnFrequency;

        public static Difficulty GetByStageRound(int stageRound, bool isBoss = false)
        {
            var difficulty = new Difficulty();
            
            difficulty.stageNumber = stageRound;
            difficulty.spawnFrequency = stageRound;
            difficulty.enemyHpMultiplier = (int)Mathf.Pow(1.2f, (stageRound - 1) / 3f);
            difficulty.enemyAttackMultiplier = stageRound;

            if (isBoss)
            {
                difficulty.EnemyID = 5;
                difficulty.battleLength = 120;
                difficulty.spawnFrequency = 0;
                difficulty.spawnMode = SpawnMode.Once;
            }
            
            return difficulty;
        }
    }
}
