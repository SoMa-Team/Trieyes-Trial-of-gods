using AttackSystem;
using CharacterSystem;
using UnityEngine;

namespace AttackComponents
{
    /// <summary>
    /// 속성이 부여되면 공격할 때마다 전방으로 해당 속성의 검기를 발사하는 컴포넌트
    /// 트리거만 설정하고, SK001의 TriggerRandomEnchantment에서 AC106을 생성합니다.
    /// </summary>
    public class RAC006_ProjectileGenerator : AttackComponent
    {
        public AttackData projectileAttackData; // AC106용 AttackData
        private Character001_Hero hero;

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
                hero.RAC006Trigger = true;
                Debug.Log("[RAC006] Lock 상태에서 속성 검기 발사 트리거 활성화!");
            }
        }

        protected override void Update()
        {
            base.Update();
            
            // Lock 상태일 때는 Update 실행하지 않음
            if (isLocked) return;
        }

        /// <summary>
        /// SK001에서 호출할 AC106 생성 메서드
        /// </summary>
        /// <param name="direction">발사 방향</param>
        public void CreateProjectile(Vector2 direction)
        {
            if (projectileAttackData == null)
            {
                Debug.LogError("[RAC006] projectileAttackData가 설정되지 않았습니다!");
                return;
            }

            // AC106 Projectile 생성
            var projectileAttack = AttackFactory.Instance.Create(projectileAttackData, attack.attacker, null, direction);
            
            // AC106 컴포넌트 설정
            var projectileComponent = projectileAttack.components[0] as AC106_Projectile;
            if (projectileComponent != null)
            {
                projectileComponent.destroyType = ProjectileDestroyType.OnTime;
                projectileComponent.maxLifetime = 2f;

                // 속성별 검기 설정
                var hero = attack.attacker as Character001_Hero;
                if (hero != null)
                {
                    SetupProjectileByElement(projectileComponent, hero.weaponElementState);
                    switch (hero.weaponElementState)
                    {
                        case HeroWeaponElementState.Fire:
                            projectileComponent.projectileVFXPrefab = FireVFX;
                            break;
                        case HeroWeaponElementState.Ice:
                            projectileComponent.projectileVFXPrefab = IceVFX;
                            break;
                        case HeroWeaponElementState.Lightning:
                            projectileComponent.projectileVFXPrefab = LightningVFX;
                            break;
                        case HeroWeaponElementState.Light:
                            projectileComponent.projectileVFXPrefab = LightVFX;
                            break;
                    }
                }
            }
            
            Debug.Log($"[RAC006] {attack.attacker.name}의 속성 검기 발사!");
        }

        private void SetupProjectileByElement(AC106_Projectile projectileComponent, HeroWeaponElementState elementState)
        {
            switch (elementState)
            {
                case HeroWeaponElementState.Fire:
                    // 불 속성 검기 설정
                    projectileComponent.SetProjectileSpeed(8f);
                    projectileComponent.SetPierceCount(0);
                    break;
                    
                case HeroWeaponElementState.Ice:
                    // 얼음 속성 검기 설정
                    projectileComponent.SetProjectileSpeed(6f);
                    projectileComponent.SetPierceCount(0);
                    break;
                    
                case HeroWeaponElementState.Lightning:
                    // 번개 속성 검기 설정
                    projectileComponent.SetProjectileSpeed(12f);
                    projectileComponent.SetPierceCount(0); // 무한 관통
                    break;
                    
                case HeroWeaponElementState.Light:
                    // 빛 속성 검기 설정
                    projectileComponent.SetProjectileSpeed(10f);
                    projectileComponent.SetPierceCount(0);
                    break;
                    
                default:
                    // 기본 설정
                    projectileComponent.SetProjectileSpeed(7f);
                    projectileComponent.SetPierceCount(0);
                    break;
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            if (hero != null)
            {
                // 트리거 초기화
                hero.RAC006Trigger = false;
                Debug.Log("[RAC006] 속성 검기 발사 트리거 비활성화!");
            }
        }
    }
} 