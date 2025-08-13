using UnityEngine;
using System.Linq;
using AttackSystem;
using Stats;
using AttackComponents;

namespace CharacterSystem
{
    public enum HeroWeaponElementState
    {
        None,
        Fire,
        Ice,
        Lightning,
        Light,
    }
    
    public class Character001_Hero : Character
    {
        // ===== [필드] =====

        public HeroWeaponElementState weaponElementState = HeroWeaponElementState.None;
        public bool activateLight = false;

        public bool lockBasicAttack = false;

        public int minRandomEnchantmentID = 1;
        public int maxRandomEnchantmentID = 4;

        public int activeSkill001Count = 0;
        public int activeSkill002Count = 0;
        public int killedDuringSkill001 = 0;
        public int killedDuringSkill002 = 0;
        
        // RAC 관련 트리거 변수들
        public bool RAC006Trigger = false; // 속성 검기 발사용
        public bool RAC008Trigger = false; // 지속시간 증가용
        public bool RAC010Trigger = false; // 번개 속성 공격속도 증가용
        public bool RAC011Trigger = false; // 화상 중첩 효과용
        public bool RAC012Trigger = false; // 둔화 중첩 효과용
        
        // RAC009 컴포넌트 (hero가 직접 소유)
        public AC108_OrbitingManager orbitingManager;
        
        // Pawn의 추상 멤버 구현
        
        // ===== [Unity 생명주기] =====
        protected override void Start()
        {
            base.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void Update()
        {
            base.Update();
        }

        // ===== [커스텀 메서드] =====
        public override void Activate()
        {
            base.Activate();
            CapsuleCollider2D capsuleCollider = Collider as CapsuleCollider2D;
            capsuleCollider.offset = new Vector2(0, 0.3f);
            capsuleCollider.size = new Vector2(0.7f, 1.2f);
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);
        }

        // ===== [공격 처리 메서드] =====
        /// <summary>
        /// 공격을 실행합니다. lockBasicAttack이 true면 기본 공격을 차단합니다.
        /// </summary>
        public override bool ExecuteAttack(PawnAttackType attackType = PawnAttackType.BasicAttack)
        {
            // lockBasicAttack이 true면 기본 공격 차단
            if (lockBasicAttack && attackType == PawnAttackType.BasicAttack)
            {
                //Debug.Log("<color=red>[HERO] Basic attack blocked by lockBasicAttack!</color>");
                return false;
            }
            
            return base.ExecuteAttack(attackType);
        }

        /// <summary>
        /// 자동 공격을 수행합니다. lockBasicAttack이 true면 자동 공격을 차단합니다.
        /// </summary>
        public override void PerformAutoAttack()
        {
            // lockBasicAttack이 true면 자동 공격 차단
            if (lockBasicAttack)
            {
                //Debug.Log("<color=red>[HERO] Auto attack blocked by lockBasicAttack!</color>");
                return;
            }
            
            base.PerformAutoAttack();
        }

        // ===== [이벤트 처리 메서드] =====
        /// <summary>
        /// 이벤트를 처리합니다.
        /// </summary>
        /// <param name="eventType">이벤트 타입</param>
        /// <param name="param">이벤트 파라미터</param>
        public override bool OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param);
            if (eventType == Utils.EventType.OnKilled || eventType == Utils.EventType.OnKilledByCritical)
            {
                if (weaponElementState != HeroWeaponElementState.None)
                {
                    killedDuringSkill001++;
                    killedDuringSkill002++;
                    
                    return true;
                }
                return false;
            }
            return false;
        }

        public void SetRandomEnchantmentMinID(int min)
        {
            minRandomEnchantmentID = min;
        }

        public void SetRandomEnchantmentMaxID(int max)
        {
            maxRandomEnchantmentID = max;
        }
    }
}