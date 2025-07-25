using UnityEngine;

namespace Utils
{
    public class Difficulty
    {
        // ===== [기능 1] 난이도 정보 =====
        public int spawnFrequency => 1;
        public float enemyStatMultiplier = 1.0f;
        public float battleLength => 120; // 초단위
        public int EnemyID = 0;
        public int shopLevel = 1;
        public int stageNumber => 12;

        public float SpawnInterval => 1f / spawnFrequency;

        // ===== [기능 2] 난이도 효과 적용 =====
        public virtual void ApplyDifficultyEffects()
        {
            Debug.Log("Applying base difficulty effects.");
        }

        public static Difficulty GetByStageRound(int stageRound)
        {
            // TODO : 난이도 설정 로직 추가 필요
            return new Difficulty();
        }
    }
}
