using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;

namespace AttackComponents
{
    /// <summary>
    /// 둔화가 걸린 적이 다시 둔화에 걸리는 경우 해당 적의 방어력이 대폭 감소하는 컴포넌트
    /// </summary>
    public class RAC012_IceSlowStackEffect : AttackComponent
    {
        private Character001_Hero hero;

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            hero = attack.attacker as Character001_Hero;
        }

        public override void PerformLockedSetup()
        {
            base.PerformLockedSetup();
            
            // Lock 상태에서 enchantment min, max 값 조절 + 트리거 설정
            SetupIceEnchantment();
            if (hero != null)
            {
                hero.RAC012Trigger = true;
                Debug.Log("[RAC012] Lock 상태에서 얼음 속성 트리거 활성화!");
            }
        }

        private void SetupIceEnchantment()
        {
            if (hero != null)
            {
                // 얼음 속성만 나오도록 설정 (2번만)
                hero.SetRandomEnchantmentMinID(2);
                hero.SetRandomEnchantmentMaxID(2);
                
                Debug.Log("[RAC012] Lock 상태에서 얼음 속성만 나오도록 설정됨!");
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            if (hero != null)
            {
                // 트리거 초기화
                hero.RAC012Trigger = false;
                
                Debug.Log("[RAC012] 얼음 속성 둔화 중첩 효과 종료!");
            }
        }
    }
} 