using System.Collections.Generic;
using AttackSystem;
using CharacterSystem;
using UnityEngine;
using Stats;
using System;
using BattleSystem;

namespace AttackComponents
{
    /// <summary>
    /// 1.5초 후 전방으로 공격력 [140]%의 부패 마법을 발사합니다. 
    /// 부패 마법을 맞은 대상이 죽지 않았다면 2초후에 전방 <2> Radius에 [100]%의 광역 부패 공격이 발사됩니다.
    /// 쿨타임 [8]초
    /// </summary>
    public class C002_S002_TimerBomb : AttackComponent
    {
        private Character002_Magician character;

        private Enemy hitEnemy;

        public AttackData AOEAttackData;
        // FSM 상태 관리
        private TimerBombState timerBombState = TimerBombState.None;

        public float timerBombRadius = 4f;
        public float timerBombAOERadius = 2f;

        public SpriteRenderer timerBombSprite;
        public ParticleSystem timerBombParticle;
        public GameObject timerBombAOEParticle;

        private Vector3 timerBombPosition;
        private Vector3 timerBombDirection;

        private const float timerBombZAngleDefault = 40f;
        
        private float timerBombTimer = 0f;
        private float timerBombMoveTimer = 0f;
        private float timerBombWaitingDuration = 2f;

        // 텔레포트 상태 열거형
        private enum TimerBombState
        {
            None,
            Preparing,
            Active,
            Active2,
            AOE,
            Finishing,
            Finished
        }
        
        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            character = attack.attacker as Character002_Magician;

            if (character is null)
            {
                Debug.LogError("[S001] Character001_Hero 컴포넌트를 찾을 수 없습니다!");
                return;
            }
            
            timerBombState = TimerBombState.Preparing;
            timerBombPosition = character.transform.position;
            timerBombDirection = direction;

            timerBombSprite.transform.rotation = Quaternion.Euler(0, 0, timerBombZAngleDefault + Mathf.Atan2(timerBombDirection.y, timerBombDirection.x) * Mathf.Rad2Deg);
            timerBombSprite.enabled = true;
            attack.GetComponent<Collider2D>().enabled = true;

            // 1 Update 마다 단위 벡터만큼 이동할 때, radius만큼 이동하면 종료
            timerBombMoveTimer = timerBombRadius / timerBombDirection.normalized.magnitude;
        }

        protected override void Update()
        {
            base.Update();
            
            // Lock 상태일 때는 Update 실행하지 않음
            if (isLocked) return;
            
            // 강화 효과 상태 처리
            ProcesstimerBombState();
        }

        public override void ProcessComponentCollision(Pawn targetPawn)
        {
            base.ProcessComponentCollision(targetPawn);

            hitEnemy = targetPawn as Enemy;
            if(hitEnemy != null)
            {
                if(!hitEnemy.isDead)
                {
                    timerBombState = TimerBombState.Active2;
                    timerBombTimer = 0f;
                }
                else
                {
                    timerBombState = TimerBombState.Finishing;
                    timerBombTimer = 0f;
                }

                timerBombSprite.enabled = false;
                attack.GetComponent<Collider2D>().enabled = false;
            }
        }

        private void ProcesstimerBombState()
        {
            switch (timerBombState)
            {
                case TimerBombState.None:
                    break;

                case TimerBombState.Preparing:
                    timerBombTimer += Time.deltaTime;
                    
                    if (timerBombTimer >= 0.1f) // 준비 시간
                    {
                        timerBombState = TimerBombState.Active;
                        timerBombTimer = 0f;
                    }
                    break;

                case TimerBombState.Active:
                    timerBombTimer += Time.deltaTime;
                    attack.transform.position = timerBombPosition + timerBombDirection * timerBombTimer;

                    if (timerBombTimer >= timerBombMoveTimer)
                    {
                        timerBombState = TimerBombState.Finishing;
                        timerBombTimer = 0f;
                    }
                    break;
                case TimerBombState.Active2:
                    timerBombTimer += Time.deltaTime;
                    
                    if (timerBombTimer >= timerBombWaitingDuration)
                    {
                        timerBombState = TimerBombState.AOE;
                        timerBombTimer = 0f;
                    }
                    break;
                case TimerBombState.AOE:
                    ExecuteAOE();
                    timerBombState = TimerBombState.Finishing;
                    timerBombTimer = 0f;
                    break;

                case TimerBombState.Finishing:
                    timerBombTimer += Time.deltaTime;
                    
                    if (timerBombTimer >= 0.1f) // 종료 시간
                    {
                        timerBombState = TimerBombState.Finished;
                    }
                    break;

                case TimerBombState.Finished:
                    timerBombState = TimerBombState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ExecuteAOE()
        {
            if(hitEnemy.isDead)
            {
                return;
            }
            // Hit Enemy를 중심으로 timerBombAOERadius 반경의 적들에게 AC100 AOE 공격을 소환합니다.
            var aoeAttack = AttackFactory.Instance.Create(AOEAttackData, attack.attacker, null, Vector2.zero);
            var aoeComponent = aoeAttack.components[0] as AC100_AOE;
            aoeComponent.aoeRadius = timerBombAOERadius;
            aoeComponent.aoeDamage = (int)character.GetStatValue(StatType.AttackPower);
            aoeComponent.aoeDuration = 1;
            aoeComponent.aoeInterval = 1;
            aoeComponent.aoeTargetType = AOETargetType.AreaAtPosition;
            aoeComponent.aoeShapeType = AOEShapeType.Circle;
            aoeComponent.SetAOEPosition((Vector2)hitEnemy.transform.position);
            aoeComponent.aoeVFXPrefab = timerBombAOEParticle;
            aoeComponent.Activate(aoeAttack, Vector2.zero);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            timerBombState = TimerBombState.None;
            timerBombTimer = 0f;
            timerBombMoveTimer = 0f;
            timerBombWaitingDuration = 2f;
            timerBombDirection = Vector3.zero;
            timerBombPosition = Vector3.zero;
            timerBombSprite.enabled = false;
            attack.GetComponent<Collider2D>().enabled = false;
        }
    }
} 