using AttackSystem;
using UnityEngine;
using CharacterSystem;
using System.Collections.Generic;

namespace AttackComponents
{
    public enum DOTTargetType
    {
        SingleTarget,      // 단일 대상에게만 데미지
        MultipleTargets    // 다중 대상에게 데미지
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

        // DOT 상태 관리
        private float dotTimer = 0f;
        private float dotDurationTimer = 0f;
        public List<Enemy> dotTargets = new List<Enemy>(10);

        // 추가 버프 설정
        [Header("추가 버프 설정")]
        public AdditionalBuffType additionalBuffType = AdditionalBuffType.None;
        public float additionalBuffDuration = 5f;
        public float additionalBuffMultiplier = 1.5f;
        public int additionalBuffValue = 10;

        // DOT VFX 설정
        [Header("DOT VFX 설정")]
        public GameObject dotVFXPrefab;
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
            dotTargets.Clear();
            
            // DOT 공격 시작
            StartAC101Attack();
        }

        /// <summary>
        /// 다중 대상을 설정합니다.
        /// </summary>
        /// <param name="targets">DOT를 적용할 대상 리스트</param>
        public void SetMultipleTargets(List<Enemy> targets)
        {
            dotTargets.Clear();
            dotTargets.AddRange(targets);
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

        private void ProcessAC101Attack()
        {
            // N회 발동
            if (dotTimer >= dotInterval)
            {
                ExecuteAC101Attack();
                dotTimer = 0f;
            }
        }

        private void ExecuteAC101Attack()
        {
            switch (dotTargetType)
            {
                case DOTTargetType.SingleTarget:
                    ExecuteSingleTargetAttack();
                    break;
                case DOTTargetType.MultipleTargets:
                    ExecuteMultipleTargetsAttack();
                    break;
            }
        }

        private void ExecuteSingleTargetAttack()
        {
            if (dotTargets == null) return;

            // 단일 대상에게 데미지 적용
            var attackResult = AttackResult.Create(attack, dotTargets[0]);
            attackResult.totalDamage = dotDamage;
            dotTargets[0].ApplyDamage(attackResult);

            // 버프 적용
            ApplyAdditionalBuffEffect(dotTargets[0]);

            // DOT VFX 생성
            CreateAC101VFX(dotTargets[0].transform.position);

            Debug.Log($"<color=yellow>[AC101] 단일 대상 {dotTargets[0].pawnName}에게 {dotDamage} 데미지 적용</color>");
        }

        private void ExecuteMultipleTargetsAttack()
        {
            // 다중 대상에게 데미지 적용
            for (int i = 0; i < dotTargets.Count; i++)
            {
                Pawn enemy = dotTargets[i];
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    var attackResult = AttackResult.Create(attack, enemy);
                    attackResult.totalDamage = dotDamage;
                    enemy.ApplyDamage(attackResult);

                    // 버프 적용
                    ApplyAdditionalBuffEffect(enemy);

                    // DOT VFX 생성
                    CreateAC101VFX(enemy.transform.position);
                }
            }

            Debug.Log($"<color=yellow>[AC101] 다중 대상 {dotTargets.Count}명에게 {dotDamage} 데미지 적용</color>");
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
            Debug.Log("<color=orange>[AC101] 대상 기반 DOT 공격 종료!</color>");
        }

        private void CreateAC101VFX(Vector3 position)
        {
            if (dotVFXPrefab != null)
            {
                GameObject dotVFX = Instantiate(dotVFXPrefab);
                dotVFX.transform.position = position;
                
                // DOT VFX 지속시간 후 제거
                Destroy(dotVFX, dotVFXDuration);
                
                Debug.Log("<color=blue>[AC101] DOT VFX 생성</color>");
            }
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
                globalHeal = 0
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
                globalHeal = 0
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
                globalHeal = 0
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
                globalHeal = 0
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
                globalHeal = 0
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
                globalHeal = 0
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
                globalHeal = 0
            };

            var buff = new BUFF();
            buff.Activate(buffInfo);
            Debug.Log($"<color=green>[AC101] {target.pawnName}에게 치명타 데미지 증가 버프 적용</color>");
        }
    }
} 