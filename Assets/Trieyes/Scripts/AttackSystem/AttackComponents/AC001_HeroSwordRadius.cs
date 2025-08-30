using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;

namespace AttackComponents
{
    /// <summary>
    /// 캐릭터 소드 공격
    /// 캐릭터 소드 공격은 AC100 AOE 컴포넌트를 사용하여 구현합니다.
    /// 1. 캐릭터의 R_Weapon 게임 오브젝트를 가져옵니다. 여기가 공격 기준 좌표 입니다.
    /// 2. AC100 AOE 컴포넌트를 생성하여 Rect 형태의 공격을 수행합니다.
    /// 3. 0.2초 동안 지속되는 짧은 AOE 공격을 실행합니다.
    /// </summary>
    public class AC001_HeroSwordRadius : AttackComponent
    {
        // FSM 상태 관리
        private AttackState attackState = AttackState.None;

        private bool bIsColliderCreated = false;

        private float vfxSize = 0f;
        private float attackTimer = 0f;

        private float vfxDuration = 0.6f;
        private Vector2 attackDirection;

        // VFX 설정
        [Header("VFX Settings")]

        public float attackRadius = 1f;
        public float attackSpeed = 1f;

        [SerializeField] private GameObject vfxPrefab; // 인스펙터에서 받을 VFX 프리팹


        // 공격 상태 열거형
        private enum AttackState
        {
            None,
            Preparing,
            Active,
            Finishing,
            Finished
        }

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            attackState = AttackState.Preparing;
            attackTimer = 0f;
            attackDirection = direction.normalized;
            attackSpeed = attack.attacker.statSheet[StatType.AttackSpeed] / 10f * 1.5f;
            attackRadius = attack.attacker.statSheet[StatType.AttackRange] / 10f;

            // 공격 시작
            StartAttack();
        }

        private void StartAttack()
        {
            attackState = AttackState.Preparing;
            attackTimer = 0f;
            
            // 1. 캐릭터의 R_Weapon 게임 오브젝트를 가져옵니다. 여기가 공격 기준 좌표 입니다.
            var pawnPrefab = attack.attacker.PawnPrefab;

            Vector2 vfxPosition = (Vector2)pawnPrefab.transform.position;
            spawnedVFX = CreateAndSetupVFX(vfxPrefab, vfxPosition, attackDirection);

            if (!bIsColliderCreated)
            {
                attack.gameObject.AddComponent<BoxCollider2D>();
                bIsColliderCreated = true;
            }

            CreateBoxColliderComponent();
            attack.attackCollider = attack.gameObject.GetComponent<BoxCollider2D>();
        }

        private void CreateBoxColliderComponent()
        {
            var boxCollider = attack.gameObject.GetComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(attackRadius * 1.5f, attackRadius * 1.5f);
            boxCollider.offset = new Vector2(vfxSize / 2 * (attackDirection.x > 0 ? 1 : -1), 0);

            boxCollider.isTrigger = true;
            boxCollider.enabled = true;
        }

        protected override void Update()
        {
            base.Update();
            
            // Lock 상태일 때는 Update 실행하지 않음
            if (isLocked) return;
            
            // 공격 상태 처리
            ProcessAttackState();
        }

        private void ProcessAttackState()
        {
            switch (attackState)
            {
                case AttackState.None:
                    break;

                case AttackState.Preparing:
                    StartAttack(spawnedVFX, attack.attackCollider);
                    attackState = AttackState.Active;
                    attackTimer = 0f;
                    break;

                case AttackState.Active:
                    // VFX가 완료될 때까지 대기
                    if (attackTimer >= vfxDuration)
                    {
                        attackState = AttackState.Finishing;
                        attackTimer = 0f;
                    }
                    else
                    {
                        attack.attackCollider.enabled = false;
                        attackTimer += Time.deltaTime;
                    }

                    break;

                case AttackState.Finishing:
                    FinishAttack();
                    attackState = AttackState.Finished;
                    break;

                case AttackState.Finished:
                    attackState = AttackState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        protected override GameObject CreateAndSetupVFX(GameObject vfxPrefab, Vector2 position, Vector2 direction)
        {
            // 기본 VFX 생성 (base 호출)
            if (spawnedVFX is null)
            {
                spawnedVFX = base.CreateAndSetupVFX(vfxPrefab, position, direction);
            }

            // vfx의 가로 세로 길이 구하기
            var psr = spawnedVFX.transform.GetChild(0).GetComponent<ParticleSystemRenderer>();
            
            if(direction.x <= 0)
            {
                psr.flip = new Vector3(1, 0, 0);
            }
            else
            {
                psr.flip = new Vector3(0, 0, 0);
            }

            spawnedVFX.transform.localScale = new Vector3(attackRadius, attackRadius, 1f);

            vfxSize = psr.bounds.size.x;
            
            spawnedVFX.transform.SetParent(attack.attacker.transform);

            var offsetX = direction.x > 0 ? vfxSize / 2 : -vfxSize / 2;
            spawnedVFX.transform.localPosition = new Vector3(offsetX, 0, 0);
            
            SetVFXSpeed(spawnedVFX, attackSpeed);

            spawnedVFX.SetActive(true);
            return spawnedVFX;
        }

        private void FinishAttack()
        {
            // AOE 컴포넌트 정리
            if (attack.attackCollider != null)
            {
                attack.attackCollider.enabled = false;
                attack.attackCollider = null;
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
            StopAndDestroyVFX(spawnedVFX);
        }
    }
}