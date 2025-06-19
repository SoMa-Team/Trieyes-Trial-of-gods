using System.Collections.Generic;
using CharacterSystem;
using Utils;
using UnityEngine;
using AttackComponents;
using Stats;

namespace AttackSystem
{
    /// <summary>
    /// 게임 내 공격 행위를 정의하는 클래스입니다.
    /// 이 클래스는 IEventHandler를 구현하여 자체적으로 이벤트를 처리하고 발동시킬 수 있습니다.
    /// </summary>
    public abstract class Attack : MonoBehaviour, IEventHandler
    {
        // ===== [기능 1] 공격 데이터 및 컴포넌트 관리 =====
        public AttackData attackData;
        public Pawn attacker; // 공격자 (투사체를 발사한 캐릭터)
        public Attack parentAttack; // 부모 Attack (투사체가 다른 Attack의 하위인 경우)
        public List<AttackComponent> components = new List<AttackComponent>();
        public List<StatInfo> projectileStats = new List<StatInfo>(); // 투사체가 가질 스탯 정보 (복사본)
        
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
            rb = GetComponent<Rigidbody2D>();
            attackCollider = GetComponent<Collider2D>();
            
            if (rb != null)
            {
                rb.gravityScale = 0f; // 중력 비활성화
            }
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
            currentDistance = Vector3.Distance(transform.position, spawnPosition);
            if (currentDistance >= maxDistance)
            {
                DestroyProjectile();
            }
        }

        // ===== [기능 4] 투사체 초기화 =====
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
            
            // 투사체 이동 설정
            if (rb != null)
            {
                rb.velocity = direction.normalized * projectileSpeed;
            }
            
            // 투사체 회전 (이동 방향으로)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            Debug.Log($"<color=blue>[PROJECTILE] {gameObject.name} initialized by {attacker?.gameObject.name}</color>");
        }

        /// <summary>
        /// Pawn의 스탯을 투사체에 복사합니다.
        /// </summary>
        /// <param name="pawn">스탯을 복사할 Pawn</param>
        protected virtual void CopyStatsFromPawn(Pawn pawn)
        {
            if (pawn == null) return;
            
            projectileStats.Clear();
            
            // Pawn의 모든 스탯을 깊은 복사
            foreach (var stat in pawn.statInfos)
            {
                projectileStats.Add(new StatInfo(stat.Type, stat.Value));
            }
            
            Debug.Log($"<color=green>[PROJECTILE] {gameObject.name} copied {projectileStats.Count} stats from {pawn.gameObject.name}</color>");
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
            Debug.Log($"<color=orange>[COLLISION] {gameObject.name} hit {targetPawn.gameObject.name}</color>");
            
            // 이미 맞은 대상으로 기록
            hitTargets.Add(hitObject);
            
            // 이벤트 발생 순서: OnAttackHit → OnDamageHit → 회피 판정 → OnAttackMiss/OnAttack + OnEvaded/OnDamaged
            if (attacker != null)
            {
                // 1. 공격자의 OnAttackHit 이벤트
                attacker.OnAttackHit(targetPawn);
                
                // 2. 피격자의 OnDamageHit 이벤트
                targetPawn.OnDamageHit(attacker);
                
                // 3. 회피 판정 (피격자에서)
                bool isEvaded = CheckEvasion(targetPawn);
                
                if (isEvaded)
                {
                    // 회피 성공: OnEvaded (피격자) + OnAttackMiss (공격자)
                    targetPawn.OnEvaded();
                    attacker.OnAttackMiss(targetPawn);
                }
                else
                {
                    // 회피 실패: OnDamaged (피격자) + OnAttack (공격자)
                    // 투사체 정보를 포함하여 이벤트 발생
                    targetPawn.OnDamaged(attacker, this);
                    attacker.OnAttack(targetPawn);
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
            
            // 오브젝트 풀링을 위한 비활성화 (실제 구현에서는 풀로 반환)
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

        public void Activate()
        {
            Debug.Log("Attack Activated!");
            // 공격 활성화 로직
        }

        public void Deactivate()
        {
            Debug.Log("Attack Deactivated!");
            // 공격 비활성화 로직
        }

        public void Execute()
        {
            foreach (var comp in components)
                comp.Execute(this);
        }
        
        // ===== [기능 8] 이벤트 처리 =====
        public abstract void OnEvent(Utils.EventType eventType, object param);
    }
} 