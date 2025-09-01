using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Threading;
using BattleSystem;
using System.Collections.Generic;
using VFXSystem;

namespace AttackComponents
{
    /// <summary>
    /// 캐릭터 소드 능력 부여 강화
    /// 캐릭터 소드 공격은 AC001의 Physics.OverlapBox 로직을 사용하여 구현합니다.
    /// 7초 동안 검에 무작위 속성을 부여하고, 기본 공격(AC001)에 다음의 추가효과가 적용되고, 추가 피해를 입힙니다.
    /// 천상 : 이동속도와 사거리가 증가합니다. 방어력이 감소합니다. AC1001_BUFF 버프를 줍니다.
    /// </summary>
    public class AC005_HeroSwordEnchantmentLight : AttackComponent
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

        // Skill 002에 대하여 AOE 공격 발동 시 AOE의 기본 정보들
        public AOETargetType dotCollisionType = AOETargetType.AreaAtPosition;
        public AOEShapeType dotShapeType = AOEShapeType.Circle;
        public float dotRadius = 3f;
        public float dotWidth = 1f;
        public float dotHeight = 1f;
        public int dotDamage = 100;
        public float dotDuration = 1f;
        public float dotInterval = 1f;

        public AttackData aoeAttackData;

        // 생성된 VFX 인스턴스
        [Header("VFX Settings")]
        [SerializeField] private GameObject vfxPrefab; // 인스펙터에서 받을 VFX 프리팹

        // AC100 AOE VFX 설정
        [Header("AC100 AOE VFX Settings")]
        [SerializeField] private GameObject aoeVFXPrefab; // AOE VFX 프리팹 (AC100에 전달용)

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
            
            // VFX 정리
            if (spawnedVFX != null)
            {
                StopAndDestroyVFX(spawnedVFX);
                spawnedVFX = null;
            }
            
            attackState = AttackState.None;
            attackTimer = 0f;
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
            attackCenter = spawnedVFX.transform.position;
            attackSize = new Vector2(attackRadius * 2f, attackRadius * 2f);
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
                    // 빛 속성일 때 AOE 공격
                    var hero = attack.attacker as Character001_Hero;
                    if (hero != null && hero.weaponElementState == HeroWeaponElementState.Light && hero.activateLight)
                    {
                        SpawnAC100Attack(targetPawn);
                    }
                }
            }
        }
        
        /// <summary>
        /// Light 속성일 때 AC100 AOE 공격을 소환합니다
        /// </summary>
        /// <param name="targetPawn">타겟 적</param>
        private void SpawnAC100Attack(Pawn targetPawn)
        {
            var aoeAttack = AttackFactory.Instance.Create(aoeAttackData, attack.attacker, null, Vector2.zero);
            if (aoeAttack != null)
            {
                var aoeComponent = aoeAttack.components[0] as AC100_AOE;
                aoeComponent.aoeTargetType = dotCollisionType;
                aoeComponent.aoeShapeType = dotShapeType;
                aoeComponent.aoeRadius = dotRadius;
                aoeComponent.aoeDamage = dotDamage;
                aoeComponent.aoeDuration = dotDuration;
                aoeComponent.aoeInterval = dotInterval;

                // AOE 위치 설정 (타겟 위치)
                aoeComponent.SetAOEPosition((Vector2)targetPawn.transform.position);
                
                // AOE VFX 프리팹 전달
                aoeComponent.aoeVFXPrefab = aoeVFXPrefab;
                
                //Debug.Log("<color=cyan>[AC006] Light 속성으로 AC100 AOE 공격 소환! AOE VFX 프리팹 전달</color>");
            }
        }

        protected override GameObject CreateAndSetupVFX(GameObject vfxPrefab, Vector2 position, Vector2 direction)
        {
            // 프리팹이 없으면 VFX 없이 진행
            if (vfxPrefab == null)
            {
                return null;
            }

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
            spawnedVFX.transform.localPosition = new Vector3(offsetX, attack.attacker.vfxYOffset, 0);
            
            SetVFXSpeed(spawnedVFX, attackSpeed);

            spawnedVFX.SetActive(true);
            return spawnedVFX;
        }

        void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackCenter, attackSize);
            #endif
        }
    }
}