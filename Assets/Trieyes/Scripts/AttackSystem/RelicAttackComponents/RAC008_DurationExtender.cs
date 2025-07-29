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
        private float lastKillCount = 0;

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            hero = attack.attacker as Character001_Hero;
            if (hero != null)
            {
                lastKillCount = hero.killedDuringSkill001;
                hero.RAC008Trigger = true;
            }
        }

        protected override void Update()
        {
            base.Update();
            
            // Lock 상태일 때는 Update 실행하지 않음
            if (isLocked) return;
            
            if (hero != null && hero.RAC008Trigger)
            {
                UpdateDuration();
            }
        }

        private void UpdateDuration()
        {
            var currentKillCount = hero.killedDuringSkill001;
            var killCountDiff = currentKillCount - lastKillCount;
            
            if (killCountDiff > 0)
            {
                // SK001의 enchantmentDuration을 증가시킴
                // SK001 컴포넌트를 찾아서 지속시간 증가
                var skill001Component = FindSkill001Component();
                if (skill001Component != null)
                {
                    skill001Component.enchantmentDuration += killCountDiff * 0.1f;
                    Debug.Log($"[RAC008] {killCountDiff}명 처치로 지속시간 {killCountDiff * 0.1f}초 증가!");
                }
                
                lastKillCount = currentKillCount;
            }
        }

        private Hero_S001_AttackEnchantment FindSkill001Component()
        {
            // 현재 활성화된 SK001 컴포넌트를 찾음
            var allAttacks = FindObjectsOfType<Attack>();
            foreach (var attack in allAttacks)
            {
                if (attack.attacker == hero)
                {
                    foreach (var component in attack.components)
                    {
                        if (component is Hero_S001_AttackEnchantment skill001)
                        {
                            return skill001;
                        }
                    }
                }
            }
            return null;
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