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

        // ===== [기능 2] 스탯 시스템 =====
        protected Dictionary<StatType, StatInfo> stats = new();

        // ===== [기능 3] 이벤트 처리 =====
        public abstract void OnEvent(Utils.EventType eventType, object param);

        // ===== [기능 4] 공격 시스템 =====
        public abstract void TakeAttack(Attack attack);

        public abstract void PerformAttack(Pawn target, Attack attack);

        // ===== [기능 5] 공격/스탯/유물/덱 관리 =====
        public Attack basicAttack; // 기본 공격
        public AttackData[] attackDataList; // 여러 공격 데이터
        public List<AttackComponent> attackComponentList = new(); // 공격 컴포넌트 리스트
        public List<StatInfo> statInfos = new(); // 여러 스탯 정보
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
            if (!statInfos.Any(s => s.Type == StatType.Health))
            {
                statInfos.Add(new StatInfo(StatType.Health, 100));
            }
            if (!statInfos.Any(s => s.Type == StatType.MoveSpeed))
            {
                statInfos.Add(new StatInfo(StatType.MoveSpeed, moveSpeed));
            }
        }
        public virtual void Update() 
        {
            StatInfo healthStat = statInfos.FirstOrDefault(s => s.Type == StatType.Health);
            if (healthStat != null && healthStat.Value <= 0 && !isDead)
            {
                isDead = true;
                Debug.Log($"<color=red>{gameObject.name} has died. Triggering OnDeath event.</color>");
                TriggerDeckActionInit(Utils.EventType.OnDeath, this);
                ChangeAnimationState("Die"); 
                if (rb != null) rb.bodyType = RigidbodyType2D.Static; 
                if (boxCollider != null) boxCollider.enabled = false; 
                Destroy(gameObject, 0.01f); 
            }
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
        public virtual void Jump()
        {
            if (isGrounded && rb != null)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                ChangeAnimationState(JUMP_ANIM);
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

        // ===== [기능 7] 스탯 관련 =====
        public float GetStatValue(StatType statType)
        {
            var stat = statInfos.FirstOrDefault(s => s.Type == statType);
            return stat?.Value ?? 0f;
        }
        public void SetStatValue(StatType statType, float value)
        {
            var stat = statInfos.FirstOrDefault(s => s.Type == statType);
            if (stat != null)
            {
                stat.Value = value;
            }
            else
            {
                statInfos.Add(new StatInfo(statType, value));
            }
        }
        public void IncreaseStatValue(StatType statType, float amount)
        {
            var stat = statInfos.FirstOrDefault(s => s.Type == statType);
            if (stat != null)
            {
                stat.Value += amount;
            }
            else
            {
                statInfos.Add(new StatInfo(statType, amount));
            }
        }
        public void DecreaseStatValue(StatType statType, float amount)
        {
            var stat = statInfos.FirstOrDefault(s => s.Type == statType);
            if (stat != null)
            {
                stat.Value -= amount;
            }
        }
        public StatInfo GetStat(StatType statType)
        {
            return statInfos.FirstOrDefault(s => s.Type == statType);
        }
        public void ModifyStat(StatType statType, int amount)
        {
            var stat = statInfos.FirstOrDefault(s => s.Type == statType);
            if (stat != null)
            {
                stat.Value += amount;
            }
        }
        public void ApplyStatMultiplier(float difficultyEnemyStatMultiplier)
        {
            foreach (var stat in statInfos)
            {
                stat.Value *= difficultyEnemyStatMultiplier;
            }
        }

        // ===== [기능 8] 덱 액션 트리거 =====
        public void TriggerDeckActionInit(Utils.EventType eventType, object param = null)
        {
            deck.CalcActionInitStat(eventType, param);
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
    }
} 