using AttackSystem;
using CharacterSystem;
using UnityEngine;

namespace AttackComponents
{
    /// <summary>
    /// 속성이 부여되면 공격할 때마다 전방으로 해당 속성의 검기를 발사하는 컴포넌트
    /// AC106을 생성하고 바로 종료됩니다.
    /// </summary>
    public class RAC006_ProjectileGenerator : AttackComponent
    {
        public AttackData projectileAttackData; // AC106용 AttackData

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // AC106 Projectile 생성
            CreateProjectile(direction);
            
            // 바로 종료
            AttackFactory.Instance.Deactivate(attack);
        }

        private void CreateProjectile(Vector2 direction)
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
                // 속성별 검기 설정
                var hero = attack.attacker as Character001_Hero;
                if (hero != null)
                {
                    SetupProjectileByElement(projectileComponent, hero.weaponElementState);
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
                    projectileComponent.SetPierceCount(1);
                    break;
                    
                case HeroWeaponElementState.Ice:
                    // 얼음 속성 검기 설정
                    projectileComponent.SetProjectileSpeed(6f);
                    projectileComponent.SetPierceCount(2);
                    break;
                    
                case HeroWeaponElementState.Lightning:
                    // 번개 속성 검기 설정
                    projectileComponent.SetProjectileSpeed(12f);
                    projectileComponent.SetPierceCount(0); // 무한 관통
                    break;
                    
                case HeroWeaponElementState.Light:
                    // 빛 속성 검기 설정
                    projectileComponent.SetProjectileSpeed(10f);
                    projectileComponent.SetPierceCount(3);
                    break;
                    
                default:
                    // 기본 설정
                    projectileComponent.SetProjectileSpeed(7f);
                    projectileComponent.SetPierceCount(1);
                    break;
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
} 