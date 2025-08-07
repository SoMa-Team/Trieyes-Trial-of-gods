using AttackSystem;
using UnityEngine;
using CharacterSystem;
using System.Collections.Generic;
using BattleSystem;
using Stats;

namespace AttackComponents
{
    public enum DOTTargetType
    {
        SingleTarget,
    }

    public enum DOTMode
    {
        MultiHit   // N회 발동
    }

    public enum AdditionalBuffType
    {
        None,
        SpeedBoost,
        MoveSpeedBoost,
        AttackSpeedBoost,
        AttackPowerBoost,
        DefenseBoost,
        CriticalChanceBoost,
        CriticalDamageBoost
    }

    /// <summary>
    /// 대상 기반 DOT(Damage Over Time) 공격 컴포넌트
    /// 대상 또는 대상 리스트에 대한 지속적인 데미지를 구현합니다.
    /// DOT VFX가 대상을 parent로 하여 자동으로 따라갑니다.
    /// </summary>
    public class AC101_DOT : AttackComponent
    {   
        // DOT 타겟팅 설정
        [Header("DOT 타겟팅 설정")]
        public DOTTargetType dotTargetType = DOTTargetType.SingleTarget;

        // DOT 발동 설정
        [Header("DOT 발동 설정")]
        public int dotDamage = 20;
        public float dotDuration = 5f;     // 지속시간
        public float dotInterval = 1f;     // 간격

        public PawnStatusType dotStatusType;

        // DOT 상태 관리
        private float dotTimer = 0f;
        private float dotDurationTimer = 0f;
        public Enemy dotTarget;

        // 추가 버프 설정
        [Header("추가 버프 설정")]
        public AdditionalBuffType additionalBuffType = AdditionalBuffType.None;
        public float additionalBuffDuration = 5f;
        public float additionalBuffMultiplier = 1.5f;
        public int additionalBuffValue = 10;

        // DOT VFX 설정
        [Header("DOT VFX 설정")]
        [SerializeField] public GameObject dotVFXPrefab; // DOT VFX 프리팹 (외부에서 설정 가능)
        private GameObject spawnedVFX; // 단일 VFX 관리
        public float dotVFXDuration = 0.3f;

        // FSM 상태 관리
        private DOTAttackState attackState = DOTAttackState.None;

        // DOT 공격 상태 열거형
        private enum DOTAttackState
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
            attackState = DOTAttackState.Preparing;
            dotTimer = 0f;
            dotDurationTimer = 0f;
            dotTarget = null;
            
            // DOT 공격 시작
            StartAC101Attack();
        }

        private void StartAC101Attack()
        {
            attackState = DOTAttackState.Preparing;
            dotTimer = 0f;
            dotDurationTimer = 0f;
            
            Debug.Log("<color=orange>[AC101] 대상 기반 DOT 공격 시작!</color>");
        }

        protected override void Update()
        {
            base.Update();
            
            // DOT 공격 상태 처리
            ProcessAC101AttackState();
        }


