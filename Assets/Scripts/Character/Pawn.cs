using System.Collections.Generic;
using Core;
using AttackSystem;
using Utils;
using RelicSystem;
using UnityEngine;
using CardSystem;
using System.Linq;

namespace CharacterSystem
{
    public abstract class Pawn : MonoBehaviour, IEventHandler, IMovable
    {
        public Attack basicAttack; // 기본 공격
        public AttackData[] AttackDataList; // 여러 공격 데이터
        public List<AttackComponent> attackComponentList = new(); // 공격 컴포넌트 리스트
        public List<StatInfo> statInfos = new(); // 여러 스탯 정보

        public List<Relic> relics = new(); // 장착 가능한 유물 리스트

        public Deck deck = new Deck(); // Pawn이 관리하는 Deck 인스턴스

        /// <summary>
        /// 이 Pawn 인스턴스에 등록된 이벤트 핸들러들을 관리하는 딕셔너리입니다.
        /// 각 EventType에 대해 여러 개의 EventDelegate를 가질 수 있습니다.
        /// </summary>
        private Dictionary<Core.EventType, List<EventDelegate>> eventHandlers = new(); // 이벤트핸들러

        // 이동 관련 필드 여기는 필요 시 수정
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float jumpForce = 5f;
        protected Vector2 moveDirection;
        protected bool isGrounded;
        protected Rigidbody2D rb;
        protected BoxCollider2D boxCollider;

        // 시각적 요소 관련 필드
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Animator animator;
        protected string currentAnimationState;

        // 애니메이션 상태 상수
        protected const string IDLE_ANIM = "Idle";
        protected const string WALK_ANIM = "Walk";
        protected const string JUMP_ANIM = "Jump";
        protected const string ATTACK_ANIM = "Attack";
        protected const string HIT_ANIM = "Hit";

        protected bool isDead = false; // 사망 여부 플래그

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
            // 기본 스탯 초기화
            if (!statInfos.Any(s => s.type == StatType.Health))
            {
                statInfos.Add(new StatInfo(StatType.Health, 100));
            }
            if (!statInfos.Any(s => s.type == StatType.MoveSpeed))
            {
                statInfos.Add(new StatInfo(StatType.MoveSpeed, moveSpeed));
            }
        }

        /// <summary>
        /// 특정 스탯의 값을 가져옵니다.
        /// </summary>
        /// <param name="statType">가져올 스탯 타입</param>
        /// <returns>스탯 값, 없으면 0</returns>
        public float GetStatValue(StatType statType)
        {
            var stat = statInfos.FirstOrDefault(s => s.type == statType);
            return stat?.value ?? 0f;
        }

        /// <summary>
        /// 특정 스탯의 값을 설정합니다.
        /// </summary>
        /// <param name="statType">설정할 스탯 타입</param>
        /// <param name="value">설정할 값</param>
        public void SetStatValue(StatType statType, float value)
        {
            var stat = statInfos.FirstOrDefault(s => s.type == statType);
            if (stat != null)
            {
                stat.value = value;
            }
            else
            {
                statInfos.Add(new StatInfo(statType, value));
            }
        }

        /// <summary>
        /// 특정 스탯의 값을 증가시킵니다.
        /// </summary>
        /// <param name="statType">증가시킬 스탯 타입</param>
        /// <param name="amount">증가시킬 양</param>
        public void IncreaseStatValue(StatType statType, float amount)
        {
            var stat = statInfos.FirstOrDefault(s => s.type == statType);
            if (stat != null)
            {
                stat.value += amount;
            }
            else
            {
                statInfos.Add(new StatInfo(statType, amount));
            }
        }

        /// <summary>
        /// 특정 스탯의 값을 감소시킵니다.
        /// </summary>
        /// <param name="statType">감소시킬 스탯 타입</param>
        /// <param name="amount">감소시킬 양</param>
        public void DecreaseStatValue(StatType statType, float amount)
        {
            var stat = statInfos.FirstOrDefault(s => s.type == statType);
            if (stat != null)
            {
                stat.value -= amount;
            }
        }

