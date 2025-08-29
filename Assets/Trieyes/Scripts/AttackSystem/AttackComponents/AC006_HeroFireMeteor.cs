using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Threading;
using BattleSystem;

namespace AttackComponents
{
    /// <summary>
    /// 파이어 메테오 공격
    /// AC103_FALL을 소환하고 바로 종료하는 FSM 패턴 구현
    /// </summary>
    public class AC006_HeroFireMeteor : AttackComponent
    {
        public AttackData fallAttackData;

        // 메테오 설정
        [Header("Meteor Settings")]
        public float meteorRadius = 2f; // 메테오 반지름

        // AC103 FALL VFX 설정
        [Header("AC103 FALL VFX Settings")]
        [SerializeField] private GameObject fallingVFXPrefab; // 떨어지는 VFX 프리팹 (AC103에 전달용)
        [SerializeField] private GameObject explosionVFXPrefab; // 폭발 VFX 프리팹 (AC103에 전달용)

        // FSM 상태 관리
        private FireMeteorState attackState = FireMeteorState.None;
        private float attackTimer = 0f;

        // 파이어 메테오 공격 상태 열거형
        private enum FireMeteorState
        {
            None,
            Preparing,
            Summoning,
            Finishing,
            Finished
        }

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            attackState = FireMeteorState.Preparing;
            attackTimer = 0f;
            
            // 파이어 메테오 공격 시작
            StartFireMeteorAttack();
        }

        private void StartFireMeteorAttack()
        {
            attackState = FireMeteorState.Preparing;
            attackTimer = 0f;
            
            Debug.Log("<color=red>[AC007] 파이어 메테오 공격 시작!</color>");
        }

        protected override void Update()
        {
            base.Update();
            
            // Lock 상태일 때는 Update 실행하지 않음
            if (isLocked) return;
            
            // 파이어 메테오 공격 상태 처리
            ProcessFireMeteorAttackState();
        }

        private void ProcessFireMeteorAttackState()
        {
            switch (attackState)
            {
                case FireMeteorState.None:
                    break;

                case FireMeteorState.Preparing:
                    attackTimer += Time.deltaTime;
                    
                    if (attackTimer >= 0.1f) // 준비 시간
                    {
                        attackState = FireMeteorState.Summoning;
                        attackTimer = 0f;
                        SummonFireMeteor();
                    }
                    break;

                case FireMeteorState.Summoning:
                    attackTimer += Time.deltaTime;
                    
                    if (attackTimer >= 0.1f) // 소환 완료 시간
                    {
                        attackState = FireMeteorState.Finishing;
                        attackTimer = 0f;
                    }
                    break;

                case FireMeteorState.Finishing:
                    attackTimer += Time.deltaTime;
                    
                    if (attackTimer >= 0.1f) // 종료 시간
                    {
                        attackState = FireMeteorState.Finished;
                    }
                    break;

                case FireMeteorState.Finished:
                    attackState = FireMeteorState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void SummonFireMeteor()
        {
            // AC103_FALL 소환
            var fallAttack = AttackFactory.Instance.Create(fallAttackData, attack.attacker, null, Vector2.zero);
            
            var fallComponent = fallAttack.components[0] as AC103_FALL;
            if (fallComponent != null)
            {
                fallComponent.fallXYOffset = Vector2.zero;
                fallComponent.fallXRandomOffsetMin = -2;
                fallComponent.fallXRandomOffsetMax = 2;
                fallComponent.fallYRandomOffsetMin = -2;
                fallComponent.fallYRandomOffsetMax = 2;
                fallComponent.fallRadius = meteorRadius;
                fallComponent.fallDuration = 0.5f;
                fallComponent.fallDelay = 0.1f;
                fallComponent.fallDamage = 50;
                
                // VFX 프리팹 전달
                fallComponent.fallingVFXPrefab = fallingVFXPrefab;
                fallComponent.explosionVFXPrefab = explosionVFXPrefab;
            }
            
            Debug.Log("<color=red>[AC007] AC103_FALL 파이어 메테오 소환 완료!</color>");
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            attackState = FireMeteorState.None;
            attackTimer = 0f;
        }


    }
}