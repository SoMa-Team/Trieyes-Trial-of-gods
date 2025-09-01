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
    /// - 번개 : 공격에 맞은 대상 주변 적들이 연쇄적인 번개(쓰리쿠션 데미지-관통 개수에 비례) 피해를 입습니다
    /// </summary>
    public class AC004_HeroSwordEnchantmentLightning : AttackComponent
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

        // 번개 연쇄 설정
        public int chainDamage; // Attack.StatSheet.stats 에서 가져와야 함
        public float chainRadius;
        public int chainCount;
        public float chainDelay;

        public AttackData chainAttackData;

        // VFX 설정
        [Header("VFX Settings")]
        [SerializeField] private GameObject vfxPrefab; // 인스펙터에서 받을 VFX 프리팹
        [SerializeField] private GameObject chainVFXPrefab; // 번개 연쇄 VFX 프리팹 (AC102_CHAIN에 전달용)

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

                    // AC102_CHAIN Attack 생성
                    Attack lightningChainAttack = AttackFactory.Instance.Create(chainAttackData, attack.attacker, null, Vector2.zero);
                    
                    // AC102_CHAIN 컴포넌트 설정
                    var lightningChainComponent = lightningChainAttack.components[0] as AC102_CHAIN;
                    if (lightningChainComponent != null)
                    {
                        // 기본 설정
                        lightningChainComponent.chainDamage = chainDamage;
                        lightningChainComponent.chainRadius = chainRadius;
                        lightningChainComponent.chainCount = chainCount;
                        lightningChainComponent.chainDelay = chainDelay;
                        lightningChainComponent.chainRadius = chainRadius;
                        
                        // VFX 프리팹 전달
                        lightningChainComponent.chainVFXPrefab = chainVFXPrefab;

                        lightningChainComponent.statusType = PawnStatusType.ElectricShock;
                        lightningChainComponent.statusDuration = 3f;
                        
                        // 번개 연쇄 시작
                        lightningChainComponent.StartLightningChain(targetPawn.transform.position);
                    }
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

        void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackCenter, attackSize);
            #endif
        }
    }
}