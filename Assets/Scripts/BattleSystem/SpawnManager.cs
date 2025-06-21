using UnityEngine;
using System.Collections.Generic;
using Utils;
using CharacterSystem;

namespace BattleSystem
{
    public class SpawnManager : MonoBehaviour
    {
        // ===== [기능 1] 싱글톤 패턴 =====
        public static SpawnManager Instance { get; private set; }

        [Header("Spawn Settings")]
        public List<Spawner> spawners = new List<Spawner>(); // 스폰 지점 목록
        public Difficulty difficulty; // 현재 전투 난이도
        public Dictionary<string, GameObject> prefabIDs = new Dictionary<string, GameObject>(); // ID에 따른 프리팹 맵핑 (에디터에서 할당하기 어려우므로 예시용)

        [Header("Enemy Prefabs")]
        public List<GameObject> enemyPrefabsToSpawn; // 실제 에디터에서 할당할 적 프리팹 리스트

        private void Awake()
        {
            Activate();
        }

        private void OnDestroy()
        {
            Deactivate();
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        public virtual void Activate()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public virtual void Deactivate()
        {
            // 리스트 초기화
            spawners.Clear();
            enemyPrefabsToSpawn.Clear();
            prefabIDs.Clear();
            
            // 싱글톤 참조 정리
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 주어진 프리팹을 사용하여 오브젝트를 생성하거나 풀에서 불러와 활성화합니다.
        /// </summary>
        /// <param name="prefab">생성할 GameObject 프리팹</param>
        /// <returns>생성되거나 활성화된 GameObject 인스턴스</returns>
        public GameObject Create(GameObject prefab)
        {
            GameObject instance = null;
            // 여기서는 풀링 로직을 가정합니다.
            // 풀에서 오브젝트를 가져오거나 새로 Instantiate 후 활성화 (SetActive(true))
            instance = Instantiate(prefab); // 예시: 일단 Instantiate로 구현
            instance.SetActive(true); // 활성화

            Debug.Log($"SpawnManager: Created/Activated {instance.name}.");
            return instance;
        }

        /// <summary>
        /// 모든 스포너를 사용하여 적을 스폰하는 메서드 (예시)
        /// </summary>
        public void SpawnEnemies()
        {
            foreach (var spawner in spawners)
            {
                if (spawner != null && enemyPrefabsToSpawn.Count > 0)
                {
                    // 난이도에 따라 스폰할 적 결정 및 스탯 조절
                    GameObject enemyPrefab = enemyPrefabsToSpawn[Random.Range(0, enemyPrefabsToSpawn.Count)];
                    GameObject spawnedEnemyGO = Create(enemyPrefab);
                    spawnedEnemyGO.transform.position = spawner.GetSpawnPosition();

                    Pawn enemyPawn = spawnedEnemyGO.GetComponent<Pawn>();
                    if (enemyPawn != null)
                    {
                        continue;
                    }
                    BattleStageManager.Instance.enemies.Add(enemyPawn); // 전투 매니저의 적 리스트에 추가
                    Debug.Log($"Spawned enemy {spawnedEnemyGO.name} at {spawnedEnemyGO.transform.position}");
                }
            }
        }

        /// <summary>
        /// 특정 ID에 해당하는 프리팹을 가져옵니다.
        /// </summary>
        /// <param name="id">프리팹의 ID</param>
        /// <returns>해당하는 GameObject 프리팹</returns>
        public GameObject GetPrefabByID(string id)
        {
            if (prefabIDs.ContainsKey(id))
            {
                return prefabIDs[id];
            }
            Debug.LogWarning($"Prefab with ID '{id}' not found in SpawnManager.");
            return null;
        }

        // 여기에 풀링 관련 로직 (비활성화, 풀에 반환 등) 추가 가능
        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false); // 비활성화
            // 풀에 반환하는 실제 로직
            Debug.Log($"SpawnManager: Returned {obj.name} to pool.");
        }
    }
} 