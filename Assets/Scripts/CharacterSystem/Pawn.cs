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
        public StatSheet statSheet = new(); // 여러 스탯 정보
        public StatPresetSO statPresetSO;
        
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
        public Deck deck; // Pawn이 관리하는 Deck 인스턴스

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
            // Awake에서는 아무것도 하지 않음
        }

        protected virtual void Start()
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
            // 컴포넌트 초기화
            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            deck = GetComponent<Deck>();
            
            // 스탯 시트 초기화
            statSheet = new StatSheet();
            
            // Inspector에서 할당된 Deck을 사용하도록 수정
            if (deck != null)
            {
                // Deck 초기화 (owner 참조 설정, 프리팹 데이터 보존)
                deck.Initialize(this, true);
            }
            else
            {
                // Deck이 할당되지 않은 경우 경고를 표시하고, 빈 Deck을 생성합니다.
                Debug.LogWarning($"<color=yellow>[PAWN] {gameObject.name}에 Deck이 할당되지 않았습니다. Inspector에서 Deck을 할당해주세요. 임시로 빈 Deck을 생성합니다.</color>");
                deck = gameObject.AddComponent<Deck>();
                deck.Initialize(this, true);
            }
            
            initBaseStat();
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public virtual void Deactivate()
        {
            // 이벤트 핸들러 정리
            eventHandlers.Clear();
            
            // 리스트 초기화
            if (attackComponentList != null)
                attackComponentList.Clear();
            if (relics != null)
                relics.Clear();
        }
        
        protected virtual void initBaseStat()
        {
            if(statSheet == null)
                statSheet = new StatSheet();
            if(statPresetSO != null)
                ApplyStatPresetSO(statPresetSO);
            //TODO : 현재 체력 등 초기화
        }
        protected void ApplyStatPresetSO(StatPresetSO preset)
        {
            if (preset == null || preset.stats == null) return;

            foreach (var pair in preset.stats)
                statSheet[pair.type].SetBasicValue(pair.value);
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

        // ===== [기능 4] 스탯 관련 ===== 많이 안쓰일 것 같다면 지워도 됨
        public int GetStatValue(StatType statType)
        {
            return statSheet[statType];
        }
        
        public void SetStatValue(StatType statType, int value)
        {
            statSheet[statType].SetBasicValue(value);
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

        // ===== [기능 3] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            Debug.Log($"<color=blue>[EVENT] {gameObject.name} ({GetType().Name}) received {eventType} event</color>");
            
            // Pawn 자체의 이벤트 처리
            if (eventType == Utils.EventType.OnAttack)
            {
                if (param is Pawn target) 
                {
                    Debug.Log($"<color=yellow>[EVENT] {gameObject.name} ({GetType().Name}) processing OnAttack against {target.gameObject.name} ({target.GetType().Name})</color>");
                    ProcessAndTriggerDamage(target);
                }
            }
            
            if (eventType == Utils.EventType.OnDamaged)
            {
                if (param is AttackDamageInfo damageInfo) 
                {
                    Debug.Log($"<color=red>[EVENT] {gameObject.name} ({GetType().Name}) processing OnDamaged from {damageInfo.attacker?.gameObject.name} ({damageInfo.attacker?.GetType().Name})</color>");
                    ApplyDamage(damageInfo);
                }
            }
            
            if (eventType == Utils.EventType.OnDeath)
            {
                Debug.Log($"<color=red>[EVENT] {gameObject.name} ({GetType().Name}) processing OnDeath</color>");
                HandleDeath(param as Pawn);
            }

            else
            {
                // 유물들의 이벤트 처리
                if (relics != null)
                {
                    foreach (var relic in relics)
                    {
                        if (relic != null)
                        {
                            Debug.Log($"<color=purple>[EVENT] {gameObject.name} ({GetType().Name}) -> Relic {relic.info.name} processing {eventType}</color>");
                            relic.OnEvent(eventType, param);
                        }
                    }
                }

                // 덱의 카드 액션들 처리 (Deck을 통해)
                if (deck != null)
                {
                    Debug.Log($"<color=cyan>[EVENT] {gameObject.name} ({GetType().Name}) -> Deck processing {eventType}</color>");
                    deck.OnEvent(eventType, param);
                }

                // 모든 AttackComponent의 이벤트 처리
                if (attackComponentList != null)
                {
                    foreach (var attackComp in attackComponentList)
                    {
                        if (attackComp != null)
                        {
                            Debug.Log($"<color=orange>[EVENT] {gameObject.name} ({GetType().Name}) -> AttackComponent {attackComp.GetType().Name} processing {eventType}</color>");
                            attackComp.OnEvent(eventType, param);
                        }
                    }
                }
            }
        }

        private void ProcessAndTriggerDamage(Pawn target)
        {
            StatSheet tempStatSheet = new StatSheet();
            tempStatSheet[StatType.AttackPower].SetBasicValue(GetStatValue(StatType.AttackPower));
            OnEvent(Utils.EventType.OnAttack, new AttackEventData(this, target));
            
            bool isCritical = UnityEngine.Random.Range(0f, 1f) < (GetStatValue(StatType.CriticalRate) / 100f);
            
            if (isCritical)
            {
                OnEvent(Utils.EventType.OnCriticalAttack, new AttackEventData(this, target));
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