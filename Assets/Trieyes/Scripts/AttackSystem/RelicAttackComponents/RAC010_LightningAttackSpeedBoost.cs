using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;

namespace AttackComponents
{
    /// <summary>
    /// 번개 속성이 부여됐을 때 공격속도가 크게 증가하는 컴포넌트
    /// 1회만 증가하며, Deactivate될 때 트리거를 초기화합니다.
    /// </summary>
    public class RAC010_LightningAttackSpeedBoost : AttackComponent
    {
        private Character001_Hero hero;
        private bool boostApplied = false;

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            hero = attack.attacker as Character001_Hero;
        }

        public override void OnLockActivate()
        {
            base.OnLockActivate();
            
            // Lock 상태에서 enchantment min, max 값 조절 + 트리거 설정
            SetupLightningEnchantment();
            if (hero != null)
            {
                hero.RAC010Trigger = true;
                Debug.Log("[RAC010] Lock 상태에서 번개 속성 트리거 활성화!");
            }
        }

        private void SetupLightningEnchantment()
        {
            if (hero != null)
            {
                // 번개 속성만 나오도록 설정 (3번만)
                hero.SetRandomEnchantmentMinID(3);
                hero.SetRandomEnchantmentMaxID(3);
                
                Debug.Log("[RAC010] Lock 상태에서 번개 속성만 나오도록 설정됨!");
            }
        }

        protected override void Update()
        {
            base.Update();
            
            // Lock 상태일 때는 Update 실행하지 않음
            if (isLocked) return;
            
            if (hero != null && hero.RAC010Trigger && !boostApplied)
            {
                ApplyAttackSpeedBoost();
            }
        }

        private void ApplyAttackSpeedBoost()
        {
            if (boostApplied) return;

            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseAttackSpeed,
                attack = attack,
                target = hero,
                buffMultiplier = 50f,
                buffDuration = 10f,
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            
            boostApplied = true;
            
            Debug.Log($"[RAC010] 번개 속성으로 공격속도 증가!");
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            if (hero != null)
            {
                // 트리거 초기화 (min, max는 계속 유지됨)
                hero.RAC010Trigger = false;
                
                Debug.Log("[RAC010] 번개 속성 공격속도 증가 효과 종료!");
            }
        }
    }
} 