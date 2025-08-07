using UnityEngine;
using Utils;
using CharacterSystem;
using Stats;
using System.Collections.Generic;

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
        public GameObject[] spawnPoints;
        
        // ===== 내부 상태 =====
        private bool _isActivate = false;
        private Difficulty _difficulty;
        private List<float> validAngles = new();
        [SerializeField] private float _elapsedTime;
        [SerializeField] private float minDistance;
        [SerializeField] private float maxDistance;
        [SerializeField] private Vector2 mapMin;
        [SerializeField] private Vector2 mapMax;

        // ===== 초기화 =====
        
        /// <summary>
        /// 싱글톤 패턴을 위한 초기화
        /// 중복 인스턴스가 생성되지 않도록 합니다.
        /// </summary>
        void Awake()
        {
            if (Instance is not null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        // ===== 활성화/비활성화 =====
        
        /// <summary>
        /// 스폰 매니저를 활성화합니다.
        /// BattleStage가 존재할 경우에만 동작합니다.
        /// </summary>
        /// <param name="difficulty">스폰 난이도 설정</param>
        public void Activate(Difficulty difficulty)
        {
            _isActivate = true;
            this._difficulty = difficulty;
            _elapsedTime = 0f;
        }
        
        /// <summary>
        /// 스폰 매니저를 비활성화합니다.</summary>
        public void Deactivate()
        {
            _isActivate = false;
            _difficulty = null;
        }
        
        // ===== 스폰 로직 =====
        
        /// <summary>
        /// 매 프레임마다 스폰 조건을 확인하고 적을 생성합니다.</summary>
        private void Update()
        {
            if (!_isActivate)
                return;
            
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= _difficulty.SpawnInterval)
            {
                var spawnCount = (int)(_elapsedTime / _difficulty.SpawnInterval); 
                _elapsedTime %= _difficulty.SpawnInterval;

                for (int i = 0; i < spawnCount; i++)
                {
                    var enemy = SpawnEnemy();
                    BattleStage.now.AttachEnemy(enemy, GetRandomSpawnPoint());
                }
            }
        }

        // ===== 내부 헬퍼 =====
        
        /// <summary>
        /// 랜덤한 스폰 포인트를 반환합니다.</summary>
        /// <returns>선택된 스폰 포인트 GameObject</returns>
        private Vector3 GetRandomSpawnPoint()
        {
            return GetRandomPointAround(BattleStage.now.mainCharacter, minDistance, maxDistance);
        }
        
        private Vector3 GetRandomPointAround(Pawn pawn, float minDistance, float maxDistance)
        {
            Vector2 center = pawn.transform.position;
            ComputeValidAngles(center, maxDistance);
            
            float angle = validAngles[Random.Range(0, validAngles.Count)];
            float rad = angle * Mathf.Deg2Rad;
            float distance = Random.Range(minDistance, maxDistance);
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector2 spawnPos = center + dir * distance;
            return spawnPos;
        }
        
        private void ComputeValidAngles(Vector2 center, float maxDistance, float angleStep = 1f)
        {
            validAngles.Clear();
            for (float angle = 0f; angle < 360f; angle += angleStep)
            {
                float rad = angle * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                Vector2 point = center + dir * maxDistance;
                if (point.x >= mapMin.x && point.x <= mapMax.x && point.y >= mapMin.y && point.y <= mapMax.y)
                    validAngles.Add(angle);
            }
        }

        /// <summary>
        /// 난이도에 따라 적을 생성합니다.</summary>
        /// <returns>생성된 적 Pawn</returns>
        private Enemy SpawnEnemy()
        {
            var enemy = EnemyFactory.Instance.Create(0);
            // var enemy = EnemyFactory.Instance.Create(_difficulty.EnemyID);
            enemy.statSheet[StatType.AttackPower].MultiplyToBasicValue(_difficulty.enemyAttackMultiplier);
            enemy.statSheet[StatType.Health].MultiplyToBasicValue(_difficulty.enemyHpMultiplier);
            enemy.SyncHP();
            Debug.Log($"Enemy HP: {enemy.statSheet[StatType.Health].Value}, Enemy Attack: {enemy.statSheet[StatType.AttackPower].Value}");
            return enemy;
        }
    }
} 