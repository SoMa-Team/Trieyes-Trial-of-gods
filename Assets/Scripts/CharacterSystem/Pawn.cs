using System.Collections.Generic;
using Utils;
using Stats;
using AttackSystem;
using AttackComponents;
using RelicSystem;
using UnityEngine;
using CardSystem;
using CardActions;
using System.Linq;
using System;

namespace CharacterSystem
{
    /// <summary>
    /// 게임 내 모든 캐릭터의 기본이 되는 클래스입니다.
    /// 캐릭터의 기본적인 속성과 동작을 정의합니다.
    /// </summary>
    public abstract class Pawn : MonoBehaviour, IEventHandler, IMovable
    {
        // ===== [기능 1] 캐릭터 기본 정보 =====
        public int pawnId { get; private set; }
        public string pawnName { get; protected set; }
        public int level { get; protected set; }
        public int maxHp { get; protected set; }
        public int currentHp { get; protected set; }
        public int gold { get; protected set; } // 골드 시스템 추가

        // ===== [기능 2] 스탯 시스템 =====
        public StatSheet statSheet = new(); // 여러 스탯 정보

        // ===== [기능 3] 이벤트 처리 =====
        public abstract void OnEvent(Utils.EventType eventType, object param);

        // ===== [기능 4] 공격 시스템 =====
        public abstract void TakeAttack(Attack attack);

        public abstract void PerformAttack(Pawn target, Attack attack);

        // ===== [기능 5] 공격/스탯/유물/덱 관리 =====
        public Attack basicAttack; // 기본 공격
        public AttackData[] attackDataList; // 여러 공격 데이터
        public List<AttackComponent> attackComponentList = new(); // 공격 컴포넌트 리스트
        public List<Relic> relics = new(); // 장착 가능한 유물 리스트
        public Deck deck = new Deck(); // Pawn이 관리하는 Deck 인스턴스

