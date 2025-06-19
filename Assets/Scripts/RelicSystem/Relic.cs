using System.Collections.Generic;
using Utils;
using UnityEngine;
using CharacterSystem;

namespace RelicSystem
{
    /// <summary>
    /// 게임 내 유물을 나타내는 클래스입니다.
    /// 유물은 자체적으로 이벤트를 등록하고 처리할 수 있는 IEventHandler를 구현합니다.
    /// </summary>
    public abstract class Relic : IEventHandler
    {
        // ===== [기능 1] 유물 정보 및 생성 =====
        public RelicInfo info;
        protected Pawn owner; // 유물의 소유자 (Pawn)
        
        public Relic(RelicInfo info)
        {
            this.info = info;
        }

        // ===== [기능 2] 소유자 설정 =====
        /// <summary>
        /// 유물의 소유자를 설정합니다.
        /// </summary>
        /// <param name="pawn">유물의 소유자</param>
        public void SetOwner(Pawn pawn)
        {
            owner = pawn;
        }

        // ===== [기능 3] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            switch (eventType)
            {
                case Utils.EventType.OnDeath:
                    HandleOnDeath(param);
                    break;
                case Utils.EventType.OnBattleStart:
                    HandleOnBattleStart(param);
                    break;
                case Utils.EventType.OnBattleEnd:
                    HandleOnBattleEnd(param);
                    break;
                case Utils.EventType.OnAttack:
                    HandleOnAttack(param);
                    break;
                case Utils.EventType.OnCriticalAttack:
                    HandleOnCriticalAttack(param);
                    break;
                case Utils.EventType.OnDamaged:
                    HandleOnDamaged(param);
                    break;
                case Utils.EventType.OnAttackHit:
                    HandleOnAttackHit(param);
                    break;
                case Utils.EventType.OnDamageHit:
                    HandleOnDamageHit(param);
                    break;
                case Utils.EventType.OnAttackMiss:
                    HandleOnAttackMiss(param);
                    break;
                case Utils.EventType.OnEvaded:
                    HandleOnEvaded(param);
                    break;
                case Utils.EventType.OnKilled:
                    HandleOnKilled(param);
                    break;
                case Utils.EventType.OnKilledByCritical:
                    HandleOnKilledByCritical(param);
                    break;
                case Utils.EventType.OnSkillCooldownEnd:
                    HandleOnSkillCooldownEnd(param);
                    break;
                case Utils.EventType.OnSkillInput:
                    HandleOnSkillInput(param);
                    break;
                case Utils.EventType.OnHPUpdated:
                    HandleOnHPUpdated(param);
                    break;
                case Utils.EventType.OnGoldUpdated:
                    HandleOnGoldUpdated(param);
                    break;
                case Utils.EventType.OnLevelUp:
                    HandleOnLevelUp(param);
                    break;
                case Utils.EventType.OnStatChange:
                    HandleOnStatChange(param);
                    break;
                case Utils.EventType.OnDefend:
                    HandleOnDefend(param);
                    break;
                case Utils.EventType.OnHit:
                    HandleOnHit(param);
                    break;
                case Utils.EventType.OnCardPurchase:
                    HandleOnCardPurchase(param);
                    break;
                case Utils.EventType.OnBattleSceneChange:
                    HandleOnBattleSceneChange(param);
                    break;
                case Utils.EventType.CalcActionInitOrder:
                    HandleCalcActionInitOrder(param);
                    break;
            }
        }

        // ===== [기능 4] 이벤트 핸들러 메서드들 =====
        
        protected virtual void HandleOnDeath(object param) { }
        protected virtual void HandleOnBattleStart(object param) { }
        protected virtual void HandleOnBattleEnd(object param) { }
        protected virtual void HandleOnAttack(object param) { }
        protected virtual void HandleOnCriticalAttack(object param) { }
        protected virtual void HandleOnDamaged(object param) { }
        protected virtual void HandleOnAttackHit(object param) { }
        protected virtual void HandleOnDamageHit(object param) { }
        protected virtual void HandleOnAttackMiss(object param) { }
        protected virtual void HandleOnEvaded(object param) { }
        protected virtual void HandleOnKilled(object param) { }
        protected virtual void HandleOnKilledByCritical(object param) { }
        protected virtual void HandleOnSkillCooldownEnd(object param) { }
        protected virtual void HandleOnSkillInput(object param) { }
        protected virtual void HandleOnHPUpdated(object param) { }
        protected virtual void HandleOnGoldUpdated(object param) { }
        protected virtual void HandleOnLevelUp(object param) { }
        protected virtual void HandleOnStatChange(object param) { }
        protected virtual void HandleOnDefend(object param) { }
        protected virtual void HandleOnHit(object param) { }
        protected virtual void HandleOnCardPurchase(object param) { }
        protected virtual void HandleOnBattleSceneChange(object param) { }
        protected virtual void HandleCalcActionInitOrder(object param) { }

        // ===== [기능 5] AttackComponent 제공 =====
        /// <summary>
        /// 이 유물이 제공하는 AttackComponent 목록을 반환합니다.
        /// 기본적으로는 null을 반환하며, 하위 클래스에서 오버라이드하여 구현합니다.
        /// </summary>
        /// <returns>AttackComponent 목록 또는 null</returns>
        public virtual List<AttackComponents.AttackComponent> GetAttackComponents()
        {
            return null; // 기본적으로는 AttackComponent를 제공하지 않음
        }
    }
} 