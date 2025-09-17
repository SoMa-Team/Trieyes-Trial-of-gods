using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Threading;
using BattleSystem;
using System.Collections.Generic;
using System;
using VFXSystem;

namespace AttackComponents
{
    /// <summary>
    /// 캐릭터 소드 능력 부여 강화
    /// 캐릭터 소드 공격은 AC001의 Physics.OverlapBox 로직을 사용하여 구현합니다.
    /// 7초 동안 검에 무작위 속성을 부여하고, 기본 공격(AC001)에 다음의 추가효과가 적용되고, 추가 피해를 입힙니다.
    /// - 얼음 : 공격에 맞은 적들을 둔화 시킵니다.
    /// </summary>
    public class AC003_HeroSwordEnchantmentIce : AttackComponent
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

        public float debuffDuration = 3f; // 둔화 지속 시간

        public float debuffMultiplier = 10f;

        // VFX 설정
        [Header("VFX Settings")]
        [SerializeField] private GameObject vfxPrefab; // 인스펙터에서 받을 VFX 프리팹

        [SerializeField] private GameObject DebuffVFXPrefab; // DOT VFX 프리팹 (AC 전달용)

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
            attackSpeed =  attack.attacker.GetStatValue(StatType.AttackSpeed);
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
                    if (hero != null && hero.RAC012Trigger && targetPawn.bIsStatusValid(PawnStatusType.Freeze))
                    {
                        // 둔화 중첩 효과 처리
                        ProcessSlowStackEffect(targetPawn);
                        continue;
                    }

                    // 기본 둔화 효과 적용
                    if (targetPawn.bIsStatusValid(PawnStatusType.Freeze))
                    {
                        continue;
                    }
                    
                    ApplyBasicSlowEffect(targetPawn);
                }
            }
        }

        /// <summary>
        /// 둔화 중첩 효과를 처리합니다.
        /// </summary>
        /// <param name="targetPawn">대상</param>
        private void ProcessSlowStackEffect(Pawn targetPawn)
        {
            //둔화가 걸린 적이 다시 둔화에 걸리는 경우 해당 적의 방어력이 대폭 감소합니다.
            var _target = targetPawn as Enemy;
            if (_target != null)
            {
                var debuffInfo = new DebuffInfo
                {
                    debuffType = DEBUFFType.DecreaseDefense,
                    attack = attack,
                    target = targetPawn,
                    debuffMultiplier = debuffMultiplier,
                    debuffDuration = debuffDuration,
                };

                var debuff = new DEBUFF();
                debuff.Activate(debuffInfo);
            }

            targetPawn.AddStatus(PawnStatusType.Freeze, new PawnStatus 
            { duration = debuffDuration, lastTime = Time.time });
        }

        /// <summary>
        /// 기본 둔화 효과를 적용합니다.
        /// </summary>
        /// <param name="targetPawn">대상</param>
        private void ApplyBasicSlowEffect(Pawn targetPawn)
        {
            // 새로운 DEBUFF 클래스 사용
            var debuffInfo = new DebuffInfo
            {
                debuffType = DEBUFFType.Slow,
                attack = attack,
                target = targetPawn,
                debuffMultiplier = debuffMultiplier,
                debuffDuration = debuffDuration,
                debuffVFXPrefab = DebuffVFXPrefab,
            };

            var debuff = new DEBUFF();
            debuff.Activate(debuffInfo);

            targetPawn.AddStatus(PawnStatusType.Freeze, new PawnStatus
            {
                duration = debuffDuration,
                lastTime = Time.time,
            });
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