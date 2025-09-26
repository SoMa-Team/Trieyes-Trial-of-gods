using UnityEngine;
using CharacterSystem;
using Stats;
using System.Collections.Generic;
using GameFramework;

namespace BattleSystem
{
    /// <summary>
    /// 난이도에 따라 적 스폰을 관리하는 매니저 클래스
    /// EnemyFactory를 통해 적을 생성하고 BattleStage에 연결합니다.
    /// </summary>
    public class SpawnManager : MonoBehaviour
    {
        // ===== 싱글톤 =====
        public static SpawnManager Instance { private set; get; }

        // ===== 스폰 장소 설정 =====
        // public GameObject[] spawnPoints;
        
        // ===== 내부 상태 =====
        private bool _isActivate = false;
        private Difficulty _difficulty;
        private bool isBossSpawned = false;

        [Header("스폰 확률")]
        
        [SerializeField] private float _elapsedTime;
        [HideInInspector] private Vector2 TopLeft;
        [HideInInspector] private Vector2 BottomRight;
        [SerializeField] private float minDistance;
        [SerializeField] private float maxDistance;

        [SerializeField] private float spawnPointCheckInterval = 1f;

        [Header("스폰 마릿 수 계수")]
        [SerializeField] private int SpawnCountMin = 0;
        [SerializeField] private int SpawnCountMax = 3;

        [Header("4사분면 균형 추적 시스템")]
        private float lastSpawnPointCheckTime = 0f;

        // ex) 3f = 3배 더 빠르게 스폰
        public float SpawnIntervalMultiplier = 1f;
        
        // ===== 4사분면 균형 추적 시스템 =====
        private float[] quadrantSpawnProbabilities = { 0.25f, 0.25f, 0.25f, 0.25f };  // 각 사분면 스폰 확률

        // ===== 초기화 =====
        
        /// <summary>
        /// 싱글톤 패턴을 위한 초기화
        /// 중복 인스턴스가 생성되지 않도록 합니다.
        /// </summary>
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ===== 스폰 로직 =====
        /// <summary>
        /// 매 프레임마다 스폰 조건을 확인하고 적을 생성합니다.</summary>
        private void Update()
        {
            if (!_isActivate)
                return;

            // 4사분면 균형 체크
            CheckEnemyBalance();
            
            // 스폰 로직 실행
            CheckAndSpawnEnemy();
        }
        
        /// <summary>
        /// 스폰 매니저를 활성화합니다.
        /// BattleStage가 존재할 경우에만 동작합니다.
        /// </summary>
        /// <param name="difficulty">스폰 난이도 설정</param>
        public void Activate(Difficulty difficulty)
        {
            _isActivate = true;
            _difficulty = difficulty;
            _elapsedTime = 0f;
            isBossSpawned = false;

            // BattleStage와 View가 존재할 때만 스폰 범위 설정
            if (BattleStage.now != null && BattleStage.now.View != null)
            {
                GetMinMaxPosition(BattleStage.now.View);
            }
            else
            {
                TopLeft = new Vector2(-10f, 10f);
                BottomRight = new Vector2(10f, -10f);
            }
        }

        /// <summary>
        /// 스폰 매니저를 비활성화합니다.</summary>
        public void Deactivate()
        {
            _isActivate = false;
            _difficulty = null;
            isBossSpawned = false;
        }
        

        // ===== 4사분면 균형 추적 시스템 =====
        
