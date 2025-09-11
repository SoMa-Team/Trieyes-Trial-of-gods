using System;
using UnityEngine;
using System.Linq;
using AttackSystem;
using Stats;
using BattleSystem;
using UnityEngine.EventSystems;

namespace CharacterSystem
{
    public class Character : Pawn
    {
        // ===== [필드] =====
        
        // Pawn의 추상 멤버 구현
        public Vector3 lastPosition;

        public int spawnID;
        
        protected float lastTriggerEnterTime = 0f;
        public float collisionDamageInterval = 0.5f;

        public override Vector2 CenterOffset { get { return Vector2.zero; } }
        
        // ===== [Unity 생명주기] =====
        protected override void Start()
        {
            base.Start();
            
            // Collision Layer를 Character로 설정
            gameObject.layer = LayerMask.NameToLayer("Character");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void Update()
        {
            base.Update();
            Controller?.ProcessInputActions();
            lastPosition = transform.position;
        }

        // ===== [커스텀 메서드] =====
        public override void Activate()
        {
            base.Activate();
            
            this.transform.position = Vector3.zero;

            //Debug.Log("Character001 Activated.");
        }

        public override void Deactivate()
        {
            base.Deactivate();
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

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);
            if(other.gameObject.CompareTag("Enemy"))
            {
                lastTriggerEnterTime = Time.time;
            }
        }

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
            if(!other.gameObject.CompareTag("Enemy"))
            {
                return;
            }

            var currentTime = Time.time;
            if(currentTime - lastTriggerEnterTime >= collisionDamageInterval)
            {
                var enemy = other.gameObject.GetComponent<Enemy>();
                DamageProcessor.ProcessHit(enemy, this);
                lastTriggerEnterTime = currentTime;
            }
        }

        protected override void OnTriggerExit2D(Collider2D other)
        {
            base.OnTriggerExit2D(other);
            if(other.gameObject.CompareTag("Enemy"))
            {
                lastTriggerEnterTime = 0f;
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

        public override bool ExecuteAttack(PawnAttackType attackType = PawnAttackType.BasicAttack)
        {
            switch (attackType)
            {
                case PawnAttackType.BasicAttack:
                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        CalculateAttackCooldown();
                        lastAttackTime = Time.time;
                        ChangeAnimationState("ATTACK");
                        return true;
                    }
                    return false;
                case PawnAttackType.Skill1:
                    if (skill1Attack is not null && CheckSkillCooldown(PawnAttackType.Skill1))
                    {
                        lastSkillAttack1Time = Time.time;
                        ChangeAnimationState("SKILL001");
                        return true;
                    }
                    return false;

                case PawnAttackType.Skill2:
                    if (skill2Attack is not null &&CheckSkillCooldown(PawnAttackType.Skill2))
                    {
                        lastSkillAttack2Time = Time.time;
                        ChangeAnimationState("SKILL002");
                        return true;
                    }
                    return false;
                    
                default:
                    return false;
            }
        }
    }
}
