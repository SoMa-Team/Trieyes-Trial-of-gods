using UnityEngine;
using System.Collections.Generic;

namespace VFXSystem
{
    /// <summary>
    /// VFX 오브젝트 풀링을 위한 클래스
    /// </summary>
    [System.Serializable]
    public class VFXPool
    {
        [Header("VFX Prefabs")]
        [SerializeField] private List<GameObject> vfxPrefabs = new List<GameObject>();
        
        [Header("Pool Settings")]
        [SerializeField] private int maxPoolSize = 20; // 최대 풀 크기
        
        private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
        private Dictionary<GameObject, List<GameObject>> activeObjects = new Dictionary<GameObject, List<GameObject>>();

        /// <summary>
        /// VFX 프리팹을 풀에 추가합니다.
        /// </summary>
        /// <param name="prefab">추가할 VFX 프리팹</param>
        public void AddPrefab(GameObject prefab)
        {
            if (prefab != null && !vfxPrefabs.Contains(prefab))
            {
                vfxPrefabs.Add(prefab);
                pools[prefab] = new Queue<GameObject>();
                activeObjects[prefab] = new List<GameObject>();
            }
        }

        /// <summary>
        /// 특정 프리팹에서 VFX를 가져옵니다.
        /// </summary>
        /// <param name="prefab">사용할 VFX 프리팹</param>
        /// <returns>VFX 게임오브젝트</returns>
        public GameObject Get(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[VFXPool] 프리팹이 null입니다!");
                return null;
            }

            // 프리팹이 등록되지 않았으면 등록
            if (!pools.ContainsKey(prefab))
            {
                AddPrefab(prefab);
            }

            GameObject obj;
            //Queue<GameObject> pool = pools[prefab];
            
            // if (pool.Count > 0)
            // {
            //     // 풀에서 가져오기
            //     obj = pool.Dequeue();
            // }
            // else
            // {
            //     // 풀이 비어있으면 새로 생성
            //     obj = Object.Instantiate(prefab);
            // }
            obj = Object.Instantiate(prefab);
            obj.SetActive(true);
            activeObjects[prefab].Add(obj);
            return obj;
        }

        public void PoolClear()
        {
            pools.Clear();
            activeObjects.Clear();
        }

        /// <summary>
        /// VFX를 풀로 반환합니다.
        /// </summary>
        /// <param name="obj">반환할 VFX 오브젝트</param>
        /// <param name="prefab">원본 프리팹</param>
        public void Return(GameObject obj, GameObject prefab)
        {
            if (obj == null || prefab == null) return;

            if (!pools.ContainsKey(prefab))
            {
                Debug.LogWarning("[VFXPool] 해당 프리팹이 등록되지 않았습니다!");
                return;
            }

            obj.SetActive(false);
            activeObjects[prefab].Remove(obj);
            pools[prefab].Enqueue(obj);
        }

        /// <summary>
        /// 모든 활성 VFX를 풀로 반환합니다.
        /// </summary>
        public void ReturnAll()
        {
            foreach (var kvp in activeObjects)
            {
                GameObject prefab = kvp.Key;
                List<GameObject> activeList = kvp.Value;
                
                for (int i = activeList.Count - 1; i >= 0; i--)
                {
                    if (activeList[i] != null)
                    {
                        Return(activeList[i], prefab);
                    }
                }
            }
        }

        /// <summary>
        /// 특정 프리팹의 활성 VFX 개수를 반환합니다.
        /// </summary>
        /// <param name="prefab">확인할 프리팹</param>
        /// <returns>활성 VFX 개수</returns>
        public int GetActiveCount(GameObject prefab)
        {
            if (activeObjects.ContainsKey(prefab))
            {
                return activeObjects[prefab].Count;
            }
            return 0;
        }

        /// <summary>
        /// 특정 프리팹의 풀 크기를 반환합니다.
        /// </summary>
        /// <param name="prefab">확인할 프리팹</param>
        /// <returns>풀 크기</returns>
        public int GetPoolSize(GameObject prefab)
        {
            if (pools.ContainsKey(prefab))
            {
                return pools[prefab].Count;
            }
            return 0;
        }
    }
} 