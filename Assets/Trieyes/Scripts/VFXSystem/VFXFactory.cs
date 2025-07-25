using UnityEngine;
using System.Collections.Generic;

namespace VFXSystem
{
    /// <summary>
    /// VFX 팩토리 클래스
    /// ID 기반으로 VFX를 생성하고 풀링을 관리합니다.
    /// </summary>
    public class VFXFactory : MonoBehaviour
    {
        [Header("VFX Prefabs")]
        [SerializeField] private List<GameObject> vfxPrefabs = new List<GameObject>();
        
        [Header("Pool Settings")]
        [SerializeField] private int defaultPoolSize = 5;
        
        private static VFXFactory instance;
        public static VFXFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<VFXFactory>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("VFXFactory");
                        instance = go.AddComponent<VFXFactory>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        // ID별 VFX 풀 관리
        private Dictionary<int, VFXPool> vfxPools = new Dictionary<int, VFXPool>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeVFX();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeVFX()
        {
            // 각 VFX 프리팹에 대해 풀 생성
            for (int i = 0; i < vfxPrefabs.Count; i++)
            {
                if (vfxPrefabs[i] != null)
                {
                    VFXPool pool = new VFXPool();
                    pool.AddPrefab(vfxPrefabs[i]);
                    vfxPools[i] = pool;
                }
            }
        }

        public void Clear()
        {
            foreach (VFXPool pool in vfxPools.Values)
            {
                pool.Clear();
            }
        }

        /// <summary>
        /// VFX ID로 VFX를 생성합니다. (기본 생성만 담당)
        /// </summary>
        /// <param name="vfxId">VFX ID</param>
        /// <param name="position">생성 위치</param>
        /// <param name="direction">방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        public GameObject SpawnVFX(int vfxId, Vector2 position, Vector2 direction)
        {
            if (!vfxPools.ContainsKey(vfxId))
            {
                Debug.LogError($"[VFXFactory] VFX ID {vfxId}에 해당하는 프리팹이 등록되지 않았습니다!");
                return null;
            }

            if (vfxId >= vfxPrefabs.Count || vfxPrefabs[vfxId] == null)
            {
                Debug.LogError($"[VFXFactory] VFX ID {vfxId}에 해당하는 프리팹이 null입니다!");
                return null;
            }
            
            return vfxPools[vfxId].Get(vfxPrefabs[vfxId]);
        }

        public void ClearVFXPool()
        {
            foreach (KeyValuePair<int, VFXPool> vfxPool in vfxPools)
            {
                vfxPool.Value.PoolClear();
            }
        }

        /// <summary>
        /// VFX의 모든 Particle System을 재생합니다.
        /// </summary>
        /// <param name="vfx">재생할 VFX</param>
        public void PlayVFX(GameObject vfx)
        {
            if (vfx == null) return;

            ParticleSystem[] particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Play();
            }
        }

        /// <summary>
        /// VFX의 모든 Particle System을 정지합니다.
        /// </summary>
        /// <param name="vfx">정지할 VFX</param>
        public void StopVFX(GameObject vfx)
        {
            if (vfx == null) return;

            ParticleSystem[] particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Stop();
            }
        }

        /// <summary>
        /// VFX를 풀로 반환합니다.
        /// </summary>
        /// <param name="vfx">반환할 VFX</param>
        /// <param name="vfxId">VFX ID</param>
        public void ReturnVFX(GameObject vfx, int vfxId)
        {
        }
    }
} 