        /// <summary>
        /// 특정 이벤트 타입에 대한 핸들러를 등록합니다.
        /// 이 Pawn이 해당 이벤트를 발동시켰을 때 handler 메서드가 호출됩니다.
        /// </summary>
        /// <param name="eventType">등록할 이벤트의 타입</param>
        /// <param name="handler">이벤트 발생 시 호출될 델리게이트 (메서드)</param>
        public virtual void RegisterEvent(Core.EventType eventType, EventDelegate handler)
        {
            if (!eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType] = new List<EventDelegate>();
            eventHandlers[eventType].Add(handler);
        }

        /// <summary>
        /// 특정 이벤트 타입에 등록된 핸들러를 해제합니다.
        /// 더 이상 해당 이벤트를 수신하지 않을 때 사용됩니다.
        /// </summary>
        /// <param name="eventType">해제할 이벤트의 타입</param>
        /// <param name="handler">해제할 델리게이트 (메서드)</param>
        public virtual void UnregisterEvent(Core.EventType eventType, EventDelegate handler)
        {
            if (eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType].Remove(handler);
        }

        /// <summary>
        /// 이벤트 발생 시 호출되는 메서드입니다. 모든 이벤트 처리는 이 메서드를 통해 이루어집니다.
        /// </summary>
        /// <param name="eventType">발생한 이벤트 타입</param>
        /// <param name="param">이벤트 매개변수</param>
        public virtual void OnEvent(Core.EventType eventType, object param)
        {
            switch (eventType)
            {
                case Core.EventType.OnDeath:
                    OnDeath(param);
                    break;
                case Core.EventType.OnBattleStart:
                    OnBattleStart(param);
                    break;
                case Core.EventType.OnBattleEnd:
                    OnBattleEnd(param);
                    break;
                // 필요한 다른 이벤트 케이스들을 여기에 추가
                default:
                    Debug.LogWarning($"Pawn {gameObject.name}: 처리되지 않은 이벤트 타입 {eventType}");
                    break;
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

        /// <summary>
        /// 전투 시작 이벤트 발생 시 호출되는 메서드입니다.
        /// </summary>
        protected virtual void OnBattleStart(object param)
        {
            Debug.Log($"<color=green>{gameObject.name} (Pawn) battle started!</color>");
            // 기본 전투 시작 처리 로직
        }

        /// <summary>
        /// 전투 종료 이벤트 발생 시 호출되는 메서드입니다.
        /// </summary>
        protected virtual void OnBattleEnd(object param)
        {
            Debug.Log($"<color=blue>{gameObject.name} (Pawn) battle ended!</color>");
            // 기본 전투 종료 처리 로직
        }

        /// <summary>
        /// Deck의 CalcActionInitStat을 호출하여 Pawn의 덱에 있는 카드들의 특정 이벤트를 발동시킵니다.
        /// 이는 Pawn의 사망 같은 주요 이벤트에 따라 덱 내의 카드 액션들이 반응하도록 합니다.
        /// </summary>
        /// <param name="eventType">발동할 이벤트 타입 (예: Core.EventType.OnDeath)</param>
        /// <param name="param">이벤트 매개변수</param>
        public void TriggerDeckActionInit(Core.EventType eventType, object param = null)
        {
            deck.CalcActionInitStat(eventType, param);
        }

        public virtual void Update() 
        {
            StatInfo healthStat = statInfos.FirstOrDefault(s => s.type == StatType.Health);
            if (healthStat != null && healthStat.value <= 0 && !isDead)
            {
                isDead = true;
                Debug.Log($"<color=red>{gameObject.name} has died. Triggering OnDeath event.</color>");

                // Pawn 자체의 사망 이벤트를 발동시킵니다. 이 Pawn에 등록된 모든 핸들러가 호출됩니다.
                TriggerEvent(Core.EventType.OnDeath, this); // 사망한 Pawn 자신을 매개변수로 전달

                // Pawn의 덱에 있는 CardAction들에게도 OnDeath 이벤트를 전파합니다.
                TriggerDeckActionInit(Core.EventType.OnDeath, this);

                // 사망 애니메이션 재생 등 시각적/물리적 처리
                ChangeAnimationState("Die"); 
                if (rb != null) rb.bodyType = RigidbodyType2D.Static; 
                if (boxCollider != null) boxCollider.enabled = false; 
                Destroy(gameObject, 0.01f); 
            }
        }

        /// <summary>
        /// Pawn이 데미지를 입었을 때 호출됩니다.
        /// </summary>
        /// <param name="damage">입을 데미지 양</param>
        public virtual void TakeDamage(int damage)
        {
            if (isDead) return;

            StatInfo healthStat = statInfos.FirstOrDefault(s => s.type == StatType.Health);
            if (healthStat != null)
            {
                healthStat.value -= damage; // 체력 감소
                Debug.Log($"{gameObject.name} took {damage} damage. Current Health: {healthStat.value}");
            }
            // Update()에서 사망 여부를 체크하므로 여기서 별도로 사망 로직을 호출할 필요 없음
        }

        // 여기 밑에는 기본적인 이동 로직 구현임, 없애도 됨

        public virtual void Move(Vector2 direction)
        {
            moveDirection = direction;
            
            // 이동 방향에 따라 스프라이트 뒤집기
            if (direction.x != 0)
            {
                spriteRenderer.flipX = direction.x < 0;
            }

            // 애니메이션 상태 업데이트
            if (direction.magnitude > 0)
            {
                ChangeAnimationState(WALK_ANIM);
            }
            else
            {
                ChangeAnimationState(IDLE_ANIM);
            }
        }

        public virtual void Jump()
        {
            if (isGrounded)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                ChangeAnimationState(JUMP_ANIM);
            }
        }

        protected virtual void FixedUpdate()
        {
            // 이동 처리
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
            }

            // 지면 체크
            CheckGrounded();
        }

