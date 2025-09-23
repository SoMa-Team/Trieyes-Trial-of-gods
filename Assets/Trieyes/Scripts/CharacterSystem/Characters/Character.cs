using UnityEngine;
using AttackSystem;
using BattleSystem;
using Stats;
using System;

namespace CharacterSystem
{
    public class Character : Pawn
    {
        // ===== [필드] =====
        
        // Pawn의 추상 멤버 구현
        
        protected float lastTriggerEnterTime = 0f;
        [HideInInspector] public float collisionDamageInterval = 0.5f;

        public override Vector2 CenterOffset { get; set; } = Vector2.zero;
        
        // ===== [Unity 생명주기] =====
        protected override void Start()
        {
            base.Start();
            
            if (Animator is null)
            {
                Animator = pawnPrefab.transform.Find("UnitRoot").GetComponent<Animator>();
            }
            
            // Collision Layer를 Character로 설정
            gameObject.layer = LayerMask.NameToLayer("Character");
        }

        public override void Update()
        {
            base.Update();
            
            Controller?.ProcessInputActions();
        }

        // ===== [커스텀 메서드] =====
        public override void Activate()
        {
            base.Activate();
            
            this.transform.position = Vector3.zero;

            //Debug.Log("Character001 Activated.");
        }

        // ===== [이벤트 처리 메서드] =====
        /// <summary>
        /// 이벤트를 처리합니다.
        /// </summary>
        /// <param name="eventType">이벤트 타입</param>
        /// <param name="param">이벤트 파라미터</param>
        public override bool OnEvent(Utils.EventType eventType, object param)
        {
            // 부모의 이벤트 전파 로직 호출 (필터링 적용됨)
            base.OnEvent(eventType, param);

            // Character001 고유의 이벤트 처리
            switch (eventType)
            {
                case Utils.EventType.OnLevelUp:
                    return true;
                
                case Utils.EventType.OnDeath:
                    // 죽고 나서 할 것
                    if(BattleStage.now.mainCharacter == this)
                    {
                        BattleStage.now.OnPlayerDeath();
                        return true;
                    }
                    return false;
            }
            return false;
        }
        
        public virtual bool ExecuteAttack(PawnAttackType attackType = PawnAttackType.BasicAttack)
        {
            if(CheckCooldown(attackType))
            {
                switch (attackType)
                {
                    case PawnAttackType.BasicAttack:
                        // t2 = t1;
                        // t1 = Time.time;
                        // Debug.LogError($"delta time : {t1 - t2}");
                        lastAttackTime = Time.time;
                        ChangeAnimationState("ATTACK");
                        CreateAttack(PawnAttackType.BasicAttack);
                        return true;
                    case PawnAttackType.Skill1:
                        if (skill1Attack is null)
                        {
                            return false;
                        }
                        lastSkillAttack1Time = Time.time;
                        ChangeAnimationState("SKILL001");
                        CreateAttack(PawnAttackType.Skill1);
                        return true;

                    case PawnAttackType.Skill2:
                        if (skill2Attack is null)
                        {
                            return false;
                        }
                        lastSkillAttack2Time = Time.time;
                        ChangeAnimationState("SKILL002");
                        CreateAttack(PawnAttackType.Skill2);
                        return true;
                        
                    default:
                        return false;
                }
            }
            return false;
        }

        protected override void OnCollisionEnter2D(Collision2D other)
        {
            base.OnCollisionEnter2D(other);
            if(other.gameObject.CompareTag("Enemy"))
            {
                lastTriggerEnterTime = Time.time;

                var enemy = other.gameObject.GetComponent<Enemy>();
                enemy.ExecuteAttack();
                DamageProcessor.ProcessHit(enemy, this);
            }
        }

        protected virtual void OnCollisionStay2D(Collision2D other)
        {
            if(!other.gameObject.CompareTag("Enemy"))
            {
                return;
            }

            var currentTime = Time.time;
            if(currentTime - lastTriggerEnterTime >= collisionDamageInterval)
            {
                var enemy = other.gameObject.GetComponent<Enemy>();
                enemy.ExecuteAttack();
                DamageProcessor.ProcessHit(enemy, this);
                lastTriggerEnterTime = currentTime;
            }
        }

        protected override void OnCollisionExit2D(Collision2D other)
        {
            base.OnCollisionExit2D(other);
            if(other.gameObject.CompareTag("Enemy"))
            {
                lastTriggerEnterTime = 0f;
            }
        }

        private const int MAX_ANIMATION_MULTIPLIER = 3;
        private const int SKIP_ANIMIATION_MULTIPLIER = 10000;
        protected override void ChangeAnimationState(string newState)
        {
            float attackSpeedMultiplier = Mathf.Min(MAX_ANIMATION_MULTIPLIER, Mathf.Max(GetStatValue(StatType.AttackSpeed)/3, 1f));
            if (Animator != null && Animator.HasState(0, Animator.StringToHash(newState)) && newState == "ATTACK")
            {
                if(GetStatValue(StatType.AttackSpeed) >= SKIP_ANIMIATION_MULTIPLIER)
                {
                    return;
                }
                Animator.SetFloat("AttackSpeedMultiplier", attackSpeedMultiplier);
                Animator.SetTrigger("2_Attack");
            }
            else
            {
                base.ChangeAnimationState(newState);
            }
        }

        public void CreateAttack(PawnAttackType attackType)
        {
            switch (attackType)
            {
                case PawnAttackType.BasicAttack:
                    AttackFactory.Instance.Create(basicAttack, this, null, LastMoveDirection); 
                    break;
                case PawnAttackType.Skill1:
                    AttackFactory.Instance.Create(skill1Attack, this, null, LastMoveDirection);
                    break;
                case PawnAttackType.Skill2:
                    AttackFactory.Instance.Create(skill2Attack, this, null, LastMoveDirection);
                    break;
            }
        }
    }
}
