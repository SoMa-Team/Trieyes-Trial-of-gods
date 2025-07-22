using System.Collections.Generic;
using Utils;
using Stats;
using AttackSystem;
using RelicSystem;
using UnityEngine;
using CardSystem;
using System;
using BattleSystem; 
using UnityEngine.EventSystems;
using CharacterSystem.Enemies;

namespace CharacterSystem
{
    public enum PawnAttackType
    {
        BasicAttack,
        Skill1, 
        Skill2,
    }
    
    /// <summary>
    /// 게임 내 모든 캐릭터의 기본이 되는 클래스입니다.
    /// 캐릭터의 기본적인 속성과 동작을 정의합니다.
    /// </summary>
    public abstract class Pawn : MonoBehaviour, IEventHandler, IMovable
    {
        // ===== [필드] =====
        [Header("Pawn Settings")]
        public int maxHp = 100;
        public int currentHp;

        public float moveSpeed => GetStatValue(StatType.MoveSpeed);

        [Header("Components")] 
        public Rigidbody2D rb;
        public Collider2D Collider;

        protected Controller Controller;
        protected Animator Animator;
        
        [Header("Stats")]

        public StatSheet statSheet = new();
        
        public StatPresetSO statPresetSO;

        private List<object> eventHandlers = new List<object>();
        protected string currentAnimationState;

        protected float lastAttackTime = 0f;

        protected float attackCooldown = 0f;
        
        public Vector2 lastestDirection;
        // ===== [프로퍼티] =====
        public int? enemyID;
        public bool isEnemy => enemyID is not null; 
        
        public string pawnName { get; protected set; }
        
        public int level { get; protected set; }
        public Vector2 LastMoveDirection => Controller.lastMoveDir;
        
        public int gold { get; protected set; }

        // ===== [기능별 필드] =====
        /// <summary>
        /// 기본 공격이자 관리자 공격
        /// </summary>
        public AttackData basicAttack;
        private AttackData backupBasicAttack;

        public AttackData skill1Attack;
        private AttackData backupSkill1Attack;
        public float skillAttack1Cooldown = 0f;
        public float lastSkillAttack1Time = -999f;
        
        public AttackData skill2Attack;
        private AttackData backupSkill2Attack;
        public float skillAttack2Cooldown = 0f;
        public float lastSkillAttack2Time = -999f;
        
        /// <summary>
        /// 장착 가능한 유물 리스트
        /// </summary>
        public List<Relic> relics = new();
        
        /// <summary>
        /// Pawn이 관리하는 Deck 인스턴스
        /// </summary>
        public Deck deck = new Deck();

        /// <summary>
        /// SPUM 프리팹
        /// </summary>
        protected GameObject pawnPrefab;

        public GameObject PawnPrefab
        {
            get { return pawnPrefab; }
        }

        // ===== [이벤트 필터링 시스템] =====
        /// <summary>
        /// 이 Pawn이 받을 이벤트들
        /// </summary>
        protected HashSet<Utils.EventType> acceptedEvents = new HashSet<Utils.EventType>();
        
        protected Dictionary<Utils.EventType, int> relicAcceptedEvents = new Dictionary<Utils.EventType, int>();
        public bool isDead { get; protected set; }
        
        public int objectID;

        // ===== [Unity 생명주기] =====
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            Collider = GetComponent<Collider2D>();
            Collider.enabled = true; // Instantiate에서 Awake가 호출되어 Collider가 설정되는데, 이것이 먼저 호출되는 문제

            Controller = GetComponent<Controller>();
            Controller.Activate(this);
            
            pawnPrefab = transform.GetChild(0).gameObject;
            Animator = pawnPrefab.transform.Find("UnitRoot").GetComponent<Animator>();
            
            // 스탯 시트 초기화
            statSheet = new StatSheet();
            
            deck.Activate(this, true);
            initBaseStat();

            if (rb != null)
            {
                rb.freezeRotation = true;
            }

            isDead = false;
        }

        protected virtual void Start()
        {
        }

        protected virtual void OnDestroy()
        {
            Deactivate();
        }

        public virtual void Update() 
        {
        }

