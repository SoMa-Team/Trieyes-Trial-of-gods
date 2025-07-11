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
        protected Attack attack; // 부모 Attack
        protected Pawn attacker => attack?.attacker; // 소유자 (Attack의 attacker)

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

        // ===== [기능 4] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            // 하위 클래스에서 이 메서드를 오버라이드하여
            // 개별 이벤트에 대한 구체적인 로직을 구현합니다.
        }

        public void SetAttack(Attack attack)
        {
            this.attack = attack;
        }
    }
} 
