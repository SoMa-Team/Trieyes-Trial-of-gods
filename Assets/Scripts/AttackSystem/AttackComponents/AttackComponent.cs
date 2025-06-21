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

        /// <summary>
        /// 이 컴포넌트가 Attack의 Collider 크기에 더할 xy축 증감값입니다.
        /// (0,0)이면 크기 변화 없음, 값이 있으면 해당 값만큼 Collider 크기 증감
        /// </summary>
        public Vector2 colliderSizeDelta = Vector2.zero;

        protected virtual void Awake()
        {
            Activate();
        }

        protected virtual void OnDestroy()
        {
            Deactivate();
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        public virtual void Activate()
        {
            // 부모 Attack 찾기
            parentAttack = GetComponent<Attack>();
            if (parentAttack != null)
            {
                owner = parentAttack.attacker;
            }
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
                owner.OnEvent(Utils.EventType.OnAttackHit, targetPawn);
                
                // 피격자의 OnDamageHit 이벤트
                targetPawn.OnEvent(Utils.EventType.OnDamageHit, owner);
            }
        }

        // ===== [기능 4] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            // owner 참조가 없으면 자동으로 찾기
            EnsureOwnerReference();

            // 하위 클래스에서 이 메서드를 오버라이드하여
            // 개별 이벤트에 대한 구체적인 로직을 구현합니다.
        }

        /// <summary>
        /// owner 참조가 설정되어 있는지 확인하고, 없으면 자동으로 찾아서 설정합니다.
        /// </summary>
        protected void EnsureOwnerReference()
        {
            if (owner == null)
            {
                // 현재 씬에서 Pawn을 찾아서 owner 참조 설정
                Pawn foundPawn = FindFirstObjectByType<Pawn>();
                if (foundPawn != null)
                {
                    owner = foundPawn;
                    Debug.Log($"<color=green>[AttackComponent] Found owner through scene search: {foundPawn.gameObject.name}</color>");
                }
                else
                {
                    Debug.LogError("<color=red>[AttackComponent] No Pawn found in scene!</color>");
                }
            }
        }

        // ===== [기능 6] 공격 컴포넌트 실행 및 이벤트 반응 =====
        public abstract void Execute(Attack attack);
    }
} 