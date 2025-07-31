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
        
        private static VFXFactory instance;
        public static VFXFactory Instance
        {
            get
            {
                if (instance is null)
                {
                    instance = FindFirstObjectByType<VFXFactory>();
                    if (instance is null)
                    {
                        GameObject go = new GameObject("VFXFactory");
                        instance = go.AddComponent<VFXFactory>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance is null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// VFX의 모든 Particle System을 재생합니다.
        /// </summary>
        /// <param name="vfx">재생할 VFX</param>
        public void PlayVFX(GameObject vfx)
        {
            if (vfx is null) return;

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
            if (vfx is null) return;

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