        /// <summary>
        /// 적들의 4사분면 균형을 체크하고 스폰 확률을 업데이트합니다.
        /// </summary>
        private void CheckEnemyBalance()
        {
            if (Time.time - lastSpawnPointCheckTime < spawnPointCheckInterval)
                return;
                
            lastSpawnPointCheckTime = Time.time;
            
            if (BattleStage.now?.mainCharacter == null)
            {
                // 기본 균등 확률로 초기화
                for (int i = 0; i < 4; i++)
                    quadrantSpawnProbabilities[i] = 0.25f;
                return;
            }
            
            // 적 분포 계산 및 확률 업데이트를 한 번에 처리
            Vector3 playerPos = BattleStage.now.mainCharacter.transform.position;
            int[] quadrantCounts = { 0, 0, 0, 0 }; // 1사분면, 2사분면, 3사분면, 4사분면
            
            foreach (var enemyPair in BattleStage.now.enemies)
            {
                var enemy = enemyPair.Value;
                if (enemy == null || enemy.isDead) continue;
                
                Vector3 enemyPos = enemy.transform.position;
                Vector3 relativePos = enemyPos - playerPos;
                
                // 4사분면 분류
                int quadrant = GetQuadrant(relativePos);
                quadrantCounts[quadrant]++;
            }
            
            // 스폰 확률 계산 (적이 적은 사분면에 높은 확률)
            int totalEnemies = quadrantCounts[0] + quadrantCounts[1] + quadrantCounts[2] + quadrantCounts[3];
            
            if (totalEnemies == 0)
            {
                // 적이 없으면 균등 확률
                for (int i = 0; i < 4; i++)
                    quadrantSpawnProbabilities[i] = 0.25f;
            }
            else
            {
                // 각 사분면의 적 수에 반비례하는 확률 계산
                float totalInverse = 0f;
                float[] inverseCounts = new float[4];
                
                for (int i = 0; i < 4; i++)
                {
                    inverseCounts[i] = 1f / (quadrantCounts[i] + 1f); // +1로 0으로 나누기 방지
                    totalInverse += inverseCounts[i];
                }
                
                for (int i = 0; i < 4; i++)
                {
                    quadrantSpawnProbabilities[i] = inverseCounts[i] / totalInverse;
                }
            }
        }
        
        /// <summary>
        /// 상대 위치를 기반으로 사분면을 반환합니다.
        /// </summary>
        /// <param name="relativePos">플레이어를 기준으로 한 상대 위치</param>
        /// <returns>사분면 인덱스 (0: 1사분면, 1: 2사분면, 2: 3사분면, 3: 4사분면)</returns>
        private int GetQuadrant(Vector3 relativePos)
        {
            if (relativePos.x >= 0 && relativePos.y >= 0) return 0; // 1사분면 (우상)
            if (relativePos.x < 0 && relativePos.y >= 0) return 1;  // 2사분면 (좌상)
            if (relativePos.x < 0 && relativePos.y < 0) return 2;   // 3사분면 (좌하)
            return 3; // 4사분면 (우하)
        }

        // ===== 내부 헬퍼 =====
        private void GetMinMaxPosition(BattleStageView view)
        {
            if (view?.TopLeft != null && view?.BottomRight != null)
            {
                TopLeft = view.TopLeft;
                BottomRight = view.BottomRight;
            }
            else
            {
                // 기본값 설정
                TopLeft = new Vector2(-10f, 10f);
                BottomRight = new Vector2(10f, -10f);
            }
        }
        
        /// <summary>
        /// 랜덤한 스폰 포인트를 반환합니다.</summary>
        /// <returns>선택된 스폰 포인트 GameObject</returns>
        private Vector3 GetRandomSpawnPoint()
        {
            return GetRandomPointAround(BattleStage.now.mainCharacter, minDistance, maxDistance);
        }

        private Vector3 GetRandomSpawnPoint(float minDist, float maxDist)
        {
            return GetRandomPointAround(BattleStage.now.mainCharacter, minDist, maxDist);
        }
        
        private Vector3 GetRandomPointAround(Pawn pawn, float minDistance, float maxDistance)
        {
            Vector2 center = pawn.transform.position;
            Vector2 spawnPos = GetRandomSpawnPointInBounds(center, minDistance, maxDistance);
            return spawnPos;
        }
        
        /// <summary>
        /// 4사분면 균형을 고려하여 TopLeft와 BottomRight 범위 내에서 랜덤 스폰 포인트를 생성합니다.
        /// </summary>
        private Vector2 GetRandomSpawnPointInBounds(Vector2 center, float minDistance, float maxDistance)
        {
            const int maxAttempts = 20; // 시도 횟수 감소
            
            for (int attempts = 0; attempts < maxAttempts; attempts++)
            {
                // 4사분면 균형을 고려한 스폰 방향 결정
                int selectedQuadrant = SelectQuadrantByProbability();
                
                // 선택된 사분면에 따른 각도 범위 설정
                float angle = GetRandomAngleInQuadrant(selectedQuadrant);
                
                float distance = Random.Range(minDistance, maxDistance);
                Vector2 spawnPos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                
                if (IsPointInBounds(spawnPos))
                    return spawnPos;
            }
            
            // 실패 시 범위 중앙에 스폰
            return new Vector2((TopLeft.x + BottomRight.x) * 0.5f, (TopLeft.y + BottomRight.y) * 0.5f);
        }
        
