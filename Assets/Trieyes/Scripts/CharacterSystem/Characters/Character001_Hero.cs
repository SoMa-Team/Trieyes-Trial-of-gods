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

        public int minRandomEnchantmentID = 1;
        public int maxRandomEnchantmentID = 4;

        public int killedDuringSkill001 = 0;
        public int killedDuringSkill002 = 0;

        public AttackData _basicAttack;
        public AttackData Skill001Attack;

        public Hero_S001_AttackEnchantment currentEnchantment;
        
        // RAC 관련 트리거 변수들
        public bool RAC006Trigger = false; // 속성 검기 발사용
        public bool RAC008Trigger = false; // 지속시간 증가용
        public bool RAC010Trigger = false; // 번개 속성 공격속도 증가용
        public bool RAC011Trigger = false; // 화상 중첩 효과용
        public bool RAC012Trigger = false; // 둔화 중첩 효과용
        
        // RAC009 컴포넌트 (hero가 직접 소유)
        public AC108_OrbitingManager orbitingManager;

        // ===== [커스텀 메서드] =====
        public override void Activate()
        {
            base.Activate();
            weaponElementState = HeroWeaponElementState.None;
            _basicAttack = basicAttack;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            weaponElementState = HeroWeaponElementState.None;
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
