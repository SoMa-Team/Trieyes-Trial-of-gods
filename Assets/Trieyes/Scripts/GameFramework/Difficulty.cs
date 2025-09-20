using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFramework
{
    public enum SpawnMode
    {
        Frequency,
        Once
    }
    public class Difficulty
    {
        public float battleLength = 60; // 초단위

        private int[] enemyIDs = { 0, 1, 2, 3, 4 };

        private const int BOSS_ENEMY_ID_START = 2;
        public int EnemyID => getEnemyID();
        public int shopLevel = 1;

        private int LevelCount { get; set; }
        private int RoundCount { get; set; }

        public SpawnMode spawnMode = SpawnMode.Frequency;
        public int stageNumber;

        public int enemyHpMultiplier;
        public int enemyAttackMultiplier;

        public float SpawnInterval => 1f;

        public void RoundCountUp()
        {
            RoundCount++;
            UpdateDifficultyByRoundCount();
        }

        private void UpdateDifficultyByRoundCount()
        {
            // Rond Conunt가 0이면 보스 모드
            if (RoundCount == 0)
            {
                spawnMode = SpawnMode.Once;
            }
            else
            {
                spawnMode = SpawnMode.Frequency;
            }

            enemyHpMultiplier = 1;
            enemyAttackMultiplier = 1;
        }

        public void LevelCountUp()
        {
            LevelCount++;
            RoundCount = 0;
            UpdateDifficultyByLevelCount();
        }

        private int getEnemyID()
        {
            if (spawnMode == SpawnMode.Once)
            {
                // BOSS_ENEMY_ID_START이상 enemyIDs.Length 미만 사이의 랜덤 숫자 생성
                return enemyIDs[Random.Range(BOSS_ENEMY_ID_START, enemyIDs.Length)];
            }
            else
            {
                // 0 이상 BOSS_ENEMY_ID_START 미만 사이의 랜덤 숫자 생성
                return enemyIDs[Random.Range(0, BOSS_ENEMY_ID_START)];
            }
        }

        private void UpdateDifficultyByLevelCount()
        {
            // 이 함수는 오직 보스 클리어 이후에만 호출되고, 레벨에 따른 큰 난이도 조절을 수행하는 함수
            spawnMode = SpawnMode.Frequency;
            
            enemyHpMultiplier *= 2;
            enemyAttackMultiplier *= 2;

            Debug.Log($"LevelCount: {LevelCount}, enemyHpMultiplier: {enemyHpMultiplier}, enemyAttackMultiplier: {enemyAttackMultiplier}");
        }

        internal int GetBreakThroughCount()
        {
            // RoundCount와 LevelCount를 이용하여 breakThroughCount를 계산
            return 10 * RoundCount + 100 * (LevelCount - 1);
        }

        internal void GameStart()
        {
            RoundCount = 1;
            LevelCount = 1;
            UpdateDifficultyByRoundCount();
        }
    }
}