using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Collections.Generic;
using BattleSystem;
using Utils;

namespace AttackComponents
{
    /// <summary>
    /// 캐릭터 소드 공격
    /// 캐릭터 소드 공격은 Physics.OverlapBox을 사용하여 구현합니다.
    /// 1. 캐릭터의 위치를 기준으로 Physics.OverlapBox을 사용하여 충돌을 감지합니다.
    /// 2. 콜라이더 없이 물리 오버랩을 통해 효율적인 충돌 감지를 수행합니다.
    /// 3. 0.6초 동안 지속되는 공격을 실행합니다.
    /// </summary>
    public class AC001_HeroSwordRadius : AttackComponent
    {
        // FSM 상태 관리
        private AttackState attackState = AttackState.None;

        private List<Enemy> target = new List<Enemy>();

        private float vfxSize = 0f;
        private float attackTimer = 0f;

        private float vfxDuration = 0.6f;
        private Vector2 attackDirection;

        // Physics.OverlapBox 설정
        [Header("Physics Overlap Settings")]
        public float attackRadius = 1f;
        public float attackSpeed = 1f;
        public LayerMask targetLayerMask = -1; // 기본적으로 모든 레이어

        // 충돌 감지 설정
        private Vector2 attackCenter;
        private Vector2 attackSize;

        // VFX 설정
        [Header("VFX Settings")]
        [SerializeField] private GameObject vfxPrefab; // 인스펙터에서 받을 VFX 프리팹

        // 공격 상태 열거형
        private enum AttackState { None, Preparing, Active, Finishing, Finished }

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            attackState = AttackState.Preparing;
            attackTimer = 0f;
            attackDirection = direction.normalized;
            attackSpeed = attack.attacker.statSheet.Get(StatType.AttackSpeed) / 10f * 1.5f;
            attackRadius = attack.attacker.statSheet.Get(StatType.AttackRange) / 10f;
            
            // 공격 시작
            StartAttack();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            target.Clear();
            StopAndDestroyVFX(spawnedVFX);
        }

        private void StartAttack()
        {
            attackState = AttackState.Preparing;
            attackTimer = 0f;
            
            // 1. 캐릭터의 위치를 기준으로 공격 영역 설정
            var pawnPrefab = attack.attacker.PawnPrefab;
            Vector2 vfxPosition = (Vector2)pawnPrefab.transform.position;

            spawnedVFX = CreateAndSetupVFX(vfxPrefab, vfxPosition, attackDirection);

            // 공격 중심점과 크기 계산
            // TODO : 공통적으로 적용되도록 수정
            float characterXLength = 1f;
            attackCenter = spawnedVFX.transform.position + (attackDirection.x >= 0 ? -new Vector3(characterXLength * 0.5f, 0, 0) : new Vector3(characterXLength * 0.5f, 0, 0));
            attackSize = new Vector2(attackRadius * 2f + 0.5f * characterXLength, attackRadius * 2f);
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
                    // 공격 활성화
                    PlayVFX(spawnedVFX);
                    DetectCollisions();
                    attackState = AttackState.Active;
                    attackTimer = 0f;
                    break;

                case AttackState.Active:
                    // VFX가 완료될 때까지 대기
                    if (attackTimer >= vfxDuration)
                    {
                        foreach (var targetPawn in target)
                        {
                            targetPawn.allIn1SpriteShaderHandler.SetShaderAllObjects(AllIn1SpriteShaderType.Off);
                        }
                        attackState = AttackState.Finishing;
                        attackTimer = 0f;
                    }
                    else
                    {
                        attackTimer += Time.deltaTime;
                    }

                    break;

                case AttackState.Finishing:
                    attackState = AttackState.Finished;
                    break;

                case AttackState.Finished:
                    attackState = AttackState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        /// <summary>
        /// Physics.OverlapBox을 사용하여 충돌을 감지합니다.
        /// </summary>
        private void DetectCollisions()
        {
            // Physics.OverlapBox을 사용하여 충돌 감지
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(attackCenter, attackSize, 0f, targetLayerMask);

            foreach (Collider2D hitCollider in hitColliders)
            {
                // 공격자 자신은 제외
                if (hitCollider.TryGetComponent(out Enemy targetPawn))
                {
                    target.Add(targetPawn);
                    targetPawn.allIn1SpriteShaderHandler.SetShaderAllObjects(AllIn1SpriteShaderType.DamageNormal);
                    DamageProcessor.ProcessHit(attack, targetPawn);
                }
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
            spawnedVFX.transform.localPosition = new Vector3(offsetX, attack.attacker.CenterOffset.y, 0);
            
            SetVFXSpeed(spawnedVFX, attackSpeed);

            spawnedVFX.SetActive(true);
            return spawnedVFX;
        }
    }
}