        // ===== [커스텀 메서드] =====
        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        public virtual void Activate()
        {
            isDead = false;
            currentHp = maxHp;
            
            transform.position = Vector3.zero;
            
            // 기본 이벤트 등록
            // TODO: 폰에서는 이벤트 필터 안하기
            RegisterAcceptedEvents(
                Utils.EventType.OnAttackHit,
                Utils.EventType.OnDamageHit,
                Utils.EventType.OnAttack,
                Utils.EventType.OnDamaged,
                Utils.EventType.OnEvaded,
                Utils.EventType.OnAttackMiss,
                Utils.EventType.OnDeath,
                Utils.EventType.OnKilled,
                Utils.EventType.OnHPUpdated
            );

            skillAttack1Cooldown = backupSkill1Attack is not null ? skill1Attack.cooldown : 0f;
            skillAttack2Cooldown = backupSkill2Attack is not null ? skill2Attack.cooldown : 0f;
            
            gameObject.SetActive(true);
            
            // relic에 따른 Attack 적용
            if (relics.Count > 0)
            {
                backupBasicAttack = basicAttack.Copy();
                basicAttack = AttackFactory.Instance.RegisterRelicAppliedAttack(basicAttack, this);
                
                backupSkill1Attack = skill1Attack.Copy();
                skill1Attack = AttackFactory.Instance.RegisterRelicAppliedAttack(skill1Attack, this);
                
                backupSkill2Attack = skill2Attack.Copy();
                skill2Attack = AttackFactory.Instance.RegisterRelicAppliedAttack(skill2Attack, this);
            }
            
            Controller.Activate(this);
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public virtual void Deactivate()
        {
            //gameObject.SetActive(false);
            // 이벤트 핸들러 정리
            eventHandlers.Clear();
            
            // Relic에 따른 Attack 초기화
            if (relics.Count > 0)
            {
                AttackFactory.Instance.DeregisterAttack(basicAttack);
                basicAttack = backupBasicAttack;
                skill1Attack = backupSkill1Attack;
                skill2Attack = backupSkill2Attack;
            }   

            Controller.Deactivate();
        }

        public void ApplyRelic()
        {
            if (relics.Count > 0)
            {
                backupBasicAttack = basicAttack.Copy();
                basicAttack = AttackFactory.Instance.RegisterRelicAppliedAttack(basicAttack, this);
                
                backupSkill1Attack = skill1Attack.Copy();
                skill1Attack = AttackFactory.Instance.RegisterRelicAppliedAttack(skill1Attack, this);
                
                backupSkill2Attack = skill2Attack.Copy();
                skill2Attack = AttackFactory.Instance.RegisterRelicAppliedAttack(skill2Attack, this);
            }
        }

        /// <summary>
        /// 기본 스탯을 초기화합니다.
        /// </summary>
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

        /// <summary>
        /// 스탯 프리셋을 적용합니다.
        /// </summary>
        /// <param name="preset">적용할 스탯 프리셋</param>
        protected void ApplyStatPresetSO(StatPresetSO preset)
        {
            if (preset == null || preset.stats == null) return;

            foreach (var pair in preset.stats)
            {
                statSheet[pair.type].SetBasicValue(pair.value);
                //Debug.Log($"<color=green>[STAT] {gameObject.name} applied stat preset: {pair.type} : {pair.value}</color>");
            }
        }

        /// <summary>
        /// 데미지를 받습니다.
        /// </summary>
        /// <param name="damage">받을 데미지</param>
        public virtual void TakeDamage(int damage)
        {
            
        }

        /// <summary>
        /// 지정된 방향으로 이동합니다.
        /// </summary>
        /// <param name="direction">이동할 방향</param>
        public virtual void Move(Vector2 direction)
        {
            if(isDead)
            {
                return;
            }
            if (direction.magnitude > 0.1f)
            {
                // 360도 자연스러운 이동
                rb.linearVelocity = direction.normalized * moveSpeed;

                // 이동 방향에 따라 전체 flip (Y축 회전)
                if (direction.x != 0)
                {
                    pawnPrefab.transform.rotation = direction.x > 0
                        ? Quaternion.Euler(0, 180, 0)
                        : Quaternion.identity;
                }

                ChangeAnimationState("MOVE");
            }
            else
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                ChangeAnimationState("IDLE");
            }
        }

        /// <summary>
        /// 애니메이션 상태를 변경합니다.
        /// </summary>
        /// <param name="newState">새로운 애니메이션 상태</param>
        private void ChangeAnimationState(string newState)
        {
            if (isDead && newState != "DEATH")
                return;
            
            if (Animator != null && currentAnimationState != newState && Animator.HasState(0, Animator.StringToHash(newState)))
            {
                // switch로 각 newStat에 대한 Parameter 값을 변경
                switch (newState)
                {
                    case "MOVE":
                        Animator.SetBool("1_Move", true);
                        break;
                    case "IDLE":
                        Animator.SetBool("1_Move", false);
                        break;
                    case "ATTACK":
                        Animator.SetTrigger("2_Attack");
                        break;
                    case "DAMAGED":
                        Animator.SetBool("3_Damaged", true);
                        break;
                    case "DEATH":
                        Animator.SetBool("4_Death", true);
                        break;
                }
            }
        }

