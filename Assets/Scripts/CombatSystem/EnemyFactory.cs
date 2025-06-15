using UnityEngine;
using CharacterSystem;
using System.Collections.Generic;

namespace CombatSystem
{
    public class EnemyFactory : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        public List<GameObject> enemyPrefabs; // 에디터에서 할당할 적 프리팹 목록

        /// <summary>
        /// 특정 타입 또는 ID의 적을 생성합니다.
        /// 이 메서드는 풀링된 객체를 가져오거나 새로 인스턴스화하고 활성화(Active) 상태로 만듭니다.
        /// </summary>
        /// <param name="enemyPrefab">생성할 적 프리팹 (선택 사항, 지정하지 않으면 목록에서 무작위 선택)</param>
        /// <returns>생성된 적의 GameObject 인스턴스</returns>
        public GameObject CreateEnemy(GameObject enemyPrefab = null)
        {
            GameObject prefabToUse = enemyPrefab;
            if (prefabToUse == null && enemyPrefabs.Count > 0)
            {
                prefabToUse = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            }

            if (prefabToUse == null)
            {
                Debug.LogError("EnemyFactory: No enemy prefab available to create.");
                return null;
            }

            GameObject enemyInstance = Instantiate(prefabToUse);
            enemyInstance.SetActive(true); // 불명 Active 선언 (활성화)

            Debug.Log($"EnemyFactory: Created enemy '{enemyInstance.name}'.");
            return enemyInstance;
        }

        /// <summary>
        /// 생성된 적을 비활성화하고 (풀링 개념) 관리합니다.
        /// </summary>
        /// <param name="enemyGO">비활성화할 적 GameObject</param>
        public void DeactivateEnemy(GameObject enemyGO)
        {
            if (enemyGO != null)
            {
                enemyGO.SetActive(false); // 불명 Active 선언 (비활성화)
                Debug.Log($"EnemyFactory: Deactivated enemy '{enemyGO.name}'.");
                // 여기에 풀에 반환하는 로직 추가
            }
        }
    }
} 