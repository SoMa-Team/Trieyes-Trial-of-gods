using UnityEngine;
using System.Linq;
using AttackSystem;
using Stats;

namespace CharacterSystem
{
    public class C001_Hero : Character
    {
        // ===== [필드] =====
        public enum HeroWeaponElementState
        {
            None,
            Fire,
            Ice,
            Lightning,
            Light,
        }

        public HeroWeaponElementState weaponElementState = HeroWeaponElementState.None;
        
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