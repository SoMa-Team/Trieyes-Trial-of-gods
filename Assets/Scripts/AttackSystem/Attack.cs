using System.Collections.Generic;
using CharacterSystem;
using Utils;
using UnityEngine;
using AttackComponents;
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

        [CanBeNull] public Attack parent; // 부모 Attack (null이면 관리자, 아니면 투사체)
        public List<Attack> children;
        public List<AttackComponent> components;
        
        // ===== [기능 2] 투사체 관련 (투사체일 때만 사용) =====
        // [SerializeField] protected int pierceCount = 1; // 관통 개수
        // [SerializeField] protected float projectileSpeed = 10f; // 투사체 속도
        // [SerializeField] protected float maxDistance = 2f; // 최대 이동 거리
        // protected int currentPierceCount = 0; // 현재 관통 횟수
        // protected float currentDistance = 0f; // 현재 이동 거리
        // protected Vector3 spawnPosition; // 발사 위치
        // protected float currentLifetime = 0f; // 현재 수명
        protected Rigidbody2D rb;
        protected Collider2D attackCollider;
        
        // ===== [기능 3] 충돌 처리 (투사체일 때만 사용) =====
        // protected HashSet<GameObject> hitTargets = new HashSet<GameObject>(); // 이미 맞은 대상들 (관통 시 중복 방지)

        // ===== [기능 4] 역할 구분 프로퍼티 =====
        /// <summary>
        /// 이 Attack이 관리자인지 확인합니다.
        /// </summary>
        // public bool IsManager => parent == null;
        
        /// <summary>
        /// 이 Attack이 투사체인지 확인합니다.
        /// </summary>
        // public bool IsProjectile => parent != null;
        // public List<GameObject> projectiles = new List<GameObject>();

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
            switch (hitObject.tag)
            {
                case "Player": 
                case "Enemy":
                    if (attacker.gameObject.CompareTag(hitObject.tag))
                        return;
                    
                    // Player가 맞음
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
            
            // 이벤트 발생 순서: OnAttackHit → OnDamageHit → 회피 판정 → OnAttackMiss/OnAttack
            if (attacker != null)
            {
                //Debug.Log($"<color=yellow>[EVENT] {gameObject.name} -> Attacker {attacker.gameObject.name} ({attacker.GetType().Name}) OnAttackHit -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name})</color>");
                // 1. 공격자의 OnAttackHit 이벤트 (유물, 카드 순회)
                attacker.OnEvent(Utils.EventType.OnAttackHit, targetPawn);
                
                //Debug.Log($"<color=red>[EVENT] {gameObject.name} -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name}) OnDamageHit <- Attacker {attacker.gameObject.name} ({attacker.GetType().Name})</color>");
                // 2. 피격자의 OnDamageHit 이벤트 (회피 판정)
                targetPawn.OnEvent(Utils.EventType.OnDamageHit, attacker);
                
                // 3. 회피 판정 (피격자에서)
                bool isEvaded = CheckEvasion(targetPawn);
                
                if (isEvaded)
                {
                    //Debug.Log($"<color=cyan>[EVENT] {gameObject.name} -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name}) OnEvaded (SUCCESS)</color>");
                    //Debug.Log($"<color=cyan>[EVENT] {gameObject.name} -> Attacker {attacker.gameObject.name} ({attacker.GetType().Name}) OnAttackMiss -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name})</color>");
                    // 회피 성공: OnEvaded (피격자) + OnAttackMiss (공격자)
                    targetPawn.OnEvent(Utils.EventType.OnEvaded, null);
                    attacker.OnEvent(Utils.EventType.OnAttackMiss, targetPawn);
                }
                else
                {
                    //Debug.Log($"<color=green>[EVENT] {gameObject.name} -> Attacker {attacker.gameObject.name} ({attacker.GetType().Name}) OnAttack -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name})</color>");
                    // 회피 실패: OnAttack (공격자) - 데미지 계산 및 OnDamaged 호출
                    attacker.OnEvent(Utils.EventType.OnAttack, targetPawn);
                }
            }

            foreach (var attackComponent in components)
            {
                attackComponent.ProcessComponentCollision(targetPawn);
            }
        }

        /// <summary>
        /// 회피 판정을 수행합니다. 이 부분은 Pawn으로 가도 될 것 같긴 한데, Flow를 한번에 보여주려면 여기에 적는게 낫습니다.
        /// </summary>
        /// <param name="targetPawn">피격 대상</param>
        /// <returns>회피 성공 여부</returns>
        protected virtual bool CheckEvasion(Pawn targetPawn)
        {
            // 피격자의 회피율 계산
            float evasionRate = targetPawn.GetStatValue(StatType.Evasion) / 100f;
            bool isEvaded = UnityEngine.Random.Range(0f, 1f) < evasionRate;
            
            //Debug.Log($"<color=cyan>[EVASION] {targetPawn.gameObject.name} evasion check: {evasionRate * 100}% -> {(isEvaded ? "SUCCESS" : "FAILED")}</color>");
            
            return isEvaded;
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
            
            children = new List<Attack>();
            foreach (var attackComponent in components)
            {
                attackComponent.Activate(this, direction);
            }
            
            gameObject.SetActive(true);
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
            Debug.Log($"<color=blue>[ATTACK] {gameObject.name} received event: {eventType}</color>");
            
            // 이벤트 타입에 따른 처리
            switch (eventType)
            {
                case Utils.EventType.OnAttackHit:
                    // 공격 시작 이벤트 처리
                    Debug.Log($"<color=blue>[ATTACK] {gameObject.name} processing OnAttackHit</color>");
                    break;
                
                case Utils.EventType.OnAttack:
                    // 공격 종료 이벤트 처리
                    Debug.Log($"<color=blue>[ATTACK] {gameObject.name} processing OnAttack</color>");
                    break;
                
                case Utils.EventType.OnDamageHit:
                    // 타격 이벤트 처리
                    Debug.Log($"<color=blue>[ATTACK] {gameObject.name} processing OnDamageHit</color>");
                    break;
                
                default:
                    // 기본 이벤트 처리
                    Debug.Log($"<color=blue>[ATTACK] {gameObject.name} processing {eventType}</color>");
                    break;
            }
        }

        public void AddAttack(Attack newAttack)
        {
            children.Add(newAttack);
        }
    }
} 
