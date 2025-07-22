using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace VFXSystem
{
    /// <summary>
    /// VFX 시스템을 관리하는 매니저 클래스
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        [Header("VFX Prefabs")]
        [SerializeField] private GameObject basicAttackVFXPrefab;
        [SerializeField] private GameObject critVFXPrefab;
        [SerializeField] private GameObject magicVFXPrefab;
        
        [Header("Pool Settings")]
        [SerializeField] private VFXPool vfxPool;

        [Header("VFX Delay")]
        [SerializeField] private static float vfxDelay = 2f;
        
        private static VFXManager instance;
        public static VFXManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<VFXManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("VFXManager");
                        instance = go.AddComponent<VFXManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

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
            // VFX 풀 초기화
            if (vfxPool == null)
            {
                vfxPool = new VFXPool();
            }

            // 프리팹들을 풀에 등록
            if (basicAttackVFXPrefab != null)
            {
                vfxPool.AddPrefab(basicAttackVFXPrefab);
            }
            
            if (critVFXPrefab != null)
            {
                vfxPool.AddPrefab(critVFXPrefab);
            }
            
            if (magicVFXPrefab != null)
            {
                vfxPool.AddPrefab(magicVFXPrefab);
            }
        }

        /// <summary>
        /// 기본 공격 VFX를 생성합니다.
        /// </summary>
        /// <param name="position">생성 위치</param>
        /// <param name="direction">공격 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        public GameObject SpawnBasicAttackVFX(Vector2 position, Vector2 direction)
        {
            return SpawnVFX(basicAttackVFXPrefab, position, direction);
        }

        /// <summary>
        /// 크리티컬 공격 VFX를 생성합니다.
        /// </summary>
        /// <param name="position">생성 위치</param>
        /// <param name="direction">공격 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        public GameObject SpawnCritVFX(Vector2 position, Vector2 direction)
        {
            return SpawnVFX(critVFXPrefab, position, direction);
        }

        /// <summary>
        /// 마법 공격 VFX를 생성합니다.
        /// </summary>
        /// <param name="position">생성 위치</param>
        /// <param name="direction">공격 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        public GameObject SpawnMagicVFX(Vector2 position, Vector2 direction)
        {
            return SpawnVFX(magicVFXPrefab, position, direction);
        }

        /// <summary>
        /// VFX 타입에 따라 VFX를 생성합니다.
        /// </summary>
        /// <param name="vfxType">VFX 타입 문자열</param>
        /// <param name="position">생성 위치</param>
        /// <param name="direction">공격 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        public GameObject SpawnVFXByType(string vfxType, Vector2 position, Vector2 direction)
        {
            GameObject prefab = GetPrefabByType(vfxType);
            return SpawnVFX(prefab, position, direction);
        }

        /// <summary>
        /// VFX 타입에 따라 VFX를 생성합니다. (회전 직접 지정)
        /// </summary>
        /// <param name="vfxType">VFX 타입 문자열</param>
        /// <param name="position">생성 위치</param>
        /// <param name="rotation">회전값 (도 단위)</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        public GameObject SpawnVFXByType(string vfxType, Vector2 position, float rotation)
        {
            GameObject prefab = GetPrefabByType(vfxType);
            return SpawnVFX(prefab, position, rotation);
        }

        /// <summary>
        /// VFX를 생성하고 설정합니다.
        /// </summary>
        /// <param name="prefab">사용할 VFX 프리팹</param>
        /// <param name="position">생성 위치</param>
        /// <param name="direction">방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        private GameObject SpawnVFX(GameObject prefab, Vector2 position, Vector2 direction)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[VFXManager] VFX 프리팹이 설정되지 않았습니다!");
                return null;
            }

            // 풀에서 VFX 가져오기
            GameObject vfx = vfxPool.Get(prefab);
            vfx.transform.position = position;
            
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // vfx의 Z Rotation을 Angle로 설정
            vfx.transform.rotation = Quaternion.Euler(0, 0, angle);

            // 모든 Particle System 재생
            ParticleSystem[] particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Play();
            }

            Debug.Log($"<color=green>[VFXManager] VFX 생성 완료! 위치: {position}, 방향: {direction}, 각도: {angle}도</color>");
            return vfx;
        }

        /// <summary>
        /// VFX를 생성하고 설정합니다. (회전 직접 지정)
        /// </summary>
        /// <param name="prefab">사용할 VFX 프리팹</param>
        /// <param name="position">생성 위치</param>
        /// <param name="rotation">회전값 (도 단위)</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        private GameObject SpawnVFX(GameObject prefab, Vector2 position, float rotation)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[VFXManager] VFX 프리팹이 설정되지 않았습니다!");
                return null;
            }

            // 풀에서 VFX 가져오기
            GameObject vfx = vfxPool.Get(prefab);
            vfx.transform.position = position;
            
            // 직접 회전값 적용
            vfx.transform.rotation = Quaternion.Euler(0, 0, rotation);

            // 모든 Particle System 재생
            ParticleSystem[] particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Play();
            }

            Debug.Log($"<color=green>[VFXManager] VFX 생성 완료! 위치: {position}, 회전: {rotation}도</color>");
            return vfx;
        }

        /// <summary>
        /// VFX 타입에 따른 프리팹을 반환합니다.
        /// </summary>
        /// <param name="vfxType">VFX 타입 문자열</param>
        /// <returns>해당하는 VFX 프리팹</returns>
        private GameObject GetPrefabByType(string vfxType)
        {
            switch (vfxType.ToLower())
            {
                case "basicattack":
                    return basicAttackVFXPrefab;
                case "crit":
                    return critVFXPrefab;
                case "magic":
                    return magicVFXPrefab;
                default:
                    return basicAttackVFXPrefab;
            }
        }

        /// <summary>
        /// VFX를 풀로 반환합니다.
        /// </summary>
        /// <param name="vfx">반환할 VFX</param>
        /// <param name="prefab">원본 프리팹</param>
        /// <param name="delay">지연 시간</param>
        public void ReturnVFX(GameObject vfx, GameObject prefab, float delay = 2f)
        {
            if (vfx == null || prefab == null) return;

            // 모든 Particle System 정지
            ParticleSystem[] particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Stop();
            }

            // 즉시 풀로 반환 (지연 없이)
            vfxPool.Return(vfx, prefab);
        }

        /// <summary>
        /// VFX를 타입에 맞는 풀로 반환합니다.
        /// </summary>
        /// <param name="vfx">반환할 VFX</param>
        /// <param name="vfxType">VFX 타입 문자열</param>
        /// <param name="delay">지연 시간 (사용하지 않음)</param>
        public void ReturnVFXByType(GameObject vfx, string vfxType, float delay = 2f)
        {
            GameObject prefab = GetPrefabByType(vfxType);
            ReturnVFX(vfx, prefab, delay);
        }

        /// <summary>
        /// 모든 활성 VFX를 풀로 반환합니다.
        /// </summary>
        public void ReturnAllVFX()
        {
            vfxPool.ReturnAll();
        }
    }
} 