        private void ProcessAC101AttackState()
        {
            switch (attackState)
            {
                case DOTAttackState.None:
                    break;

                case DOTAttackState.Preparing:
                    dotTimer += Time.deltaTime;
                    
                    if (dotTimer >= 0.1f) // 준비 시간
                    {
                        attackState = DOTAttackState.Active;
                        dotTimer = 0f;
                        ActivateAC101Attack();
                    }
                    break;

                case DOTAttackState.Active:
                    dotTimer += Time.deltaTime;
                    dotDurationTimer += Time.deltaTime;
                    
                    // DOT 공격 처리
                    ProcessAC101Attack();
                    
                    // 종료 조건 체크
                    if (ShouldFinishAC101Attack())
                    {
                        attackState = DOTAttackState.Finishing;
                        dotTimer = 0f;
                        FinishAC101Attack();
                    }
                    break;

                case DOTAttackState.Finishing:
                    dotTimer += Time.deltaTime;
                    
                    if (dotTimer >= 0.1f) // 종료 시간
                    {
                        attackState = DOTAttackState.Finished;
                    }
                    break;

                case DOTAttackState.Finished:
                    attackState = DOTAttackState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ProcessAC101TargetStatus(Pawn target)
        {
            target.AddStatus(dotStatusType, 
            new PawnStatus { duration = dotDuration, lastTime = Time.time });
        }

        private void ProcessAC101Attack()
        {
            // 대상이 유효한지 체크
            if (!IsTargetValid())
            {
                // 대상이 죽었거나 유효하지 않으면 Finishing 상태로 변경
                attackState = DOTAttackState.Finishing;
                dotTimer = 0f;
                return;
            }

            // N회 발동
            if (dotTimer >= dotInterval)
            {
                ExecuteSingleTargetAttack();
                dotTimer = 0f;
                
                // interval과 duration이 같을 때 1번 발동 후 바로 종료
                if (dotInterval >= dotDuration)
                {
                    attackState = DOTAttackState.Finishing;
                    dotTimer = 0f;
                }
            }
        }

        /// <summary>
        /// 대상이 유효한지 체크합니다.
        /// </summary>
        /// <returns>대상이 유효하면 true, 아니면 false</returns>
        private bool IsTargetValid()
        {
            if (dotTarget == null || !dotTarget.gameObject.activeInHierarchy)
            {
                return false;
            }
            return true;
        }

        private void ExecuteSingleTargetAttack()
        {
            if (!IsTargetValid()) return;

            // 대상 상태 적용
            ProcessAC101TargetStatus(dotTarget);

            // 단일 대상에게 데미지 적용
            attack.statSheet[StatType.AttackPower] = new IntegerStatValue(dotDamage);
            DamageProcessor.ProcessHit(attack, dotTarget);

            // 버프 적용
            ApplyAdditionalBuffEffect(dotTarget);

            // DOT VFX 생성 (대상을 parent로 설정)
            if (!dotTarget.IsVFXCached(dotVFXPrefab.name))
            {
                CreateDOTVFXForTarget(dotTarget);
            }
            else
            {
                spawnedVFX = dotTarget.GetVFX(dotVFXPrefab.name);
                spawnedVFX.SetActive(true);
            }

            Debug.Log($"<color=yellow>[AC101] 단일 대상 {dotTarget.pawnName}에게 {dotDamage} 데미지 적용</color>");
        }

        /// <summary>
        /// 대상에 DOT VFX를 생성하고 대상을 parent로 설정합니다.
        /// </summary>
        /// <param name="target">DOT VFX를 적용할 대상</param>
        private void CreateDOTVFXForTarget(Enemy target)
        {
            if (target == null || dotVFXPrefab == null) return;

            // 대상이 유효한지 추가 체크
            if (!target.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[AC101] 대상 {target.pawnName}이 비활성화되어 VFX 생성 취소");
                return;
            }

            // 기존 VFX가 있으면 제거
            if (spawnedVFX != null)
            {
                StopAndDestroyVFX(spawnedVFX);
                spawnedVFX = null;
            }

            // VFX 생성
            spawnedVFX = CreateAndSetupVFX(dotVFXPrefab, Vector2.zero, Vector2.zero);
            target.AddVFX(dotVFXPrefab.name, spawnedVFX);
            
            if (spawnedVFX != null)
            {
                // VFX를 대상의 자식으로 설정하여 자동으로 따라가도록 함
                spawnedVFX.transform.SetParent(target.transform, false);
                spawnedVFX.transform.localPosition = Vector3.zero; // 대상 중심에 위치
                spawnedVFX.transform.localRotation = Quaternion.identity;
                
                // VFX 재생
                PlayVFX(spawnedVFX);
                
                Debug.Log($"<color=green>[AC101] {target.pawnName}에게 DOT VFX 생성 및 부착</color>");
            }
        }

        private bool ShouldFinishAC101Attack()
        {
            // 지속시간 체크
            return dotDurationTimer >= dotDuration;
        }

        private void ActivateAC101Attack()
        {
            Debug.Log("<color=green>[AC101] 대상 기반 DOT 공격 활성화!</color>");
        }

        private void FinishAC101Attack()
        {
            // VFX 해제 작업 (Object Pooling 문제 해결 포함)
            CleanupVFX();
            
            Debug.Log("<color=orange>[AC101] 대상 기반 DOT 공격 종료!</color>");
        }

        /// <summary>
        /// VFX를 정리합니다.
        /// Object Pooling에서 Enemy가 비활성화되어도 VFX가 남아있는 문제를 해결합니다.
        /// </summary>
        private void CleanupVFX()
        {
            if (spawnedVFX == null) return;

            // VFX의 부모(Enemy)가 비활성화되었는지 체크
            Transform parent = spawnedVFX.transform.parent;
            if (parent == null || !parent.gameObject.activeInHierarchy)
            {
                Debug.Log($"<color=blue>[AC101] 대상이 비활성화되어 VFX 정리: {spawnedVFX.name}</color>");
            }
            else
            {
                Debug.Log("<color=red>[AC101] DOT VFX 해제 완료</color>");
            }

            // VFX 정리
            StopAndDestroyVFX(spawnedVFX);
            spawnedVFX = null;
        }

        /// <summary>
        /// VFX를 생성하고 설정합니다.
        /// </summary>
        /// <param name="vfxPrefab">VFX 프리팹</param>
        /// <param name="position">VFX 생성 위치</param>
        /// <param name="direction">VFX 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        protected override GameObject CreateAndSetupVFX(GameObject vfxPrefab, Vector2 position, Vector2 direction)
        {
            // 프리팹이 없으면 VFX 없이 진행
            if (vfxPrefab == null)
            {
                return null;
            }

            // 기본 VFX 생성 (base 호출)
            GameObject vfx = base.CreateAndSetupVFX(vfxPrefab, position, direction);
            
            if (vfx != null)
            {
                vfx.transform.position = position;
                
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                vfx.transform.rotation = Quaternion.Euler(0, 0, angle);
                vfx.transform.localScale = new Vector3(1.0f, 1.0f, 1f);
                
                vfx.SetActive(true);
            }
            
            return vfx;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            // VFX 정리
            CleanupVFX();
        }

        private void ApplyAdditionalBuffEffect(Pawn target)
        {
            switch (additionalBuffType)
            {
                case AdditionalBuffType.SpeedBoost:
                    ApplySpeedBoost(target);
                    break;
                case AdditionalBuffType.MoveSpeedBoost:
                    ApplyMoveSpeedBoost(target);
                    break;
                case AdditionalBuffType.AttackSpeedBoost:
                    ApplyAttackSpeedBoost(target);
                    break;
                case AdditionalBuffType.AttackPowerBoost:
                    ApplyAttackPowerBoost(target);
                    break;
                case AdditionalBuffType.DefenseBoost:
                    ApplyDefenseBoost(target);
                    break;
                case AdditionalBuffType.CriticalChanceBoost:
                    ApplyCriticalChanceBoost(target);
                    break;
                case AdditionalBuffType.CriticalDamageBoost:
                    ApplyCriticalDamageBoost(target);
                    break;
                default:
                    break;
            }
        }

        private void ApplySpeedBoost(Pawn target)
        {
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.Haste,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC101] {target.pawnName}에게 속도 증가 버프 적용</color>");
        }

        private void ApplyMoveSpeedBoost(Pawn target)
        {
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseMoveSpeed,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC101] {target.pawnName}에게 이동속도 증가 버프 적용</color>");
        }

        private void ApplyAttackSpeedBoost(Pawn target)
        {
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseAttackSpeed,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC101] {target.pawnName}에게 공격속도 증가 버프 적용</color>");
        }

        private void ApplyAttackPowerBoost(Pawn target)
        {
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseAttackPower,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC101] {target.pawnName}에게 공격력 증가 버프 적용</color>");
        }

        private void ApplyDefenseBoost(Pawn target)
        {
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseDefense,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC101] {target.pawnName}에게 방어력 증가 버프 적용</color>");
        }

        private void ApplyCriticalChanceBoost(Pawn target)
        {
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseCriticalChance,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC101] {target.pawnName}에게 치명타 확률 증가 버프 적용</color>");
        }

        private void ApplyCriticalDamageBoost(Pawn target)
        {
            var buffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseCriticalDamage,
                attack = attack,
                target = target,
                buffValue = additionalBuffValue,
                buffMultiplier = additionalBuffMultiplier,
                buffDuration = additionalBuffDuration,
                buffInterval = 1f,
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC101] {target.pawnName}에게 치명타 데미지 증가 버프 적용</color>");
        }
    }
} 