        /// <summary>
        /// 확률에 따라 사분면을 선택합니다.
        /// </summary>
        /// <returns>선택된 사분면 인덱스</returns>
        private int SelectQuadrantByProbability()
        {
            float randomValue = Random.Range(0f, 1f);
            float cumulativeProbability = 0f;
            
            for (int i = 0; i < 4; i++)
            {
                cumulativeProbability += quadrantSpawnProbabilities[i];
                if (randomValue <= cumulativeProbability)
                    return i;
            }
            
            return 3; // 기본값 (4사분면)
        }
        
        /// <summary>
        /// 지정된 사분면 내에서 랜덤 각도를 반환합니다.
        /// </summary>
        /// <param name="quadrant">사분면 인덱스 (0: 1사분면, 1: 2사분면, 2: 3사분면, 3: 4사분면)</param>
        /// <returns>라디안 각도</returns>
        private float GetRandomAngleInQuadrant(int quadrant)
        {
            switch (quadrant)
            {
                case 0: // 1사분면 (우상): 0° ~ 90°
                    return Random.Range(0f, 90f) * Mathf.Deg2Rad;
                case 1: // 2사분면 (좌상): 90° ~ 180°
                    return Random.Range(90f, 180f) * Mathf.Deg2Rad;
                case 2: // 3사분면 (좌하): 180° ~ 270°
                    return Random.Range(180f, 270f) * Mathf.Deg2Rad;
                case 3: // 4사분면 (우하): 270° ~ 360°
                    return Random.Range(270f, 360f) * Mathf.Deg2Rad;
                default:
                    return Random.Range(0f, 360f) * Mathf.Deg2Rad;
            }
        }
        
        /// <summary>
        /// 주어진 포인트가 TopLeft와 BottomRight 범위 내에 있는지 확인합니다.
        /// </summary>
        /// <param name="point">확인할 포인트</param>
        /// <returns>범위 내에 있으면 true</returns>
        private bool IsPointInBounds(Vector2 point)
        {
            return point.x >= TopLeft.x && point.x <= BottomRight.x &&
                   point.y >= BottomRight.y && point.y <= TopLeft.y;
        }

        private bool CheckAndSpawnEnemy()
        {
            if (_difficulty.spawnMode == SpawnMode.Once)
            {
                if (isBossSpawned)
                    return false;
                
                isBossSpawned = true;
                SpawnEnemy(1);
                return true;
            }
            
            _elapsedTime += Time.deltaTime;

            // 배속 적용된 실제 스폰 간격 계산
            float actualSpawnInterval = _difficulty.SpawnInterval / SpawnIntervalMultiplier;
            
            if (_elapsedTime >= actualSpawnInterval)
            {
                // 경과 시간 동안 몇 번 스폰해야 하는지 계산
                var spawnCount = (int)(_elapsedTime / actualSpawnInterval);
                
                // 남은 시간 계산 (정확한 시간 관리)
                _elapsedTime = _elapsedTime % actualSpawnInterval;

                SpawnEnemy(spawnCount);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 난이도에 따라 적을 생성합니다.</summary>
        public void SpawnEnemy(int count=1, float minDist=0f, float maxDist=0f)
        {
            for (int i = 0; i < count + Random.Range(SpawnCountMin, SpawnCountMax); i++)
            {
                var enemy = EnemyFactory.Instance.Create(_difficulty.EnemyID);
                enemy.statSheet[StatType.AttackPower].MultiplyToBasicValue(_difficulty.enemyAttackMultiplier);
                enemy.statSheet[StatType.Health].MultiplyToBasicValue(_difficulty.enemyHpMultiplier);
                enemy.SyncHP();

                if (minDist != 0f && maxDist != 0f)
                {
                    BattleStage.now.AttachEnemy(enemy, GetRandomSpawnPoint(minDist, maxDist));
                }
                else
                {
                    BattleStage.now.AttachEnemy(enemy, GetRandomSpawnPoint());
                }
            }
        }
    }
} 