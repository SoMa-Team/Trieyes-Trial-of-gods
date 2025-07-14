using System;
using System.Collections.Generic;
using CharacterSystem;
using Utils;
using UnityEngine;
using AttackComponents;
using BattleSystem;
using Stats;
using JetBrains.Annotations;

namespace AttackSystem
{
    /// <summary>
    /// 게임 내 공격 행위를 정의하는 클래스입니다.
    /// 이 클래스는 IEventHandler를 구현하여 자체적으로 이벤트를 처리하고 발동시킬 수 있습니다.
    /// 
    /// 역할 구분:
    /// - parentAttack이 null: 관리자 (Manager) - 공격을 관리하고 투사체를 생성
    /// - parentAttack이 Attack: 투사체 (Projectile) - 실제 공격을 수행하는 투사체
    /// </summary>
    public class Attack : MonoBehaviour, IEventHandler
    {
        // ===== [기능 1] 공격 데이터 및 컴포넌트 관리 =====
        public AttackData attackData; // 기획적으로 논의 필요
        public Pawn attacker; // 공격자 (투사체를 발사한 캐릭터)

        public Pawn target; // 투사체가 다른 고정 대상으로 가는 투사체를 만드는 경우 사용
        public StatSheet statSheet;

        [CanBeNull] public Attack parent; // 부모 Attack (null이면 관리자, 아니면 투사체)
        public List<Attack> children;
        public List<AttackComponent> components;
        
        public Rigidbody2D rb { get; set; }
        public Collider2D attackCollider { get; set; }

        private void Update()
        {
            // TODO : 공격의 거리 제한에 대한 임시 코드
            float distance = Vector2.Distance(transform.position, attacker.transform.position);
            var maxDistance = 30f;
            if (distance > maxDistance)
            {
                AttackFactory.Instance.Deactivate(this);
            }
            // TODO END
        }

        private void Start()
        {
            // 컴포넌트 초기화
            rb = GetComponent<Rigidbody2D>();
            attackCollider = GetComponent<Collider2D>();
            
            if (rb is not null)
            {
                rb.gravityScale = 0f; // 중력 비활성화
            }

            foreach (var attackComponent in components)
            {
                attackComponent.SetAttack(this);
            }
        }

        protected virtual void OnDestroy()
        {
            AttackFactory.Instance.Deactivate(this);
        }

        // ===== [기능 6] 충돌 처리 (투사체일 때만 사용) =====
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            // null 체크 추가
            if (other == null || other.gameObject == null) return;
            
            foreach (var attackComponent in components)
            {
                attackComponent.OnTriggerEnter2D(other);
            }
            
            HandleCollision(other.gameObject);
        }

        /// <summary>
        /// 충돌을 처리합니다. (투사체 전용)
        /// </summary>
        /// <param name="hitObject">충돌한 객체</param>
        protected virtual void HandleCollision(GameObject hitObject)
        {
            // null 체크 추가
            if (hitObject == null) return;
            if (attacker == null) return;
            if (attacker.gameObject == null) return;
            
            // tag null 체크 추가
            if (string.IsNullOrEmpty(hitObject.tag)) return;
                
            switch (hitObject.tag)
            {
                case "Player": 
                    break;
                    
                case "Enemy":
                    if (attacker.gameObject.CompareTag(hitObject.tag))
                        return;
                    
                    ProcessAttackCollision(hitObject.GetComponent<Pawn>());
                    break;
                
                default:
                    return;
            }
        }

        /// <summary>
        /// 공격 성공 시 충돌을 처리합니다. (투사체 전용)
        /// </summary>
        /// <param name="targetPawn">피격 대상</param>
        protected virtual void ProcessAttackCollision(Pawn targetPawn)
        {
            //Debug.Log($"<color=orange>[ATTACK_PROJECTILE] {gameObject.name} hit {targetPawn.gameObject.name} ({targetPawn.GetType().Name})</color>");
            DamageProcessor.ProcessHit(this, targetPawn);
            
            // TODO : OnEvent를 활용한 처리 필요
            foreach (var attackComponent in components)
            {
                attackComponent.ProcessComponentCollision(targetPawn);
            }
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="direction"></param>
        /// <param name="pawn"></param>
        public virtual void Activate(Pawn attacker, Vector2 direction)
        {
            this.attacker = attacker;

            ApplyStatSheet(attacker.statSheet);
            
            children = new List<Attack>();
            foreach (var attackComponent in components)
            {
                attackComponent.Activate(this, direction);
            }
            
            gameObject.SetActive(true);
        }

        private void ApplyStatSheet(StatSheet attackerStatSheet)
        {
            // TODO: statSheet DeepCopy 필요
            statSheet = attackerStatSheet;
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public virtual void Deactivate()
        {
            // 컴포넌트 정리
            // TODO: AttackComponent 초기화 필요시 초기화
            children.Clear();
            
            // 물리 속성 초기화
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            
            // 위치 및 회전 초기화
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            
            // 참조 정리
            attacker = null;
            parent = null;
            
            gameObject.SetActive(false);
        }
        
        // ===== [기능 9] 이벤트 처리 =====
        public void OnEvent(Utils.EventType eventType, object param)
        {
            // 역할에 따른 로그 출력
            //Debug.Log($"<color=blue>[ATTACK] {gameObject.name} received event: {eventType}</color>");
            
            // 이벤트 타입에 따른 처리
            switch (eventType)
            {
                case Utils.EventType.OnAttackHit:
                    // 공격 시작 이벤트 처리
                    //Debug.Log($"<color=blue>[ATTACK] {gameObject.name} processing OnAttackHit</color>");
                    break;
                
                case Utils.EventType.OnAttack:
                    // 공격 종료 이벤트 처리
                    //Debug.Log($"<color=blue>[ATTACK] {gameObject.name} processing OnAttack</color>");
                    break;
                
                case Utils.EventType.OnDamageHit:
                    // 타격 이벤트 처리
                    //Debug.Log($"<color=blue>[ATTACK] {gameObject.name} processing OnDamageHit</color>");
                    break;
                
                default:
                    // 기본 이벤트 처리
                    //Debug.Log($"<color=blue>[ATTACK] {gameObject.name} processing {eventType}</color>");
                    break;
            }
        }

        public void AddAttack(Attack newAttack)
        {
            children.Add(newAttack);
        }
    }
} 
