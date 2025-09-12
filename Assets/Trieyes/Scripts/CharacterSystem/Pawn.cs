using System.Collections.Generic;
using Utils;
using Stats;
using AttackSystem;
using RelicSystem;
using UnityEngine;
using CardSystem;
using System;
using BattleSystem; 

namespace CharacterSystem
{
    public enum PawnAttackType
    {
        BasicAttack,
        Skill1, 
        Skill2,
    }

    public enum PawnStatusType
    {
        ElectricShock, // 감전
        Freeze, // 빙결
        Burn, // 화상
    }

    public struct PawnStatus
    {
        public float duration;
        public float lastTime;
    }
    
    /// <summary>
    /// 게임 내 모든 캐릭터의 기본이 되는 클래스입니다.
    /// 캐릭터의 기본적인 속성과 동작을 정의합니다.
    /// </summary>
    public abstract class Pawn : MonoBehaviour, IEventHandler, IMovable
    {
        // ===== [필드] =====
        [Header("Pawn Settings")]
        public int maxHp;
        public int currentHp;

        public float moveSpeed => GetStatValue(StatType.MoveSpeed);

        [Header("Components")]
        public Rigidbody2D rb;

        [SerializeField] public Controller Controller; // TODO : 테스트 컨트롤러를 위한 임시 접근데어자 변경
        [SerializeField] protected Animator Animator;

        public abstract Vector2 CenterOffset { get; }

        public AllIn1SpriteShaderHandler allIn1SpriteShaderHandler = new AllIn1SpriteShaderHandler();
        
        [Header("Stats")]
        public StatSheet statSheet { get; private set; }
        
        public StatPresetSO statPresetSO;

        private List<object> eventHandlers = new List<object>();
        protected string currentAnimationState;

        protected float lastAttackTime = 0f;

        protected float attackCooldown = 0f;
        
        // ===== [프로퍼티] =====
        public int? enemyID;
        public bool isEnemy => enemyID is not null; 
        
        public string pawnName { get; protected set; }
        
        public int level { get; protected set; }
        public Vector2 LastMoveDirection => Controller.lastMoveDir;

        public Dictionary<PawnStatusType, object> statuses = new();
        
        public int gold { get; set; }

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
        
        public float BasicAttackCoolDownRate => Mathf.Max(1 - (Time.time - lastAttackTime) / attackCooldown, 0);
        public float Skill1CoolDownRate => Mathf.Max(1 - (Time.time - lastSkillAttack1Time) / skillAttack1Cooldown, 0);
        public float Skill2CoolDownRate => Mathf.Max(1 - (Time.time - lastSkillAttack2Time) / skillAttack2Cooldown, 0);
        public float HpRate => (float)currentHp / maxHp;
        
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
        public bool isAutoAttack {
            get
            {
                return Controller.isAutoAttack;
            }
            set
            {
                Controller.isAutoAttack = value;
            }
        }

        public int objectID;

        // ===== [Unity 생명주기] =====
        protected virtual void Start()
        {
            if(rb is null) rb = GetComponent<Rigidbody2D>();
            
            pawnPrefab = transform.GetChild(0).gameObject;
            if(Animator is null) Animator = pawnPrefab.transform.Find("UnitRoot").GetComponent<Animator>();
            
            deck.Activate(this, true);

            if (rb != null)
            {
                rb.freezeRotation = true;
            }
        }

