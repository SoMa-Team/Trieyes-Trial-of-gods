using System.Collections.Generic;
using Utils;
using AttackSystem;
using UnityEngine;
using CharacterSystem;
using System;

namespace AttackComponents
{
    /// <summary>
    /// 공격 관련 컴포넌트의 기본 동작을 정의하는 추상 클래스입니다.
    /// 이 컴포넌트는 IEventHandler를 구현하여 이벤트를 처리할 수 있습니다.
    /// </summary>
    public abstract class AttackComponent : MonoBehaviour, IEventHandler
    {
        // ===== [기능 1] 기본 정보 =====
        protected Attack parentAttack; // 부모 Attack

        public void SetParentAttack(Attack attack)
        {
            parentAttack = attack;
        }

        protected Pawn owner; // 소유자 (Attack의 attacker)

        public GameObject prefab;
        protected Rigidbody2D rb;
        protected Animator animator;
        protected SpriteRenderer spriteRenderer;
        
        // ===== [기능 2] 충돌 처리 =====
        protected HashSet<GameObject> hitTargets = new HashSet<GameObject>(); // 이미 맞은 대상들

        /// <summary>
        /// 이 컴포넌트가 Attack의 Collider 크기에 더할 xy축 증감값입니다.
        /// (0,0)이면 크기 변화 없음, 값이 있으면 해당 값만큼 Collider 크기 증감
        /// </summary>
        public Vector2 colliderSizeDelta = Vector2.zero;

        protected float maxDistance = 5f;

        protected virtual void Start()
        {
            Activate();
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
            if (owner != null)
            {
                float dist = Vector3.Distance(transform.position, owner.transform.position);
                if (dist > maxDistance && parentAttack != null && parentAttack.projectiles != null)
                {
                    // attack의 Projectiles 리스트에서 자신을 제거
                    parentAttack.projectiles.Remove(gameObject);
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        public virtual void Activate()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public virtual void Deactivate()
        {
            // 상태 초기화
            hitTargets.Clear();
            
            // 참조 정리
            parentAttack = null;
            owner = null;
        }

        // ===== [기능 3] 충돌 처리 =====
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            HandleComponentCollision(other.gameObject);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            HandleComponentCollision(collision.gameObject);
        }

        /// <summary>
        /// 컴포넌트 충돌을 처리합니다.
        /// </summary>
        /// <param name="hitObject">충돌한 객체</param>
        protected virtual void HandleComponentCollision(GameObject hitObject)
        {
            // 이미 맞은 대상인지 확인
            if (hitTargets.Contains(hitObject))
            {
                return;
            }

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

            if (hitPawn != null && owner != null)
            {
                // 소유자와 피격자가 다른 경우에만 처리
                if (hitPawn != owner)
                {
                    ProcessComponentCollision(hitPawn, hitObject);
                }
            }
        }

        /// <summary>
        /// 컴포넌트 충돌을 처리합니다.
        /// </summary>
        /// <param name="targetPawn">피격 대상</param>
        /// <param name="hitObject">충돌한 객체</param>
        protected virtual void ProcessComponentCollision(Pawn targetPawn, GameObject hitObject)
        {
            Debug.Log($"<color=orange>[COMPONENT] {gameObject.name} hit {targetPawn.gameObject.name}</color>");
            
            // 이미 맞은 대상으로 기록
            hitTargets.Add(hitObject);
            
            // 이벤트 발생
            if (owner != null)
            {
                // 소유자의 OnAttackHit 이벤트
                owner.OnEvent(Utils.EventType.OnAttackHit, targetPawn);
                
                // 피격자의 OnDamageHit 이벤트
                targetPawn.OnEvent(Utils.EventType.OnDamageHit, owner);
            }
        }

        // ===== [기능 4] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            // 하위 클래스에서 이 메서드를 오버라이드하여
            // 개별 이벤트에 대한 구체적인 로직을 구현합니다.
        }

        // ===== [기능 6] 공격 컴포넌트 실행 및 이벤트 반응 =====
        public abstract void Execute(Attack attack, Vector2 direction);
        
        // ===== [기능 7] 투사체 스탯 수정 =====
        /// <summary>
        /// 투사체의 스탯을 수정합니다.
        /// 하위 클래스에서 오버라이드하여 구체적인 스탯 수정 로직을 구현합니다.
        /// </summary>
        /// <param name="projectileStats">수정할 투사체 스탯</param>
        public virtual void ModifyProjectileStats(Stats.StatSheet projectileStats)
        {
            // 기본적으로는 아무것도 하지 않음
            // 하위 클래스에서 오버라이드하여 구현
            Debug.Log($"<color=cyan>[AttackComponent] {GetType().Name} modifying projectile stat sheet</color>");
        }
    }
} 