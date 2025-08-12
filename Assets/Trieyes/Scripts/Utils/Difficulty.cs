using UnityEngine;

namespace Utils
{
    public class Difficulty
    {
        public float battleLength = 10; // 초단위

        public int EnemyID = 0;
        public int shopLevel = 1;

        public int stageNumber;
        public int spawnFrequency;
        public int enemyHpMultiplier;
        public int enemyAttackMultiplier;

        private int baseHp = 5;

        public float SpawnInterval => 1f / spawnFrequency;

        public static Difficulty GetByStageRound(int stageRound)
        {
            var difficulty = new Difficulty();
            difficulty.stageNumber = stageRound;
            difficulty.spawnFrequency = stageRound;
            difficulty.enemyHpMultiplier = (int)Mathf.Pow(5f, stageRound - 1);
            difficulty.enemyAttackMultiplier = stageRound;
            return difficulty;
        }
    }
}
