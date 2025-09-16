using UnityEngine;
using AttackSystem;
using BattleSystem;
using Stats;

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

        public override Vector2 CenterOffset { get; set; } = Vector2.zero;
        
        // ===== [Unity 생명주기] =====
        protected override void Start()
        {
            base.Start();
            
            // Collision Layer를 Character로 설정
            gameObject.layer = LayerMask.NameToLayer("Character");
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
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

        protected override void OnCollisionEnter2D(Collision2D other)
        {
            base.OnCollisionEnter2D(other);
            if(other.gameObject.CompareTag("Enemy"))
            {
                lastTriggerEnterTime = Time.time;
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

        protected override void ChangeAnimationState(string newState)
        {
            if (Animator != null && Animator.HasState(0, Animator.StringToHash(newState)) && newState == "ATTACK")
            {
                Animator.SetFloat("AttackSpeedMultiplier", GetStatValue(StatType.AttackSpeed)/3);
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
