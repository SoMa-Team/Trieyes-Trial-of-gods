using System.Collections.Generic;
using Utils; // For IEventHandler and EventType
using UnityEngine;
using CharacterSystem;

namespace CardActions
{
    /// <summary>
    /// 카드 액션의 기본 동작을 정의하는 추상 클래스입니다.
    /// 모든 구체적인 카드 액션은 이 클래스를 상속받아 고유한 OnEvent 로직을 구현해야 합니다.
    /// </summary>
    public abstract class CardAction : IEventHandler
    {
        // ===== [기능 1] 카드 액션 정보 및 생성 =====
        public int cardId { get; private set; }
        public string cardDescription { get; protected set; }
        protected Pawn owner; // 카드 액션의 소유자 (Pawn)
        protected CardSystem.Deck deck; // 덱 참조 (카드 호출 순서 조정용)
        private Dictionary<Utils.EventType, List<EventDelegate>> eventHandlers = new();
        
        protected CardAction(int id, string description = "")
        {
            cardId = id;
            cardDescription = description;
        }

        // ===== [기능 2] 소유자 설정 =====
        /// <summary>
        /// 카드 액션의 소유자를 설정합니다.
        /// </summary>
        /// <param name="pawn">카드 액션의 소유자</param>
        public void SetOwner(Pawn pawn)
        {
            owner = pawn;
            if (pawn != null)
            {
                deck = pawn.deck; // 덱 참조 설정
            }
        }

        // ===== [기능 3] 이벤트 처리(추상) =====
        /// <summary>
        /// 이벤트를 처리하는 추상 메서드입니다.
        /// 상속받는 클래스에서 특정 이벤트(예: OnAttack, OnDeath) 발생 시 수행할 고유한 로직을 구현합니다.
        /// </summary>
        /// <param name="eventType">발생한 이벤트의 타입</param>
        /// <param name="param">이벤트와 함께 전달된 매개변수</param>
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

        /// <summary>
        /// CalcActionInitOrder 이벤트를 처리합니다.
        /// 하위 클래스에서 오버라이드하여 카드 호출 순서를 조정할 수 있습니다.
        /// </summary>
        /// <param name="param">현재 카드 인덱스</param>
        protected virtual void HandleCalcActionInitOrder(object param)
        {
            // 기본 구현: 아무것도 하지 않음
            // 하위 클래스에서 오버라이드하여 카드 호출 순서 조정 로직 구현
        }

        /// <summary>
        /// OnBattleSceneChange 이벤트를 처리합니다.
        /// 하위 클래스에서 오버라이드하여 전투 씬 전환 시 수행할 로직을 구현합니다.
        /// </summary>
        /// <param name="param">이벤트 매개변수</param>
        protected virtual void HandleOnBattleSceneChange(object param)
        {
            // 기본 구현: 아무것도 하지 않음
            // 하위 클래스에서 오버라이드하여 전투 씬 전환 시 로직 구현
        }

        // ===== [기능 4] AttackComponent 제공 =====
        /// <summary>
        /// 이 카드 액션이 제공하는 AttackComponent 목록을 반환합니다.
        /// 기본적으로는 null을 반환하며, 하위 클래스에서 오버라이드하여 구현합니다.
        /// </summary>
        /// <returns>AttackComponent 목록 또는 null</returns>
        public virtual List<AttackComponents.AttackComponent> GetAttackComponents()
        {
            return null; // 기본적으로는 AttackComponent를 제공하지 않음
        }

        // ===== [기능 5] 기타 이벤트 핸들러 메서드들 =====
        
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
    }
} 