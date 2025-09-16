using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using BattleSystem;

namespace AttackComponents
{
    /// <summary>
    /// 캐릭터 소드 능력 부여 강화
    /// 캐릭터 소드 공격은 AC001의 Physics.OverlapBox 로직을 사용하여 구현합니다.
    /// 7초 동안 검에 무작위 속성을 부여하고, 기본 공격(AC001)에 다음의 추가효과가 적용되고, 추가 피해를 입힙니다.
    /// - 불꽃 : 공격에 맞은 대상에게 지속적으로 화상데미지(도트)를 입힙니다.
    /// </summary>
    public class AC002_HeroSwordEnchantmentFire : AttackComponent
    {
        // FSM 상태 관리
        private AttackState attackState = AttackState.None;

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

        // 불꽃 도트 데미지 필드
        public DOTTargetType dotTargetType = DOTTargetType.SingleTarget;
        public int dotDamage = 20;
        public float dotDuration = 2f;
        public float dotInterval = 0.2f;

        public AttackData dotAttackData;

        // VFX 설정
        [Header("VFX Settings")]
        [SerializeField] private GameObject vfxPrefab; // 인스펙터에서 받을 VFX 프리팹
        [SerializeField] private GameObject dotVFXPrefab; // DOT VFX 프리팹 (AC101_DOT에 전달용)

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
            attackSpeed = attack.attacker.GetStatValue(StatType.AttackSpeed);
            attackRadius = attack.attacker.GetStatValue(StatType.AttackRange) / 10f;

            // 공격 시작
            StartAttack();
        }

        public override bool OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param);
            if (eventType == Utils.EventType.OnKilled || eventType == Utils.EventType.OnKilledByCritical)
            {
                var _attacker = attack.attacker as Character001_Hero;
                if (_attacker != null)
                {
                    _attacker.killedDuringSkill001++;
                    return true;
                }

                return false;
            }

            return false;
        }

        public override void Deactivate()
        {
            base.Deactivate();
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
                    DamageProcessor.ProcessHit(attack, targetPawn);
                    
                    var hero = attack.attacker as Character001_Hero;
                    if (hero != null && hero.RAC011Trigger && targetPawn.bIsStatusValid(PawnStatusType.Burn))
                    {
                        // 화상 중첩 효과 처리
                        ProcessBurnStackEffect(targetPawn);
                        continue;
                    }

                    // 단일 대상에게 도트 데미지를 주는 DOT 소환
                    var dotAttack = AttackFactory.Instance.Create(dotAttackData, attack.attacker, null, Vector2.zero);

                    var dotComponent = dotAttack.components[0] as AC101_DOT;
                    if (dotComponent != null)
                    {
                        // 기본 설정
                        dotComponent.dotTargetType = dotTargetType;
                        dotComponent.dotDamage = dotDamage;
                        dotComponent.dotDuration = dotDuration;
                        dotComponent.dotInterval = dotInterval;
                        dotComponent.dotTarget = targetPawn;
                        dotComponent.dotStatusType = PawnStatusType.Burn;
                        
                        // VFX 프리팹 전달
                        dotComponent.dotVFXPrefab = dotVFXPrefab;
                    }
                }
            }
        }

        /// <summary>
        /// 화상 중첩 효과를 처리합니다.
        /// </summary>
        /// <param name="targetPawn">대상</param>
        private void ProcessBurnStackEffect(Pawn targetPawn)
        {
            // 남은 화상 피해량 계산
            var _status = (PawnStatus)targetPawn.statuses[PawnStatusType.Burn];
            float dotStartTime = _status.lastTime;
            float currentTime = Time.time;
            float remainingTime = dotStartTime + dotDuration - currentTime;
            int remainingDamage = (int)(dotDamage * (remainingTime / dotInterval));

            attack.statSheet[StatType.AttackPower] = new IntegerStatValue(remainingDamage);
            DamageProcessor.ProcessHit(attack, targetPawn);

            targetPawn.RemoveStatus(PawnStatusType.Burn);
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