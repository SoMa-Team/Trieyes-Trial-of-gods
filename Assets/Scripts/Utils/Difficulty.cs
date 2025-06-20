using UnityEngine;

namespace Utils
{
    public class Difficulty
    {
        // ===== [기능 1] 난이도 정보 =====
        public float enemyStatMultiplier = 1.0f;
        public int spawnFrequency = 1;
        // ===== [기능 2] 난이도 효과 적용 =====
        public virtual void ApplyDifficultyEffects()
        {
            Debug.Log("Applying base difficulty effects.");
        }

        public static Difficulty GetByStageRound(int stageRound)
        {
            return new Difficulty();
        }
    }
}