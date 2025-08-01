using AttackSystem;
using CharacterSystem;
using UnityEngine;

namespace AttackComponents
{
    /// <summary>
    /// 속성 부여 스킬을 사용할 때마다 해당 라운드가 끝날 때까지 자신의 주위를 공전하는 속성 별을 생성하는 컴포넌트
    /// 트리거만 설정하고, SK001 스킬에서 1회 제한을 관리합니다.
    /// </summary>
    public class RAC009_OrbitingStarGenerator : AttackComponent
    {
        public AttackData orbitingStarAttackData; // AC107용 AttackData
        private Character001_Hero hero;
        private AC107_OrbitingElement orbitingComponent; // 생성된 AC107 컴포넌트 참조

        public GameObject FireVFX;
        public GameObject IceVFX;
        public GameObject LightningVFX;
        public GameObject LightVFX;

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            hero = attack.attacker as Character001_Hero;
        }

        public override void OnLockActivate()
        {
            base.OnLockActivate();
            
            // Lock 상태에서 트리거만 설정
            if (hero != null)
            {
                hero.RAC009Trigger = true;
                Debug.Log("[RAC009] Lock 상태에서 공전 별 생성 트리거 활성화!");
            }
        }

        protected override void Update()
        {
            base.Update();
            
            // Lock 상태일 때는 Update 실행하지 않음
            if (isLocked) return;

            CreateOrbitingStar();
        }

        /// <summary>
        /// SK001에서 호출할 AC107 생성 메서드 (한 번만 생성)
        /// </summary>
        public void CreateOrbitingStar()
        {
            if (orbitingStarAttackData == null || orbitingComponent != null || hero == null)
            {
                return;
            }

            // AC107 Orbiting Star 생성
            var orbitingStarAttack = AttackFactory.Instance.Create(orbitingStarAttackData, attack.attacker, null, Vector2.zero);
            
            // AC107 컴포넌트 설정
            orbitingComponent = orbitingStarAttack.components[0] as AC107_OrbitingElement;
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
                    orbitingComponent.SetColliderParameters(1, 0.3f, 0.3f, 2f); // 4개 콜라이더
                    orbitingComponent.orbitVFXPrefab = FireVFX;
                    break;
                    
                case HeroWeaponElementState.Ice:
                    // 얼음 속성 별 설정
                    orbitingComponent.SetOrbitParameters(2.5f, 60f, 1.2f); // 반지름 2.5, 속도 60도/초, 타원형
                    orbitingComponent.SetColliderParameters(1, 0.4f, 0.4f, 2.5f); // 6개 콜라이더
                    orbitingComponent.orbitVFXPrefab = IceVFX;
                    break;
                    
                case HeroWeaponElementState.Lightning:
                    // 번개 속성 별 설정
                    orbitingComponent.SetOrbitParameters(1.8f, 120f, 0.8f); // 반지름 1.8, 속도 120도/초, 타원형
                    orbitingComponent.SetColliderParameters(1, 0.2f, 0.2f, 1.8f); // 8개 콜라이더
                    orbitingComponent.orbitVFXPrefab = LightningVFX;
                    break;
                    
                case HeroWeaponElementState.Light:
                    // 빛 속성 별 설정
                    orbitingComponent.SetOrbitParameters(3f, 45f, 1.0f); // 반지름 3, 속도 45도/초, 원형
                    orbitingComponent.SetColliderParameters(1, 0.5f, 0.5f, 3f); // 5개 콜라이더
                    orbitingComponent.orbitVFXPrefab = LightVFX;
                    break;
                    
                default:
                    // 기본 설정
                    orbitingComponent.SetOrbitParameters(2f, 90f, 1.0f);
                    orbitingComponent.SetColliderParameters(1, 0.3f, 0.3f, 2f);
                    orbitingComponent.orbitVFXPrefab = null;
                    break;
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            if (hero != null)
            {
                // 트리거 초기화
                hero.RAC009Trigger = false;
                Debug.Log("[RAC009] 공전 별 생성 트리거 비활성화!");
            }
        }
    }
} 