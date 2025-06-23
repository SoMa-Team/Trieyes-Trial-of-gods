using System.Collections.Generic;
using Utils; // For IEventHandler and EventType
using UnityEngine;
using CharacterSystem;
using CardSystem;

namespace CardActions
{
    /// <summary>
    /// 카드 액션의 기본 동작을 정의하는 추상 클래스입니다.
    /// 모든 구체적인 카드 액션은 이 클래스를 상속받아 고유한 OnEvent 로직을 구현해야 합니다.
    /// </summary>
    public abstract class CardAction : MonoBehaviour, IEventHandler
    {
        [Header("Action Info")]
        public int cardId;
        public Property[] properties;
        public Rarity rarity;
        public string cardName;
        public HashSet<Utils.EventType> EventTypes { get; protected set; } = new();//해당 카드 액션이 반응하는 이벤트 타입
        [System.NonSerialized] public string cardDescription;

        protected Pawn owner; // 카드 액션의 소유자 (Pawn)
        protected CardSystem.Deck deck; // 덱 참조 (카드 호출 순서 조정용)

        // ===== [기능 1] 프로퍼티 =====
        /// <summary>
        /// 카드 액션의 소유자를 반환합니다.
        /// </summary>
        public Pawn Owner => owner;

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

        public virtual void Activate()
        {
            
        }//허위 클래스에서 해당 메소드를 오버라이드하여 활성화로직을 구현합니다.

        public virtual void Deactivate()
        {
            
        }//허위 클래스에서 해당 메소드를 오버라이드하여 활성화로직을 구현합니다.

        // ===== [기능 3] 이벤트 처리(추상) =====
        /// <summary>
        /// 이벤트를 처리하는 추상 메서드입니다.
        /// 상속받는 클래스에서 특정 이벤트(예: OnAttack, OnDeath) 발생 시 수행할 고유한 로직을 구현합니다.
        /// </summary>
        /// <param name="eventType">발생한 이벤트의 타입</param>
        /// <param name="param">이벤트와 함께 전달된 매개변수</param>
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            // deck 참조가 없으면 에러 발생
            if (deck == null)
            {
                Debug.LogError($"<color=red>[CardAction] {GetType().Name} has no deck reference! Ensure SetOwner() is called before using this card action.</color>");
                return;
            }
            
            // 하위 클래스에서 이 메서드를 오버라이드하여
            // 개별 이벤트에 대한 구체적인 로직을 구현합니다.
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
    }
} 