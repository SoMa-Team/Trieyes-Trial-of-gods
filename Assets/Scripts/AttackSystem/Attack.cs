using System.Collections.Generic;
using CharacterSystem;
using Utils;
using UnityEngine;
using AttackComponents;
using Stats;
using RelicSystem;
using CardSystem;

namespace AttackSystem
{
    /// <summary>
    /// 게임 내 공격 행위를 정의하는 클래스입니다.
    /// 이 클래스는 IEventHandler를 구현하여 자체적으로 이벤트를 처리하고 발동시킬 수 있습니다.
    /// </summary>
    public class Attack : MonoBehaviour, IEventHandler
    {
        // ===== [기능 1] 공격 데이터 및 컴포넌트 관리 =====
        public AttackData attackData;
        public Pawn attacker; // 공격자 (투사체를 발사한 캐릭터)
        public Attack parentAttack; // 부모 Attack (투사체가 다른 Attack의 하위인 경우)
        public List<AttackComponent> components = new List<AttackComponent>();
        public StatSheet projectileStats = new(); // 투사체가 가질 스탯 정보 (복사본)
        
        // ===== [기능 2] 투사체 관련 =====
        [SerializeField] protected int pierceCount = 1; // 관통 개수
        [SerializeField] protected float projectileSpeed = 10f; // 투사체 속도
        [SerializeField] protected float maxDistance = 20f; // 최대 이동 거리 (화면 크기의 2배)
        protected int currentPierceCount = 0; // 현재 관통 횟수
        protected float currentDistance = 0f; // 현재 이동 거리
        protected Vector3 spawnPosition; // 발사 위치
        protected float currentLifetime = 0f; // 현재 수명
        protected Rigidbody2D rb;
        protected Collider2D attackCollider;
        
        // ===== [기능 3] 충돌 처리 =====
        protected HashSet<GameObject> hitTargets = new HashSet<GameObject>(); // 이미 맞은 대상들 (관통 시 중복 방지)

        protected virtual void Awake()
        {
            Activate();
        }

        protected virtual void OnDestroy()
        {
            Deactivate();
        }

        protected virtual void Start()
        {
            // 투사체 초기화
            currentPierceCount = 0;
            currentLifetime = 0f;
            hitTargets.Clear();
        }

        protected virtual void Update()
        {
            // 투사체 거리 관리
            // currentDistance = Vector3.Distance(transform.position, spawnPosition);
            // if (currentDistance >= maxDistance)
            // {
            //     DestroyProjectile();
            // }
        }

        // ===== [기능 4] 투사체 초기화 =====
        /// <summary>
        /// Attack 컴포넌트를 초기화합니다. Pawn에서 호출됩니다.
        /// </summary>
        /// <param name="attacker">공격자</param>
        public virtual void Initialize(Pawn attacker)
        {
            this.attacker = attacker;
            
            // 컴포넌트 초기화
            rb = GetComponent<Rigidbody2D>();
            attackCollider = GetComponent<Collider2D>();

            // attacker의 스탯 정보를 기반으로 공격 데이터 초기화
            // attackData = new AttackData(attacker.statSheet);
            
            // 기본 AttackComponent들 초기화
            var existingComponents = GetComponents<AttackComponent>();
            components.AddRange(existingComponents);
            
            Debug.Log($"<color=blue>[ATTACK] {gameObject.name} initialized for {attacker?.gameObject.name}</color>");
        }

        /// <summary>
        /// 투사체를 초기화합니다.
        /// </summary>
        /// <param name="attacker">공격자</param>
        /// <param name="direction">발사 방향</param>
        /// <param name="parentAttack">부모 Attack (선택사항)</param>
        public virtual void InitializeProjectile(Pawn attacker, Vector2 direction, Attack parentAttack = null)
        {
            this.attacker = attacker;
            this.parentAttack = parentAttack;
            this.spawnPosition = transform.position; // 발사 위치 저장
            
            // 부모 Attack에서 공격자 정보 가져오기
            if (parentAttack != null && this.attacker == null)
            {
                this.attacker = parentAttack.attacker;
            }
            
            // 스탯 정보 복사 (깊은 복사)
            CopyStatsFromPawn(attacker);
            
            // AttackComponent 조립 (Relic, CardAction에서 AttackComponent 추가)
            AssembleAttackComponents();
            
            // AttackComponent의 colliderSizeDelta를 모두 합산하여 Collider 크기 조정
            AdjustColliderSize();
            
            // 투사체 이동 설정
            if (rb != null)
            {
                rb.linearVelocity = direction.normalized * projectileSpeed;
            }
            
            // 투사체 회전 (이동 방향으로)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            Debug.Log($"<color=blue>[PROJECTILE] {gameObject.name} initialized by {attacker?.gameObject.name}</color>");
        }

