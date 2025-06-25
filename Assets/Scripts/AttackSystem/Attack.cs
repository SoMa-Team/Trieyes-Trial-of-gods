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
        public Attack parentAttack; // 부모 Attack (null이면 관리자, 아니면 투사체)
        public List<AttackComponent> components = new List<AttackComponent>();
        public Stats.StatSheet projectileStats = new(); // 투사체가 가질 스탯 정보 (복사본)
        
        // ===== [기능 2] 투사체 관련 (투사체일 때만 사용) =====
        [SerializeField] protected int pierceCount = 1; // 관통 개수
        [SerializeField] protected float projectileSpeed = 10f; // 투사체 속도
        [SerializeField] protected float maxDistance = 20f; // 최대 이동 거리
        protected int currentPierceCount = 0; // 현재 관통 횟수
        protected float currentDistance = 0f; // 현재 이동 거리
        protected Vector3 spawnPosition; // 발사 위치
        protected float currentLifetime = 0f; // 현재 수명
        protected Rigidbody2D rb;
        protected Collider2D attackCollider;
        
        // ===== [기능 3] 충돌 처리 (투사체일 때만 사용) =====
        protected HashSet<GameObject> hitTargets = new HashSet<GameObject>(); // 이미 맞은 대상들 (관통 시 중복 방지)

        // ===== [기능 4] 역할 구분 프로퍼티 =====
        /// <summary>
        /// 이 Attack이 관리자인지 확인합니다.
        /// </summary>
        public bool IsManager => parentAttack == null;
        
        /// <summary>
        /// 이 Attack이 투사체인지 확인합니다.
        /// </summary>
        public bool IsProjectile => parentAttack != null;

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
            // 투사체일 때만 초기화
            if (IsProjectile)
            {
                // 투사체 초기화
                currentPierceCount = 0;
                currentLifetime = 0f;
                hitTargets.Clear();
                
                Debug.Log($"<color=blue>[ATTACK_PROJECTILE] {gameObject.name} started as projectile</color>");
            }
            else
            {
                Debug.Log($"<color=blue>[ATTACK_MANAGER] {gameObject.name} started as manager</color>");
            }
        }

        protected virtual void Update()
        {
            // 투사체일 때만 Update 처리
            if (IsProjectile)
            {
                // 투사체 거리 관리
                currentDistance = Vector3.Distance(transform.position, spawnPosition);
                if (currentDistance >= maxDistance)
                {
                    Debug.Log($"<color=orange>[ATTACK_PROJECTILE] {gameObject.name} reached max distance ({maxDistance})</color>");
                    DestroyProjectile();
                }
                
                // 투사체 수명 관리
                currentLifetime += Time.deltaTime;
                if (currentLifetime > 10f) // 10초 후 자동 파괴
                {
                    Debug.Log($"<color=orange>[ATTACK_PROJECTILE] {gameObject.name} lifetime expired</color>");
                    DestroyProjectile();
                }
            }
        }

        // ===== [기능 5] 초기화 =====
        /// <summary>
        /// Attack 컴포넌트를 초기화합니다. Pawn에서 호출됩니다. (관리자용)
        /// </summary>
        /// <param name="attacker">공격자</param>
        public virtual void Initialize(Pawn attacker)
        {
            this.attacker = attacker;
            this.parentAttack = null; // 관리자임을 명시
            
            // 컴포넌트 초기화
            rb = GetComponent<Rigidbody2D>();
            attackCollider = GetComponent<Collider2D>();
            
            // 기본 AttackComponent들 초기화
            var existingComponents = GetComponents<AttackComponent>();
            components.AddRange(existingComponents);
            
            Debug.Log($"<color=blue>[ATTACK_MANAGER] {gameObject.name} initialized as manager for {attacker?.gameObject.name}</color>");
        }

        /// <summary>
        /// 투사체를 초기화합니다. (투사체용)
        /// </summary>
        /// <param name="attacker">공격자</param>
        /// <param name="direction">발사 방향</param>
        /// <param name="parentAttack">부모 Attack (관리자)</param>
        public virtual void InitializeProjectile(Pawn attacker, Vector2 direction, Attack parentAttack = null)
        {
            this.attacker = attacker;
            this.parentAttack = parentAttack; // 투사체임을 명시
            this.spawnPosition = transform.position; // 발사 위치 저장
            
            // 부모 Attack에서 공격자 정보 가져오기
            if (parentAttack != null && this.attacker == null)
            {
                this.attacker = parentAttack.attacker;
            }
            
            // 투사체 전용 컴포넌트 초기화
            rb = GetComponent<Rigidbody2D>();
            attackCollider = GetComponent<Collider2D>();
            
            // AttackComponent 조립 (Relic, CardAction에서 AttackComponent 추가)
            AssembleAttackComponents();
            
            // AttackComponent의 colliderSizeDelta를 모두 합산하여 Collider 크기 조정
            AdjustColliderSize();
            
            // 투사체 이동 설정 (자동 움직임 활성화)
            if (rb != null)
            {
                rb.linearVelocity = direction.normalized * projectileSpeed;
                rb.gravityScale = 0f; // 중력 비활성화
            }
            
            // 투사체 회전 (이동 방향으로)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            Debug.Log($"<color=blue>[ATTACK_PROJECTILE] {gameObject.name} initialized as projectile by {attacker?.gameObject.name}, moving at {projectileSpeed} speed</color>");
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

        // ===== [기능 6] 충돌 처리 (투사체일 때만 사용) =====
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            // 투사체일 때만 충돌 처리
            if (IsProjectile)
            {
                HandleCollision(other.gameObject);
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            // 투사체일 때만 충돌 처리
            if (IsProjectile)
            {
                HandleCollision(collision.gameObject);
            }
        }

        /// <summary>
        /// 충돌을 처리합니다. (투사체 전용)
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
        /// 공격 충돌을 처리합니다. (투사체 전용)
        /// </summary>
        /// <param name="targetPawn">피격 대상</param>
        /// <param name="hitObject">충돌한 객체</param>
        protected virtual void ProcessAttackCollision(Pawn targetPawn, GameObject hitObject)
        {
            Debug.Log($"<color=orange>[ATTACK_PROJECTILE] {gameObject.name} hit {targetPawn.gameObject.name} ({targetPawn.GetType().Name})</color>");
            
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
                Debug.Log($"<color=red>[ATTACK_PROJECTILE] {gameObject.name} reached pierce limit ({pierceCount})</color>");
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

        // ===== [기능 7] 투사체 파괴 (투사체 전용) =====
        /// <summary>
        /// 투사체를 파괴합니다. (투사체 전용)
        /// </summary>
        protected virtual void DestroyProjectile()
        {
            Debug.Log($"<color=red>[ATTACK_PROJECTILE] {gameObject.name} destroyed</color>");
            
            // 오브젝트 풀링을 위한 비활성화
            Deactivate();
            gameObject.SetActive(false);
            
            // 또는 완전히 파괴
            Destroy(gameObject);
        }

        // ===== [기능 8] 공격 실행 =====
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
        
        // ===== [기능 9] 이벤트 처리 =====
        public void OnEvent(Utils.EventType eventType, object param)
        {
            // 역할에 따른 로그 출력
            string role = IsManager ? "MANAGER" : "PROJECTILE";
            Debug.Log($"<color=blue>[ATTACK_{role}] {gameObject.name} received event: {eventType}</color>");
            
            // 이벤트 타입에 따른 처리
            switch (eventType)
            {
                case Utils.EventType.OnAttackHit:
                    // 공격 시작 이벤트 처리
                    if (IsManager)
                    {
                        Debug.Log($"<color=blue>[ATTACK_MANAGER] {gameObject.name} processing OnAttackHit as manager</color>");
                    }
                    else
                    {
                        Debug.Log($"<color=blue>[ATTACK_PROJECTILE] {gameObject.name} processing OnAttackHit as projectile</color>");
                    }
                    break;
                case Utils.EventType.OnAttack:
                    // 공격 종료 이벤트 처리
                    if (IsManager)
                    {
                        Debug.Log($"<color=blue>[ATTACK_MANAGER] {gameObject.name} processing OnAttack as manager</color>");
                    }
                    else
                    {
                        Debug.Log($"<color=blue>[ATTACK_PROJECTILE] {gameObject.name} processing OnAttack as projectile</color>");
                    }
                    break;
                case Utils.EventType.OnDamageHit:
                    // 타격 이벤트 처리
                    if (IsManager)
                    {
                        Debug.Log($"<color=blue>[ATTACK_MANAGER] {gameObject.name} processing OnDamageHit as manager</color>");
                    }
                    else
                    {
                        Debug.Log($"<color=blue>[ATTACK_PROJECTILE] {gameObject.name} processing OnDamageHit as projectile</color>");
                    }
                    break;
                default:
                    // 기본 이벤트 처리
                    if (IsManager)
                    {
                        Debug.Log($"<color=blue>[ATTACK_MANAGER] {gameObject.name} processing {eventType} as manager</color>");
                    }
                    else
                    {
                        Debug.Log($"<color=blue>[ATTACK_PROJECTILE] {gameObject.name} processing {eventType} as projectile</color>");
                    }
                    break;
            }
        }

        // ===== [기능 10] 공격 실행 (관리자 전용) =====
        /// <summary>
        /// 스탯 정보를 받아서 공격을 실행합니다. (관리자 전용)
        /// </summary>
        /// <param name="attackStats">공격에 사용할 스탯 정보</param>
        public virtual void ExecuteAttackWithStats(Stats.StatSheet attackStats)
        {
            if (!IsManager)
            {
                Debug.LogWarning($"<color=yellow>[ATTACK_MANAGER] {gameObject.name} is not a manager, cannot execute attack</color>");
                return;
            }
            
            Debug.Log($"<color=green>[ATTACK_MANAGER] {gameObject.name} executing attack with stats</color>");
            
            // 2단계: AttackData와 StatSheet를 기반으로 투사체 생성
            CreateProjectiles(attackData, attackStats);
        }
        
        /// <summary>
        /// 투사체들을 생성합니다.
        /// </summary>
        /// <param name="data">공격 데이터</param>
        /// <param name="attackStats">공격 스탯</param>
        protected virtual void CreateProjectiles(AttackData data, Stats.StatSheet attackStats)
        {
            if (data == null || data.statSheet == null)
            {
                Debug.LogWarning($"<color=yellow>[ATTACK_MANAGER] {gameObject.name} has no attack data or statSheet</color>");
                return;
            }
            int projectileCount = data.statSheet[Stats.StatType.ProjectileCount].Value;
            float spreadAngle = data.statSheet[Stats.StatType.ProjectileSpread].Value;
            if (projectileCount <= 1)
            {
                Vector2 direction = Vector2.right;
                CreateSingleProjectile(direction, data, attackStats);
            }
            else
            {
                float angleStep = spreadAngle / (projectileCount - 1);
                float startAngle = -spreadAngle / 2f;
                for (int i = 0; i < projectileCount; i++)
                {
                    float angle = startAngle + (angleStep * i);
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
                    CreateSingleProjectile(direction, data, attackStats);
                }
            }
        }
        
        /// <summary>
        /// 단일 투사체를 생성합니다.
        /// </summary>
        /// <param name="direction">발사 방향</param>
        /// <param name="data">공격 데이터</param>
        /// <param name="attackStats">공격 스탯</param>
        public virtual void CreateSingleProjectile(Vector2 direction, AttackData data, Stats.StatSheet attackStats)
        {
            // 투사체 프리팹 생성 (임시로 현재 오브젝트를 복제)
            // TO-DO : 화살 같은 투사체 프리팹 만들어서 여기에 넣기
            GameObject projectileObj = Instantiate(gameObject, transform.position, Quaternion.identity);
            Attack projectile = projectileObj.GetComponent<Attack>();
            
            if (projectile != null)
            {
                // 3단계: 기본 투사체 초기화
                projectile.InitializeProjectile(attacker, direction, this);
                
                // 투사체 스탯 설정 (ProjectileInfo 사용)
                projectile.SetProjectileStats(attackStats, data);
                
                // 4단계: AssembleAttackComponents 실행
                projectile.AssembleAttackComponents();
                
                // 5단계: 투사체 스탯 업데이트
                projectile.UpdateProjectileStatsFromComponents();
                
                Debug.Log($"<color=blue>[ATTACK_MANAGER] {gameObject.name} created projectile {projectileObj.name}</color>");
            }
            else
            {
                Debug.LogError($"<color=red>[ATTACK_MANAGER] {gameObject.name} failed to create projectile</color>");
                Destroy(projectileObj);
            }
        }
        
        /// <summary>
        /// 투사체 스탯을 설정합니다. (투사체 전용)
        /// </summary>
        /// <param name="baseStats">기본 스탯</param>
        /// <param name="data">공격 데이터</param>
        public virtual void SetProjectileStats(Stats.StatSheet baseStats, AttackData data)
        {
            if (!IsProjectile) return;
            
            // 기본 스탯 복사
            projectileStats = new Stats.StatSheet();
            foreach (Stats.StatType statType in System.Enum.GetValues(typeof(Stats.StatType)))
            {
                projectileStats[statType].SetBasicValue(baseStats[statType].Value);
            }
            
            // StatSheet에서 투사체 전용 스탯 설정
            pierceCount = baseStats[Stats.StatType.ProjectilePierce].Value;
            projectileSpeed = baseStats[Stats.StatType.ProjectileSpeed].Value;
            
            Debug.Log($"<color=cyan>[ATTACK_PROJECTILE] {gameObject.name} stats set: pierce={pierceCount}, speed={projectileSpeed}</color>");
        }
        
        /// <summary>
        /// AttackComponents에서 투사체 스탯을 업데이트합니다. (투사체 전용)
        /// </summary>
        public virtual void UpdateProjectileStatsFromComponents()
        {
            if (!IsProjectile) return;
            
            // AttackComponents에서 스탯 수정
            foreach (var component in components)
            {
                if (component != null)
                {
                    component.ModifyProjectileStats(projectileStats);
                }
            }
            
            // 최종 스탯 적용
            pierceCount = projectileStats[Stats.StatType.ProjectilePierce].Value;
            projectileSpeed = projectileStats[Stats.StatType.ProjectileSpeed].Value;
            
            Debug.Log($"<color=cyan>[ATTACK_PROJECTILE] {gameObject.name} final stats: pierce={pierceCount}, speed={projectileSpeed}</color>");
        }

        protected const string IDLE_ANIM = "char001_stand";
        protected const string WALK_ANIM = "char001_walk";
        protected const string ATTACK_ANIM = "char001_stand"; // 공격 애니메이션이 따로 없으면 임시로 stand 사용
        protected const string HIT_ANIM = "char001_stand";    // 피격 애니메이션이 따로 없으면 임시로 stand 사용
    }
} 