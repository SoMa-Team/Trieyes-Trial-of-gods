using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Threading;
using BattleSystem;
using System.Collections.Generic;
using static CharacterSystem.C001_Hero;

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
        private C001_Hero character;
        private float lastEnchantmentTime = 0f;
        private float generationInterval = 1f;

        // 강화 효과 상태 관리
        private float enchantmentTimer = 0f;
        private bool isActive = false;

        private HeroWeaponElementState weaponElementState;

        private const int FIRE_METEOR_ID = 7;
        private const int ICE_STORM_ID = 8;
        private const int LIGHTNING_FIELD_ID = 9;
        private const int LIGHT_GREAT_EXPLOSION_ID = 10;
        private const int NONE_ID = 11;

        public override void Activate(Attack attack, Vector2 direction)
        {
            character = attack.attacker as C001_Hero;
            // 강화 효과 초기화
            attack.attacker.bIsLockAttack = true;
            isActive = true;

            weaponElementState = character.weaponElementState;

            switch (weaponElementState)
            {
                case HeroWeaponElementState.Fire:
                    activateFireMeteor();
                    break;
                case HeroWeaponElementState.Ice:
                    activateIceStorm();
                    break;
                case HeroWeaponElementState.Lightning:
                    activateLightningField();
                    break;
                case HeroWeaponElementState.Light:
                    activateLightGreatExplosion();
                    break;
                default:
                    activateNone();
                    break;
            }

            AttackFactory.Instance.Deactivate(attack);
        }

        private void activateFireMeteor()
        {
            Attack fireMeteor = AttackFactory.Instance.ClonePrefab(FIRE_METEOR_ID);
            BattleStage.now.AttachAttack(fireMeteor);
            fireMeteor.Activate(character, Vector2.zero);
            character.ExecuteSkillAttack(fireMeteor.attackData);
        }

        private void activateIceStorm()
        {
            Attack iceStorm = AttackFactory.Instance.ClonePrefab(ICE_STORM_ID);
            BattleStage.now.AttachAttack(iceStorm);
            iceStorm.Activate(character, Vector2.zero);
            character.ExecuteSkillAttack(iceStorm.attackData);
        }

        private void activateLightningField()
        {
            Attack lightningField = AttackFactory.Instance.ClonePrefab(LIGHTNING_FIELD_ID);
            BattleStage.now.AttachAttack(lightningField);
            lightningField.Activate(character, Vector2.zero);
            character.ExecuteSkillAttack(lightningField.attackData);
        }

        private void activateLightGreatExplosion()
        {
            Attack lightGreatExplosion = AttackFactory.Instance.ClonePrefab(LIGHT_GREAT_EXPLOSION_ID);
            BattleStage.now.AttachAttack(lightGreatExplosion);
            lightGreatExplosion.Activate(character, Vector2.zero);
            character.ExecuteSkillAttack(lightGreatExplosion.attackData);
        }
        
        private void activateNone()
        {
            // 스킬 001 쿨타임 초기화
            // character.lastSkillAttack001Time = 0f;
        }
    }
} 