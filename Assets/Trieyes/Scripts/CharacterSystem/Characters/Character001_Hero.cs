using UnityEngine;
using System.Linq;
using AttackSystem;
using Stats;

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
        
        // Pawn의 추상 멤버 구현
        
        // ===== [Unity 생명주기] =====
        protected override void Awake()
        {
            base.Awake();
        }

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
        }

        public override void Deactivate()
        {
            base.Deactivate();
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
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param);
        }
    }
}