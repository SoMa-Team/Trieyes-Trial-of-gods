using System.Collections.Generic;
using AttackSystem;
using CharacterSystem;
using UnityEngine;
using Stats;
using System;

namespace AttackComponents
{
    /// <summary>
    /// 0.3초 후 바라보는 방향 [4] Radius로 순간이동합니다. 
    /// 원래 위치와 순간이동된 위치 사이에 있는 모든 적들은 [125]%의 번개 공격을 받습니다.
    /// </summary>
    public class C002_S001_Teleport : AttackComponent
    {
        private Character002_Magician character;
        // FSM 상태 관리
        private TeleportState teleportState = TeleportState.None;

        public float teleportDuration; // 텔레포트 끝나는데 걸리는 시간
        public float teleportRadius = 4f;

        public ParticleSystem teleportParticle;
        public TrailRenderer teleportTrail;

        private Vector3 teleportPosition;
        private Vector3 teleportDirection;
        
        private float teleportTimer = 0f;

        // 텔레포트 상태 열거형
        private enum TeleportState
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
            
            character = attack.attacker as Character002_Magician;

            if (character is null)
            {
                Debug.LogError("[S001] Character001_Hero 컴포넌트를 찾을 수 없습니다!");
                return;
            }
            
            character.SetLockMovement(true);
            teleportPosition = character.transform.position;
            teleportDirection = direction;
            teleportParticle.transform.localPosition = character.CenterOffset;
            teleportState = TeleportState.Preparing;
            teleportTimer = 0f;
        }

        protected override void Update()
        {
            base.Update();
            
            // Lock 상태일 때는 Update 실행하지 않음
            if (isLocked) return;
            
            // 강화 효과 상태 처리
            ProcessteleportState();
        }

        private void ProcessteleportState()
        {
            switch (teleportState)
            {
                case TeleportState.None:
                    break;

                case TeleportState.Preparing:
                    teleportTimer += Time.deltaTime;
                    
                    if (teleportTimer >= 0.1f) // 준비 시간
                    {
                        Disappear();
                        teleportState = TeleportState.Active;
                        teleportTimer = 0f;
                    }
                    break;

                case TeleportState.Active:
                    teleportTimer += Time.deltaTime;

                    if (teleportTimer >= teleportDuration)
                    {
                        Appear();
                        teleportState = TeleportState.Finishing;
                        teleportTimer = 0f;
                    }
                    break;

                case TeleportState.Finishing:
                    teleportTimer += Time.deltaTime;
                    
                    if (teleportTimer >= 0.1f) // 종료 시간
                    {
                        teleportState = TeleportState.Finished;
                    }
                    break;

                case TeleportState.Finished:
                    teleportState = TeleportState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void Appear()
        {
            teleportParticle.Stop();
            character.transform.position = teleportPosition + teleportDirection * teleportRadius;
            character.gameObject.SetActive(true);
        }

        private void Disappear()
        {
            teleportParticle.Play();
            character.gameObject.SetActive(false);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            character.SetLockMovement(false);
            teleportParticle.transform.localPosition = Vector3.zero;
            teleportTimer = 0f;
        }
    }
} 