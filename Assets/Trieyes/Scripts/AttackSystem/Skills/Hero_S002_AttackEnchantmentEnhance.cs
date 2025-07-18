using AttackSystem;
using CharacterSystem;
using UnityEngine;
using BattleSystem;
using System.Collections.Generic;

namespace AttackComponents
{
    /// <summary>
    /// 영웅 소드 공격 강화 효과
    /// - 현재 검에 부여된 속성에 따라 다양한 효과가 발생합니다.
    /// - 불 : 유성우가 떨어집니다. (AC102) 5)
    /// - 얼음 : 맵 전체에 눈보라를 불러일으킵니다. (AC103) 6)
    /// - 번개 : 플레이어 주변에 자기장을 형성하여 닿는 적에게 피해를 주고, 이동속도가 증가합니다. (AC100, 101) 7)
    /// - 천상 : 잠시동안 신의 가호를 받아 공격이 자그마한 폭발을 일으킵니다. (AC102) 8)
    /// - 무속성 : 원소 부여 스킬의 쿨타임을 초기화시킵니다. (AC101) 9)
    /// </summary>
    public class Hero_S002_AttackEnchantmentEnhance : AttackComponent
    {
        private Character001_Hero character;
        private HeroWeaponElementState weaponElementState;

        private const int FIRE_METEOR_ID = 7;
        private const int ICE_STORM_ID = 8;
        private const int LIGHTNING_FIELD_ID = 9;
        private const int LIGHT_ID = 6;

        // FSM 상태 관리
        private EnhancementState currentState = EnhancementState.None;
        private float stateTimer = 0f;
        private float attackInterval = 1f; // 공격 주기
        private float lastAttackTime = 0f;
        
        // 1회 실행 플래그
        private bool iceStormActivated = false;
        private bool lightningFieldActivated = false;

        // FSM 상태 열거형
        private enum EnhancementState
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
            
            character = attack.attacker as Character001_Hero;
            weaponElementState = character.weaponElementState;
            
            // 초기 상태 설정
            currentState = EnhancementState.Preparing;
            stateTimer = 0f;
            lastAttackTime = 0f;
            
            // 플래그 초기화
            iceStormActivated = false;
            lightningFieldActivated = false;
            
            // 속성별 초기화
            InitializeByElementState();
        }

