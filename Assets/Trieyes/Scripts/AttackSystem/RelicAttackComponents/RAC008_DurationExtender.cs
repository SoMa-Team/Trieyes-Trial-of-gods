using AttackSystem;
using CharacterSystem;
using UnityEngine;

namespace AttackComponents
{
    /// <summary>
    /// 속성 부여 상태에서 적을 죽일 때마다 지속시간이 0.1초 증가하는 컴포넌트
    /// SK001의 enchantmentDuration을 증가시킵니다.
    /// </summary>
    public class RAC008_DurationExtender : AttackComponent
    {
        private Character001_Hero hero;

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            hero = attack.attacker as Character001_Hero;
            if (hero != null)
            {
                hero.RAC008Trigger = true;
            }
        }

        protected override void Update()
        {
            base.Update();
            
            // Lock 상태일 때는 Update 실행하지 않음
            if (isLocked) return;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            if (hero != null)
            {
                hero.RAC008Trigger = false;
            }
        }
    }
} 