        /// <summary>
        /// Relic, CardAction을 조회하여 AttackComponent를 조립합니다.
        /// </summary>
        protected virtual void AssembleAttackComponents()
        {
            if (attacker == null) return;
            
            // 기존 AttackComponent 목록 초기화
            components.Clear();
            
            // 기본 AttackComponent 추가 (이미 부착된 것들)
            var existingComponents = GetComponents<AttackComponent>();
            components.AddRange(existingComponents);
            
            // Relic에서 AttackComponent 추가
            foreach (var relic in attacker.relics)
            {
                if (relic != null)
                {
                    // Relic이 AttackComponent를 제공하는 경우 추가
                    var relicComponents = relic.GetAttackComponents();
                    if (relicComponents != null)
                    {
                        components.AddRange(relicComponents);
                    }
                }
            }
            
            // CardAction에서 AttackComponent 추가 (덱의 카드들)
            foreach (var card in attacker.deck.GetCards())
            {
                if (card?.cardAction != null)
                {
                    var cardComponents = card.cardAction.GetAttackComponents();
                    if (cardComponents != null)
                    {
                        components.AddRange(cardComponents);
                    }
                }
            }
            
            Debug.Log($"<color=green>[PROJECTILE] {gameObject.name} assembled {components.Count} attack components</color>");
        }

        /// <summary>
        /// AttackComponent의 colliderSizeDelta를 모두 합산하여 Collider 크기를 조정합니다.
        /// </summary>
        protected virtual void AdjustColliderSize()
        {
            if (attackCollider is BoxCollider2D boxCollider)
            {
                Vector2 baseSize = boxCollider.size;
                Vector2 totalDelta = Vector2.zero;
                
                foreach (var comp in components)
                {
                    if (comp != null)
                    {
                        totalDelta += comp.colliderSizeDelta;
                    }
                }
                
                boxCollider.size = baseSize + totalDelta;
                
                Debug.Log($"<color=cyan>[PROJECTILE] {gameObject.name} collider size adjusted: {baseSize} + {totalDelta} = {boxCollider.size}</color>");
            }
        }

        /// <summary>
        /// Pawn의 스탯을 투사체에 복사합니다.
        /// </summary>
        /// <param name="pawn">스탯을 복사할 Pawn</param>
        protected virtual void CopyStatsFromPawn(Pawn pawn)
        {
            if (pawn == null) return;
            
            // StatSheet는 Dictionary 기반이므로 Clear() 대신 새로운 인스턴스 생성
            projectileStats = new StatSheet();

            // Pawn의 모든 스탯을 깊은 복사
            foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
            {
                // Pawn의 스탯 값을 가져와서 새로운 IntegerStatValue로 복사
                int pawnStatValue = pawn.GetStatValue(statType);
                projectileStats[statType].SetBasicValue(pawnStatValue);
            }
            
            Debug.Log($"<color=green>[PROJECTILE] {gameObject.name} copied stats from {pawn.gameObject.name}</color>");
        }

        // ===== [기능 5] 충돌 처리 =====
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            HandleCollision(other.gameObject);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            HandleCollision(collision.gameObject);
        }

        /// <summary>
        /// 충돌을 처리합니다.
        /// </summary>
        /// <param name="hitObject">충돌한 객체</param>
        protected virtual void HandleCollision(GameObject hitObject)
        {
            // 이미 맞은 대상인지 확인 (관통 시 중복 방지)
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

            if (hitPawn != null && attacker != null)
            {
                // 공격자와 피격자가 다른 경우에만 처리
                if (hitPawn != attacker)
                {
                    ProcessAttackCollision(hitPawn, hitObject);
                }
            }
        }

