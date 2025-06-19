using System.Collections.Generic;
using Utils;
using AttackSystem;
using UnityEngine;
using CharacterSystem;

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
        protected Pawn owner; // 소유자 (Attack의 attacker)
        
        // ===== [기능 2] 충돌 처리 =====
        protected HashSet<GameObject> hitTargets = new HashSet<GameObject>(); // 이미 맞은 대상들

        protected virtual void Awake()
        {
            // 부모 Attack 찾기
            parentAttack = GetComponent<Attack>();
            if (parentAttack != null)
            {
                owner = parentAttack.attacker;
            }
        }

        protected virtual void Start()
        {
            hitTargets.Clear();
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
                owner.OnAttackHit(targetPawn);
                
                // 피격자의 OnDamageHit 이벤트
                targetPawn.OnDamageHit(owner);
            }
        }

        // ===== [기능 4] 이벤트 처리(추상) =====
        public abstract void OnEvent(Utils.EventType eventType, object param);

        // ===== [기능 5] 공격 컴포넌트 실행 및 이벤트 반응 =====
        public abstract void Execute(Attack attack);
    }
} 