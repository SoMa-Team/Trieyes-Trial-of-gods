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
        [Header("Pawn Settings")]
        public int maxHp = 100;
        public int currentHp;
        public float moveSpeed = 5f;
        
        [Header("Components")]
        public Rigidbody2D rb;
        public BoxCollider2D boxCollider;
        public Animator animator;
        
        [Header("Stats")]
        public StatSheet statSheet;
        
        // 이벤트 처리용
        private List<object> eventHandlers = new List<object>();
        
        // ===== [기능 1] 캐릭터 기본 정보 =====
        public int pawnId { get; private set; }
        public string pawnName { get; protected set; }
        public int level { get; protected set; }
        public int gold { get; protected set; } // 골드 시스템 추가

        // ===== [기능 2] 스탯 시스템 =====
        public Attack basicAttack; // 기본 공격
        public AttackData[] attackDataList; // 여러 공격 데이터
        public List<AttackComponent> attackComponentList = new(); // 공격 컴포넌트 리스트
        public List<Relic> relics = new(); // 장착 가능한 유물 리스트
        public Deck deck = new Deck(); // Pawn이 관리하는 Deck 인스턴스

        // ===== [기능 6] 이동 및 물리/애니메이션 관련 =====
        protected Vector2 moveDirection;
        [SerializeField] protected SpriteRenderer spriteRenderer;
        protected string currentAnimationState;
        protected const string IDLE_ANIM = "Idle";
        protected const string WALK_ANIM = "Walk";
        protected const string ATTACK_ANIM = "Attack";
        protected const string HIT_ANIM = "Hit";
        
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            statSheet = new StatSheet();
            initBaseStat();
        }
        
        protected virtual void initBaseStat()
        {
            
        }
        
        public virtual void Update() 
        {
            HandleMovement();
            HandleAnimation();
        }
        
        public virtual void TakeDamage(int damage)
        {
            
        }
        
        public virtual void Move(Vector2 direction)
        {
            moveDirection = direction;
            if (rb != null)
            {
                // 탑다운 게임용 2D 이동
                rb.linearVelocity = direction * moveSpeed;
            }
            
            // 이동 방향에 따른 애니메이션 처리
            if (direction.magnitude > 0.1f)
            {
                ChangeAnimationState(WALK_ANIM);
            }
            else
            {
                ChangeAnimationState(IDLE_ANIM);
            }
        }
        
        protected virtual void HandleMovement()
        {
            // 하위 클래스에서 구현
        }
        
        protected virtual void HandleAnimation()
        {
            // 하위 클래스에서 구현
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
        // 많이 안쓰일 것 같다면 지워도 됨
        public int GetStatValue(StatType statType)
        {
            return statSheet[statType];
        }
        
        public void SetStatValue(StatType statType, int value)
        {
            statSheet[statType].SetBasicValue(value);
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

        // ===== [기능 8] 이벤트 발생 메서드들 =====
        /// <summary>
        /// 공격 이벤트 발생 시 유물, 카드, AttackComp를 순회하여 StatSheet를 수정합니다.
        /// </summary>
        /// <param name="target">공격 대상</param>
        /// <param name="tempStatSheet">수정할 임시 StatSheet</param>
        /// <param name="eventType">발생한 이벤트 타입</param>
        protected virtual void ProcessAttackEventModifications(Pawn target, StatSheet tempStatSheet, Utils.EventType eventType)
        {
            // 유물들의 이벤트 처리 (StatSheet 수정)
            foreach (var relic in relics)
            {
                if (relic != null)
                {
                    relic.OnEvent(eventType, new AttackEventData(this, target));
                }
            }
            
            // 덱의 카드들의 이벤트 처리 (StatSheet 수정)
            foreach (var card in deck.Cards)
            {
                if (card?.cardAction != null)
                {
                    card.cardAction.OnEvent(eventType, new AttackEventData(this, target));
                }
            }
            
            // AttackComponent들의 이벤트 처리 (StatSheet 수정)
            foreach (var attackComp in attackComponentList)
            {
                if (attackComp != null)
                {
                    attackComp.OnEvent(eventType, new AttackEventData(this, target));
                }
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
                this.OnEvent(Utils.EventType.OnHPUpdated, new StatUpdateEventData(this, preHP));
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
                this.OnEvent(Utils.EventType.OnGoldUpdated, new StatUpdateEventData(this, preGold));
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

        // ===== [기능 3] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            // Pawn 자체의 이벤트 처리
            if (eventType == Utils.EventType.OnAttack)
            {
                if (param is Pawn target) ProcessAndTriggerDamage(target);
            }
            
            if (eventType == Utils.EventType.OnDamaged)
            {
                if (param is AttackDamageInfo damageInfo) ApplyDamage(damageInfo);
            }
            
            if (eventType == Utils.EventType.OnDeath)
            {
                HandleDeath(param as Pawn);
            }

            else
            {
                // 모든 AttackComponent의 이벤트 처리
                foreach (var attackComp in attackComponentList)
                {
                    attackComp.OnEvent(eventType, param);
                }

                // 모든 이벤트는 먼저 하위(유물, 카드 등)로 전파        
                // 유물들의 이벤트 처리
                foreach (var relic in relics)
                {
                    if (relic != null)
                    {
                        relic.OnEvent(eventType, param);
                    }
                }

                // 덱의 카드 액션들 처리
                deck.OnEvent(eventType, param);
            }
        }

        private void ProcessAndTriggerDamage(Pawn target)
        {
            StatSheet tempStatSheet = new StatSheet();
            tempStatSheet[StatType.AttackPower].SetBasicValue(GetStatValue(StatType.AttackPower));
            ProcessAttackEventModifications(target, tempStatSheet, Utils.EventType.OnAttack);
            
            bool isCritical = UnityEngine.Random.Range(0f, 1f) < (GetStatValue(StatType.CriticalRate) / 100f);
            
            if (isCritical)
            {
                ProcessAttackEventModifications(target, tempStatSheet, Utils.EventType.OnCriticalAttack);
                OnEvent(Utils.EventType.OnCriticalAttack, target);
            }
            
            var damageInfo = new AttackDamageInfo(this, tempStatSheet);
            target.OnEvent(Utils.EventType.OnDamaged, damageInfo);
        }

        private void ApplyDamage(AttackDamageInfo damageInfo)
        {
            if (damageInfo.attacker == null) return;

            float attackPower = damageInfo.finalStatSheet[StatType.AttackPower].Value;
            float defense = GetStatValue(StatType.Defense);
            float defensePenetration = damageInfo.finalStatSheet[StatType.DefensePenetration].Value;
            float effectiveDefense = defense * (1f - (defensePenetration / 100f));
            int finalDamage = Mathf.Max(1, Mathf.RoundToInt(attackPower - effectiveDefense));
            
            int previousHP = currentHp;
            ChangeHP(-finalDamage);

            Debug.Log($"<color=red>[DAMAGE] {gameObject.name} took {finalDamage} damage from {damageInfo.attacker.gameObject.name}</color>");
            ApplyKnockback(damageInfo.attacker);

            if (currentHp <= 0 && previousHP > 0)
            {
                OnEvent(Utils.EventType.OnDeath, damageInfo.attacker);
                damageInfo.attacker.OnEvent(Utils.EventType.OnKilled, this);
            }
        }

        private void HandleDeath(Pawn killer)
        {
            Debug.Log($"<color=red>[EVENT] {gameObject.name} - OnDeath triggered</color>");
            ChangeAnimationState("Die"); 
            if (rb != null) rb.bodyType = RigidbodyType2D.Static; 
            if (boxCollider != null) boxCollider.enabled = false; 
            Destroy(gameObject, 2f);
        }

        public class AttackDamageInfo
        {
            public Pawn attacker;
            public StatSheet finalStatSheet;

            public AttackDamageInfo(Pawn attacker, StatSheet statSheet)
            {
                this.attacker = attacker;
                this.finalStatSheet = statSheet;
            }
        }
    }
} 