        protected virtual void OnDestroy()
        {
            if (BattleStage.now is null) return;
            if (isEnemy)
            {
                EnemyFactory.Instance.Deactivate(this as Enemy);
            }
            else
            {
                CharacterFactory.Instance.Deactivate(this as Character);
            }
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
            if (rb is not null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            if (Controller is not null)
            {
                Controller.Activate(this);
            }
            isDead = false;
            
            // PlayerController를 동적으로 붙이거나, 인스펙터에서 할당
            if (Controller is null)
            {
                Controller = GetComponent<Controller>();
                if (Controller == null)
                {
                    throw new Exception("PlayerController not found on " + gameObject.name);
                }
            }
            Controller.Activate(this);
            
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

            skillAttack1Cooldown = skill1Attack?.cooldown ?? 0f;
            skillAttack2Cooldown = skill2Attack?.cooldown ?? 0f;

            deck.Activate(this, true);
            
            SyncHP();
            
            gameObject.SetActive(true);
            
            // relic에 따른 Attack 적용
            ApplyRelic();
            
            Controller.Activate(this);
        }

        /// <summary>
        /// 애니메이터를 초기 상태로 리셋합니다.
        /// </summary>
        protected virtual void ResetAnimator()
        {
            if (Animator != null)
            {
                Animator.Rebind();
            }
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public virtual void Deactivate()
        {
            //gameObject.SetActive(false);
            // 이벤트 핸들러 정리
            eventHandlers.Clear();
            
            // 애니메이터 초기화
            ResetAnimator();
            
            // Relic에 따른 Attack 초기화
            if (relics.Count > 0)
            {
                AttackFactory.Instance.DeregisterAttack(basicAttack);
                AttackFactory.Instance.DeregisterAttack(skill1Attack);
                AttackFactory.Instance.DeregisterAttack(skill2Attack);
                
                basicAttack = backupBasicAttack;
                skill1Attack = backupSkill1Attack;
                skill2Attack = backupSkill2Attack;
            }

            if (allIn1SpriteShaderHandler.mat is not null)   
            {
                allIn1SpriteShaderHandler.Deactivate();
            }

            Controller.Deactivate();
            statuses.Clear();
            ClearStatModifier();
        }
        
        public void SyncHP()
        {
            maxHp = Mathf.RoundToInt(GetStatValue(StatType.Health));
            currentHp = maxHp;
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
        }
        
        public void ApplyRelic()
        {
            backupBasicAttack = basicAttack;
            backupSkill1Attack = skill1Attack;
            backupSkill2Attack = skill2Attack;
            
            if (relics.Count > 0)
            {
                if (basicAttack is not null)
                {
                    basicAttack = basicAttack.Copy();
                    basicAttack = AttackFactory.Instance.RegisterRelicAppliedAttack(basicAttack, this);
                }
                if (skill1Attack is not null)
                {
                    skill1Attack = skill1Attack.Copy();
                    skill1Attack = AttackFactory.Instance.RegisterRelicAppliedAttack(skill1Attack, this);
                }
                if (skill2Attack is not null)
                {
                    skill2Attack = skill2Attack.Copy();
                    skill2Attack = AttackFactory.Instance.RegisterRelicAppliedAttack(skill2Attack, this);
                }
            }
        }

        /// <summary>
        /// 기본 스탯을 초기화합니다.
        /// </summary>
        public void initBaseStat()
        {
            if(statSheet == null)
            {
                statSheet = new StatSheet();
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
        protected virtual void ChangeAnimationState(string newState)
        {          
            if (Animator != null && currentAnimationState != newState && Animator.HasState(0, Animator.StringToHash(newState)))
            {
                Animator.speed = 1f;
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
                        float attackSpeed = GetStatValue(StatType.AttackSpeed);
                        // TODO: StatManager에서 공속 값 가져와서 연동하기
                        Animator.speed = Mathf.Max(0f, attackSpeed / 10f);
                        Animator.SetTrigger("2_Attack");
                        break;
                    case "DAMAGED":
                        Animator.SetTrigger("3_Damaged");
                        break;
                    case "DEATH":
                        Animator.SetBool("isDeath", true);
                        break;
                    case "SKILL001":
                        Animator.SetTrigger("SKILL001");
                        break;
                    case "SKILL002":
                        Animator.SetTrigger("SKILL002");
                        break;
                }
                
                currentAnimationState = newState;
            }
        }

        // ===== [스탯 관련 메서드] =====
        /// <summary>
        /// 지정된 스탯 타입의 값을 가져옵니다.
        /// </summary>
        /// <param name="statType">스탯 타입</param>
        /// <returns>스탯 값</returns>
        public float GetStatValue(StatType statType)
        {
            return statSheet.Get(statType);
        }

        public int GetRawStatValue(StatType statType)
        {
            return statSheet.GetRaw(statType);
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

        // ===== [기능 10] 상태 관리 =====
        public void AddStatus(PawnStatusType statusType, object status)
        {
            statuses[statusType] = status;
        }

        public void RemoveStatus(PawnStatusType statusType)
        {
            statuses.Remove(statusType);
        }

        public bool bIsStatusValid(PawnStatusType appliedStatusType)
        {
            if (statuses.ContainsKey(appliedStatusType))
            {
                var _status = statuses[appliedStatusType];
                if (_status is PawnStatus status && status.lastTime + status.duration > Time.time)
                {
                    return true;
                }
            }
            return false;
        }

        // ===== [기능 3] 이벤트 처리 =====
        public virtual bool OnEvent(Utils.EventType eventType, object param)
        {
            // 유물들의 이벤트 처리 (필터링 적용)
            if (relics != null && IsRelicEventAccepted(eventType))
            {
                foreach (var relic in relics)
                {
                    if (relic != null)
                    {
                        var result = relic.OnEvent(eventType, param);
                    }
                }
            }
            // 덱의 카드 액션들 처리 (필터링 적용)
            if (deck != null && IsCardEventAccepted(eventType))
            {
                Debug.Log($"<color=cyan>[EVENT] {gameObject.name} ({GetType().Name}) -> Deck processing {eventType}</color>");
                var result = deck.OnEvent(eventType, param);
            }
            
            // Pawn 자체의 이벤트 처리
            switch (eventType)
            {
                case Utils.EventType.OnDamaged:
                    if (param is AttackResult result) 
                    {
                        ApplyDamage(result);
                    }

                    return true;
                case Utils.EventType.OnDeath:
                    HandleDeath();
                    return true;
                
                default:
                    return false;
            }
        }

        public abstract bool ExecuteAttack(PawnAttackType attackType = PawnAttackType.BasicAttack);

        public void ApplyDamage(AttackResult result)
        {
            // 여러번 OnDeath 이벤트가 발생되지 않기 위한 예외문
            if (isDead) return;
            
            int previousHP = currentHp;
            ChangeHP(-result.totalDamage);
            ChangeAnimationState("DAMAGED");

            //Debug.Log($"<color=red>[DAMAGE] {gameObject.name} took {result.totalDamage} damage from {result.attacker.gameObject.name}</color>");
            // TODO : 넉백 확률 스탯 부분 추가 필요
            // ApplyKnockback(damageInfo.attacker);
            
            if (currentHp <= 0)
            {
                result.attack?.OnEvent(Utils.EventType.OnKilled, result);
                result.attacker.OnEvent(Utils.EventType.OnKilled, result);

                if (result.isCritical)
                {
                    result.attack?.OnEvent(Utils.EventType.OnKilledByCritical, result);
                    result.attacker.OnEvent(Utils.EventType.OnKilledByCritical, result);
                }

                OnEvent(Utils.EventType.OnDeath, result);
            }
        }

        public void ApplyStealHealth(AttackResult result)
        {
            //Debug.Log($"<color=red>[DAMAGE] {gameObject.name} steal {result.attackerHealed} HP from {result.target.gameObject.name}</color>");
            ChangeHP(result.attackerHealed);
        }

        public void ApplyReflectDamage(AttackResult result)
        {
            //Debug.Log($"<color=red>[DAMAGE] {gameObject.name} took reflect damage {result.attackerReflectDamage} from {result.target.gameObject.name}</color>");
            ChangeHP(-result.attackerReflectDamage);
        }

        private void HandleDeath()
        {
            rb.linearVelocity = Vector3.zero;

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
            float attackSpeed = GetStatValue(StatType.AttackSpeed);
            // 공격속도 10을 기준으로 1초에 1개 발사
            // 공격속도가 높을수록 쿨다운이 짧아짐
            attackCooldown = 1f / (attackSpeed / 10f);
            
            //Debug.Log($"<color=yellow>[AUTO_ATTACK] {gameObject.name} attack speed: {attackSpeed}, cooldown: {attackCooldown:F2}s</color>");
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

        public void SetLockMovement(bool lockMovement)
        {
            Controller.lockMovement = lockMovement;
        }

        public void ClearStatModifier()
        {
            statSheet.ClearBuffs();
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
