using UnityEngine;

namespace Utils
{
    public enum SpawnMode
    {
        Frequency,
        Once
    }
    public class Difficulty
    {
        public float battleLength = 1000; // 초단위

        public int EnemyID = 0;
        public int shopLevel = 1;

        public SpawnMode spawnMode = SpawnMode.Frequency;
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
            difficulty.enemyHpMultiplier = (int)Mathf.Pow(5f, (stageRound - 1) / 3f);
            difficulty.enemyAttackMultiplier = stageRound;
            
            difficulty.EnemyID = 5; // TODO : Boss 설계 이후 삭제해야함. PR 리뷰시 보이면 말해주세요.!!!!!!!
            difficulty.spawnFrequency = 0;
            difficulty.spawnMode = SpawnMode.Once;
            
            return difficulty;
        }
    }
}
