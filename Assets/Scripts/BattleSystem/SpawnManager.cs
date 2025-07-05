using UnityEngine;
using Utils;
using CharacterSystem;

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
        private float _elapsedTime;

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
                    var spawnPoint = GetRandomSpawnPoint();
                    var enemy = SpawnEnemy();
                    BattleStage.now.AttachEnemy(enemy, spawnPoint.transform);
                }
            }
        }

        // ===== 내부 헬퍼 =====
        
        /// <summary>
        /// 랜덤한 스폰 포인트를 반환합니다.</summary>
        /// <returns>선택된 스폰 포인트 GameObject</returns>
        private GameObject GetRandomSpawnPoint()
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        /// <summary>
        /// 난이도에 따라 적을 생성합니다.</summary>
        /// <returns>생성된 적 Pawn</returns>
        private Pawn SpawnEnemy()
        {
            var enemy = EnemyFactory.Instance.Create(_difficulty.EnemyID);
            return enemy;
        }
    }
} 