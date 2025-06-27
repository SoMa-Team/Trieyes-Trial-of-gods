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
using System.IO.Compression;

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
        //public SpriteRenderer spriteRenderer;
        public GameObject spumPrefabs;
        public Rigidbody2D rb;
        public CapsuleCollider2D capsuleCollider;
        public PlayerController playerController;
        
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
        public GameObject basicAttack; // 기본 공격이자 관리자 공격
        public List<Relic> relics = new(); // 장착 가능한 유물 리스트
        public Deck deck = new Deck(); // Pawn이 관리하는 Deck 인스턴스

        // ===== [기능 11] 이벤트 필터링 시스템 =====
        protected HashSet<Utils.EventType> acceptedEvents = new HashSet<Utils.EventType>(); // 이 Pawn이 받을 이벤트들
        protected Dictionary<Utils.EventType, int> cardAcceptedEvents = new Dictionary<Utils.EventType, int>(); // 카드들이 받을 이벤트들 (카운트 관리)
        protected Dictionary<Utils.EventType, int> relicAcceptedEvents = new Dictionary<Utils.EventType, int>(); // 유물들이 받을 이벤트들 (카운트 관리)

        public Dictionary<Utils.EventType, int> GetCardAcceptedEvents()
        {
            return cardAcceptedEvents;
        }

        public Dictionary<Utils.EventType, int> GetRelicAcceptedEvents()
        {
            return relicAcceptedEvents;
        }
        
        public HashSet<Utils.EventType> GetAcceptedEvents()
        {
            return acceptedEvents;
        }

        /// <summary>
        /// 이 Pawn이 받을 이벤트를 등록합니다.
        /// </summary>
        /// <param name="eventTypes">등록할 이벤트 타입들</param>
        protected virtual void RegisterAcceptedEvents(params Utils.EventType[] eventTypes)
        {
            foreach (var eventType in eventTypes)
            {
                acceptedEvents.Add(eventType);
            }
        }

        /// <summary>
        /// 카드들이 받을 이벤트를 등록합니다. (카운트 관리)
        /// </summary>
        /// <param name="eventTypes">등록할 이벤트 타입들</param>
        protected virtual void RegisterCardAcceptedEvents(params Utils.EventType[] eventTypes)
        {
            foreach (var eventType in eventTypes)
            {
                if (cardAcceptedEvents.ContainsKey(eventType))
                    cardAcceptedEvents[eventType]++;
                else
                    cardAcceptedEvents[eventType] = 1;
            }
        }

        /// <summary>
        /// 유물들이 받을 이벤트를 등록합니다. (카운트 관리)
        /// </summary>
        /// <param name="eventTypes">등록할 이벤트 타입들</param>
        protected virtual void RegisterRelicAcceptedEvents(params Utils.EventType[] eventTypes)
        {
            foreach (var eventType in eventTypes)
            {
                if (relicAcceptedEvents.ContainsKey(eventType))
                    relicAcceptedEvents[eventType]++;
                else
                    relicAcceptedEvents[eventType] = 1;
            }
        }

        /// <summary>
        /// 카드들이 받을 이벤트를 해제합니다. (카운트 관리)
        /// </summary>
        /// <param name="eventTypes">해제할 이벤트 타입들</param>
        protected virtual void UnregisterCardAcceptedEvents(params Utils.EventType[] eventTypes)
        {
            foreach (var eventType in eventTypes)
            {
                if (cardAcceptedEvents.ContainsKey(eventType))
                {
                    cardAcceptedEvents[eventType]--;
                    if (cardAcceptedEvents[eventType] <= 0)
                        cardAcceptedEvents.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// 유물들이 받을 이벤트를 해제합니다. (카운트 관리)
        /// </summary>
        /// <param name="eventTypes">해제할 이벤트 타입들</param>
        protected virtual void UnregisterRelicAcceptedEvents(params Utils.EventType[] eventTypes)
        {
            foreach (var eventType in eventTypes)
            {
                if (relicAcceptedEvents.ContainsKey(eventType))
                {
                    relicAcceptedEvents[eventType]--;
                    if (relicAcceptedEvents[eventType] <= 0)
                        relicAcceptedEvents.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// 이벤트가 허용되는지 확인합니다.
        /// </summary>
        /// <param name="eventType">확인할 이벤트 타입</param>
        /// <returns>허용되면 true, 아니면 false</returns>
        protected virtual bool IsEventAccepted(Utils.EventType eventType)
        {
            return acceptedEvents.Contains(eventType);
        }

        /// <summary>
        /// 카드 이벤트가 허용되는지 확인합니다. (카운트 기반)
        /// </summary>
        /// <param name="eventType">확인할 이벤트 타입</param>
        /// <returns>허용되면 true, 아니면 false</returns>
        protected virtual bool IsCardEventAccepted(Utils.EventType eventType)
        {
            return cardAcceptedEvents.ContainsKey(eventType) && cardAcceptedEvents[eventType] > 0;
        }

        /// <summary>
        /// 유물 이벤트가 허용되는지 확인합니다. (카운트 기반)
        /// </summary>
        /// <param name="eventType">확인할 이벤트 타입</param>
        /// <returns>허용되면 true, 아니면 false</returns>
        protected virtual bool IsRelicEventAccepted(Utils.EventType eventType)
        {
            return relicAcceptedEvents.ContainsKey(eventType) && relicAcceptedEvents[eventType] > 0;
        }

        // 각 이벤트 Clear 함수
        public void ClearAcceptedEvents()
        {
            acceptedEvents.Clear();
        }

        public void ClearCardAcceptedEvents()
        {
            cardAcceptedEvents.Clear();
        }

        public void ClearRelicAcceptedEvents()
        {
            relicAcceptedEvents.Clear();
        }

        // ===== [기능 6] 이동 및 물리/애니메이션 관련 =====
        protected string currentAnimationState;
        
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
            // spriteRenderer = GetComponent<SpriteRenderer>();
            spumPrefabs = Instantiate(spumPrefabs, transform);
            spumPrefabs.transform.localPosition = Vector3.zero;

            rb = GetComponent<Rigidbody2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
            playerController = GetComponent<PlayerController>();
            
            // 스탯 시트 초기화
            statSheet = new StatSheet();
            
            // 기본 이벤트 등록
            RegisterAcceptedEvents(
                Utils.EventType.OnAttack,
                Utils.EventType.OnDamaged,
                Utils.EventType.OnDeath,
                Utils.EventType.OnKilled,
                Utils.EventType.OnHPUpdated
            );
           

            deck.Initialize(this, true);
            initBaseStat();

            // 기본 공격 초기화
            GameObject attackObj = Instantiate(basicAttack);
            attackObj.transform.SetParent(transform);
            attackObj.transform.localPosition = Vector3.zero;
            attackObj.transform.localRotation = Quaternion.identity;

            basicAttack = attackObj;
            basicAttack.GetComponent<Attack>().Initialize(this);
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public virtual void Deactivate()
        {
            // 이벤트 핸들러 정리
            eventHandlers.Clear();
            
            // 리스트 초기화
            if (relics != null)
            {
                relics.Clear();
            }
        }
        
        protected void initBaseStat()
        {
            if(statSheet == null)
            {
                throw new Exception("statSheet is null");
            }
            if(statPresetSO != null)
            {
                ApplyStatPresetSO(statPresetSO);
            }
        }
        protected void ApplyStatPresetSO(StatPresetSO preset)
        {
            if (preset == null || preset.stats == null) return;

            foreach (var pair in preset.stats)
            {
                statSheet[pair.type].SetBasicValue(pair.value);
                Debug.Log($"<color=green>[STAT] {gameObject.name} applied stat preset: {pair.type} : {pair.value}</color>");
            }
        }
        
        public virtual void Update() 
        {    
            // 자동공격 수행
            PerformAutoAttack();
        }
        
        public virtual void TakeDamage(int damage)
        {
            
        }
        
        public virtual void Move(Vector2 direction)
        {
            if (direction.magnitude > 0.1f)
            {
                transform.Translate(direction * Time.deltaTime * moveSpeed);

                // 이동 방향에 따라 전체 flip (Y축 회전)
                if (direction.x != 0)
                {
                    // SPUM 프리팹의 부모(혹은 UnitRoot 등)에 적용
                    spumPrefabs.transform.rotation = direction.x > 0
                        ? Quaternion.Euler(0, 180, 0)
                        : Quaternion.identity;
                }

                ChangeAnimationState("MOVE");
            }
            else
            {
                ChangeAnimationState("IDLE");
            }
        }
        
        protected virtual void ChangeAnimationState(string newState)
        {
            // SPUM Prefab 내부에 UnitRoot 오브젝트가 있고, 그 안에 Animator가 있음
            // To-Do : GetComponentInChildren 사용해서 찾아보기
            Animator animator = spumPrefabs.transform.Find("UnitRoot").GetComponent<Animator>();

            if (animator != null && currentAnimationState != newState && animator.HasState(0, Animator.StringToHash(newState)))
            {
                // switch로 각 newStat에 대한 Parameter 값을 변경
                switch (newState)
                {
                    case "MOVE":
                        animator.SetBool("1_Move", true);
                        break;
                    case "IDLE":
                        animator.SetBool("1_Move", false);
                        break;
                    case "ATTACK":
                        animator.SetTrigger("2_Attack");
                        break;
                    case "DAMAGED":
                        animator.SetBool("3_Damaged", true);
                        break;
                    case "DEATH":
                        animator.SetBool("4_Death", true);
                        animator.SetTrigger("4_Death");
                        break;
                }
            }
            else
            {
                Debug.Log($"<color=red>[ANIMATION] {gameObject.name} animation state '{newState}' not found</color>");
            }
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
        /// 공격에 필요한 스탯 정보를 수집합니다.
        /// Relic, Card의 영향을 포함한 최종 스탯을 반환합니다.
        /// </summary>
        /// <returns>공격용 스탯 정보</returns>
        public virtual StatSheet CollectAttackStats()
        {
            // 기본 스탯 복사
            StatSheet attackStats = new StatSheet();
            
            // Pawn의 모든 스탯을 복사
            foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
            {
                int statValue = GetStatValue(statType);
                attackStats[statType].SetBasicValue(statValue);
            }
            
            Debug.Log($"<color=cyan>[STATS] {gameObject.name} collected attack stats: ATK={attackStats[StatType.AttackPower].Value}, SPD={attackStats[StatType.AttackSpeed].Value}</color>");
            
            return attackStats;
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
                
                // 유물의 이벤트 셋을 Pawn의 relicAcceptedEvents에 합치기 (카운트 관리)
                var relicEvents = relic.GetAcceptedEvents();
                if (relicEvents != null)
                {
                    foreach (var eventType in relicEvents)
                    {
                        if (relicAcceptedEvents.ContainsKey(eventType))
                            relicAcceptedEvents[eventType]++;
                        else
                            relicAcceptedEvents[eventType] = 1;
                    }
                }
                
                Debug.Log($"<color=purple>[RELIC] {gameObject.name} acquired relic: {relic.GetInfo().name}</color>");
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

                // 유물의 이벤트 셋을 Pawn의 relicAcceptedEvents에서 제거 (카운트 관리)
                var relicEvents = relic.GetAcceptedEvents();
                if (relicEvents != null)
                {
                    foreach (var eventType in relicEvents)
                    {
                        if (relicAcceptedEvents.ContainsKey(eventType))
                        {
                            relicAcceptedEvents[eventType]--;
                            if (relicAcceptedEvents[eventType] <= 0)
                                relicAcceptedEvents.Remove(eventType);
                        }
                    }
                }

                Debug.Log($"<color=purple>[RELIC] {gameObject.name} lost relic: {relic.GetInfo().name}</color>");
            }
        }

        // ===== [기능 3] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            // 이벤트 필터링: 이 Pawn이 받지 않는 이벤트는 무시
            if (!IsEventAccepted(eventType))
            {
                Debug.Log($"<color=gray>[EVENT_FILTER] {gameObject.name} ignoring event: {eventType} (not in accepted events: {string.Join(", ", acceptedEvents)})</color>");
                return;
            }

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

            // 유물들의 이벤트 처리 (필터링 적용)
            if (relics != null && IsRelicEventAccepted(eventType))
            {
                Debug.Log($"<color=purple>[EVENT_FILTER] {gameObject.name} processing {eventType} for {relics.Count} relics (relic events: {string.Join(", ", relicAcceptedEvents)})</color>");
                foreach (var relic in relics)
                {
                    if (relic != null)
                    {
                        Debug.Log($"<color=purple>[EVENT] {gameObject.name} ({GetType().Name}) -> Relic {relic.GetInfo().name} processing {eventType}</color>");
                        relic.OnEvent(eventType, param);
                    }
                }
            }

            // 덱의 카드 액션들 처리 (필터링 적용)
            if (deck != null && IsCardEventAccepted(eventType))
            {
                Debug.Log($"<color=cyan>[EVENT_FILTER] {gameObject.name} processing {eventType} for deck (card events: {string.Join(", ", cardAcceptedEvents)})</color>");
                Debug.Log($"<color=cyan>[EVENT] {gameObject.name} ({GetType().Name}) -> Deck processing {eventType}</color>");
                deck.OnEvent(eventType, param);
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
            ChangeAnimationState("4_Death"); 
            if (rb != null) rb.bodyType = RigidbodyType2D.Static; 
            if (capsuleCollider != null) capsuleCollider.enabled = false; 
            Destroy(gameObject, 2f);
        }
        
        // ===== [기능 12] 자동공격 시스템 =====
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

        
        protected float lastAttackTime = 0f; // 마지막 공격 시간
        protected float attackCooldown = 0f; // 공격 쿨다운 시간
        
        /// <summary>
        /// 공격속도 스탯을 기반으로 공격 쿨다운을 계산합니다.
        /// 공격속도 10 = 60fps 기준 1초에 1개 발사
        /// </summary>
        protected virtual void CalculateAttackCooldown()
        {
            int attackSpeed = GetStatValue(StatType.AttackSpeed);
            // 공격속도 10을 기준으로 1초에 1개 발사
            // 공격속도가 높을수록 쿨다운이 짧아짐
            attackCooldown = 1f / (attackSpeed / 10f);
            
            Debug.Log($"<color=yellow>[AUTO_ATTACK] {gameObject.name} attack speed: {attackSpeed}, cooldown: {attackCooldown:F2}s</color>");
        }
        
        /// <summary>
        /// 자동공격을 수행합니다.
        /// </summary>
        public virtual void PerformAutoAttack()
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                // 공격 쿨다운 계산 (스탯 변경 시 대응)
                CalculateAttackCooldown();
                
                // 공격 수행
                ExecuteAttack();
                
                // 마지막 공격 시간 업데이트
                lastAttackTime = Time.time;
            }
        }
        
        /// <summary>
        /// 공격을 실행합니다. 스탯 정보를 수집하여 Attack 관리자에게 전달합니다.
        /// </summary>
        protected virtual void ExecuteAttack()
        {
            try
            {
                if (basicAttack == null)
                {
                    Debug.LogWarning($"<color=yellow>[AUTO_ATTACK] {gameObject.name} has no basicAttack component!</color>");
                    return;
                }
                
                Debug.Log($"<color=green>[AUTO_ATTACK] {gameObject.name} executing attack</color>");
                
                // 1단계: 스탯 정보 수집 (Pawn에서 수행)
                StatSheet attackStats = CollectAttackStats();

                // Attack 관리자의 AttackCompList에 지금 발사해야 하는 투사체들에 어떤 Component가 있는지 확인
                foreach (var componentPrefab in basicAttack.GetComponent<Attack>().componentPrefabs)
                {
                    if (componentPrefab != null)
                    {
                        Debug.Log($"<color=green>[AUTO_ATTACK] {gameObject.name} component: {componentPrefab.GetType().Name}</color>");
                    }
                }
                
                // Attack 관리자에게 공격 실행 요청
                int projectileCount = attackStats[Stats.StatType.ProjectileCount].Value;
                float spreadAngle = attackStats[Stats.StatType.ProjectileSpread].Value;

                if (projectileCount <= 1)
                {
                    Vector2 direction = Vector2.right;
                    basicAttack.GetComponent<Attack>().CreateSingleProjectile(direction, statSheet);
                }
                else
                {
                    float angleStep = spreadAngle / (projectileCount - 1);
                    float startAngle = -spreadAngle / 2f;
                    for (int i = 0; i < projectileCount; i++)
                    {
                        float angle = startAngle + (angleStep * i);
                        Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
                        basicAttack.GetComponent<Attack>().CreateSingleProjectile(direction, statSheet);
                    }
                }
                // 애니메이션 실행
                ChangeAnimationState("ATTACK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ERROR] ExecuteAttack 예외: {ex}");
            }
        }
        public bool IsFacingRight()
        {
            float yRotation = spumPrefabs.transform.rotation.eulerAngles.y;
            return yRotation > 90f && yRotation < 270f;
        }
        public bool IsFacingUp()
        {
            float yRotation = spumPrefabs.transform.rotation.eulerAngles.y;
            return yRotation > 0f && yRotation < 180f;
        }
    }
} 