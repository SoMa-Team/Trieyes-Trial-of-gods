using AttackSystem;
using CharacterSystem;
using UnityEngine;

namespace AttackComponents
{
    /// <summary>
    /// FSM 패턴을 사용하여 속성이 부여되면 공격할 때마다 전방으로 해당 속성의 검기를 발사하는 컴포넌트
    /// </summary>
    public class AC2001_EnemyMagicBall : AttackComponent
    {
        public AttackData projectileAttackData; // AC106용 AttackData
        private Enemy enemy;

        // FSM 관련 필드
        private ProjectileState currentState;
        private Vector2 attackDirection;

        // VFX 프리팹들
        public GameObject MagicBallVFX;

        // FSM 상태 열거형
        private enum ProjectileState
        {
            Idle,           // 대기 상태
            Firing          // 발사 상태
        }

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            attackDirection = direction;
            enemy = attack.attacker as Enemy;
            currentState = ProjectileState.Idle;
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
                    // 1회 발사하고 종료
                    CreateProjectile(attackDirection);
                    currentState = ProjectileState.Firing;
                    break;
                case ProjectileState.Firing:
                    AttackFactory.Instance.Deactivate(attack);
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
                projectileComponent.destroyType = ProjectileDestroyType.OnTime;
                projectileComponent.maxLifetime = 1f;
                projectileComponent.vfxLifetimeMishmatch = true;
                projectileComponent.vfxLifetime = 0.65f;
                projectileComponent.colliderType = ProjectileColliderType.Box;
                projectileComponent.colliderWidth = 2f;
                projectileComponent.colliderHeight = 2f;

                projectileComponent.projectileDirection = direction;

                projectileComponent.SetProjectileSpeed(5f);
                projectileComponent.SetPierceCount(0);
                projectileComponent.projectileVFXPrefab = MagicBallVFX;

                // 외부에서 발사체를 Active 상태로 전환
                projectileComponent.ActivateProjectile();
            }
            
            Debug.Log($"[AC2001] {attack.attacker.name}의 마법 구슬 발사! (상태: {currentState})");
        }

        public override void Deactivate()
        {
            base.Deactivate();

            // 상태 초기화
            currentState = ProjectileState.Idle;
        }
    }
} 