        // ===== [스탯 관련 메서드] =====
        /// <summary>
        /// 지정된 스탯 타입의 값을 가져옵니다.
        /// </summary>
        /// <param name="statType">스탯 타입</param>
        /// <returns>스탯 값</returns>
        public int GetStatValue(StatType statType)
        {
            return statSheet[statType];
        }
        
        /// <summary>
        /// 지정된 스탯 타입의 값을 설정합니다.
        /// </summary>
        /// <param name="statType">스탯 타입</param>
        /// <param name="value">설정할 값</param>
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
            
            //Debug.Log($"<color=cyan>[STATS] {gameObject.name} collected attack stats: ATK={attackStats[StatType.AttackPower].Value}, SPD={attackStats[StatType.AttackSpeed].Value}</color>");
            
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
                
                //Debug.Log($"<color=orange>[KNOCKBACK] {gameObject.name} knocked back by {attacker.gameObject.name}</color>");
            }
        }

        // ===== [기능 1] 캐릭터 기본 정보 =====
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
            return deck.EventTypeCount.ContainsKey(eventType) && deck.EventTypeCount[eventType] > 0;
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

        public void ClearRelicAcceptedEvents()
        {
            relicAcceptedEvents.Clear();
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
            relic.owner = this;
            relics.Add(relic);
            
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

                //Debug.Log($"<color=purple>[RELIC] {gameObject.name} lost relic: {relic.GetInfo().name}</color>");
            }
        }

        // ===== [기능 3] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            // 이벤트 필터링: 이 Pawn이 받지 않는 이벤트는 무시
            // if (!IsEventAccepted(eventType))
            // {
            //     Debug.Log($"<color=gray>[EVENT_FILTER] {gameObject.name} ignoring event: {eventType} (not in accepted events: {string.Join(", ", acceptedEvents)})</color>");
            //     return;
            // }
            // 버그 발생하여 테스트를 위해 일단 무시

            //Debug.Log($"<color=blue>[EVENT] {gameObject.name} ({GetType().Name}) received {eventType} event</color>");
            
            // Pawn 자체의 이벤트 처리
            
            if (eventType == Utils.EventType.OnDamaged)
            {
                if (param is AttackResult result) 
                {
                    ApplyDamage(result);
                }
            }
            
            if (eventType == Utils.EventType.OnDeath)
            {
                //Debug.Log($"<color=red>[EVENT] {gameObject.name} ({GetType().Name}) processing OnDeath</color>");
                HandleDeath();
            }

            // 유물들의 이벤트 처리 (필터링 적용)
            if (relics != null && IsRelicEventAccepted(eventType))
            {
                //Debug.Log($"<color=purple>[EVENT_FILTER] {gameObject.name} processing {eventType} for {relics.Count} relics (relic events: {string.Join(", ", relicAcceptedEvents)})</color>");
                foreach (var relic in relics)
                {
                    if (relic != null)
                    {
                        //Debug.Log($"<color=purple>[EVENT] {gameObject.name} ({GetType().Name}) -> Relic {relic.GetInfo().name} processing {eventType}</color>");
                        relic.OnEvent(eventType, param);
                    }
                }
            }
            // 덱의 카드 액션들 처리 (필터링 적용)
            if (deck != null && IsCardEventAccepted(eventType))
            {
                Debug.Log($"<color=cyan>[EVENT] {gameObject.name} ({GetType().Name}) -> Deck processing {eventType}</color>");
                deck.OnEvent(eventType, param);
            }
        }

        public void ApplyDamage(AttackResult result)
        {
            // 여러번 OnDeath 이벤트가 발생되지 않기 위한 예외문
            if (isDead) return;
            
            int previousHP = currentHp;
            ChangeHP(-result.totalDamage);

            Debug.Log($"<color=red>[DAMAGE] {gameObject.name} took {result.totalDamage} damage from {result.attacker.gameObject.name}</color>");
            // TODO : 넉백 확률 스탯 부분 추가 필요
            // ApplyKnockback(damageInfo.attacker);
            
            if (currentHp <= 0)
            {
                OnEvent(Utils.EventType.OnDeath, result);
                result.attack.OnEvent(Utils.EventType.OnKilled, result);
                result.attacker.OnEvent(Utils.EventType.OnKilled, result);

                if (result.isCritical)
                {
                    result.attack.OnEvent(Utils.EventType.OnKilledByCritical, result);
                    result.attacker.OnEvent(Utils.EventType.OnKilledByCritical, result);
                }
            }
        }

        private void HandleDeath()
        {
            ////Debug.Log($"<color=red>[EVENT] {gameObject.name} - OnDeath triggered</color>");
            // TO-DO : 이부분 테스트 필요
            Collider.enabled = false;
            rb.linearVelocity = Vector3.zero;
            
            if (isDead)
                return;
            isDead = true;
            ChangeAnimationState("DEATH");
        }
        
        // ===== [기능 12] 자동공격 시스템 =====
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
            
            //Debug.Log($"<color=yellow>[AUTO_ATTACK] {gameObject.name} attack speed: {attackSpeed}, cooldown: {attackCooldown:F2}s</color>");
        }
        
        /// <summary>
        /// 자동공격을 수행합니다.
        /// </summary>
        
        public bool CheckTimeInterval()
        {
            return Time.time - lastAttackTime >= attackCooldown ? true : false;
        }

        /// <summary>
        /// 스킬 쿨타임을 계산합니다.
        /// </summary>
        /// <param name="skillType">스킬 타입</param>
        /// <returns>쿨타임이 지났으면 true, 아니면 false</returns>
        public bool CheckSkillCooldown(PawnAttackType skillType)
        {
            switch (skillType)
            {
                case PawnAttackType.Skill1:
                    return Time.time - lastSkillAttack1Time >= skillAttack1Cooldown;
                case PawnAttackType.Skill2:
                    return Time.time - lastSkillAttack2Time >= skillAttack2Cooldown;
                default:
                    return false;
            }
        }

        public virtual void PerformAutoAttack()
        {
            // 공격 수행
            var res = ExecuteAttack();
        }

        /// <summary>
        /// 공격을 실행합니다. 스탯 정보를 수집하여 Attack에게 전달합니다.
        /// </summary>
        public virtual bool ExecuteAttack(PawnAttackType attackType = PawnAttackType.BasicAttack)
        {
            switch (attackType)
            {
                case PawnAttackType.BasicAttack:
                    if (CheckTimeInterval())
                    {
                        CalculateAttackCooldown();
                        lastAttackTime = Time.time;
                        ChangeAnimationState("ATTACK");
                        AttackFactory.Instance.Create(basicAttack, this, null, LastMoveDirection); 
                        return true;
                    }
                    return false;
                case PawnAttackType.Skill1:
                    if (CheckSkillCooldown(PawnAttackType.Skill1))
                    {
                        lastSkillAttack1Time = 0f;
                        ChangeAnimationState("ATTACK");
                        Attack temp = AttackFactory.Instance.Create(skill1Attack, this, null, LastMoveDirection);
                        Debug.Log($"<color=yellow>[SKILL1] {temp.gameObject.name} skill1Attack: {temp.attackData.attackId}, attacker: {temp.attacker.gameObject.name}</color>");
                        return true;
                    }
                    Debug.Log($"<color=yellow>[SKILL1] {gameObject.name} skillAttack1Cooldown: {skillAttack1Cooldown}, lastSkillAttack1Time: {lastSkillAttack1Time}</color>");
                    return false;

                case PawnAttackType.Skill2:
                    if (CheckSkillCooldown(PawnAttackType.Skill2))
                    {
                        lastSkillAttack2Time = 0f;
                        ChangeAnimationState("ATTACK");
                        Attack temp = AttackFactory.Instance.Create(skill2Attack, this, null, LastMoveDirection);
                        Debug.Log($"<color=yellow>[SKILL2] {temp.gameObject.name} skill2Attack: {temp.attackData.attackId}, attacker: {temp.attacker.gameObject.name}</color>");
                        return true;
                    }
                    Debug.Log($"<color=yellow>[SKILL2] {gameObject.name} skillAttack2Cooldown: {skillAttack2Cooldown}, lastSkillAttack2Time: {lastSkillAttack2Time}</color>");
                    return false;
                    
                default:
                    return false;
            }
        }

        // ===== [내부 클래스] =====
        /// <summary>
        /// 스탯 업데이트 관련 이벤트 데이터
        /// </summary>
        public class StatUpdateEventData
        {
            /// <summary>
            /// 스탯 소유자
            /// </summary>
            public Pawn owner;
            
            /// <summary>
            /// 이전 값
            /// </summary>
            public int previousValue;
            
            /// <summary>
            /// 스탯 업데이트 이벤트 데이터를 생성합니다.
            /// </summary>
            /// <param name="owner">스탯 소유자</param>
            /// <param name="previousValue">이전 값</param>
            public StatUpdateEventData(Pawn owner, int previousValue)
            {
                this.owner = owner;
                this.previousValue = previousValue;
            }
        }
    }
} 