        /// <summary>
        /// 공격 충돌을 처리합니다.
        /// </summary>
        /// <param name="targetPawn">피격 대상</param>
        /// <param name="hitObject">충돌한 객체</param>
        protected virtual void ProcessAttackCollision(Pawn targetPawn, GameObject hitObject)
        {
            Debug.Log($"<color=orange>[COLLISION] {gameObject.name} hit {targetPawn.gameObject.name} ({targetPawn.GetType().Name})</color>");
            
            // 이미 맞은 대상으로 기록
            hitTargets.Add(hitObject);
            
            // 이벤트 발생 순서: OnAttackHit → OnDamageHit → 회피 판정 → OnAttackMiss/OnAttack
            if (attacker != null)
            {
                Debug.Log($"<color=yellow>[EVENT] {gameObject.name} -> Attacker {attacker.gameObject.name} ({attacker.GetType().Name}) OnAttackHit -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name})</color>");
                // 1. 공격자의 OnAttackHit 이벤트 (유물, 카드 순회)
                attacker.OnEvent(Utils.EventType.OnAttackHit, targetPawn);
                
                Debug.Log($"<color=red>[EVENT] {gameObject.name} -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name}) OnDamageHit <- Attacker {attacker.gameObject.name} ({attacker.GetType().Name})</color>");
                // 2. 피격자의 OnDamageHit 이벤트 (회피 판정)
                targetPawn.OnEvent(Utils.EventType.OnDamageHit, attacker);
                
                // 3. 회피 판정 (피격자에서)
                bool isEvaded = CheckEvasion(targetPawn);
                
                if (isEvaded)
                {
                    Debug.Log($"<color=cyan>[EVENT] {gameObject.name} -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name}) OnEvaded (SUCCESS)</color>");
                    Debug.Log($"<color=cyan>[EVENT] {gameObject.name} -> Attacker {attacker.gameObject.name} ({attacker.GetType().Name}) OnAttackMiss -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name})</color>");
                    // 회피 성공: OnEvaded (피격자) + OnAttackMiss (공격자)
                    targetPawn.OnEvent(Utils.EventType.OnEvaded, null);
                    attacker.OnEvent(Utils.EventType.OnAttackMiss, targetPawn);
                }
                else
                {
                    Debug.Log($"<color=green>[EVENT] {gameObject.name} -> Attacker {attacker.gameObject.name} ({attacker.GetType().Name}) OnAttack -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name})</color>");
                    // 회피 실패: OnAttack (공격자) - 데미지 계산 및 OnDamaged 호출
                    attacker.OnEvent(Utils.EventType.OnAttack, targetPawn);
                }
            }
            
            // 관통 개수 증가
            currentPierceCount++;
            
            // 관통 한계에 도달했는지 확인
            if (currentPierceCount >= pierceCount)
            {
                Debug.Log($"<color=red>[PROJECTILE] {gameObject.name} reached pierce limit ({pierceCount})</color>");
                DestroyProjectile();
            }
        }

        /// <summary>
        /// 회피 판정을 수행합니다.
        /// </summary>
        /// <param name="targetPawn">피격 대상</param>
        /// <returns>회피 성공 여부</returns>
        protected virtual bool CheckEvasion(Pawn targetPawn)
        {
            // 피격자의 회피율 계산
            float evasionRate = targetPawn.GetStatValue(StatType.Evasion) / 100f;
            bool isEvaded = UnityEngine.Random.Range(0f, 1f) < evasionRate;
            
            Debug.Log($"<color=cyan>[EVASION] {targetPawn.gameObject.name} evasion check: {evasionRate * 100}% -> {(isEvaded ? "SUCCESS" : "FAILED")}</color>");
            
            return isEvaded;
        }

        // ===== [기능 6] 투사체 파괴 =====
        /// <summary>
        /// 투사체를 파괴합니다.
        /// </summary>
        protected virtual void DestroyProjectile()
        {
            Debug.Log($"<color=red>[PROJECTILE] {gameObject.name} destroyed</color>");
            
            // 오브젝트 풀링을 위한 비활성화
            Deactivate();
            gameObject.SetActive(false);
            
            // 또는 완전히 파괴
            Destroy(gameObject);
        }

        // ===== [기능 7] 공격 실행 =====
        public virtual void Execute(Pawn target)
        {
            // 공격 실행 로직
            Debug.Log($"<color=yellow>[ATTACK] {gameObject.name} executing attack on {target.gameObject.name}</color>");
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        public virtual void Activate()
        {
            // 컴포넌트 초기화
            rb = GetComponent<Rigidbody2D>();
            attackCollider = GetComponent<Collider2D>();
            
            if (rb != null)
            {
                rb.gravityScale = 0f; // 중력 비활성화
            }
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public virtual void Deactivate()
        {
            // 투사체 상태 초기화
            currentPierceCount = 0;
            currentLifetime = 0f;
            currentDistance = 0f;
            hitTargets.Clear();
            
            // 컴포넌트 정리
            components.Clear();
            
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
            parentAttack = null;
        }

        public void Execute()
        {
            foreach (var comp in components)
                comp.Execute(this);
        }
        
        // ===== [기능 8] 이벤트 처리 =====
        public void OnEvent(Utils.EventType eventType, object param)
        {
            // 이벤트 처리 로직
            Debug.Log($"<color=blue>[EVENT] {gameObject.name} received event: {eventType}</color>");
            
            // 이벤트 타입에 따른 처리
            switch (eventType)
            {
                case Utils.EventType.OnAttackHit:
                    // 공격 시작 이벤트 처리
                    break;
                case Utils.EventType.OnAttack:
                    // 공격 종료 이벤트 처리
                    break;
                case Utils.EventType.OnDamageHit:
                    // 타격 이벤트 처리
                    break;
                default:
                    // 기본 이벤트 처리
                    break;
            }
        }
    }
} 