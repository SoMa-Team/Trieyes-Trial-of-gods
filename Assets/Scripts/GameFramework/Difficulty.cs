using UnityEngine;

namespace GameFramework
{
    public class Difficulty
    {
        // 난이도 관련 속성 (예: 스탯 배율, 적 생성 빈도 등)
        public float enemyStatMultiplier = 1.0f;
        public int spawnFrequency = 1;

        // 난이도에 따른 특정 로직을 정의할 메서드
        public virtual void ApplyDifficultyEffects()
        {
            Debug.Log("Applying base difficulty effects.");
        }
    }
} 