        // ===== [기능 6] 이동 및 물리/애니메이션 관련 =====
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float jumpForce = 5f;
        protected Vector2 moveDirection;
        protected bool isGrounded;
        protected Rigidbody2D rb;
        protected BoxCollider2D boxCollider;
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Animator animator;
        protected string currentAnimationState;
        protected const string IDLE_ANIM = "Idle";
        protected const string WALK_ANIM = "Walk";
        protected const string JUMP_ANIM = "Jump";
        protected const string ATTACK_ANIM = "Attack";
        protected const string HIT_ANIM = "Hit";
        protected bool isDead = false;
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            initBaseStat();
        }
        protected virtual void initBaseStat()
        {
            
        }
        public virtual void Update() 
        {
            IntegerStatValue healthStat = statSheet[StatType.Health];
            if (healthStat != null && healthStat.Value <= 0 && !isDead)
            {
                isDead = true;
                Debug.Log($"<color=red>{gameObject.name} has died. Triggering OnDeath event.</color>");
                TriggerEventActionInit(Utils.EventType.OnDeath, this);
                ChangeAnimationState("Die"); 
                if (rb != null) rb.bodyType = RigidbodyType2D.Static; 
                if (boxCollider != null) boxCollider.enabled = false; 
                Destroy(gameObject, 0.01f); 
            }
        }
        public virtual void TakeDamage(int damage)
        {
            
        }
        public virtual void Move(Vector2 direction)
        {
            moveDirection = direction;
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
            }
            if (direction.x != 0)
            {
                ChangeAnimationState(WALK_ANIM);
            }
        }
        protected virtual void FixedUpdate()
        {
            CheckGrounded();
        }
        protected virtual void CheckGrounded()
        {
            if (boxCollider != null)
            {
                isGrounded = Physics2D.OverlapBox(boxCollider.bounds.center, boxCollider.bounds.size, 0f, LayerMask.GetMask("Ground"));
            }
        }
        protected virtual void ChangeAnimationState(string newState)
        {
            if (animator != null && currentAnimationState != newState)
            {
                animator.Play(newState);
                currentAnimationState = newState;
            }
        }
        public virtual void PlayAttackAnimation()
        {
            ChangeAnimationState(ATTACK_ANIM);
        }
        public virtual void PlayHitAnimation()
        {
            ChangeAnimationState(HIT_ANIM);
        }

        // ===== [기능 4] 스탯 관련 =====
        public int GetStatValue(StatType statType)
        {
            return statSheet[statType];
        }
        
        public void SetStatValue(StatType statType, int value)
        {
            statSheet[statType].SetBasicValue(value);
        }
        
        public void IncreaseStatValue(StatType statType, int amount)
        {
            statSheet[statType].AddToBasicValue(amount);
        }
        
        public void DecreaseStatValue(StatType statType, int amount)
        {
            statSheet[statType].AddToBasicValue(-amount);
        }
        
        public void ModifyStat(StatType statType, int amount)
        {
            statSheet[statType].AddToBasicValue(amount);
        }
        
        public void ApplyStatMultiplier(float difficultyEnemyStatMultiplier)
        {
            // 모든 스탯에 배율 적용
            foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
            {
                int currentValue = statSheet[statType].Value;
                int newValue = Mathf.RoundToInt(currentValue * difficultyEnemyStatMultiplier);
                statSheet[statType].SetBasicValue(newValue);
            }
        }

        // ===== [기능 8] 덱 액션 트리거 =====
        public void TriggerEventActionInit(Utils.EventType eventType, object param = null)
        {
            // 덱의 카드 액션들 처리
            deck.CalcActionInitStat(eventType, param);
            
            // 유물들의 이벤트 처리
            foreach (var relic in relics)
            {
                if (relic != null)
                {
                    relic.OnEvent(eventType, param);
                }
            }
        }

        /// <summary>
        /// 사망 이벤트 발생 시 호출되는 메서드입니다.
        /// </summary>
        protected virtual void OnDeath(object param)
        {
            Debug.Log($"<color=red>{gameObject.name} (Pawn) is dead!</color>");
            // 기본 사망 처리 로직
        }

        // ===== [기능 9] 이벤트 엔트리 포인트들 =====
        
        /// <summary>
        /// 전투 시작 이벤트를 발생시킵니다.
        /// </summary>
        public void OnBattleStart()
        {
            Debug.Log($"<color=green>[EVENT] {gameObject.name} - OnBattleStart triggered</color>");
            TriggerEventActionInit(Utils.EventType.OnBattleStart, this);
        }

        /// <summary>
        /// 전투 클리어 이벤트를 발생시킵니다.
        /// </summary>
        public void OnBattleClear()
        {
            Debug.Log($"<color=blue>[EVENT] {gameObject.name} - OnBattleClear triggered</color>");
            TriggerEventActionInit(Utils.EventType.OnBattleClear, this);
        }

        /// <summary>
        /// OnTick 이벤트를 발생시킵니다. (30TPS로 호출 예정)
        /// </summary>
        public void OnTick()
        {
            // TODO: 30TPS로 호출되도록 구현 필요
            TriggerEventActionInit(Utils.EventType.OnTick, this);
        }

        /// <summary>
        /// 공격 성공 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="target">공격 대상</param>
        public void OnAttack(Pawn target)
        {
            Debug.Log($"<color=yellow>[EVENT] {gameObject.name} - OnAttack triggered against {target.gameObject.name}</color>");
            TriggerEventActionInit(Utils.EventType.OnAttack, new AttackEventData(this, target));
        }

        /// <summary>
        /// 치명타 공격 성공 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="target">공격 대상</param>
        public void OnCriticalAttack(Pawn target)
        {
            Debug.Log($"<color=red>[EVENT] {gameObject.name} - OnCriticalAttack triggered against {target.gameObject.name}</color>");
            TriggerEventActionInit(Utils.EventType.OnCriticalAttack, new AttackEventData(this, target));
        }

        /// <summary>
        /// 공격 투사체 명중 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="target">공격 대상</param>
        public void OnAttackHit(Pawn target)
        {
            Debug.Log($"<color=orange>[EVENT] {gameObject.name} - OnAttackHit triggered against {target.gameObject.name}</color>");
            TriggerEventActionInit(Utils.EventType.OnAttackHit, new AttackEventData(this, target));
        }

        /// <summary>
        /// 공격이 회피당한 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="target">회피한 대상</param>
        public void OnAttackMiss(Pawn target)
        {
            Debug.Log($"<color=cyan>[EVENT] {gameObject.name} - OnAttackMiss triggered against {target.gameObject.name}</color>");
            TriggerEventActionInit(Utils.EventType.OnAttackMiss, new AttackEventData(this, target));
        }

        /// <summary>
        /// 피격 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="attacker">공격자</param>
        /// <param name="projectile">투사체 (선택사항)</param>
        public void OnDamaged(Pawn attacker, Attack projectile = null)
        {
            Debug.Log($"<color=red>[EVENT] {gameObject.name} - OnDamaged triggered by {attacker.gameObject.name}</color>");
            
            // 이벤트 발생 (카드 액션이 스탯을 수정할 수 있도록)
            TriggerEventActionInit(Utils.EventType.OnDamaged, new AttackEventData(attacker, this, projectile));
            
            // 실제 데미지 계산 및 적용 (이벤트 처리 후)
            CalculateAndApplyDamage(attacker, projectile);
        }

        /// <summary>
        /// 데미지를 계산하고 적용합니다.
        /// </summary>
        /// <param name="attacker">공격자</param>
        /// <param name="projectile">투사체 (선택사항)</param>
        protected virtual void CalculateAndApplyDamage(Pawn attacker, Attack projectile = null)
        {
            if (attacker == null)
            {
                Debug.LogWarning($"<color=orange>[DAMAGE] Attacker is null for {gameObject.name}</color>");
                return;
            }

            // 기본 공격력 가져오기
            float attackPower = 10f;
            if (projectile != null)
            {
                attackPower = projectile.projectileStats[StatType.AttackPower];
            }

            // 치명타 확률과 치명타 데미지 가져오기
            float criticalRate = 0f;
            float criticalDamage = 1.5f; // 기본 치명타 배율 1.5배
            
            if (projectile != null)
            {
                criticalRate = projectile.projectileStats[StatType.CriticalRate] / 100f; // 퍼센트를 0~1 범위로 변환
                criticalDamage = projectile.projectileStats[StatType.CriticalDamage] / 100f; // 퍼센트를 배율로 변환
            }

            // 치명타 판정
            bool isCritical = UnityEngine.Random.Range(0f, 1f) < criticalRate;
            
            // 방어력 계산
            float defense = GetStatValue(StatType.Defense);
            
            // 최종 데미지 계산
            float damageMultiplier = isCritical ? criticalDamage : 1f;
            int finalDamage = Mathf.Max(1, Mathf.RoundToInt((attackPower - defense) * damageMultiplier));
            
            // HP 감소
            ChangeHP(-finalDamage);
            
            // 데미지 로그 출력
            string damageType = isCritical ? "CRITICAL" : "NORMAL";
            Debug.Log($"<color=red>[DAMAGE] {gameObject.name} took {finalDamage} {damageType} damage from {attacker.gameObject.name} (Attack: {attackPower}, Defense: {defense}, Crit: {criticalRate * 100}%, CritDmg: {criticalDamage * 100}%)</color>");
            
            // 넉백 적용 (Rigidbody2D가 있는 경우)
            ApplyKnockback(attacker);
            
            // 치명타 발생 시 공격자에게 OnCriticalAttack 이벤트 호출
            if (isCritical)
            {
                attacker.OnCriticalAttack(this);
            }
        }

        /// <summary>
        /// 넉백을 적용합니다.
        /// </summary>
        /// <param name="attacker">공격자</param>
        protected virtual void ApplyKnockback(Pawn attacker)
        {
            Rigidbody2D targetRb = GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                Vector2 knockbackDirection = (transform.position - attacker.transform.position).normalized;
                float knockbackForce = 5f; // 기본 넉백 힘
                targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                
                Debug.Log($"<color=orange>[KNOCKBACK] {gameObject.name} knocked back by {attacker.gameObject.name}</color>");
            }
        }

        /// <summary>
        /// 피격 투사체 명중 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="attacker">공격자</param>
        public void OnDamageHit(Pawn attacker)
        {
            Debug.Log($"<color=orange>[EVENT] {gameObject.name} - OnDamageHit triggered by {attacker.gameObject.name}</color>");
            
            // 회피 판정 (스탯 기반)
            float evasionRate = GetStatValue(StatType.Evasion) / 100f; // 회피율을 0~1 범위로 변환
            bool isEvaded = UnityEngine.Random.Range(0f, 1f) < evasionRate;
            
            if (isEvaded)
            {
                Debug.Log($"<color=cyan>[EVENT] {gameObject.name} - OnEvaded triggered (Evasion Rate: {evasionRate * 100}%)</color>");
                TriggerEventActionInit(Utils.EventType.OnEvaded, this);
            }
            else
            {
                TriggerEventActionInit(Utils.EventType.OnDamageHit, new AttackEventData(attacker, this));
            }
        }

        /// <summary>
        /// 회피 성공 이벤트를 발생시킵니다.
        /// </summary>
        public void OnEvaded()
        {
            Debug.Log($"<color=cyan>[EVENT] {gameObject.name} - OnEvaded triggered</color>");
            TriggerEventActionInit(Utils.EventType.OnEvaded, this);
        }

        /// <summary>
        /// 적 처치 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="target">처치된 적</param>
        public void OnKilled(Pawn target)
        {
            Debug.Log($"<color=purple>[EVENT] {gameObject.name} - OnKilled triggered against {target.gameObject.name}</color>");
            TriggerEventActionInit(Utils.EventType.OnKilled, new AttackEventData(this, target));
        }

        /// <summary>
        /// 크리티컬 공격으로 적 처치 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="target">처치된 적</param>
        public void OnKilledByCritical(Pawn target)
        {
            Debug.Log($"<color=purple>[EVENT] {gameObject.name} - OnKilledByCritical triggered against {target.gameObject.name}</color>");
            TriggerEventActionInit(Utils.EventType.OnKilledByCritical, new AttackEventData(this, target));
        }

        /// <summary>
        /// 스킬 쿨타임 완료 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="skill">쿨타임이 완료된 스킬</param>
        public void OnSkillCooldownEnd(Attack skill)
        {
            Debug.Log($"<color=green>[EVENT] {gameObject.name} - OnSkillCooldownEnd triggered for skill</color>");
            TriggerEventActionInit(Utils.EventType.OnSkillCooldownEnd, new SkillEventData(this, skill));
        }

        /// <summary>
        /// 스킬 입력 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="skill">입력된 스킬</param>
        public void OnSkillInput(Attack skill)
        {
            Debug.Log($"<color=blue>[EVENT] {gameObject.name} - OnSkillInput triggered for skill</color>");
            TriggerEventActionInit(Utils.EventType.OnSkillInput, new SkillEventData(this, skill));
        }

        /// <summary>
        /// HP 업데이트 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="preHP">이전 HP 값</param>
        public void OnHPUpdated(int preHP)
        {
            Debug.Log($"<color=green>[EVENT] {gameObject.name} - OnHPUpdated triggered (PreHP: {preHP}, CurrentHP: {currentHp})</color>");
            TriggerEventActionInit(Utils.EventType.OnHPUpdated, new StatUpdateEventData(this, preHP));
        }

        /// <summary>
        /// 골드 업데이트 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="preGold">이전 골드 값</param>
        public void OnGoldUpdated(int preGold)
        {
            Debug.Log($"<color=yellow>[EVENT] {gameObject.name} - OnGoldUpdated triggered (PreGold: {preGold})</color>");
            TriggerEventActionInit(Utils.EventType.OnGoldUpdated, new StatUpdateEventData(this, preGold));
        }

        // ===== [기능 10] 이벤트 데이터 클래스들 =====
        
        /// <summary>
        /// 공격 관련 이벤트 데이터
        /// </summary>
        public class AttackEventData
        {
            public Pawn attacker;
            public Pawn target;
            public Attack projectile; // 투사체 정보 (데미지 계산용)
            
            public AttackEventData(Pawn attacker, Pawn target)
            {
                this.attacker = attacker;
                this.target = target;
                this.projectile = null;
            }
            
            public AttackEventData(Pawn attacker, Pawn target, Attack projectile)
            {
                this.attacker = attacker;
                this.target = target;
                this.projectile = projectile;
            }
        }

        /// <summary>
        /// 스킬 관련 이벤트 데이터
        /// </summary>
        public class SkillEventData
        {
            public Pawn owner;
            public Attack skill;
            
            public SkillEventData(Pawn owner, Attack skill)
            {
                this.owner = owner;
                this.skill = skill;
            }
        }

        /// <summary>
        /// 스탯 업데이트 관련 이벤트 데이터
        /// </summary>
        public class StatUpdateEventData
        {
            public Pawn owner;
            public int previousValue;
            
            public StatUpdateEventData(Pawn owner, int previousValue)
            {
                this.owner = owner;
                this.previousValue = previousValue;
            }
        }

        // ===== [기능 8] HP 및 골드 관리 =====
        
        /// <summary>
        /// HP를 변경하고 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="amount">변경할 HP 양 (음수면 감소, 양수면 증가)</param>
        public void ChangeHP(int amount)
        {
            int preHP = currentHp;
            currentHp = Mathf.Clamp(currentHp + amount, 0, maxHp);
            
            if (preHP != currentHp)
            {
                OnHPUpdated(preHP);
            }
        }

        /// <summary>
        /// 골드를 변경하고 이벤트를 발생시킵니다.
        /// </summary>
        /// <param name="amount">변경할 골드 양 (음수면 감소, 양수면 증가)</param>
        public void ChangeGold(int amount)
        {
            int preGold = gold;
            gold = Mathf.Max(0, gold + amount);
            
            if (preGold != gold)
            {
                OnGoldUpdated(preGold);
            }
        }

        /// <summary>
        /// 골드를 획득합니다 (적 처치 시 호출).
        /// </summary>
        /// <param name="amount">획득할 골드 양</param>
        public void EarnGold(int amount)
        {
            ChangeGold(amount);
        }

        // ===== [기능 9] 유물 관리 =====
        
        /// <summary>
        /// 유물을 추가합니다.
        /// </summary>
        /// <param name="relic">추가할 유물</param>
        public void AddRelic(Relic relic)
        {
            if (relic != null)
            {
                relic.SetOwner(this);
                relics.Add(relic);
                Debug.Log($"<color=purple>[RELIC] {gameObject.name} acquired relic: {relic.info.name}</color>");
            }
        }

        /// <summary>
        /// 유물을 제거합니다.
        /// </summary>
        /// <param name="relic">제거할 유물</param>
        public void RemoveRelic(Relic relic)
        {
            if (relic != null && relics.Contains(relic))
            {
                relics.Remove(relic);
                Debug.Log($"<color=purple>[RELIC] {gameObject.name} lost relic: {relic.info.name}</color>");
            }
        }

        // ===== [기능 6] 투사체 발사 =====
        /// <summary>
        /// 공격을 발사합니다. (모든 공격의 통합 엔트리 포인트)
        /// </summary>
        /// <param name="attackPrefab">발사할 Attack 프리팹</param>
        /// <param name="direction">발사 방향</param>
        /// <param name="parentAttack">부모 Attack (선택사항)</param>
        public virtual void FireAttack(Attack attackPrefab, Vector2 direction, Attack parentAttack = null)
        {
            if (attackPrefab == null)
            {
                Debug.LogWarning($"<color=red>[PROJECTILE] {gameObject.name} - attackPrefab is null</color>");
                return;
            }

            // 1. 연산을 통해 투사체가 가져야하는 스탯 정보를 갱신
            // (이미 Pawn의 현재 스탯이 최신 상태이므로 별도 갱신 불필요)

            // 2. 투사체를 몇개, 그리고 어떤 효과를 가지는 Comp를 몇개 만들어야 하는지 알아옴
            // (현재는 단일 투사체, 향후 확장 가능)

            // 3. Attack 객체를 만들고 쏨
            Attack projectile = Instantiate(attackPrefab, transform.position, Quaternion.identity);
            
            // 투사체 초기화
            projectile.InitializeProjectile(this, direction, parentAttack);
            
            Debug.Log($"<color=green>[PROJECTILE] {gameObject.name} fired attack {projectile.gameObject.name}</color>");
        }

        /// <summary>
        /// 기본 공격을 발사합니다.
        /// </summary>
        /// <param name="direction">발사 방향</param>
        public virtual void FireBasicAttack(Vector2 direction)
        {
            if (basicAttack != null)
            {
                FireAttack(basicAttack, direction);
            }
            else
            {
                Debug.LogWarning($"<color=red>[PROJECTILE] {gameObject.name} - basicAttack is null</color>");
            }
        }
    }
} 