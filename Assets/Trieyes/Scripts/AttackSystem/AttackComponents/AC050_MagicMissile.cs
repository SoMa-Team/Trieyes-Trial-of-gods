using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Collections.Generic;
using BattleSystem;
using Utils;
using System;
using PrimeTween;

namespace AttackComponents
{
    /// <summary>
    /// 유도 매직 미사일 공격
    /// 1. 가장 가까운 적을 타겟으로 하는 매직 미사일을 발사합니다.
    /// 2. 매직 미사일은 0.5초 후 적에게 데미지를 입힙니다.
    /// 3. 25% 확률로 도탄되어 추가 적을 공격할 수 있습니다.
    /// </summary>
    public class AC050_MagicMissile : AttackComponent
    {
        // FSM 상태 관리
        public AttackState attackState;

        private Character002_Magician magician;

        private float attackTimer = 0f;
        private const float attackDelay = 0.1f;

        // 매직 미사일 설정
        [Header("Magic Missile Settings")]
        public float missileTravelTime = 0.5f; // 미사일이 적에게 도달하는 시간 (고정)
        public float bounceChance = 0.5f; // 50% 도탄 확률
        public int maxBounces = 1; // 최대 도탄 횟수
        public LayerMask targetLayerMask = -1;

        private Enemy targetEnemy;

        // VFX 설정
        [Header("VFX Settings")]
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private ParticleSystem particle;

        // 공격 상태 열거형
        [HideInInspector] public enum AttackState { None, Preparing, Active, Finishing, Finished }

        public override void Activate(Attack attack, Vector2 direction)
        {
            // Debug.Log("Activating magic missile");
            // Debug.Log($"attack: {attack.attackData.type}");
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            attackState = AttackState.None;
            attackTimer = 0f;
            trail.enabled = false;
            magician = attack.attacker as Character002_Magician;
           
            // 공격 시작
            StartAttack();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            targetEnemy = null;
            trail.Clear();
            trail.enabled = false;
            StopAndDestroyVFX(spawnedVFX);
        }

        private void StartAttack()
        {
            maxBounces = magician.AC050_MaxBounces;
            bounceChance = magician.AC050_BounceChange;
            attackState = AttackState.None;
            attackTimer = 0f;
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
                    var choose = ChooseEnemyTarget();
                    if (!choose)
                    {
                        attackState = AttackState.Finishing;
                        return;
                    }
                    attackTimer = 0f;
                    attackState = AttackState.Preparing;
                    break;

                case AttackState.Preparing:
                    // 공격 지연 시간 대기
                    if (attackTimer >= attackDelay)
                    {
                        SetEnemyTarget(targetEnemy);
                        if (maxBounces == magician.AC050_MaxBounces)
                        {
                            attack.transform.position = magician.transform.position;
                        }
                        trail.enabled = true;
                        attackState = AttackState.Active;
                        attackTimer = 0f;
                    }
                    else
                    {
                        attackTimer += Time.deltaTime;
                    }
                    break;
                
                case AttackState.Active:
                    if (targetEnemy == null)
                    {
                        attackState = AttackState.Finishing;
                        return;
                    }

                    // 남은 시간
                    float remaining = missileTravelTime - attackTimer;
                    if (remaining <= 0f)
                    {
                        // 도탄 처리
                        var bounce = DamageToTarget();
                        if (bounce)
                        {
                            // 도탄 시 트레일 정보 초기화 하고 다시 한 번 동작
                            attackTimer = 0f;
                            trail.Clear();
                            trail.enabled = false;
                            attackState = AttackState.Preparing;
                        }
                        else
                        {
                            attackTimer = 0f;
                            attackState = AttackState.Finishing;
                        }
                    }
                    else
                    {
                        // 등속 이동 보정
                        Vector3 dir = targetEnemy.transform.position + (Vector3)targetEnemy.CenterOffset - attack.transform.position;
                        float distance = dir.magnitude;
                        if (distance > 0.01f)
                        {
                            float step = distance / remaining * Time.deltaTime;
                            attack.transform.position += dir.normalized * step;
                        }

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

        private bool DamageToTarget()
        {
            if (targetEnemy is not null)
            {
                // 도탄 여부 판단하기
                if (maxBounces > 0 && UnityEngine.Random.Range(0f, 1f) <= bounceChance)
                {
                    var newTargetList =
                        BattleStage.now.GetEnemiesInCircleRangeFromTargetOrderByDistance(targetEnemy, 10f);
                    if (newTargetList.Count > 1)
                    {
                        targetEnemy = newTargetList[1];
                        maxBounces--;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ChooseEnemyTarget()
        {
            var targetEnemyList = BattleStage.now.GetEnemiesInCircleRangeOrderByDistance(magician.transform.position, 10f, 1);
            if (targetEnemyList.Count > 0)
            {
                targetEnemy = targetEnemyList[0];
                return true;
            }
            return false;
        }

        private void SetEnemyTarget(Enemy target)
        {
            if (target == null)
            {
                attackState = AttackState.Finishing;
                return;
            }
            targetEnemy = target;
        }
    }
}