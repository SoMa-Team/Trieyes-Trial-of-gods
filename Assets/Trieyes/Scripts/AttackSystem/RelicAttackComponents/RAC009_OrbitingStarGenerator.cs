using AttackSystem;
using CharacterSystem;
using UnityEngine;

namespace AttackComponents
{
    /// <summary>
    /// 속성 부여 스킬을 사용할 때마다 해당 라운드가 끝날 때까지 자신의 주위를 공전하는 속성 별을 생성하는 컴포넌트
    /// AC107을 생성하고 바로 종료됩니다.
    /// </summary>
    public class RAC009_OrbitingStarGenerator : AttackComponent
    {
        public AttackData orbitingStarAttackData; // AC107용 AttackData

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // AC107 Orbiting Star 생성
            CreateOrbitingStar();
            
            // 바로 종료
            AttackFactory.Instance.Deactivate(attack);
        }

        private void CreateOrbitingStar()
        {
            if (orbitingStarAttackData == null)
            {
                Debug.LogError("[RAC009] orbitingStarAttackData가 설정되지 않았습니다!");
                return;
            }

            var hero = attack.attacker as Character001_Hero;
            if (hero == null) return;

            // AC107 Orbiting Star 생성
            var orbitingStarAttack = AttackFactory.Instance.Create(orbitingStarAttackData, attack.attacker, null, Vector2.zero);
            
            // AC107 컴포넌트 설정
            var orbitingComponent = orbitingStarAttack.components[0] as AC107_OrbitingElement;
            if (orbitingComponent != null)
            {
                // Hero를 공전 대상으로 설정
                orbitingComponent.SetOrbitTarget(hero.transform);
                
                // 속성별 별 설정
                SetupOrbitingStarByElement(orbitingComponent, hero.weaponElementState);
            }
            
            Debug.Log($"[RAC009] {hero.name}의 주위에 {hero.weaponElementState} 속성 별 생성!");
        }

        private void SetupOrbitingStarByElement(AC107_OrbitingElement orbitingComponent, HeroWeaponElementState elementState)
        {
            switch (elementState)
            {
                case HeroWeaponElementState.Fire:
                    // 불 속성 별 설정
                    orbitingComponent.SetOrbitParameters(2f, 90f, 1.0f); // 반지름 2, 속도 90도/초, 원형
                    orbitingComponent.SetColliderParameters(4, 0.3f, 0.3f, 2f); // 4개 콜라이더
                    break;
                    
                case HeroWeaponElementState.Ice:
                    // 얼음 속성 별 설정
                    orbitingComponent.SetOrbitParameters(2.5f, 60f, 1.2f); // 반지름 2.5, 속도 60도/초, 타원형
                    orbitingComponent.SetColliderParameters(6, 0.4f, 0.4f, 2.5f); // 6개 콜라이더
                    break;
                    
                case HeroWeaponElementState.Lightning:
                    // 번개 속성 별 설정
                    orbitingComponent.SetOrbitParameters(1.8f, 120f, 0.8f); // 반지름 1.8, 속도 120도/초, 타원형
                    orbitingComponent.SetColliderParameters(8, 0.2f, 0.2f, 1.8f); // 8개 콜라이더
                    break;
                    
                case HeroWeaponElementState.Light:
                    // 빛 속성 별 설정
                    orbitingComponent.SetOrbitParameters(3f, 45f, 1.0f); // 반지름 3, 속도 45도/초, 원형
                    orbitingComponent.SetColliderParameters(5, 0.5f, 0.5f, 3f); // 5개 콜라이더
                    break;
                    
                default:
                    // 기본 설정
                    orbitingComponent.SetOrbitParameters(2f, 90f, 1.0f);
                    orbitingComponent.SetColliderParameters(4, 0.3f, 0.3f, 2f);
                    break;
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
} 