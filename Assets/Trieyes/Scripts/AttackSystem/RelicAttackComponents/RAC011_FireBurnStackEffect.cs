using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using Utils;

namespace AttackComponents
{
    /// <summary>
    /// 화상을 입은 적이 다시 화상을 입는 경우 남은 화상 피해량의 20퍼센트를 즉시 입히고 화상의 지속시간을 초기화하는 컴포넌트
    /// </summary>
    public class RAC011_FireBurnStackEffect : AttackComponent
    {
        private Character001_Hero hero;

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            hero = attack.attacker as Character001_Hero;
        }

        public override void OnLockActivate()
        {
            base.OnLockActivate();
            
            // Lock 상태에서 enchantment min, max 값 조절 + 트리거 설정
            SetupFireEnchantment();
            if (hero != null)
            {
                hero.RAC011Trigger = true;
                Debug.Log("[RAC011] Lock 상태에서 불 속성 트리거 활성화!");
            }
        }

        private void SetupFireEnchantment()
        {
            if (hero != null)
            {
                // 불 속성만 나오도록 설정 (1번만)
                hero.SetRandomEnchantmentMinID(1);
                hero.SetRandomEnchantmentMaxID(1);
                
                Debug.Log("[RAC011] Lock 상태에서 불 속성만 나오도록 설정됨!");
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            if (hero != null)
            {
                // 트리거 초기화
                hero.RAC011Trigger = false;
                
                Debug.Log("[RAC011] 불 속성 화상 중첩 효과 종료!");
            }
        }
    }
} 