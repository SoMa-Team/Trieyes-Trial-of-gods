using UnityEngine;
using System.Collections.Generic;

namespace CombatSystem
{
    public class SceneFactory : MonoBehaviour
    {
        [Header("Map Prefabs")]
        public List<GameObject> mapDesignPrefabs; // 에디터에서 할당할 맵 디자인 프리팹 목록

        /// <summary>
        /// 특정 ID 또는 인덱스에 해당하는 맵 디자인 프리팹을 반환합니다.
        /// </summary>
        /// <param name="index">맵 프리팹 목록의 인덱스</param>
        /// <returns>해당하는 맵 디자인 GameObject 프리팹</returns>
        public GameObject GetMapDesignPrefab(int index)
        {
            if (index >= 0 && index < mapDesignPrefabs.Count)
            {
                return mapDesignPrefabs[index];
            }
            Debug.LogWarning($"SceneFactory: Map design prefab at index {index} not found.");
            return null;
        }

        /// <summary>
        /// 맵 디자인 프리팹을 이름으로 가져오는 메서드 (선택 사항)
        /// </summary>
        /// <param name="name">맵 프리팹의 이름</param>
        /// <returns>해당하는 맵 디자인 GameObject 프리팹</returns>
        public GameObject GetMapDesignPrefabByName(string name)
        {
            foreach (var prefab in mapDesignPrefabs)
            {
                if (prefab != null && prefab.name == name)
                {
                    return prefab;
                }
            }
            Debug.LogWarning($"SceneFactory: Map design prefab with name '{name}' not found.");
            return null;
        }
    }
} 