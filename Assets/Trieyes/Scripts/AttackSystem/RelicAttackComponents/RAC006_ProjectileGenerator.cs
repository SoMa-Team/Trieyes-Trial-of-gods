using AttackSystem;
using CharacterSystem;
using UnityEngine;

namespace AttackComponents
{
    /// <summary>
    /// FSM 패턴을 사용하여 속성이 부여되면 공격할 때마다 전방으로 해당 속성의 검기를 발사하는 컴포넌트
    /// </summary>
    public class RAC006_ProjectileGenerator : AttackComponent
    {
        public AttackData projectileAttackData; // AC106용 AttackData
        private Character001_Hero hero;

        // FSM 관련 필드
        private ProjectileState currentState;
        private Vector2 attackDirection;
        private float spawnInterval;
        private float lastSpawnTime;

        // VFX 프리팹들
        public GameObject FireVFX;
        public GameObject IceVFX;
        public GameObject LightningVFX;
        public GameObject LightVFX;

        // FSM 상태 열거형
        private enum ProjectileState
        {
            Idle,           // 대기 상태
            Firing          // 발사 상태
        }

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            hero = attack.attacker as Character001_Hero;
            currentState = ProjectileState.Idle;
        }

        public override void OnLockActivate()
        {
            base.OnLockActivate();
            
            // Lock 상태에서 트리거만 설정
            if (hero != null)
            {
                hero.RAC006Trigger = true;
                Debug.Log("[RAC006] Lock 상태에서 속성 검기 발사 트리거 활성화!");

                // 공격속도에 따른 스폰 간격 계산
                spawnInterval = 1f / (hero.statSheet[Stats.StatType.AttackSpeed] / 10f);
                lastSpawnTime = Time.time;
            }
        }

        protected override void Update()
        {
            base.Update();
            
            // Lock 상태일 때는 Update 실행하지 않음
            if (isLocked) return;

            // FSM 업데이트
            UpdateFSM();
        }

        private void UpdateFSM()
        {
            switch (currentState)
            {
                case ProjectileState.Idle:
                    // Idle 상태에서는 트리거가 활성화되면 Firing으로 전환
                    if (hero != null && hero.RAC006Trigger)
                    {
                        currentState = ProjectileState.Firing;
                        Debug.Log("[RAC006] Idle -> Firing 상태 전환");
                    }
                    break;
                case ProjectileState.Firing:
                    // spawnInterval에 따른 발사 로직 실행
                    if (Time.time - lastSpawnTime >= spawnInterval)
                    {
                        // Attack의 transform.rotation에서 방향 계산
                        attackDirection = attack.attacker.LastMoveDirection;
                        CreateProjectile(attackDirection);
                        lastSpawnTime = Time.time;
                        Debug.Log("[RAC006] 투사체 발사 완료!");
                    }
                    
                    // 트리거가 비활성화되면 Idle로 돌아감
                    if (hero != null && !hero.RAC006Trigger)
                    {
                        currentState = ProjectileState.Idle;
                        Debug.Log("[RAC006] Firing -> Idle 상태 전환");
                    }
                    break;
                default:
                    break;
            }
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
                // 속성별 검기 설정
                var hero = attack.attacker as Character001_Hero;
                if (hero != null)
                {
                    projectileComponent.destroyType = ProjectileDestroyType.OnTime;
                    projectileComponent.maxLifetime = 1f;
                    projectileComponent.colliderType = ProjectileColliderType.Box;
                    projectileComponent.colliderWidth = 0.5f;
                    projectileComponent.colliderHeight = 0.5f;
                    
                    SetupProjectileByElement(projectileComponent, hero.weaponElementState);
                    // 외부에서 발사체를 Active 상태로 전환
                    projectileComponent.ActivateProjectile();
                }
            }
            
            Debug.Log($"[RAC006] {attack.attacker.name}의 속성 검기 발사! (상태: {currentState})");
        }

        private void SetupProjectileByElement(AC106_Projectile projectileComponent, HeroWeaponElementState elementState)
        {
            switch (elementState)
            {
                case HeroWeaponElementState.Fire:
                    // 불 속성 검기 설정
                    projectileComponent.SetProjectileSpeed(8f);
                    projectileComponent.SetPierceCount(0);
                    projectileComponent.projectileVFXPrefab = FireVFX;
                    break;
                    
                case HeroWeaponElementState.Ice:
                    // 얼음 속성 검기 설정
                    projectileComponent.SetProjectileSpeed(6f);
                    projectileComponent.SetPierceCount(0);
                    projectileComponent.projectileVFXPrefab = IceVFX;
                    break;
                    
                case HeroWeaponElementState.Lightning:
                    // 번개 속성 검기 설정
                    projectileComponent.SetProjectileSpeed(12f);
                    projectileComponent.SetPierceCount(0); // 무한 관통
                    projectileComponent.projectileVFXPrefab = LightningVFX;
                    break;
                    
                case HeroWeaponElementState.Light:
                    // 빛 속성 검기 설정
                    projectileComponent.SetProjectileSpeed(10f);
                    projectileComponent.SetPierceCount(0);
                    projectileComponent.projectileVFXPrefab = LightVFX;
                    break;
                    
                default:
                    // 기본 설정
                    projectileComponent.SetProjectileSpeed(7f);
                    projectileComponent.SetPierceCount(0);
                    projectileComponent.projectileVFXPrefab = FireVFX;
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
            
            // 상태 초기화
            currentState = ProjectileState.Idle;
        }
    }
} 