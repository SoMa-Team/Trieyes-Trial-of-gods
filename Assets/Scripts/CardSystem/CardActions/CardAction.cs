using System.Collections.Generic;
using Utils; // For IEventHandler and EventType
using UnityEngine;

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
        private Dictionary<Utils.EventType, List<EventDelegate>> eventHandlers = new();
        protected CardAction(int id, string description = "")
        {
            cardId = id;
            cardDescription = description;
        }

        // ===== [기능 2] 이벤트 처리(추상) =====
        /// <summary>
        /// 이벤트를 처리하는 추상 메서드입니다.
        /// 상속받는 클래스에서 특정 이벤트(예: OnAttack, OnDeath) 발생 시 수행할 고유한 로직을 구현합니다.
        /// </summary>
        /// <param name="eventType">발생한 이벤트의 타입</param>
        /// <param name="param">이벤트와 함께 전달된 매개변수</param>
        public abstract void OnEvent(Utils.EventType eventType, object param);
    }
} 