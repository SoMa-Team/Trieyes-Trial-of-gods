using AttackSystem;
using CharacterSystem;
using UnityEngine;
using System.Collections.Generic;

namespace AttackComponents
{
    /// <summary>
    /// 속성 부여 스킬을 사용할 때마다 해당 라운드가 끝날 때까지 자신의 주위를 공전하는 속성 별을 생성하는 컴포넌트
    /// Hero가 직접 소유하는 컴포넌트
    /// </summary>
    public class RAC009_OrbitingStarGenerator : AttackComponent
    {
        public AttackData orbitingStarAttackData; // AC107용 AttackData
        private Character001_Hero hero;
        private List<AC107_OrbitingElement> orbitingComponents = new List<AC107_OrbitingElement>(); // 생성된 AC107 컴포넌트들 참조
        
        public GameObject FireVFX;
        public GameObject IceVFX;
        public GameObject LightningVFX;
        public GameObject LightVFX;

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            hero = attack.attacker as Character001_Hero;
            if (hero != null)
            {
                hero.RAC009Trigger = true;
                if(hero.rac009Component is null)
                {
                    hero.rac009Component = this;
                }
            }
        }

        /// <summary>
        /// SK001에서 호출할 AC107 생성 메서드
        /// </summary>
        public void CreateOrbitingStar()
        {
            if (orbitingStarAttackData == null || hero == null)
            {
                return;
            }

            // AC107 Orbiting Star 생성
            var orbitingStarAttack = AttackFactory.Instance.Create(orbitingStarAttackData, hero, null, Vector2.zero);
            
            // AC107 컴포넌트 설정
            var orbitingComponent = orbitingStarAttack.components[0] as AC107_OrbitingElement;
            if (orbitingComponent != null)
            {
                orbitingComponent.orbitDirection = OrbitDirection.Clockwise;
                orbitingComponent.collisionBehavior = CollisionBehavior.Continue;

                // Hero를 공전 대상으로 설정
                orbitingComponent.SetOrbitTarget(hero.transform);
                
                // AC107의 위치를 Hero 근처로 설정 (거리 제한 방지)
                orbitingStarAttack.transform.position = hero.transform.position;
                
                // 속성별 별 설정
                SetupOrbitingStarByElement(orbitingComponent, hero.weaponElementState);
             
                // 외부에서 공전 요소를 Active 상태로 전환
                orbitingComponent.ActivateOrbiting();
                
                // 생성된 컴포넌트를 리스트에 추가
                orbitingComponents.Add(orbitingComponent);
            }
            
            Debug.Log($"[RAC009] {hero.name}의 주위에 {hero.weaponElementState} 속성 별 생성! (현재 {orbitingComponents.Count}개)");
        }

        private void SetupOrbitingStarByElement(AC107_OrbitingElement orbitingComponent, HeroWeaponElementState elementState)
        {
            switch (elementState)
            {
                case HeroWeaponElementState.Fire:
                    // 불 속성 별 설정
                    orbitingComponent.SetOrbitParameters(2f, 90f, 1.0f); // 반지름 2, 속도 90도/초, 원형
                    orbitingComponent.SetColliderParameters(1, 0.3f, 0.3f, 2f); // 1개 콜라이더 (별 하나)
                    orbitingComponent.orbitVFXPrefab = FireVFX;
                    break;
                    
                case HeroWeaponElementState.Ice:
                    // 얼음 속성 별 설정
                    orbitingComponent.SetOrbitParameters(2f, 90f, 1.0f); // 반지름 2, 속도 90도/초, 원형
                    orbitingComponent.SetColliderParameters(1, 0.3f, 0.3f, 2f); // 1개 콜라이더 (별 하나)
                    orbitingComponent.orbitVFXPrefab = IceVFX;
                    break;
                    
                case HeroWeaponElementState.Lightning:
                    // 번개 속성 별 설정
                    orbitingComponent.SetOrbitParameters(2f, 90f, 1.0f); // 반지름 2, 속도 90도/초, 원형
                    orbitingComponent.SetColliderParameters(1, 0.3f, 0.3f, 2f); // 1개 콜라이더 (별 하나)
                    orbitingComponent.orbitVFXPrefab = LightningVFX;
                    break;
                    
                case HeroWeaponElementState.Light:
                    // 빛 속성 별 설정
                    orbitingComponent.SetOrbitParameters(2f, 90f, 1.0f); // 반지름 2, 속도 90도/초, 원형
                    orbitingComponent.SetColliderParameters(1, 0.3f, 0.3f, 2f); // 1개 콜라이더 (별 하나)
                    orbitingComponent.orbitVFXPrefab = LightVFX;
                    break;
                    
                default:
                    // 기본 설정
                    orbitingComponent.SetOrbitParameters(2f, 90f, 1.0f); // 반지름 2, 속도 90도/초, 원형
                    orbitingComponent.SetColliderParameters(1, 0.3f, 0.3f, 2f); // 1개 콜라이더 (별 하나)
                    orbitingComponent.orbitVFXPrefab = null;
                    break;
            }
        }


    }
} 