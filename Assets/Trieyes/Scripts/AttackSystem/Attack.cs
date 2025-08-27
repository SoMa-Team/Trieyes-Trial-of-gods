using System;
using System.Collections.Generic;
using CharacterSystem;
using Utils;
using UnityEngine;
using AttackComponents;
using BattleSystem;
using Stats;
using JetBrains.Annotations;
using RelicSystem;
using TagSystem;

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
        public AttackData attackData;
        public Pawn attacker; // 공격자 (투사체를 발사한 Pawn)
        public StatSheet statSheet { get; private set; }

        [CanBeNull] public Attack parent; // 부모 Attack (null이면 관리자, 아니면 투사체)
        public List<Attack> children = new ();
        public List<AttackComponent> components = new ();
        
        public Rigidbody2D rb;
        public  Collider2D attackCollider;
        public Dictionary<RelicStatType, int> relicStats = new Dictionary<RelicStatType, int>();
        public int objectID; // 공격 조회를 위한 ID

        // ===== [Lock 메커니즘] =====
        protected bool isLocked = false; // Lock 상태 관리

        public int getRelicStat(RelicStatType relicStatType)
        {
            if (!AttackTagManager.isValidRelicStat(relicStatType))
                throw new Exception("Invalid relic stat type");
            if (!relicStats.ContainsKey(relicStatType))
                return 0;
            return relicStats[relicStatType];
        }

        private void Update()
        {
            // TODO : 공격의 거리 제한에 대한 임시 코드
            float distance = Vector2.Distance(transform.position, attacker.transform.position);
            var maxDistance = 100f;
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
        }

        protected virtual void OnDestroy()
        { 
            if(BattleStage.now != null) AttackFactory.Instance.Deactivate(this);
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
            
            // TODO: Layer 충돌 적용 필요
            switch (hitObject.tag)
            {
                case "Player": 
                    
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
            DamageProcessor.ProcessHit(this, targetPawn);
            
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
            // 1. Lock 상태 설정 (맨 처음에 수행)
            SetLock(true);
            
            children = new List<Attack>();
            
            // 2. 모든 AttackComponent Activate (Lock true 상태로)
            foreach (var attackComponent in components)
            {
                AttackComponentFactory.Instance.Activate(attackComponent, this, direction);
            }

            // 3. Lock 상태에서 초기 설정 수행 (부모 → 자식 순서)
            OnLockActivate();
            
            // 4. 재귀적으로 자식 Attack들도 Activate
            foreach (var child in children)
            {
                child.Activate(attacker, direction);
            }
            
            // 5. Lock 해제
            SetLock(false);
        }

        public void ApplyStatSheet(StatSheet statSheet)
        {
            this.statSheet = statSheet.DeepCopy();
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public virtual void Deactivate()
        {
            foreach (var attack in children)
            {
                AttackFactory.Instance.Deactivate(attack);
            }

            foreach (var attackComponent in components)
            {
                AttackComponentFactory.Instance.Deactivate(attackComponent);
            }
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
        }
        
        // ===== [Lock 메커니즘] =====
        /// <summary>
        /// Lock 상태를 설정합니다.
        /// </summary>
        /// <param name="locked">Lock 여부</param>
        public virtual void SetLock(bool locked)
        {
            isLocked = locked;
            
            // 모든 자식 컴포넌트들에게도 Lock 상태 전파
            foreach (var component in components)
            {
                component.SetLock(locked);
            }
        }

        /// <summary>
        /// Lock 상태에서 실행해야 하는 초기 설정을 수행합니다.
        /// </summary>
        public virtual void OnLockActivate()
        {
            // 모든 자식 컴포넌트들의 Lock 상태 초기 설정 수행
            foreach (var component in components)
            {
                component.OnLockActivate();
            }
        }

        // ===== [기능 9] 이벤트 처리 =====
        public bool OnEvent(Utils.EventType eventType, object param)
        {
            // 역할에 따른 로그 출력
            //Debug.Log($"<color=blue>[ATTACK] {gameObject.name} received event: {eventType}</color>");
            
            // 이벤트 타입에 따른 처리
            switch (eventType)
            {
                default:
                    return false;
            }
        }

        public void AddAttack(Attack newAttack)
        {
            children.Add(newAttack);
        }

        public void AddAttackComponent(AttackComponent attackComponent)
        {
            attackComponent.transform.SetParent(transform);
            components.Add(attackComponent);
        }

        public void ApplyRelicStat(RelicStatType statType, int value)
        {
            if (!relicStats.ContainsKey(statType))
            {
                relicStats[statType] = 0;
            }
            
            relicStats[statType] += value;
        }
    }
} 
