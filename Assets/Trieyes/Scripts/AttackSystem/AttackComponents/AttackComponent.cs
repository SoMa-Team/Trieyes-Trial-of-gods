using System.Collections.Generic;
using Utils;
using AttackSystem;
using UnityEngine;
using CharacterSystem;
using System;
using VFXSystem;

namespace AttackComponents
{
    /// <summary>
    /// 공격 관련 컴포넌트의 기본 동작을 정의하는 추상 클래스입니다.
    /// 이 컴포넌트는 IEventHandler를 구현하여 이벤트를 처리할 수 있습니다.
    /// </summary>
    public abstract class AttackComponent : MonoBehaviour, IEventHandler
    {
        private int level; // Relic과 연결될 경우, 자동으로 초기화
        
        // ===== [기능 1] 기본 정보 =====
        protected Attack attack; // 부모 Attack
        protected Pawn attacker => attack?.attacker; // 소유자 (Attack의 attacker)

        // VFX GameObject 구현하는 방향으로 변경
        [SerializeField] protected List<GameObject> vfxList = new List<GameObject>();

        protected virtual void Start()
        {
        }

        protected virtual void OnDestroy()
        {
            Deactivate();
        }

        protected virtual void Update()
        {
            // 기존 Update 유지
            // ... existing code ...

            // owner와의 거리 체크 및 Destroy
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        public virtual void Activate(Attack attack, Vector2 direction)
        {
            this.attack = attack;
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public virtual void Deactivate()
        {
        }

        // ===== [기능 3] 충돌 처리 =====
        public virtual void OnTriggerEnter2D(Collider2D other)
        {
            // HandleComponentCollision(other.gameObject);
        }

        /// <summary>
        /// 컴포넌트 충돌을 처리합니다.
        /// </summary>
        /// <param name="hitObject">충돌한 객체</param>
        protected virtual void HandleComponentCollision(GameObject hitObject)
        {
            // 충돌한 객체의 Pawn 컴포넌트 찾기
            Pawn hitPawn = hitObject.GetComponent<Pawn>();
            if (hitPawn == null)
            {
                // Pawn이 없는 경우 Attack 컴포넌트 찾기
                Attack hitAttack = hitObject.GetComponent<Attack>();
                if (hitAttack != null)
                {
                    hitPawn = hitAttack.attacker;
                }
            }

            if (hitPawn != null && attacker != null)
            {
                // 소유자와 피격자가 다른 경우에만 처리
                if (hitPawn != attacker)
                {
                    ProcessComponentCollision(hitPawn);
                }
            }
        }

        /// <summary>
        /// 컴포넌트 충돌을 처리합니다.
        /// </summary>
        /// <param name="targetPawn">피격 대상</param>
        /// <param name="hitObject">충돌한 객체</param>
        public virtual void ProcessComponentCollision(Pawn targetPawn)
        {
            
        }

        // ===== [기능 5] VFX 처리 =====
        /// <summary>
        /// VFX를 생성하고 설정합니다. (하위 클래스에서 오버라이드)
        /// </summary>
        /// <param name="position">VFX 생성 위치</param>
        /// <param name="direction">VFX 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        protected virtual GameObject CreateAndSetupVFX(Vector2 position, Vector2 direction)
        {
            // 기본 구현: 하위 클래스에서 오버라이드
            return null;
        }
        
        // ===== [기능 5] VFX 처리 =====
        /// <summary>
        /// VFX를 생성하고 설정합니다. (하위 클래스에서 오버라이드)
        /// </summary>
        /// <param name="vfxPrefab">VFX 프리팹</param>
        /// <param name="position">VFX 생성 위치</param>
        /// <param name="direction">VFX 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        protected virtual GameObject CreateAndSetupVFX(GameObject vfxPrefab, Vector2 position, Vector2 direction)
        {
            if (vfxPrefab == null)
            {
                Debug.LogWarning("VFX 프리팹이 설정되지 않았습니다!");
                return null;
            }
            GameObject _vfxPrefab = Instantiate(vfxPrefab);

            // VFX 프리팹을 직접 활성화
            _vfxPrefab.SetActive(true);
            _vfxPrefab.transform.position = position;
            
            // 방향에 맞게 회전
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _vfxPrefab.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            return _vfxPrefab;
        }

        /// <summary>
        /// VFX를 재생합니다.
        /// </summary>
        /// <param name="vfx">재생할 VFX</param>
        protected virtual void PlayVFX(GameObject vfx)
        {
            if (vfx != null && VFXFactory.Instance != null)
            {
                VFXFactory.Instance.PlayVFX(vfx);
            }
        }

        /// <summary>
        /// VFX를 정지하고 반환합니다.
        /// </summary>
        /// <param name="vfx">정지할 VFX</param>
        /// <param name="vfxId">VFX ID</param>
        protected virtual void StopAndReturnVFX(GameObject vfx, int vfxId)
        {
            if (vfx != null && VFXFactory.Instance != null)
            {
                VFXFactory.Instance.StopVFX(vfx);
                VFXFactory.Instance.ReturnVFX(vfx, vfxId);
            }
        }
        
        /// <summary>
        /// VFX를 정지하고 비활성화합니다.
        /// </summary>
        /// <param name="vfx">정지할 VFX 게임오브젝트</param>
        protected virtual void StopAndDestroyVFX(GameObject vfx)
        {
            if (vfx == null) return;
            
            // 모든 ParticleSystem 컴포넌트 정지
            ParticleSystem[] particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Stop();
            }
            
            // VFX 비활성화 (Destroy 대신 SetActive(false) 사용)
            vfx.SetActive(false);
        }

        // ===== [기능 4] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            // 하위 클래스에서 이 메서드를 오버라이드하여
            // 개별 이벤트에 대한 구체적인 로직을 구현합니다.
        }

        public void SetLevel(int level)
        {
            this.level = level;
        }
    }
} 