        private void InitializeByElementState()
        {
            switch (weaponElementState)
            {
                case HeroWeaponElementState.Fire:
                    // Fire: 종료될 때까지 activateFireMeteor를 공격 주기마다 발동
                    break;
                case HeroWeaponElementState.Ice:
                    // Ice: 공격 주기에 해당하는 IceStorm(AC103)을 1회 발동하고 종료
                    break;
                case HeroWeaponElementState.Lightning:
                    // Lightning: 공격 주기에 해당하는 AC104를 1회 발동하고 종료
                    break;
                case HeroWeaponElementState.Light:
                    // Light: activateLight를 공격주기 동안 유지하고 종료할 때 deactivate에서 bool 값 전환
                    character.activateLight = true;
                    break;
                default:
                    // None: character.lastSkillAttack001Time = 0f; 설정하고 바로 종료
                    character.lastSkillAttack001Time = 0f;
                    currentState = EnhancementState.Finished;
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();
            
            // FSM 상태 처리
            ProcessEnhancementState();
        }

        private void ProcessEnhancementState()
        {
            switch (currentState)
            {
                case EnhancementState.None:
                    break;

                case EnhancementState.Preparing:
                    stateTimer += Time.deltaTime;
                    
                    if (stateTimer >= 0.1f) // 준비 시간
                    {
                        currentState = EnhancementState.Active;
                        stateTimer = 0f;
                        ActivateEnhancement();
                    }
                    break;

                case EnhancementState.Active:
                    stateTimer += Time.deltaTime;
                    
                    // 속성별 활성 상태 처리
                    ProcessActiveStateByElement();
                    
                    // 종료 조건 체크
                    if (ShouldFinishEnhancement())
                    {
                        currentState = EnhancementState.Finishing;
                        stateTimer = 0f;
                        FinishEnhancement();
                    }
                    break;

                case EnhancementState.Finishing:
                    stateTimer += Time.deltaTime;
                    
                    if (stateTimer >= 0.1f) // 종료 시간
                    {
                        currentState = EnhancementState.Finished;
                    }
                    break;

                case EnhancementState.Finished:
                    currentState = EnhancementState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ProcessActiveStateByElement()
        {
            switch (weaponElementState)
            {
                case HeroWeaponElementState.Fire:
                    // Fire: 공격 주기마다 activateFireMeteor 발동
                    if (Time.time - lastAttackTime >= attackInterval)
                    {
                        ActivateFireMeteor();
                        lastAttackTime = Time.time;
                    }
                    break;
                    
                case HeroWeaponElementState.Ice:
                    // Ice: 1회만 발동하고 종료
                    if (!iceStormActivated)
                    {
                        ActivateIceStorm();
                        iceStormActivated = true;
                    }
                    break;
                    
                case HeroWeaponElementState.Lightning:
                    // Lightning: 1회만 발동하고 종료
                    if (!lightningFieldActivated)
                    {
                        ActivateLightningField();
                        lightningFieldActivated = true;
                    }
                    break;
                    
                case HeroWeaponElementState.Light:
                    // Light: 공격주기 동안 유지 (activateLight는 이미 true로 설정됨)
                    break;
                    
                default:
                    break;
            }
        }

        private bool ShouldFinishEnhancement()
        {
            switch (weaponElementState)
            {
                case HeroWeaponElementState.Fire:
                    // Fire: 지속시간 동안 계속 (예: 10초)
                    return stateTimer >= 10f;
                    
                case HeroWeaponElementState.Ice:
                case HeroWeaponElementState.Lightning:
                    // Ice, Lightning: 1회 발동 후 바로 종료
                    return stateTimer >= 0.1f;
                    
                case HeroWeaponElementState.Light:
                    // Light: 공격주기 동안 유지 (예: 5초)
                    return stateTimer >= 5f;
                    
                default:
                    return true;
            }
        }

        private void ActivateEnhancement()
        {
            Debug.Log($"<color=green>[S002] {weaponElementState} 강화 활성화!</color>");
        }

        private void FinishEnhancement()
        {
            // Light 속성 종료 시 bool 값 전환
            if (weaponElementState == HeroWeaponElementState.Light)
            {
                character.activateLight = false;
            }
            
            Debug.Log($"<color=orange>[S002] {weaponElementState} 강화 종료!</color>");
        }

        private void ActivateFireMeteor()
        {
            Attack fireMeteor = AttackFactory.Instance.ClonePrefab(FIRE_METEOR_ID);
            if (fireMeteor != null)
            {
                BattleStage.now.AttachAttack(fireMeteor);
                fireMeteor.Activate(character, Vector2.zero);

                Debug.Log("<color=red>[S002] Fire Meteor 발동!</color>");
            }
        }

        private void ActivateIceStorm()
        {
            Attack iceStorm = AttackFactory.Instance.ClonePrefab(ICE_STORM_ID);
            if (iceStorm != null)
            {
                BattleStage.now.AttachAttack(iceStorm);
                var iceStormComponent = iceStorm.components[0] as AC008_IceStorm;
                iceStormComponent.summonDelay = 0.5f;
                iceStormComponent.vfxDuration = 0.3f;

                iceStorm.Activate(character, Vector2.zero);
                Debug.Log("<color=cyan>[S002] Ice Storm 발동!</color>");
            }
        }

        private void ActivateLightningField()
        {
            Attack lightningField = AttackFactory.Instance.ClonePrefab(LIGHTNING_FIELD_ID);
            if (lightningField != null)
            {
                BattleStage.now.AttachAttack(lightningField);
                var lightningFieldComponent = lightningField.components[0] as AC009_LightningField;
                lightningFieldComponent.lightningFieldRadius = 2.5f;
                lightningFieldComponent.lightningFieldDuration = 3f;
                lightningFieldComponent.lightningFieldDelay = 0.5f;

                lightningField.Activate(character, Vector2.zero);
                Debug.Log("<color=yellow>[S002] Lightning Field 발동!</color>");
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            // Light 속성 종료 시 bool 값 전환
            if (character != null && weaponElementState == HeroWeaponElementState.Light)
            {
                character.activateLight = false;
            }
        }
    }
} 