        protected virtual void CheckGrounded()
        {
            if (boxCollider != null)
            {
                float extraHeight = 0.1f;
                RaycastHit2D raycastHit = Physics2D.BoxCast(
                    boxCollider.bounds.center,
                    boxCollider.bounds.size,
                    0f,
                    Vector2.down,
                    extraHeight,
                    LayerMask.GetMask("Ground")
                );
                isGrounded = raycastHit.collider != null;
            }
        }

        protected virtual void ChangeAnimationState(string newState)
        {
            if (currentAnimationState == newState) return;
            
            if (animator != null)
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

        public virtual void TriggerEvent(Core.EventType eventType, object param = null)
        {
            // 이벤트 발생 시 OnEvent 메서드를 호출하여 중앙 집중식 이벤트 처리
            OnEvent(eventType, param);
            
            // 기존의 이벤트 구독자들에게도 이벤트를 전파
            if (eventHandlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    handler?.Invoke(param);
                }
            }
        }

        /// <summary>
        /// 이름으로 스탯 정보를 가져옵니다.
        /// </summary>
        /// <param name="statType">가져올 스탯의 타입</param>
        /// <returns>해당 StatInfo 객체, 없으면 null</returns>
        public StatInfo GetStat(StatType statType)
        {
            return statInfos.Find(s => s.type == statType);
        }

        /// <summary>
        /// 특정 스탯의 값을 변경합니다.
        /// </summary>
        /// <param name="statType">변경할 스탯의 타입</param>
        /// <param name="amount">변경할 값 (양수: 증가, 음수: 감소)</param>
        public void ModifyStat(StatType statType, int amount)
        {
            StatInfo stat = GetStat(statType);
            if (stat != null)
            {
                stat.value += amount;
                Debug.Log($"{gameObject.name}: {statType} 스탯이 {amount}만큼 변경되어 {stat.value}이 되었습니다.");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: {statType} 스탯을 찾을 수 없어 변경할 수 없습니다.");
            }
        }

        public void ApplyStatMultiplier(float difficultyEnemyStatMultiplier)
        {
            throw new System.NotImplementedException();
        }
    }
} 